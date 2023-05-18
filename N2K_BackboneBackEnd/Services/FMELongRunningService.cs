using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public class FMELongRunningService : BackgroundService
    {

        private readonly TimeSpan _period = TimeSpan.FromSeconds(20);
        private readonly ILogger<FMELongRunningService> _logger;
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly IOptions<ConfigSettings> _appSettings;
        private int _executionCount = 0;
        public bool IsEnabled { get; set; }


        public FMELongRunningService(
            ILogger<FMELongRunningService> logger, 
            IOptions<ConfigSettings> appSettings, 
            IServiceScopeFactory serviceProvider)
        {
            _logger = logger;
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // ExecuteAsync is executed once and we have to take care of a mechanism ourselves that is kept during operation.
            // To do this, we can use a Periodic Timer, which, unlike other timers, does not block resources.
            // But instead, WaitForNextTickAsync provides a mechanism that blocks a task and can thus be used in a While loop.
            using PeriodicTimer timer = new PeriodicTimer(_period);

            // When ASP.NET Core is intentionally shut down, the background service receives information
            // via the stopping token that it has been canceled.
            // We check the cancellation to avoid blocking the application shutdown.
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    // We cannot use the default dependency injection behavior, because ExecuteAsync is
                    // a long-running method while the background service is running.
                    // To prevent open resources and instances, only create the services and other references on a run

                    // Create scope, so we get request services
                    await using AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();

                    // Get service from scope
                    SampleService sampleService = asyncScope.ServiceProvider.GetRequiredService<SampleService>();

                    //scopedProcessingService.CheckFMEJobsStatus(_appSettings);
                    await sampleService.DoSomethingAsync();

                    // Sample count increment
                    _executionCount++;
                        _logger.LogInformation(
                            $"Executed PeriodicHostedService - Count: {_executionCount}");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(
                        $"Failed to execute PeriodicHostedService with exception message {ex.Message}. Good luck next round!");
                }
            }
        }
    }
}


