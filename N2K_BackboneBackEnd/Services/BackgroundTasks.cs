namespace N2K_BackboneBackEnd.Services
{
    public class BackgroundTasks : BackgroundService, IHostedService 
    {

        private readonly ILogger<BackgroundTasks> _logger;
        private readonly IExtractionService _extractionService;

        public BackgroundTasks(ILogger<BackgroundTasks> logger, IExtractionService extractionService)
        {
            _logger = logger;
            _extractionService = extractionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Started background service \"BackgroundTasks\"");
            
            DateTime now = new DateTime();
            DateTime executionTime = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);
            // Execute at midnight
            using PeriodicTimer timer = new(executionTime - now);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Generating new extraction");
                    await _extractionService.UpdateExtraction();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service \"BackgroundTasks\" is stopping.");
            }
        }
    }
}
