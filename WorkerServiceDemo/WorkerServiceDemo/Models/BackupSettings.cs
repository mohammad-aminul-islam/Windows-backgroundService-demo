namespace WorkerServiceDemo.Models
{
    public class BackupSettings
    {
        public int IntervalMinutes { get; set; }
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
    }

    public class TelegramSettings
    {
        public string Token { get; set; } = string.Empty;
        public long ChannelId { get; set; }
    }
}
