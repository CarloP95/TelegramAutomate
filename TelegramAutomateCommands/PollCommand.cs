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
    public class PollCommand : IUpdateCommand<Message>
    {

        private static readonly InputPollOption[] PollOptions = new InputPollOption[2] { "Hello", "World!" };

        private ITelegramBotClient _bot { get; set; }
        private bool _enabled { get; set; }

        public PollCommand(ITelegramBotClient bot, bool enabled)
        {
            _bot = bot;
            _enabled = enabled;
        }

        public string CommandDescription()
        {
            return "send a poll";
        }

        public string CommandName()
        {
            return "poll";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {
            return await _bot.SendPoll(msg.Chat, "Question", PollOptions, isAnonymous: false);
        }

        public async Task<bool> IsEnabled()
        {
            return _enabled;
        }

        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Equals("/poll", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task<bool> OnlyAdmins()
        {
            return _enabled;
        }

    }
}
