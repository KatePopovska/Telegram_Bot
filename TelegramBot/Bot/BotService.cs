using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramBot.Bot
{
    public class BotService
    {
        private readonly TelegramBotClient _botClient;

        public BotService(string token)
        {

            var cts = new CancellationTokenSource();

            _botClient = new TelegramBotClient(token, cancellationToken: cts.Token);
        }

        public TelegramBotClient GetBot() => _botClient;

    }
}
