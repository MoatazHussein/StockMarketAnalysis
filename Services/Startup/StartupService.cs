using Stock_Market.Controllers;
using StockMarket.Services.PeriodicTaskHosted;

namespace StockMarket.Services.Startup
{
    public class StartupService
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IServiceProvider _services;

        public StartupService(IServiceProvider services, ILogger<StartupService> logger)
        {
            _services = services;
            _logger = logger;

        }
        public void Initialize() { 

            _logger.LogInformation("Startup Service initialized!");

            using (var scope = _services.CreateScope())
            {
                var targetController = scope.ServiceProvider.GetRequiredService<StockMarketController>();

                //targetController.UpdateSymbolsData().GetAwaiter().GetResult();
                //targetController.updateAllSymbolsTechnicalAnalysis().GetAwaiter().GetResult();
            }
        }
    }
}
