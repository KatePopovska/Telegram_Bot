using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.Bot;
using TelegramBot.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: true);

string botToken = builder.Configuration["BotSettings:TelegramToken"] ?? throw new ArgumentNullException("Bot token not found");


var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<ITelegramBotClient>(sp => new TelegramBotClient(botToken));
services.AddSingleton<BotService>(sp => new BotService(botToken));
services.AddSingleton<MessageHandler>();
services.AddSingleton<CallbackQueryHandler>();
var serviceProvider = services.BuildServiceProvider();

var botService = serviceProvider.GetRequiredService<BotService>();
var botClient = botService.GetBot();

var messageHandler = serviceProvider.GetRequiredService<MessageHandler>();
var callbackHandler = serviceProvider.GetRequiredService<CallbackQueryHandler>();

using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
};

botClient.StartReceiving(async (bot, update, token) =>
{
    if (update.Message != null)
    {
        await messageHandler.HandleMessageAsync(update.Message);
    }
    else if (update.CallbackQuery != null)
    {
        await callbackHandler.HandleCallbackQueryAsync(update.CallbackQuery);
    }
},
  async (bot, exception, token) =>
  {
      Console.WriteLine($"Error: {exception.Message}");
  },
    receiverOptions,
    cancellationToken: cts.Token
);

Console.WriteLine("Bot is running...");
await Task.Delay(-1);
