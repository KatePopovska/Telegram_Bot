using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramBot.Services;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using System.Collections.Concurrent;

namespace TelegramBot.Handlers
{
    public class CallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly GenreHandler _genreHandler;
        private readonly GoogleDriveService _googleDriveService;
        private Dictionary<string, string> _telegramFileCache = new Dictionary<string, string>();
        private readonly ILogger<CallbackQueryHandler> _logger;
        private static readonly ConcurrentDictionary<long, DateTime> _userLastInteraction = new();
        private readonly int _cooldownSeconds = 2;

        public CallbackQueryHandler(ITelegramBotClient botClient, ILogger<CallbackQueryHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
            _genreHandler = new GenreHandler(botClient);
            _googleDriveService = new GoogleDriveService();
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            try
            {
                var user = callbackQuery.From;

                if (!IsUserAllowed(user.Id))
                {
                    _logger.LogWarning("Too many requests from user {UserId}", user.Id);
                    Console.WriteLine("Too many requests");
                    await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "⏳ Подождите перед следующим запросом!", showAlert: true);
                    return;
                }

                var chat = callbackQuery.Message.Chat;
                if (chat == null)
                {
                    _logger.LogWarning("Chat is null in callback query.");
                    return;
                }
                await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                switch (callbackQuery.Data)
                {
                    case "price":
                        {
                            await _botClient.SendTextMessageAsync(
                                        chat.Id,
                                        $"Запись вокала/инструментов в Батуми - 18 $ в час\r\nСведение - 45 $\r\nМастеринг - 25 $\r\nАранжировка с нуля - от 70 $");
                            break;
                        }

                    case "portfolio":
                        {
                            await _genreHandler.SendGenresButtons(chat);
                            break;
                        }


                    case "conditions":
                        {                        
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

📩 *Готов обсудить детали и приступить к работе\!*@MusxPass ",
                                parseMode: ParseMode.MarkdownV2
                            );
                            break;
                        }

                }

                if (callbackQuery.Data.StartsWith("genre_"))
                {
                    string genre = callbackQuery.Data.Replace("genre_", "");

                    var loadingMessage = await _botClient.SendTextMessageAsync(chat.Id, "🔄 Загрузка аудиофайлов...");
                    
                    var files = await _googleDriveService.GetMp3FilesAsync(genre);

                    foreach (var file in files.Files)
                    {
                        if (_telegramFileCache.ContainsKey(file.Name))
                        {

                            string fileId = _telegramFileCache[file.Name];
                            await _botClient.SendAudioAsync(chat.Id, audio: fileId);
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
            catch (ApiRequestException ex)
            {
                _logger.LogError(ex, "Telegram API error while handling callback query.");
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Произошла ошибка при обработке запроса. Попробуйте позже.");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "I/O error occurred.");
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Ошибка при загрузке данных.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred.");
                await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Что-то пошло не так. Попробуйте позже.");
            }


        }

        private bool IsUserAllowed(long userId)
        {
            if (_userLastInteraction.TryGetValue(userId, out DateTime lastTime))
            {
                if ((DateTime.UtcNow - lastTime).TotalSeconds < _cooldownSeconds)
                {
                    return false;
                }
            }

            _userLastInteraction[userId] = DateTime.UtcNow;
            return true;
        }

    }
}
