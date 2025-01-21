namespace TelegramAutomate;

public class BotConfiguration
{
    public string BotToken { get; init; } = default!;

    #region Authentication 
    public string AdminID { get; init; } = default!;
    public string AdminName { get; init; } = default!;
    public string AdminSurname { get; init; } = default!;
    public string AdminPassword { get; init; } = default!;
    #endregion Authentication 



    #region NAS Torrent requirement
    public bool AllowTorrents { get; init; } = default!;
    public bool TorrentingForAdmin { get; init; } = default!;
    public string TorrentPath { get; init; } = default!;
    #endregion NAS Torrent requirement

    public string NASPath { get; init; } = default!;

}
