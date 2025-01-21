using System.Threading;
using System.Threading.Tasks;

namespace TelegramAutomate.Abstract
{
    public interface IReceiverService
    {
        Task ReceiveAsync(CancellationToken stoppingToken);
    }

}