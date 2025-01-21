using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAutomate.Abstract;

namespace TelegramAutomate.Commands
{
    public class RequestCommand : IUpdateCommand<Message>
    {

        private ITelegramBotClient _bot { get; set; }
        private bool _enabled { get; set; }

        public RequestCommand(ITelegramBotClient bot, bool enabled)
        {
            _bot = bot;
            _enabled = enabled;
        }

        public string CommandDescription()
        {
            return "request location or contact";
        }

        public string CommandName()
        {
            return "request";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {
            var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddButton(KeyboardButton.WithRequestLocation("Location"))
            .AddButton(KeyboardButton.WithRequestContact("Contact"));
            return await _bot.SendMessage(msg.Chat, "Who or Where are you?", replyMarkup: replyMarkup);
        }

        public async Task<bool> IsEnabled()
        {
            return _enabled;
        }

        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Equals("/request", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task<bool> OnlyAdmins()
        {
            return _enabled;
        }

    }
}
