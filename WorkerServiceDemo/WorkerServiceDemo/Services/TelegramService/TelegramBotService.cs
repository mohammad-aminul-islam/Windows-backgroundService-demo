using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using WorkerServiceDemo.Models;

namespace WorkerServiceDemo.Services.TelegramService
{
    internal class TelegramBotService : ITelegramBotService
    {

        private readonly TelegramBotClient _client = new TelegramBotClient(Constants.TelegramToken)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        public TelegramBotService()
        {
        }
        public async Task<bool> Send(string title, IspInformation ispInformation)
        {
            var message = $"<b>{title}</b>{Environment.NewLine}" +
                  $"IP: {ispInformation.query}{Environment.NewLine}" +
                  $"ISP: {ispInformation.isp}{Environment.NewLine}" +
                  $"Address: {ispInformation.city}-{ispInformation.zip},{ispInformation.country}{Environment.NewLine}" +
                  $"ISP Org: {ispInformation.org}{Environment.NewLine}" +
                  $"Geo Location(lat,lon):({ispInformation.lat},{ispInformation.lon}){Environment.NewLine}";

            var response = await _client.SendTextMessageAsync(Constants.ChannelId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            if (response != null)
            {
                return true;
            }
            return false;
        }
    }
}
