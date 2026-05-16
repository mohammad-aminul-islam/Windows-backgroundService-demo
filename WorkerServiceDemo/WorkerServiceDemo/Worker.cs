using Newtonsoft.Json;
using System.Net;
using WorkerServiceDemo.Models;
using WorkerServiceDemo.Services;
using WorkerServiceDemo.Services.TelegramService;
namespace WorkerServiceDemo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISqlDatabaseBackUpService _sqlDatabaseBackUpService;
        private readonly ITelegramBotService _telegramBotService;
        private readonly IHttpClientFactory _httpClientFactory;
        public Worker(ILogger<Worker> logger, ISqlDatabaseBackUpService sqlDatabaseBackUpService,
            ITelegramBotService telegramBotService, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            this._sqlDatabaseBackUpService = sqlDatabaseBackUpService;
            _telegramBotService = telegramBotService;
            _httpClientFactory = httpClientFactory;
        }
        //public override Task StartAsync(CancellationToken cancellationToken)
        //{
        //    return base.StartAsync(cancellationToken);
        //}
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = _httpClientFactory.CreateClient("Location");
            var ipHttpClient = _httpClientFactory.CreateClient("Ip");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _sqlDatabaseBackUpService.BackUpDataBase();
                    var ipResponse = await ipHttpClient.GetAsync($"?format=json");
                    IpModel ip = new IpModel();
                    IspInformation ispInformation = null;
                    if (ipResponse.IsSuccessStatusCode)
                    {
                        var result = await ipResponse.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(result))
                            ip = JsonConvert.DeserializeObject<IpModel>(result);
                    }
                    var response = await httpClient.GetAsync($"/json/{ip.IP}");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        ispInformation = JsonConvert.DeserializeObject<IspInformation>(result);
                    }
                    _logger.LogInformation("Database backup started at: {time}", DateTime.Now);


                    await _telegramBotService.Send($"Backupd databases: {string.Join(',', Constants.BackUpDatabaseNames)},Back up time:{DateTime.Now}", ispInformation);

                    await Task.Delay(Constants.BackUpIntervalMinitues, stoppingToken);
                }
                catch (Exception ex)
                {
                    $"In worker: {ex.Message}".Log();
                    throw;
                }

            }
        }
    }
}