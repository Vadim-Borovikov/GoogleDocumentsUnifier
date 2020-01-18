using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using File = Google.Apis.Drive.v3.Data.File;

namespace GoogleDocumentsUnifier.Logic
{
    public class GoogleApisDriveProvider : IDisposable
    {
        private readonly DriveService _driveService;

        private static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        private const string ApplicationName = "GoogleApisDriveProvider";

        public GoogleApisDriveProvider(string projectJson)
        {
            GoogleCredential credential = GoogleCredential.FromJson(projectJson).CreateScoped(Scopes);

            var initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            };

            _driveService = new DriveService(initializer);
        }

        public void Dispose()
        {
            _driveService.Dispose();
        }

        public async Task<IDownloadProgress> DownloadFileAsync(string id, Stream stream)
        {
            FilesResource.GetRequest request = _driveService.Files.Get(id);
            return await request.DownloadAsync(stream);
        }

        public async Task<IDownloadProgress> ExportFileAsync(string id, string targetMimeType, Stream stream)
        {
            FilesResource.ExportRequest request = _driveService.Files.Export(id, targetMimeType);
            return await request.DownloadAsync(stream);
        }

        internal async Task<string> GetNameAsync(string id)
        {
            File file = await _driveService.Files.Get(id).ExecuteAsync();
            return file.Name;
        }
    }
}
