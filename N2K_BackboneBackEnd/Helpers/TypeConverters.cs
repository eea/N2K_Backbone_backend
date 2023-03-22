using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace N2K_BackboneBackEnd.Helpers
{
    public static class TypeConverters
    {

        public static T CheckNull<T>(object obj)
        {
            return obj == DBNull.Value ? default(T) : (T)obj;
        }


        public static System.Data.DataTable PrepareDataForBulkCopy<T>(this IList<T> data, SqlBulkCopy copy)
        {
            IList<string> notMappedFields = new List<string>();
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            System.Data.DataTable table = new System.Data.DataTable();
            //check if the field has a NotMapped attribute.
            //if so, do not include it in the output datatable
            foreach (PropertyDescriptor prop in properties)
            {
                var notMapped = false;
                foreach (var attr in prop.Attributes)
                {
                    if (attr.ToString().IndexOf("NotMappedAttribute") > -1)
                    {
                        notMapped = true;
                        notMappedFields.Add(prop.Name);
                        break;
                    }
                }
                if (!notMapped)
                {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    copy.ColumnMappings.Add(prop.Name, prop.Name);
                }
            }
        
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties) {
                    if (!notMappedFields.Contains(prop.Name))
                    {                     
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}