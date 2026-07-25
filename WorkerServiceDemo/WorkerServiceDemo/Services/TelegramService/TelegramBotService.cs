using Microsoft.Extensions.Options;
using Telegram.Bot;
using WorkerServiceDemo.Models;

namespace WorkerServiceDemo.Services.TelegramService
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly IOptions<TelegramSettings> _settings;
        private readonly TelegramBotClient _client;
        private readonly ILogger<TelegramBotService> _logger;

        public TelegramBotService(IOptions<TelegramSettings> settings, ILogger<TelegramBotService> logger)
        {
            _settings = settings;
            _logger = logger;
            _client = new TelegramBotClient(_settings.Value.Token)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<bool> Send(string title, IspInformation ispInformation)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.Value.Token))
                {
                    _logger.LogWarning("Telegram notification skipped: Token not configured.");
                    return false;
                }

                var message = $"<b>{title}</b>{Environment.NewLine}" +
                              $"IP: {ispInformation?.query ?? "Unknown"}{Environment.NewLine}" +
                              $"ISP: {ispInformation?.isp ?? "Unknown"}{Environment.NewLine}" +
                              $"Address: {ispInformation?.city ?? "Unknown"}-{ispInformation?.zip ?? "Unknown"},{ispInformation?.country ?? "Unknown"}{Environment.NewLine}" +
                              $"ISP Org: {ispInformation?.org ?? "Unknown"}{Environment.NewLine}" +
                              $"Geo Location(lat,lon):({ispInformation?.lat ?? 0},{ispInformation?.lon ?? 0}){Environment.NewLine}";

                _logger.LogInformation("Sending Telegram notification to channel {ChannelId}...", _settings.Value.ChannelId);
                var response = await _client.SendTextMessageAsync(_settings.Value.ChannelId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                if (response != null)
                {
                    _logger.LogInformation("Telegram notification sent successfully.");
                    return true;
                }

                _logger.LogWarning("Telegram notification returned no response.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Telegram notification.");
                return false;
            }
        }
    }
}
