using Microsoft.Extensions.Options;
using WorkerServiceDemo.Models;
using WorkerServiceDemo.Services;

namespace WorkerServiceDemo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEnumerable<IDatabaseBackupService> _backupServices;
        private readonly BackupSettings _settings;

        public Worker(
            ILogger<Worker> logger, 
            IEnumerable<IDatabaseBackupService> backupServices,
            IOptions<BackupSettings> settings)
        {
            _logger = logger;
            _backupServices = backupServices;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database Backup Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Backup cycle started at: {time}", DateTimeOffset.Now);

                foreach (var service in _backupServices)
                {
                    try
                    {
                        await service.BackUpDataBaseAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unhandled error occurred in backup service {ServiceType}", service.GetType().Name);
                    }
                }

                _logger.LogInformation("Backup cycle completed. Waiting for {Interval} minutes.", _settings.IntervalMinutes);
                
                await Task.Delay(TimeSpan.FromMinutes(_settings.IntervalMinutes), stoppingToken);
            }
        }
    }
}