using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace TelegramAutomate.Abstract
{
    /// <summary>An abstract class to compose Receiver Service and Update Handler classes</summary>
    /// <typeparam name="TUpdateHandler">Update Handler to use in Update Receiver</typeparam>
    public abstract class ReceiverServiceBase<TUpdateHandler> : IReceiverService where TUpdateHandler : IUpdateHandler
    {

        public ITelegramBotClient botClient { get; set; }
        public TUpdateHandler updateHandler { get; set; }
        public ILogger<ReceiverServiceBase<TUpdateHandler>> logger { get; set; }

        public ReceiverServiceBase(ITelegramBotClient botClient, TUpdateHandler updateHandler, ILogger<ReceiverServiceBase<TUpdateHandler>> logger)
        {
            this.botClient = botClient;
            this.updateHandler = updateHandler;
            this.logger = logger;
        }

        /// <summary>Start to service Updates with provided Update Handler class</summary>
        public async Task ReceiveAsync(CancellationToken stoppingToken)
        {
            // ToDo: we can inject ReceiverOptions through IOptions container
            var receiverOptions = new ReceiverOptions() { DropPendingUpdates = true, AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[0] };

            var me = await botClient.GetMe(stoppingToken);
            logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

            // Start receiving updates
            await botClient.ReceiveAsync(updateHandler, receiverOptions, stoppingToken);
        }
    }
}
