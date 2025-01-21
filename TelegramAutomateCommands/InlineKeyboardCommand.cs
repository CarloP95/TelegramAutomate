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
    public class InlineKeboardCommand : IUpdateCommand<Message>
    {

        private ITelegramBotClient _bot { get; set; }
        private bool _enabled { get; set; }

        public InlineKeboardCommand(ITelegramBotClient bot, bool enabled)
        {
            _bot = bot;
            _enabled = enabled;
        }

        public string CommandDescription()
        {
            return " send inline buttons";
        }

        public string CommandName()
        {
            return "inline_buttons";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {

            var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow("1.1", "1.2", "1.3")
                .AddNewRow()
                    .AddButton("WithCallbackData", "CallbackData")
                    .AddButton(InlineKeyboardButton.WithUrl("WithUrl", "https://github.com/TelegramBots/Telegram.Bot"));
            return await _bot.SendMessage(msg.Chat, "Inline buttons:", replyMarkup: inlineMarkup);

        }

        public async Task<bool> IsEnabled()
        {
            return _enabled;
        }

        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Equals("/inline_buttons", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task<bool> OnlyAdmins()
        {
            return _enabled;
        }

    }
}
