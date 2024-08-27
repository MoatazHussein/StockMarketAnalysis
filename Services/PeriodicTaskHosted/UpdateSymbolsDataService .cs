using Stock_Market.Controllers;

namespace StockMarket.Services.PeriodicTaskHosted
{

    public class UpdateSymbolsDataService : IHostedService, IDisposable
    {
        private readonly ILogger<UpdateSymbolsDataService> _logger;
        private readonly IServiceProvider _services;
        private Timer _timer_01;
        //private Timer _timer_02;
        //private TimeOnly _task_02_TargetTime; // Time of day to run the task-02



        public UpdateSymbolsDataService(IServiceProvider services, ILogger<UpdateSymbolsDataService> logger)
        {
            _services = services;
            _logger = logger;
            //_task_02_TargetTime = TimeOnly.Parse("21:00:00"); // Setting the desired time for task-02

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //--Task_01 Timer--
            _timer_01 = new Timer(DoWork_01, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            //--Task_02 Timer--
            //var nextRunTime = CalculateNextRunTime();
            //var delay = nextRunTime - DateTime.Now;
            //_timer_02 = new Timer(DoWork_02, null, delay, TimeSpan.FromDays(1));
            //_logger.LogInformation("Periodic Task-02 Hosted Service should run at: " + nextRunTime);
            //_timer_02 = new Timer(DoWork_02, null, TimeSpan.Zero, TimeSpan.FromDays(1));



            return Task.CompletedTask;
        }

        private void DoWork_01(object state)
        {
            var now = DateTime.Now;
            var startHour = 10;
            var endHour = 15;

            // Check if current time is within the range and the day is a workday
            if (now.Hour >= startHour && now.Hour < endHour && IsWorkday(now))
            {
                _logger.LogInformation("Periodic Task-01 Hosted Service has started: at " + now);

                using (var scope = _services.CreateScope())
                {
                    var targetController = scope.ServiceProvider.GetRequiredService<StockMarketController>();
                    targetController.UpdateSymbolsData().GetAwaiter().GetResult();
                }
            }
            else
            {
                _logger.LogInformation("Periodic Task-01 Hosted Service is idle.");
            }
        }

        private void DoWork_02(object state)
        {
            var now = DateTime.Now;

            if (IsWorkday(now))
            {
                _logger.LogInformation("Periodic Task-02 Hosted Service has started: at " + now);

                using (var scope = _services.CreateScope())
                {
                    var targetController = scope.ServiceProvider.GetRequiredService<StockMarketController>();
                    targetController.updateAllSymbolsTechnicalAnalysis().GetAwaiter().GetResult();
                }
            }
            else
            {
                _logger.LogInformation("Periodic Task-02 Hosted Service is idle.");
            }
        }
        private bool IsWorkday(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Friday && date.DayOfWeek != DayOfWeek.Saturday;
        }
        //private DateTime CalculateNextRunTime()
        //{
        //    var now = DateTime.Now;
        //    var targetDateTime = new DateTime(now.Year, now.Month, now.Day, _task_02_TargetTime.Hour, _task_02_TargetTime.Minute, _task_02_TargetTime.Second);

        //    if (targetDateTime < now)
        //    {
        //        // Target time has already passed today, so schedule for tomorrow
        //        targetDateTime = targetDateTime.AddDays(1);
        //    }

        //    return targetDateTime;
        //}
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Periodic Task Hosted Service is stopping.");

            _timer_01?.Change(Timeout.Infinite, 0);
            //_timer_02?.Change(Timeout.Infinite, 0);


            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer_01?.Dispose();
            //_timer_02?.Dispose();
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Periodic Task-01 Hosted Service has executed: at { DateTime.Now}");

            using var scope = _services.CreateScope();
            var myService = scope.ServiceProvider.GetRequiredService<StockMarketController>();
            await myService.UpdateSymbolsData();

        }

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentTime = DateTime.Now;
                var dayOfWeek = currentTime.DayOfWeek;

                //if (dayOfWeek != DayOfWeek.Friday && dayOfWeek != DayOfWeek.Saturday &&
                if (currentTime.Hour >= 10 && currentTime.Hour < 14)
                {
                    await DoWorkAsync(stoppingToken);
                }

                // Wait for an hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

}
