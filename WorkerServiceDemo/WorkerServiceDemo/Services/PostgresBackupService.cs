using Microsoft.Extensions.Options;
using System.Diagnostics;
using WorkerServiceDemo.Models;

namespace WorkerServiceDemo.Services
{
    public class PostgresBackupService : IDatabaseBackupService
    {
        private readonly PostgresSettings _settings;
        private readonly ILogger<PostgresBackupService> _logger;

        public PostgresBackupService(IOptions<PostgresSettings> settings, ILogger<PostgresBackupService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> BackUpDataBaseAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.Server))
                {
                    _logger.LogWarning("PostgreSQL backup skipped: Server not configured.");
                    return false;
                }

                if (!Directory.Exists(_settings.BackupDirectory))
                {
                    Directory.CreateDirectory(_settings.BackupDirectory);
                }

                foreach (var item in _settings.DatabaseNames)
                {
                    var fileName = Path.Combine(_settings.BackupDirectory, $"{DateTime.Today:dd_MMM_yyyy}_{item}.sql");
                    
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = _settings.PgDumpPath,
                        Arguments = $"-h {_settings.Server} -U {_settings.UserName} -f \"{fileName}\" {item}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    // Set password via environment variable
                    startInfo.EnvironmentVariables["PGPASSWORD"] = _settings.Password;

                    using var process = new Process { StartInfo = startInfo };
                    
                    _logger.LogInformation("Starting PostgreSQL backup for {Database}...", item);
                    process.Start();
                    
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("PostgreSQL backup completed for {Database} to {File}", item, fileName);
                        $"{DateTime.Now} : PostgreSQL Backup completed for {item}".Log(_settings.BackupDirectory);
                    }
                    else
                    {
                        _logger.LogError("PostgreSQL backup failed for {Database}: {Error}", item, error);
                        $"PostgreSQL Backup failed for {item}: {error}".Log(_settings.BackupDirectory);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PostgreSQL backup.");
                ex.Message.Log(_settings.BackupDirectory);
                return false;
            }
        }
    }
}
