using WorkerServiceDemo;
using WorkerServiceDemo.Services;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;
services.AddWindowsService(opt =>
{
    opt.ServiceName = "Sql Database Backup";
});

services.AddSingleton<ISqlDatabaseBackUpService, SqlDatabaseBackUpService>();
services.AddHostedService<Worker>();

#region config data load
var configuration = builder.Configuration;
var dbs = configuration.GetValue<string>("BackUpDbNames");
Constants.BackUpDatabaseNames = dbs.Split(',').ToList();
Constants.ServerName = configuration.GetValue<string>("Server");
Constants.UserName = configuration.GetValue<string>("DBUserName");
Constants.Password = configuration.GetValue<string>("Password");
Constants.BackUpDirectory = configuration.GetValue<string>("BackUpDirectory");
Constants.BackUpIntervalMinitues = configuration.GetValue<int>("BackUpIntervalMinitues") * 60 * 1000;
#endregion

IHost host = builder.Build();
host.Run();
