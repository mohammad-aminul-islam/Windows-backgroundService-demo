using Newtonsoft.Json;
using WorkerServiceDemo.Models;
using WorkerServiceDemo.Services;
using WorkerServiceDemo.Services.TelegramService;

namespace WorkerServiceDemo.Jobs;

public class DatabaseBackupJob
{
    private readonly ILogger<DatabaseBackupJob> _logger;
    private readonly IEnumerable<IDatabaseBackupService> _backupServices;
    private readonly ITelegramBotService _telegramBotService;
    private readonly IHttpClientFactory _httpClientFactory;

    public DatabaseBackupJob(
        ILogger<DatabaseBackupJob> logger,
        IEnumerable<IDatabaseBackupService> backupServices,
        ITelegramBotService telegramBotService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _backupServices = backupServices;
        _telegramBotService = telegramBotService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task RunAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("Location");
        var ipHttpClient = _httpClientFactory.CreateClient("Ip");

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

        _logger.LogInformation("Backup cycle completed.");
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
