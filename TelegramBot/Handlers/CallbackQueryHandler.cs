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
using Telegram.Bot.Types.Enums;
using System.Resources;
using TelegramBot;

namespace TelegramBot.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly GenreHandler _genreHandler;
        private readonly GoogleDriveService _googleDriveService;
        private Dictionary<string, string> _telegramFileCache = new Dictionary<string, string>();

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


                case "conditions":
                    {
                        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                        await _botClient.SendTextMessageAsync(
                        chat.Id,
                            @"💰 *Предоплата*

Работа начинается только после внесения предоплаты *50\%* от общей стоимости заказа\.  
Оставшиеся *50\%* оплачиваются после утверждения финального варианта перед отправкой всех файлов\.

🎼 *Что входит в работу*  
✅ Создание бита с учетом ваших пожеланий \(стиль, референсы, темп и т\. д\.\)\.  
✅ Аранжировка и проработка структуры трека\.  
✅ Сведение и мастеринг для качественного звучания\.

🛠️ *Правки*  
Включены *3 бесплатные правки* \(небольшие изменения структуры, звуков, инструментов\)\.  
Дополнительные правки после 3\-х бесплатных — оплачиваются отдельно \(стоимость обсуждается индивидуально\)\.

⏳ *Сроки выполнения*  
\- Создание бита: *от 2 до 7 дней* \(зависит от сложности\)\.  
\- Сведение и мастеринг: *до 3 дней*\.

🎵 *Форматы файлов*  
\- Финальный бит предоставляется в *WAV и MP3*\.  
\- По запросу можно получить *стемы \(отдельные дорожки\)* для дальнейшей обработки\.

📩 *Готов обсудить детали и приступить к работе\!* ",
                            parseMode: ParseMode.MarkdownV2 
                        );
                        return;
                    }





            }

            if (callbackQuery.Data.StartsWith("genre_"))
            {
                string genre = callbackQuery.Data.Replace("genre_", "");
                await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                var loadingMessage = await _botClient.SendTextMessageAsync(chat.Id, "🔄 Загрузка аудиофайлов...");

                var files = await _googleDriveService.GetMp3FilesAsync(genre);

                foreach (var file in files.Files)
                {
                    if (_telegramFileCache.ContainsKey(file.Name))
                    {

                        string fileId = _telegramFileCache[file.Name];
                        await _botClient.SendAudioAsync(chat.Id, audio:fileId);
                    }
                    else
                    {
                        var stream = await _googleDriveService.DownloadFilesAsync(file);
                        var message = await _botClient.SendAudioAsync(chat.Id, new InputFileStream(stream.Stream, stream.Name));
                        _telegramFileCache[file.Name] = message.Audio.FileId;
                    }
                }

                await _botClient.DeleteMessageAsync(chat.Id, loadingMessage.MessageId);
            }

        }       

    }
}
