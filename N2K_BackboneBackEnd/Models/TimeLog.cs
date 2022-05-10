using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Models
{
    public static class TimeLog
    {

        public static void setTime(N2KBackboneContext pDataContext, string pProcessName, string pAction) {
            try
            {
                pDataContext.Set<ProcessTimeLog>().Add(new ProcessTimeLog(pProcessName, pAction));
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
