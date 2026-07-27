using Hangfire;
using Microsoft.Extensions.Options;
using WorkerServiceDemo.Jobs;
using WorkerServiceDemo.Models;

namespace WorkerServiceDemo;

// Registers the recurring backup job with Hangfire once at startup.
// The actual backup work runs on Hangfire's own schedule/thread pool via DatabaseBackupJob.
public class DatabaseBackupWorker : IHostedService
{
    private const string JobId = "database-backup";

    private readonly BackupSettings _settings;
    private readonly IRecurringJobManager _recurringJobManager;

    public DatabaseBackupWorker(IOptions<BackupSettings> settings, IRecurringJobManager recurringJobManager)
    {
        _settings = settings.Value;
        _recurringJobManager = recurringJobManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.CronExpression))
        {
            throw new InvalidOperationException("Missing BackupSettings:CronExpression configuration.");
        }

        _recurringJobManager.AddOrUpdate<DatabaseBackupJob>(
            JobId,
            job => job.RunAsync(),
            _settings.CronExpression);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
