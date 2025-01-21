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
    public class KeyboardCommand : IUpdateCommand<Message>
    {

        private ITelegramBotClient _bot { get; set; }
        private bool _enabled { get; set; }

        public KeyboardCommand(ITelegramBotClient bot, bool enabled)
        {
            _bot = bot;
            _enabled = enabled;
        }

        public string CommandDescription()
        {
            return " send keyboard buttons";
        }

        public string CommandName()
        {
            return "keyboard";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {
            var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddNewRow("1.1", "1.2", "1.3")
            .AddNewRow().AddButton("2.1").AddButton("2.2");
            return await _bot.SendMessage(msg.Chat, "Keyboard buttons:", replyMarkup: replyMarkup);

        }

        public async Task<bool> IsEnabled()
        {
            return _enabled;
        }

        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Equals("/keyboard", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task<bool> OnlyAdmins()
        {
            return _enabled;
        }

    }
}
