using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramBot.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;

        public CallbackQueryHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            var user = callbackQuery.From;

            var chat = callbackQuery.Message.Chat;

            switch (callbackQuery.Data)
            {
                case "price":
                    {
                        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                        await _botClient.SendTextMessageAsync(
                                    chat.Id,
                                    $"Запись вокала/инструментов в Батуми - 18 $ в час\r\nСведение - 45 $\r\nМастеринг - 25 $\r\nАранжировка с нуля - от 70 $");
                        return;
                    }
                    
            }


            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "You clicked a button!");
        }
    }
}
