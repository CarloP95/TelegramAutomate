
using Telegram.Bot.Types;

namespace TelegramAutomate.Abstract
{
    public interface IAuthenticationService
    {

        bool Authenticate(Message msg);


    }
}
