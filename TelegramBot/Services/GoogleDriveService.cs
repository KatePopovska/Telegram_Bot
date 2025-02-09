using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot.Services
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly Dictionary<string, string> _folderCache = new Dictionary<string, string>();
        private readonly Dictionary<string, MemoryStream> _fileCache = new Dictionary<string, MemoryStream>();

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

        public async Task<List<(string Name, MemoryStream Stream)>> GetMp3FilesAsync(string genre)
        {
            string folderId = await GetFolderIdByGenre(genre);
            var request = _driveService.Files.List();
            request.Q = $"'{folderId}' in parents and mimeType='audio/mpeg'";
            request.Fields = "files(id, name)";
            var response = await request.ExecuteAsync();

            var stopwatch = Stopwatch.StartNew();

            var files = new List<(string Name, MemoryStream Stream)>();

            foreach (var file in response.Files)
            {
                var stream = new MemoryStream();
                var getRequest = _driveService.Files.Get(file.Id);
                await getRequest.DownloadAsync(stream); //VERY LONG
                stream.Position = 0; 
                files.Add((file.Name, stream));
            }

            stopwatch.Stop();
            Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");

            return files;
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
