using System.IO;
using System.Threading.Tasks;

namespace TelegramAutomate.Abstract
{
    /// <summary>
    /// Interface that handles file bigger than Telegram can accept.
    /// Can implement your favorite Cloud Storage service here (e.g. MediaFire, DropBox, GoogleDrive, etc.)
    /// but for each one a library and a new service is required.
    /// Then in the Program.cs it's enough to instance your favorite and add it as Singleton.
    /// </summary>
    public interface IBlobUploadService
    {
        public Task<ulong> MaximumHandleBytes();

        Task<bool> IsEnabled();

        Task<bool> CanHandle(FileInfo fileInfo);

        /// <summary>
        /// Method that will upload the blob and return an URL or a string that allows the client 
        /// on Telegram to download the document.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        Task<string> ExecuteCommandAsync(FileInfo fileInfo);
    }
}
