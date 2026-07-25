using Hangfire;
using Serilog;
using Serilog.Events;
using WorkerServiceDemo;
using WorkerServiceDemo.Jobs;
using WorkerServiceDemo.Models;
using WorkerServiceDemo.Services;
using WorkerServiceDemo.Services.TelegramService;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

// Only the log file path comes from appsettings.json — everything else about
// the logging setup below is fixed.
var logFilePath = builder.Configuration["Serilog:FilePath"] ?? "Logs/log-.txt";
const string outputTemplate = "{Timestamp:dd-MMM-yyyy hh:mm tt} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, outputTemplate: outputTemplate)
    .CreateLogger();

builder.Logging.ClearProviders();
services.AddSerilog();

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

// Hangfire
services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

services.AddHangfireServer();

// Services
services.AddSingleton<IDatabaseBackupService, SqlServerBackupService>();
services.AddSingleton<IDatabaseBackupService, PostgresBackupService>();
services.AddSingleton<ITelegramBotService, TelegramBotService>();
services.AddScoped<DatabaseBackupJob>();
services.AddHostedService<DatabaseBackupWorker>();

IHost host = builder.Build();

try
{
    host.Run();
}
finally
{
    Log.CloseAndFlush();
}
