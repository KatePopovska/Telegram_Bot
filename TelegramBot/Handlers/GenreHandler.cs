using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Handlers
{
    public class GenreHandler
    {
        private readonly ITelegramBotClient _botClient;

        public GenreHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }
        public async Task SendGenresButtons(Chat chat)
        {

            Dictionary<string, string> genreFolders = new()
            {
                { "Pop", "genre_Pop" },
                { "Rock", "genre_Rock" },
                { "Hip-Hop/Rap", "genre_HipHop"},
                {"Electronic (EDM)", "genre_EDM" },
                { "Liric", "genre_Liric" },
                { "Reggae", "genre_Reggae"}
            };

            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var genre in genreFolders)
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(genre.Key, genre.Value.ToLower()) });
            }



            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

            await _botClient.SendTextMessageAsync(
                chat.Id,
                "Выбери жанр",
                replyMarkup: inlineKeyboard);
        }

    }
}
