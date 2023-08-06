using WorkerServiceDemo.Services;

namespace WorkerServiceDemo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISqlDatabaseBackUpService _sqlDatabaseBackUpService;

        public Worker(ILogger<Worker> logger,ISqlDatabaseBackUpService sqlDatabaseBackUpService)
        {
            _logger = logger;
            this._sqlDatabaseBackUpService = sqlDatabaseBackUpService;
        }
        //public override Task StartAsync(CancellationToken cancellationToken)
        //{
        //    return base.StartAsync(cancellationToken);
        //}
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _sqlDatabaseBackUpService.BackUpDataBase();
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(Constants.BackUpIntervalMinitues, stoppingToken);
                }
                catch (Exception ex)
                {
                    $"In worker: {ex.InnerException}".Log();
                    throw;
                }
                
            }
        }
    }
}