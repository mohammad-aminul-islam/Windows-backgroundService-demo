using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WorkerServiceDemo.Models;
using WorkerServiceDemo.Services;
using WorkerServiceDemo.Services.TelegramService;

namespace WorkerServiceDemo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEnumerable<IDatabaseBackupService> _backupServices;
        private readonly BackupSettings _settings;
        private readonly ITelegramBotService _telegramBotService;
        private readonly IHttpClientFactory _httpClientFactory;

        public Worker(
            ILogger<Worker> logger, 
            IEnumerable<IDatabaseBackupService> backupServices,
            IOptions<BackupSettings> settings,
            ITelegramBotService telegramBotService,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _backupServices = backupServices;
            _settings = settings.Value;
            _telegramBotService = telegramBotService;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = _httpClientFactory.CreateClient("Location");
            var ipHttpClient = _httpClientFactory.CreateClient("Ip");
            
            _logger.LogInformation("Database Backup Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Backup cycle started at: {time}", DateTimeOffset.Now);

                IspInformation? ispInformation = await GetIspInformation(httpClient, ipHttpClient);

                var backedUpDatabases = new List<string>();

                foreach (var service in _backupServices)
                {
                    try
                    {
                        await service.BackUpDataBaseAsync();
                        // Note: For simplicity, I'm assuming we want to report success for each service type.
                        // In a real scenario, you might want more detailed reporting from each service.
                        backedUpDatabases.Add(service.GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unhandled error occurred in backup service {ServiceType}", service.GetType().Name);
                    }
                }

                if (ispInformation != null)
                {
                    var message = $"Backup cycle completed for: {string.Join(", ", backedUpDatabases)}. Time: {DateTime.Now}";
                    await _telegramBotService.Send(message, ispInformation);
                }

                _logger.LogInformation("Backup cycle completed. Waiting for {Interval} minutes.", _settings.IntervalMinutes);
                
                await Task.Delay(TimeSpan.FromMinutes(_settings.IntervalMinutes), stoppingToken);
            }
        }

        private async Task<IspInformation?> GetIspInformation(HttpClient httpClient, HttpClient ipHttpClient)
        {
            try
            {
                var ipResponse = await ipHttpClient.GetAsync("?format=json");
                if (!ipResponse.IsSuccessStatusCode) return null;

                var ipResult = await ipResponse.Content.ReadAsStringAsync();
                var ipModel = JsonConvert.DeserializeObject<IpModel>(ipResult);
                if (ipModel == null || string.IsNullOrEmpty(ipModel.IP)) return null;

                var locationResponse = await httpClient.GetAsync($"/json/{ipModel.IP}");
                if (!locationResponse.IsSuccessStatusCode) return null;

                var locationResult = await locationResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IspInformation>(locationResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve ISP information.");
                return null;
            }
        }
    }
}