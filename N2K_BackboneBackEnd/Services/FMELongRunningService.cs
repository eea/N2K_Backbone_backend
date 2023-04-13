using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using System.Text;

namespace N2K_BackboneBackEnd.Services
{
    public class FMELongRunningService : BackgroundService
    {
        private readonly BackgroundSpatialHarvestJobs fme_jobs;
        private readonly IOptions<ConfigSettings> _appSettings;

        public FMELongRunningService(BackgroundSpatialHarvestJobs jobs, IOptions<ConfigSettings> appSettings)
        {
            this.fme_jobs = jobs;
            _appSettings = appSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //check every 20seconds if fme jobs have been completed
                await Task.Delay(20000);
                fme_jobs.CheckFMEJobsStatus(_appSettings);
            }
        }
    }
}