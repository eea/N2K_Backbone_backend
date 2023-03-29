using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Services
{
    public class LongRunningBackgroundService : BackgroundService
    {
        private readonly BackgroundWorkerQueue queue;
        //private readonly N2KBackboneContext _dataContext;


        public LongRunningBackgroundService(BackgroundWorkerQueue queue)
        {
            this.queue = queue;
            //this._dataContext = dataContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await queue.DequeueAsync(stoppingToken);

                await workItem(stoppingToken);
            }
        }
    }
}
