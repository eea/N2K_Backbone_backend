using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public interface IBackgroundSpatialHarvestJobs
    {
        event EventHandler<FMEJobEventArgs> FMEJobCompleted;
        void CheckFMEJobsStatus(IOptions<ConfigSettings> appSettings);
        Task LaunchFMESpatialHarvestBackground(EnvelopesToProcess envelope);

        N2KBackboneContext GetDataContext();
    }
}
