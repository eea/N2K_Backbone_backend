using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.backbone_db;
using Microsoft.Data.SqlClient;

namespace N2K_BackboneBackEnd.Models
{
    
    public static class TimeLog 
    {

        public static void setTime(N2KBackboneContext pDataContext, string pProcessName, string pAction) {
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

        public static void setTimeStamp(string pProcessName, string pAction)
        {
            return;
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

    public static class SystemLog 
    {
        public enum errorLevel
        {
            [System.Runtime.Serialization.DataMember]
            Panic,
            [System.Runtime.Serialization.DataMember]
            Fatal,
            [System.Runtime.Serialization.DataMember]
            Error,
            [System.Runtime.Serialization.DataMember]
            Warning,
            [System.Runtime.Serialization.DataMember]
            Info,
            [System.Runtime.Serialization.DataMember]
            Debug
        }

        public static void write(errorLevel pLevel, string pMessage, string pClass, string pSource)
        {
            SqlConnection conn=null;
            SqlCommand cmd = null;
            SqlParameter param1 = null;
            SqlParameter param2 = null;
            SqlParameter param3 = null;
            SqlParameter param4 = null;
            SqlParameter param5 = null;
            //TODO: Log level configurable on the settings
            try
            {
               ;

                conn = new SqlConnection(WebApplication.CreateBuilder().Configuration.GetConnectionString("N2K_BackboneBackEndContext"));
                conn.Open();
                cmd = conn.CreateCommand();
                param1 = new SqlParameter("@Level", pLevel);
                param2 = new SqlParameter("@Message", pMessage);
                param3 = new SqlParameter("@TimeStamp", DateTime.Now);
                param4 = new SqlParameter("@Class", pClass);
                param5 = new SqlParameter("@Source", pSource);

                cmd.CommandText = "INSERT INTO SystemLog ([Level],[Message],[TimeStamp],[Class],[Source]) VALUES (@Level,@Message,@TimeStamp,@Class,@Source)";
                cmd.Parameters.Add(param1);
                cmd.Parameters.Add(param2);
                cmd.Parameters.Add(param3);
                cmd.Parameters.Add(param4);
                cmd.Parameters.Add(param5);

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
                param4 = null;
                param5 = null;
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

        public static void write(errorLevel pLevel, Exception pException, string pClass, string pSource)
        {
            //TODO: Log level configurable on the settings
            try
            {
                write(pLevel, pException.Message, pClass, pSource);
                Exception exec = pException.InnerException;
                while (exec != null) {
                    write(pLevel, exec.Message, pClass, "InnerException");
                    exec = exec.InnerException;
                }
                write(pLevel, pException.StackTrace, pClass, "StackTrace");
                
                
            }
            catch (Exception ex)
            {
                ex = null;
            }
            finally
            {
           
            }

        }

    }
    /// <summary>
    /// Adds a site inte the list of the refused sites in the harvest
    /// </summary>
    public static class RefusedSites {
        public static void addAsRefused(versioning_db.NaturaSite pSite, EnvelopesToProcess pEnvelope, DbUpdateException pException)
        {
            Exception e = null;
            try
            {
                
                string mess = pException.Message;
                e = pException.InnerException;
                while (e!=null) {
                    mess += " -> " + e.Message;
                    e = e.InnerException;
                }

                write(pEnvelope.CountryCode,pEnvelope.VersionId, pSite.SITECODE, Int32.Parse(pSite.VERSIONID.ToString()), mess, "", "", "Detected");


            }
            catch (Exception ex)
            {
                ex = null;
            }
            finally
            {
                e = null;
            }
        }


        public static void addAsRefused(versioning_db.NaturaSite pSite, EnvelopesToProcess pEnvelope, Exception pException, string pChildType, string pChildIdentificacion)
        {
            string mess = pException.Message;
            Exception e = pException.InnerException;
            while (e != null)
            {
                mess += " -> " + e.Message;
                e = e.InnerException;
            }

            write(pEnvelope.CountryCode, pEnvelope.VersionId, pSite.SITECODE, Int32.Parse(pSite.VERSIONID.ToString()), mess, "", "", "Detected");
        }


        private static void write(string pCountry, int pEnvelopVErsion, string pSitecode, int pVersionId, string pCause, string pChildEntity, string pChildIdentification, string pStatus) {
            SqlConnection conn = null;
            SqlCommand cmd = null;

            try
            {
                conn = new SqlConnection(WebApplication.CreateBuilder().Configuration.GetConnectionString("N2K_BackboneBackEndContext"));
                conn.Open();
                cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO RefusedSites ([Country],[EnvelopVersion],[SiteCode],[VersionId],[AttempDate],[Cause],[ChildEntity],[ChildIdentification],[Status]) VALUES (@Country,@EnvelopVersion,@SiteCode,@VersionId,@AttempDate,@Cause,@ChildEntity,@ChildIdentification,@Status)";
                cmd.Parameters.Add(new SqlParameter("@Country", pCountry));
                cmd.Parameters.Add(new SqlParameter("@EnvelopVersion", pEnvelopVErsion));
                cmd.Parameters.Add(new SqlParameter("@SiteCode", pSitecode));
                cmd.Parameters.Add(new SqlParameter("@VersionId", pVersionId));
                cmd.Parameters.Add(new SqlParameter("@AttempDate", DateTime.Now));
                cmd.Parameters.Add(new SqlParameter("@Cause", pCause));
                cmd.Parameters.Add(new SqlParameter("@ChildEntity", pChildEntity));
                cmd.Parameters.Add(new SqlParameter("@ChildIdentification", pChildIdentification));
                cmd.Parameters.Add(new SqlParameter("@Status", pStatus));

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ex = null;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (conn != null)
                {
                    if (conn.State != System.Data.ConnectionState.Closed) conn.Close();
                    conn.Dispose();
                }
            }

        }
    
    }


}
