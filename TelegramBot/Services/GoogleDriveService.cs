using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static Google.Apis.Requests.BatchRequest;

namespace TelegramBot.Services
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly Dictionary<string, string> _folderCache = new();

        public GoogleDriveService()
        {
            GoogleCredential credential;
            using (var stream = new FileStream("causal-setting-450114-v7-a4ae77920bdd.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.ScopeConstants.DriveReadonly);
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TelegramBot",
            });
        }

        public async Task<FileList> GetMp3FilesAsync(string genre)
        {
            string folderId = await GetFolderIdByGenre(genre);
            var request = _driveService.Files.List();
            request.Q = $"'{folderId}' in parents and mimeType='audio/mpeg'";
            request.Fields = "files(id, name)";
            var response = await request.ExecuteAsync();
            return response;
        }

        public async Task<(string Name, MemoryStream Stream)> DownloadFilesAsync(Google.Apis.Drive.v3.Data.File file)
        {
                var stream = new MemoryStream();
                var getRequest = _driveService.Files.Get(file.Id);
                await getRequest.DownloadAsync(stream);
                stream.Position = 0;


            return (file.Name, stream);
        }

        private async Task<string> GetFolderIdByGenre(string genre)
        {

            if (_folderCache.ContainsKey(genre))
                return _folderCache[genre];

            var request = _driveService.Files.List();
            request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{genre}'";
            request.Fields = "files(id, name)";
            var response = await request.ExecuteAsync();

            var folder = response.Files.FirstOrDefault();
            if (folder != null)
            {
                _folderCache[genre] = folder.Id;
                return folder.Id;
            }

            return null;

        }
    }
}
