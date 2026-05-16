using WorkerServiceDemo;
using WorkerServiceDemo.Models;
using WorkerServiceDemo.Services;
using WorkerServiceDemo.Services.TelegramService;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddWindowsService(opt =>
{
    opt.ServiceName = "Database Backup Service";
});

// Configuration Sections
services.Configure<BackupSettings>(builder.Configuration.GetSection("BackupSettings"));
services.Configure<SqlServerSettings>(builder.Configuration.GetSection("SqlServer"));
services.Configure<PostgresSettings>(builder.Configuration.GetSection("Postgres"));
services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));

// HTTP Clients
services.AddHttpClient("Location", config =>
{
    config.BaseAddress = new Uri("http://ip-api.com");
});

services.AddHttpClient("Ip", config =>
{
    config.BaseAddress = new Uri("https://api.ipify.org");
});

// Services
services.AddHostedService<Worker>();
services.AddSingleton<IDatabaseBackupService, SqlServerBackupService>();
services.AddSingleton<IDatabaseBackupService, PostgresBackupService>();
services.AddSingleton<ITelegramBotService, TelegramBotService>();

IHost host = builder.Build();
host.Run();
