using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.backbone_db;

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
            }
            catch (Exception ex)
            {
                ex = null;
            }
            finally { 
            
            }

        }

    }
}
