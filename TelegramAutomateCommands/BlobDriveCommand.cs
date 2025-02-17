using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelegramAutomate.Abstract;

namespace TelegramAutomate.Commands
{
    public class BlobDriveCommand : IBlobUploadService
    {

        private bool _enabled { get; set; }
        private string _shareFolder { get; set; }

        private DriveService _dS { get; set; }

        public BlobDriveCommand(bool enabled, string clientSecretJsonPath, string shareFolder)
        {
            _enabled = enabled;
            _dS = GetDriveService(clientSecretJsonPath);
            _shareFolder = shareFolder;
        }


        public async Task<bool> CanHandle(FileInfo fileInfo)
        {
            var maxSize = await MaximumHandleBytes();

            return maxSize > ((ulong)fileInfo.Length);
        }

        public Task<string> ExecuteCommandAsync(FileInfo fileInfo)
        {
            string folderId = EnsureFolder();
            string fileIdFound = SearchFileInFolder(folderId, fileInfo.Name);
            if (!string.IsNullOrEmpty(fileIdFound))
            {
                string urlFound = SetFilePublic(fileIdFound);
                return Task.FromResult(urlFound);
            }

            string fileId = UploadFile(fileInfo.FullName, folderId);
            if (string.IsNullOrEmpty(fileId))
            {
                return Task.FromResult(string.Empty);
            }

            string url = SetFilePublic(fileId);
            return Task.FromResult(url);

        }

        public Task<bool> IsEnabled()
        {
            return Task.FromResult(_enabled);
        }

        public Task<ulong> MaximumHandleBytes()
        {
            var request = _dS.About.Get();
            request.Fields = "storageQuota";
            var about = request.Execute();
            return Task.FromResult(
                about.StorageQuota.Limit.HasValue && about.StorageQuota.Usage.HasValue
                ? (ulong)(about.StorageQuota.Limit.Value - about.StorageQuota.Usage.Value)
                : 0
            );
        }

        private static DriveService GetDriveService(string clientSecretJson)
        {
            try
            {
                string[] Scopes = { DriveService.Scope.Drive };
                string ApplicationName = "BotTelegramDrive";

                UserCredential credential;

                using (var stream = new FileStream(clientSecretJson, FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;

                    Console.WriteLine("Autenticazione completata.");
                }

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                return service;

            }
            catch (Exception ex)
            {
                throw new Exception("Get Drive service failed.", ex);
            }
        }


        private string EnsureFolder()
        {
            string folderId = null;

            // Cerca la cartella
            var request = _dS.Files.List();
            request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{_shareFolder}' and trashed=false";
            request.Fields = "files(id, name)";
            var result = request.Execute();

            if (result.Files != null && result.Files.Count > 0)
            {
                folderId = result.Files.First().Id;
            }
            else
            {
                // Se non esiste, crea la cartella
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = _shareFolder,
                    MimeType = "application/vnd.google-apps.folder"
                };

                var createRequest = _dS.Files.Create(fileMetadata);
                createRequest.Fields = "id";
                var file = createRequest.Execute();

                folderId = file.Id;
            }

            return folderId;
        }

        private string SearchFileInFolder(string folderId, string fileName)
        {
            string fileId = null;

            var request = _dS.Files.List();
            request.Q = $"name='{fileName}' and '{folderId}' in parents and trashed=false";
            request.Fields = "files(id, name)";
            var result = request.Execute();

            if (result.Files != null && result.Files.Count > 0)
            {
                fileId = result.Files.First().Id;
            }

            return fileId;
        }

        private string UploadFile(string filePath, string folderId)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new List<string> { folderId }
            };

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var request = _dS.Files.Create(fileMetadata, stream, "application/octet-stream");
                request.Fields = "id";
                request.Upload();
                return request.ResponseBody?.Id;
            }
        }

        private string SetFilePublic(string fileId)
        {
            var permission = new Permission()
            {
                Type = "anyone",
                Role = "reader"
            };

            _dS.Permissions.Create(permission, fileId).Execute();
            return $"https://drive.google.com/uc?id={fileId}";
        }
    }
}
