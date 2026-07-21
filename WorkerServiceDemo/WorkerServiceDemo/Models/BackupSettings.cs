namespace WorkerServiceDemo.Models
{
    public class BackupSettings
    {
        // Standard 5-field cron expression (e.g. "0 2 * * *" for daily at 2 AM).
        public string CronExpression { get; set; } = string.Empty;
    }

    public abstract class DatabaseSettings
    {
        public string Server { get; set; } = string.Empty;
        public List<string> DatabaseNames { get; set; } = new();
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BackupDirectory { get; set; } = string.Empty;
    }

    public class SqlServerSettings : DatabaseSettings { }

    public class PostgresSettings : DatabaseSettings
    {
        public string PgDumpPath { get; set; } = "pg_dump";

        // When set, pg_dump runs inside this Docker container via `docker exec`
        // instead of using a locally installed pg_dump.
        public string ContainerName { get; set; } = string.Empty;
    }

    public class TelegramSettings
    {
        public string Token { get; set; } = string.Empty;
        public long ChannelId { get; set; }
    }
}
