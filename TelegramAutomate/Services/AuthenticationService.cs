using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using TelegramAutomate.Abstract;

namespace TelegramAutomate.Services;

public class AuthenticationService: IAuthenticationService
{

    private readonly ILogger _logger;
    private readonly BotConfiguration _conf;

    private DateTime _lastAuthentication = DateTime.MinValue;

    public AuthenticationService(IOptions<BotConfiguration> conf, ILogger<AuthenticationService> logger)
    {
        _conf = conf.Value;
        _logger = logger;
    }

    public bool Authenticate(Message msg)
    {
        _logger.LogInformation("Starting authentication for message with {id} sent by {senderId}", msg.Id, msg.SenderChat?.Id);

        long senderId = msg.Chat.Id;
        string senderName = msg.Chat.FirstName;
        string senderSurname = msg.Chat.LastName;

        if (string.IsNullOrEmpty(senderName) || string.IsNullOrEmpty(senderSurname))
        {
            _logger.LogWarning("Failed authentication for {id} sent by {senderId} because no senderName or senderSurname are present in the message", msg.Id, msg.SenderChat?.Id);
            return false;
        }

        if (!_conf.AdminName.Trim().Equals(senderName, StringComparison.OrdinalIgnoreCase)
            || !_conf.AdminSurname.Trim().Equals(senderSurname, StringComparison.OrdinalIgnoreCase)
            || !_conf.AdminID.Trim().Equals(senderId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Failed authentication for {id} sent by {senderId} because name, surname or id mismatch", msg.Id, msg.SenderChat?.Id);
            return false;
        }

        _logger.LogInformation("Checking datetime expiration and password {id} sent by {senderId}.", msg.Id, msg.SenderChat?.Id);
        if (_lastAuthentication.AddHours(1) < DateTime.Now)
        {
            _logger.LogInformation("Auth expired! Checking password... {id} sent by {senderId}.", msg.Id, msg.SenderChat?.Id);
            var msgPieces = msg.Text.Split(' ');
            if (msgPieces.Length <= 1 || !msgPieces[1].Equals(_conf.AdminPassword))
            {
                _logger.LogWarning("Failed authentication for {id} sent by {senderId}. Auth expired and no password", msg.Id, msg.SenderChat?.Id);
                return false;
            }
            _lastAuthentication = DateTime.Now;
            _logger.LogInformation("Authorized. {id} sent by {senderId}.", msg.Id, msg.SenderChat?.Id);
        }

        _logger.LogInformation("End authentication for {id} sent by {senderId} with OK.", msg.Id, msg.SenderChat?.Id);
        return true;
    }



}
