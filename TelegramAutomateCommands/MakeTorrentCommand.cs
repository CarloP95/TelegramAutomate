using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using MonoTorrent;
using TelegramAutomate.Abstract;
using System.IO;

namespace TelegramAutomate.Commands
{
    public class MakeTorrentCommand : IUpdateCommand<Message>
    {

        private ITelegramBotClient _bot { get; set; }
        private bool _enabled { get; set; }
        private bool _onlyForAdmin { get; set; }
        private string _torrentPath { get; set; }

        private readonly IAuthenticationService _auth;

        public MakeTorrentCommand(ITelegramBotClient bot, bool enabled, bool onlyForAdmin, string torrentPath, IAuthenticationService auth)
        {
            _bot = bot;
            _auth = auth;
            _enabled = enabled;
            _torrentPath = torrentPath;
            _onlyForAdmin = onlyForAdmin;
        }

        public string CommandDescription()
        {
            return "create torrent and put it under the configured directory to download from a torrent client";
        }

        public string CommandName()
        {
            return "torrent";
        }

        public async Task<Message> ExecuteCommandAsync(Message msg)
        {
            if (!_auth.Authenticate(msg))
            {
                return await _bot.SendMessage(msg.Chat, "You are not ADMIN. Can't use this method");
            }


            var msgPieces = msg.Text?.Split(' ');
            if (msgPieces.Length <= 1)
            {
                return await _bot.SendMessage(msg.Chat, "Please send me a message with /torrent <<magnet>>");
            }

            var magnet = msgPieces[1];
            var filename = msgPieces.Length > 2 ? msgPieces[2] : $"{DateTime.Now.ToString("%yyyy_%MM_%d_%H_%mm_%ss")}.torrent";

            var metadata = await new MonoTorrent.Client.ClientEngine().DownloadMetadataAsync(MagnetLink.Parse(magnet), new CancellationToken());
            System.IO.File.WriteAllBytes($"{_torrentPath}{filename}", metadata.ToArray());

            return await _bot.SendMessage(msg.Chat, "Ok adding it to predefined folder");
        }

        public async Task<bool> IsEnabled()
        {
            return _enabled;
        }

        public async Task<bool> CanHandle(Message msg)
        {
            return msg.Text?.Split(' ')[0].Equals("/torrent", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task<bool> OnlyAdmins()
        {
            return _onlyForAdmin;
        }

    }
}
