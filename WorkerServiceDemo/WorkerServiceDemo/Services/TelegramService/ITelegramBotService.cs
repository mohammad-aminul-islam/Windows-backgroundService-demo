using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerServiceDemo.Models;

namespace WorkerServiceDemo.Services.TelegramService
{
    public interface ITelegramBotService
    {
        Task<bool> Send(string title,IspInformation ispInformation);
    }
}
