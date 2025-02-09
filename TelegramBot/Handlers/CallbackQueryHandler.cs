using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services;
using System.Diagnostics;

namespace TelegramBot.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly GenreHandler _genreHandler;
        private readonly GoogleDriveService _googleDriveService;

        public CallbackQueryHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
            _genreHandler = new GenreHandler(botClient);
            _googleDriveService = new GoogleDriveService();
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

                case "portfolio":
                    {
                        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                        await _genreHandler.SendGenresButtons(chat);
                        return;
                    }

            }

            if (callbackQuery.Data.StartsWith("genre_"))
            {
                string genre = callbackQuery.Data.Replace("genre_", "");
                await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                var stopwatch = Stopwatch.StartNew();
                var files = await _googleDriveService.GetMp3FilesAsync(genre);
                stopwatch.Stop();
                Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
                foreach (var file in files)
                {
                    await _botClient.SendAudioAsync(
                                    chat.Id,
                                    new InputFileStream(file.Stream, file.Name));

                }
               

            }


            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "You clicked a button!");
        }       

    }
}
