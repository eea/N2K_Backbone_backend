using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.backbone_db;

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

    }
}
