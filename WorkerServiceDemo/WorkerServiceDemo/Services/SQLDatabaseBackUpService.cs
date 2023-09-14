using System.Data.SqlClient;
using WorkerServiceDemo.Services.TelegramService;

namespace WorkerServiceDemo.Services
{
    public class SqlDatabaseBackUpService : ISqlDatabaseBackUpService
    {
        private readonly ITelegramBotService _telegramBotService;

        public SqlDatabaseBackUpService(ITelegramBotService telegramBotService)
        {
            this._telegramBotService = telegramBotService;
        }
        public bool BackUpDataBase()
        {
            try
            {
                using (var connection = new SqlConnection($"Server={Constants.ServerName};Database=master;User Id={Constants.UserName};Password={Constants.Password}"))
                {
                    connection.Open();
                    foreach (var item in Constants.BackUpDatabaseNames)
                    {
                        var command = new SqlCommand($"Backup database {item} to disk='{Constants.BackUpDirectory}{Path.DirectorySeparatorChar}{DateTime.Today.ToString("dd_MMM_yyyy")}_{item}.bak'", connection);
                        command.ExecuteNonQuery();
                    }
                    $"Backup databases:  {string.Join(',', Constants.BackUpDatabaseNames)}".Log();
                    _telegramBotService.Send($"Backupd databases: {string.Join(',', Constants.BackUpDatabaseNames)},Back up time:{DateTime.Now}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.Message.Log();
                return false;
            }

        }
    }
}