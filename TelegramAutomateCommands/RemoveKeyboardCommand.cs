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
    public class RemoveKeyboardCommand : IUpdateCommand<Message>
    {

        private ITelegramBotClient _bot { get; set; }
        private bool _enabled { get; set; }

        public RemoveKeyboardCommand(ITelegramBotClient bot, bool enabled)
        {
            _bot = bot;
            _enabled = enabled;
        }

        public string CommandDescription()
        {
            return "remove keyboard buttons";
        }

        public string CommandName()
        {
            return "remove";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {
            return await _bot.SendMessage(msg.Chat, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
        }

        public async Task<bool> IsEnabled()
        {
            return _enabled;
        }

        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Equals("/remove", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task<bool> OnlyAdmins()
        {
            return _enabled;
        }

    }
}
