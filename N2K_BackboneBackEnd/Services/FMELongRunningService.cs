using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using System.Text;

namespace N2K_BackboneBackEnd.Services
{

    public class FMELongRunningService : BackgroundService
    {
        private readonly IBackgroundSpatialHarvestJobs fme_jobs;
        private readonly IOptions<ConfigSettings> _appSettings;
        
        //private readonly N2KBackboneContext _dataContext;
        private readonly IServiceProvider _serviceProvider;


        public FMELongRunningService(IBackgroundSpatialHarvestJobs jobs, IOptions<ConfigSettings> appSettings, IServiceProvider serviceProvider)
        {
            this.fme_jobs = jobs;
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //check every 20seconds if fme jobs have been completed
                await Task.Delay(20000);
                
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    IBackgroundSpatialHarvestJobs scopedProcessingService =
                        scope.ServiceProvider.GetRequiredService<IBackgroundSpatialHarvestJobs>();

                    //await scopedProcessingService.DoWorkAsync(stoppingToken);
                    //fme_jobs.CheckFMEJobsStatus(_appSettings);
                    scopedProcessingService.CheckFMEJobsStatus(_appSettings);

                }

            }
        }
    }
}