using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace GoogleDocumentsUnifier.Logic.Legacy
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

        public void DownloadFile(string id, Stream stream)
        {
            FilesResource.GetRequest request = _driveService.Files.Get(id);
            request.Download(stream);
        }

        public void ExportFile(string id, string targetMimeType, Stream stream)
        {
            FilesResource.ExportRequest request = _driveService.Files.Export(id, targetMimeType);
            request.Download(stream);
        }

        internal string GetName(string id) => _driveService.Files.Get(id).Execute().Name;
    }
}
