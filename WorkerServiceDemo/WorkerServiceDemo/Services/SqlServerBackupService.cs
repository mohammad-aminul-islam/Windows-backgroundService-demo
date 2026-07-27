using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using WorkerServiceDemo.Models;

namespace WorkerServiceDemo.Services
{
    public class SqlServerBackupService : IDatabaseBackupService
    {
        private readonly SqlServerSettings _settings;
        private readonly ILogger<SqlServerBackupService> _logger;

        public SqlServerBackupService(IOptions<SqlServerSettings> settings, ILogger<SqlServerBackupService> logger)
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
                    _logger.LogWarning("SQL Server backup skipped: Server not configured.");
                    return false;
                }

                using var connection = new SqlConnection($"Server={_settings.Server};Database=master;User Id={_settings.UserName};Password={_settings.Password};TrustServerCertificate=True");
                await connection.OpenAsync();

                if (!Directory.Exists(_settings.BackupDirectory))
                {
                    Directory.CreateDirectory(_settings.BackupDirectory);
                }

                foreach (var item in _settings.DatabaseNames)
                {
                    var fileName = Path.Combine(_settings.BackupDirectory, $"{DateTime.Today:dd_MMM_yyyy}_{item}.bak");
                    var query = $"Backup database [{item}] to disk='{fileName}'";
                    
                    using var command = new SqlCommand(query, connection);
                    await command.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("SQL Server backup completed for {Database} to {File}", item, fileName);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SQL Server backup.");
                return false;
            }
        }
    }
}
