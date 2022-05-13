using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.backbone_db;
using Microsoft.Data.SqlClient;

namespace N2K_BackboneBackEnd.Models
{
    
    public class TimeLog 
    {

        public void setTime(N2KBackboneContext pDataContext, string pProcessName, string pAction) {
            try
            {
                ProcessTimeLog ptl = new ProcessTimeLog();

                ptl.ProcessName = pProcessName;
                ptl.ActionPerformed = pAction;
                ptl.StampTime = DateTime.Now;


                pDataContext.Set<ProcessTimeLog>().Add(ptl);
                pDataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                ex = null;
            }
            finally { 
            
            }

        }

        public void setTimeStamp(string pProcessName, string pAction)
        {
            SqlConnection conn=null;
            SqlCommand cmd = null;
            SqlParameter param1 = null;
            SqlParameter param2 = null;
            SqlParameter param3 = null;

            try
            {
               ;

                conn = new SqlConnection(WebApplication.CreateBuilder().Configuration.GetConnectionString("N2K_BackboneBackEndContext"));
                conn.Open();
                cmd = conn.CreateCommand();
                param1 = new SqlParameter("@ProcessName", pProcessName);
                param2 = new SqlParameter("@ActionPerformed", pAction);
                param3 = new SqlParameter("@StampTime", DateTime.Now);

                cmd.CommandText = "INSERT INTO ProcessTimeLog  (ProcessName,ActionPerformed,StampTime) VALUES (@ProcessName,@ActionPerformed,@StampTime)";
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ex = null;
            }
            finally
            {
                param1 = null;
                param2 = null;
                param3 = null;
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (conn != null ) { 
                    if(conn.State != System.Data.ConnectionState.Closed) conn.Close();
                    conn.Dispose();
                }
               
            }

        }

    }
}
