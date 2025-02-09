using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Handlers
{
    public class MessageHandler
    {
        private readonly ITelegramBotClient _botClient;

        public MessageHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleMessageAsync(Message message)
        {
            try
            {
                if (message.Text is null) return;

                if (message.Text.StartsWith("/"))
                {
                    await HandleCommandAsync(message);
                }
                else
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "I received your message!");
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        private async Task HandleCommandAsync(Message message)
        {
            var chat = message.Chat;

            switch (message.Text.ToLower())
            {
                case "/start":
                    await SendInlineButtons(chat);
                    break;
                case "/help":
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Here are the available commands...");
                    break;
                default:
                    
                   // await _genreHandler.HandleGenreSelectionAsync(callbackQuery);
                    break;
            }
        }

         private async Task SendInlineButtons(Chat chat)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>() 
                                    {
                                        

                                        new InlineKeyboardButton[] 
                                        {
                                            InlineKeyboardButton.WithCallbackData("Мои работы", "portfolio"),
                                            InlineKeyboardButton.WithCallbackData("Прайс", "price"),
                                            InlineKeyboardButton.WithCallbackData("Условия сотрудничества", "conditions"),
                                        },
 
                                    });

            await _botClient.SendTextMessageAsync(
                chat.Id,
                "Здесь ты можешь ознакомиться с моим портфолио и прайсом)",
                replyMarkup: inlineKeyboard); 

            return;
        }
    }
}
