using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAutomate.Abstract;

namespace TelegramAutomate.Commands
{
    public class PhotoCommand : IUpdateCommand<Message>
    {

        private ITelegramBotClient _bot { get; set; }
        private bool _enabled { get; set; }

        public PhotoCommand(ITelegramBotClient bot, bool enabled) 
        {
            _bot = bot;
            _enabled = enabled;
        }

        public string CommandDescription()
        {
            return "send a photo";
        }

        public string CommandName()
        {
            return "photo";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {
            await _bot.SendChatAction(msg.Chat, ChatAction.UploadPhoto);
            await Task.Delay(2000); // simulate a long task
            await using var fileStream = new FileStream("Files/bot.gif", FileMode.Open, FileAccess.Read);
            return await _bot.SendPhoto(msg.Chat, fileStream, caption: "Read https://telegrambots.github.io/book/");
        }

        public async Task<bool> IsEnabled()
        {
            return _enabled;
        }
        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Equals("/photo", StringComparison.OrdinalIgnoreCase) ?? false;
        }


        public async Task<bool> OnlyAdmins()
        {
            return _enabled;
        }

    }
}
