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
    public class InlineModeCommand : IUpdateCommand<Message>
    {

        private ITelegramBotClient _bot { get; set; }
        private bool _enabled { get; set; }

        public InlineModeCommand(ITelegramBotClient bot, bool enabled)
        {
            _bot = bot;
            _enabled = enabled;
        }

        public string CommandDescription()
        {
            return "send inline-mode results list";
        }

        public string CommandName()
        {
            return "inline_mode";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {
            var button = InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode");
            return await _bot.SendMessage(msg.Chat, "Press the button to start Inline Query\n\n" +
                "(Make sure you enabled Inline Mode in @BotFather)", replyMarkup: new InlineKeyboardMarkup(button));
        }

        public async Task<bool> IsEnabled()
        {
            return _enabled;
        }

        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Equals("/inline_mode", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task<bool> OnlyAdmins()
        {
            return _enabled;
        }

    }
}
