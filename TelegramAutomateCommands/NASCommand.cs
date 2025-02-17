using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramAutomate.Abstract;
using static TelegramAutomate.Abstract.TrustedStrings;

namespace TelegramAutomate.Commands
{
    public class NASCommand : IUpdateCommand<Message>
    {

        private ITelegramBotClient _bot { get; set; }
        private IAuthenticationService _auth { get; set; }

        private string _nasPath { get; set; }

        private IEnumerable<IBlobUploadService> _uploadServices { get; set; }

        public NASCommand(ITelegramBotClient bot, IAuthenticationService auth, string nasPath, IEnumerable<IBlobUploadService> uploadServices)
        {
            _bot = bot;
            _auth = auth;
            _nasPath = nasPath;
            _uploadServices = uploadServices;
        }

        public string CommandDescription()
        {
            return "behaves like a nas";
        }

        public string CommandName()
        {
            return "nas";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {
            if (!_auth.Authenticate(msg))
            {
                return await _bot.SendMessage(msg.Chat, "You're not admin. Bye bye");
            }
            var msgPieces = msg.Text.Split(' ');
            var command = msgPieces.Length <= 1 ? "listroot" : msgPieces[1];

            var result = await (command switch
            {
                "get" => GetFile(msg),
                "list" => ListDirectory(msg),
                "listroot" => DefaultResponse(msg),
                _ => DefaultResponse(msg)
            });

            return result;
            //await _bot.SendChatAction(msg.Chat, ChatAction.UploadPhoto);
            //await Task.Delay(2000); // simulate a long task
            //await using var fileStream = new FileStream("Files/bot.gif", FileMode.Open, FileAccess.Read);
        }

        private async Task<Message> DefaultResponse(Message msg)
        {
            return await _bot.SendMessage(msg.Chat, ListDirectory(_nasPath));
        }

        private string ListDirectory(string path)
        {
            var results = new StringBuilder();
            var allFiles = Directory.GetFileSystemEntries(path);
            if (allFiles is null || allFiles.Length == 0)
            {
                return "Empty directory";
            }
            foreach (var file in allFiles)
            {
                var strSize = "B";
                try
                {
                    var size = new FileInfo(file).Length;
                    if (size > 1024)
                    {
                        size /= 1024;
                        strSize = "KB";
                    }
                    if (size > 1024)
                    {
                        size /= 1024;
                        strSize = "MB";
                    }
                    if (size > 1024)
                    {
                        size /= 1024;
                        strSize = "GB";
                    }
                    results.Append($"{file}\t{size}{strSize}\n");
                }
                catch (FileNotFoundException e)
                {
                    results.Append($"{file}\td\n");
                }
            }
            return results.ToString();
        }

        private async Task<Message> ListDirectory(Message msg)
        {
            var msgPieces = msg.Text.Split(' ');

            if (msgPieces.Length < 3)
            {
                return await _bot.SendMessage(msg.Chat, "Don't know what to list. Please check. Syntax is /nas list <<directory>>");
            }

            var directory = msgPieces[2];
            if (string.IsNullOrEmpty(directory.Trim()))
            {
                return await _bot.SendMessage(msg.Chat, "Don't know what to list. Please check. Syntax is /nas list <<directory>>. Please ensure directory is filled and allowed to query");
            }

            directory = directory.SafePath();
            if (!Directory.Exists(directory))
            {
                return await _bot.SendMessage(msg.Chat, "Don't know what to list. Directory does not exists");
            }

            if (!directory.StartsWith(_nasPath))
            {
                return await _bot.SendMessage(msg.Chat, $"Don't know what to list. You can't list anything else outside {_nasPath}");
            }

            return await _bot.SendMessage(msg.Chat, ListDirectory(directory));
        }

        private async Task<Message> GetFile(Message msg)
        {
            var msgPieces = msg.Text.Split(' ');
            if (msgPieces.Length < 3)
            {
                return await _bot.SendMessage(msg.Chat, "Don't know what to get. Please check. Syntax is /nas get <<filename>>");
            }

            var file = msgPieces[2];
            if (string.IsNullOrEmpty(file.Trim()))
            {
                return await _bot.SendMessage(msg.Chat, "Don't know what to get. Please check. Syntax is /nas get <<filename>>. Please ensure filename is filled");
            }

            file = file.SafeFilename();
            if (!System.IO.File.Exists(file))
            {
                return await _bot.SendMessage(msg.Chat, "Don't know what to get. File does not exists");
            }

            var fi = new FileInfo(file);
            if (fi.Length > 50 * 1024 * 1024) //File bigger than 50MB
            {
                var selectedUS = _uploadServices.First(us => us.CanHandle(fi).Result);
                if (selectedUS == null)
                {
                    return await _bot.SendMessage(msg.Chat, "Sorry i can't send you the file because it's too big and no configured services is able to handle it. Bye");
                }

                var url = await selectedUS.ExecuteCommandAsync(fi);
                if (string.IsNullOrEmpty(url))
                {
                    return await _bot.SendMessage(msg.Chat, "Sorry the admin of this bot is a dumbass and it's trusting a loose guy in Catania. It seems that some problems happened in loading the file in the BlobUploadService. Bye");
                }

                return await _bot.SendMessage(msg.Chat, $"The url to download the file is {url}");
            }

            await using var stream = System.IO.File.OpenRead(file);
            return await _bot.SendDocument(msg.Chat.Id, InputFile.FromStream(stream, Path.GetFileName(file)), "File you requested");
        }

        public async Task<bool> IsEnabled()
        {
            return true;
        }
        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Split(' ')[0].Equals("/nas", StringComparison.OrdinalIgnoreCase) ?? false;
        }


        public async Task<bool> OnlyAdmins()
        {
            return false;
        }

    }
}
