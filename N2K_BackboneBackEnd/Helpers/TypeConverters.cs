using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;

namespace N2K_BackboneBackEnd.Helpers
{
    public static class TypeConverters
    {
        public static DataTable PrepareDataForBulkCopy<T>(this IList<T> data, SqlBulkCopy copy)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                copy.ColumnMappings.Add(prop.Name, prop.Name);
            }
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}