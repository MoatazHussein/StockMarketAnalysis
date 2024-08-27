using Quartz;
using Stock_Market.Controllers;

namespace StockMarket.Services.PeriodicTaskHosted
{
    public class ScheduledTechnicalAnalysisService : IJob
    {
        private readonly ILogger<UpdateSymbolsDataService> _logger;
        private readonly IServiceProvider _services;

        public ScheduledTechnicalAnalysisService(ILogger<UpdateSymbolsDataService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public Task Execute(IJobExecutionContext context)
        {
            //task-02 -> updateAllSymbolsTechnicalAnalysis
            _logger.LogInformation($"Executing scheduled Quartz task-02 at: {DateTime.Now}");
            using (var scope = _services.CreateScope())
            {
                var targetController = scope.ServiceProvider.GetRequiredService<StockMarketController>();
                targetController.updateAllSymbolsTechnicalAnalysis().GetAwaiter().GetResult();
            }

            return Task.CompletedTask;
        }
    }

}
