using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GoogleDocumentsUnifier.Logic
{
    public class GoogleApisDriveProvider : IDisposable
    {
        private readonly DriveService _driveService;

        private static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        private const string ApplicationName = "GoogleApisDriveProvider";

        public GoogleApisDriveProvider(Stream clientSecretStream, string credentialPath, string user,
                                       CancellationToken taskCancellationToken)
        {
            GoogleClientSecrets secrets = GoogleClientSecrets.Load(clientSecretStream);

            var credentialDataStore = new FileDataStore(credentialPath, true);

            Task<UserCredential> credentialTask =
                GoogleWebAuthorizationBroker.AuthorizeAsync(secrets.Secrets, Scopes, user, taskCancellationToken,
                                                            credentialDataStore);

            var initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credentialTask.Result,
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
