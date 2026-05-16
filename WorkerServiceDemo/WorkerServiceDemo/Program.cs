using WorkerServiceDemo;
using WorkerServiceDemo.Models;
using WorkerServiceDemo.Services;

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

// Services
services.AddHostedService<Worker>();
services.AddSingleton<IDatabaseBackupService, SqlServerBackupService>();
services.AddSingleton<IDatabaseBackupService, PostgresBackupService>();

IHost host = builder.Build();
host.Run();
