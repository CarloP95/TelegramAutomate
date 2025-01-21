using Microsoft.Extensions.Logging;
using TelegramAutomate.Abstract;

namespace TelegramAutomate.Services;

// Compose Polling and ReceiverService implementations
public class PollingService: PollingServiceBase<ReceiverService>
{

    public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger) : base(serviceProvider, logger)
    {

    }

}
