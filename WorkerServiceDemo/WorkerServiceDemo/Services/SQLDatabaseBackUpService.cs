using System.Data.SqlClient;

namespace WorkerServiceDemo.Services
{
    public class SqlDatabaseBackUpService : ISqlDatabaseBackUpService
    {

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