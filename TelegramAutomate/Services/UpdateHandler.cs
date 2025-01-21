using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAutomate.Abstract;

namespace TelegramAutomate.Services;

public class UpdateHandler : IUpdateHandler
{
    private static readonly InputPollOption[] PollOptions = new InputPollOption[2] { "Hello", "World!" };


    private ITelegramBotClient _bot { get; set; }
    private ILogger<UpdateHandler> _logger { get; set; }
    private BotConfiguration _conf { get; set; }
    private IEnumerable<IUpdateCommand<Message>> _commands { get; init; }

    public UpdateHandler(ITelegramBotClient bot, ILogger<UpdateHandler> logger, IOptions<BotConfiguration> conf, IEnumerable<IUpdateCommand<Message>> commands)
    {
        _bot = bot;
        _logger = logger;
        _conf = conf.Value;
        _commands = commands;
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message } => OnMessage(message),
            { EditedMessage: { } message } => OnMessage(message),
            { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery),
            { InlineQuery: { } inlineQuery } => OnInlineQuery(inlineQuery),
            { ChosenInlineResult: { } chosenInlineResult } => OnChosenInlineResult(chosenInlineResult),
            { Poll: { } poll } => OnPoll(poll),
            { PollAnswer: { } pollAnswer } => OnPollAnswer(pollAnswer),
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            _ => UnknownUpdateHandlerAsync(update)
        });
    }

    private async Task OnMessage(Message msg)
    {
        _logger.LogInformation("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
        {
            return;
        }

        var command = _commands.Where(c => c.CanHandle(msg).Result).FirstOrDefault();
        if (command is null)
        {
            var usageRes = Usage(msg);
            _logger.LogInformation("Sent usage: {SentMessageId}", usageRes.Id);
            return;
        }

        var result = await command.ExecuteCommandAsync(msg);
        //Message sentMessage = await (messageText.Split(' ')[0] switch
        //{
        //    //"/photo" => SendPhoto(msg),
        //    //"/inline_buttons" => SendInlineKeyboard(msg),
        //    //"/keyboard" => SendReplyKeyboard(msg),
        //    //"/remove" => RemoveKeyboard(msg),
        //    //"/request" => RequestContactAndLocation(msg),
        //    //"/inline_mode" => StartInlineQuery(msg),
        //    //"/poll" => SendPoll(msg),
        //    "/poll_anonymous" => SendAnonymousPoll(msg),
        //    "/throw" => FailingHandler(msg),
        //    _ => Usage(msg)
        //});
        _logger.LogInformation("The message was sent with id: {SentMessageId}", result.Id);
    }

    async Task<Message> Usage(Message msg)
    {
        string commandsDescriptions = string.Join("\n", _commands.Select(c => $"/{c.CommandName()}\t\t\t\t- {c.CommandDescription()}").ToList());
        string usage = $"""
                <b><u>Bot menu</u></b>:{commandsDescriptions}\n /poll_anonymous - send an anonymous poll\n /throw          - what happens if handler fails
            """;

        return await _bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> SendAnonymousPoll(Message msg)
    {
        return await _bot.SendPoll(chatId: msg.Chat, "Question", PollOptions);
    }

    static Task<Message> FailingHandler(Message msg)
    {
        throw new NotImplementedException("FailingHandler");
    }

    // Process Inline Keyboard callback data
    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        await _bot.AnswerCallbackQuery(callbackQuery.Id, $"Received {callbackQuery.Data}");
        await _bot.SendMessage(callbackQuery.Message!.Chat, $"Received {callbackQuery.Data}");
    }

    #region Inline Mode

    private async Task OnInlineQuery(InlineQuery inlineQuery)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results =
            new InlineQueryResult[2]
            {
                new InlineQueryResultArticle("1", "Telegram.Bot", new InputTextMessageContent("hello")),
                new InlineQueryResultArticle("2", "is the best", new InputTextMessageContent("world"))
            };
        await _bot.AnswerInlineQuery(inlineQuery.Id, results, cacheTime: 0, isPersonal: true);
    }

    private async Task OnChosenInlineResult(ChosenInlineResult chosenInlineResult)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);
        await _bot.SendMessage(chosenInlineResult.From.Id, $"You chose result with Id: {chosenInlineResult.ResultId}");
    }

    #endregion

    private Task OnPoll(Poll poll)
    {
        _logger.LogInformation("Received Poll info: {Question}", poll.Question);
        return Task.CompletedTask;
    }

    private async Task OnPollAnswer(PollAnswer pollAnswer)
    {
        var answer = pollAnswer.OptionIds.FirstOrDefault();
        var selectedOption = PollOptions[answer];
        if (pollAnswer.User != null)
            await _bot.SendMessage(pollAnswer.User.Id, $"You've chosen: {selectedOption.Text} in poll");
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
