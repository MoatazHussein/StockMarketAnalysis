using Stock_Market.Controllers;

namespace StockMarket.Services.PeriodicTaskHosted
{
    public class GeneralPeriodicTaskService : BackgroundService
    {
        private readonly ILogger<UpdateSymbolsDataService> _logger;
        private readonly IServiceProvider _services;

        public GeneralPeriodicTaskService(IServiceProvider services, ILogger<UpdateSymbolsDataService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)

            {
                var currentTime = DateTime.Now;
                var dayOfWeek = currentTime.DayOfWeek;

                // Check if it's a weekday between 10 AM and 2 PM
                if (//dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday &&
                    currentTime.Hour >= 10 && currentTime.Hour < 15)
                {
                    // Your task logic here
                    await DoWorkAsync(stoppingToken);
                }

                // Wait for an hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                //await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            // Example of using a hosted service
            _logger.LogInformation($"Periodic Task-01 Hosted Service has executed: at {DateTime.Now}");

            using var scope = _services.CreateScope();
            var myService = scope.ServiceProvider.GetRequiredService<StockMarketController>();
            await myService.UpdateSymbolsData();
        }
}
}
