using Quartz;
using Stock_Market.Controllers;

namespace StockMarket.Services.PeriodicTaskHosted
{
    public class ScheduledSymbolsDataService : IJob
    {
        private readonly ILogger<UpdateSymbolsDataService> _logger;
        private readonly IServiceProvider _services;

        public ScheduledSymbolsDataService(ILogger<UpdateSymbolsDataService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public Task Execute(IJobExecutionContext context)
        {
            //task-01 -> UpdateSymbolsData
            _logger.LogInformation($"Executing scheduled Quartz task-01 at: {DateTime.Now}");
            using (var scope = _services.CreateScope())
            {
            var targetController = scope.ServiceProvider.GetRequiredService<StockMarketController>();
                targetController.UpdateSymbolsData().GetAwaiter().GetResult();
            }

            return Task.CompletedTask;
        }
    }

}
