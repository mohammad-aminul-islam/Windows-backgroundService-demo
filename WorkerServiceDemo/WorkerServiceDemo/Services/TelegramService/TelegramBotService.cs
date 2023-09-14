using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

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
        public async Task<bool> Send(string message)
        {
            var response = await _client.SendTextMessageAsync(Constants.ChannelId, message);
            if (response != null)
            {
                return true;
            }
            return false;
        }
    }
}
