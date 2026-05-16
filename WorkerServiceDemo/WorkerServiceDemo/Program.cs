using WorkerServiceDemo;
using WorkerServiceDemo.Services;
using Telegram.Bot;
using WorkerServiceDemo.Services.TelegramService;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;
services.AddWindowsService(opt =>
{
    opt.ServiceName = "Sql Database Backup";
});

services.AddHttpClient("Location", config =>
{
    config.BaseAddress = new Uri("http://ip-api.com");
});

services.AddHttpClient("Ip", config =>
{
    config.BaseAddress = new Uri("https://api.ipify.org");
});


services.AddSingleton<ITelegramBotService, TelegramBotService>();
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
Constants.TelegramToken = configuration.GetValue<string>("Telegram:Token");
Constants.ChannelId = configuration.GetValue<long>("Telegram:ChannelId");
#endregion

IHost host = builder.Build();
host.Run();
