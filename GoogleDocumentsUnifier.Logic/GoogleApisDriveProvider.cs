using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using File = Google.Apis.Drive.v3.Data.File;

namespace GoogleDocumentsUnifier.Logic
{
    internal class GoogleApisDriveProvider : IDisposable
    {
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

        public void Dispose() { _driveService.Dispose(); }

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

        public async Task<FileInfo> GetFileInfoAsync(string id)
        {
            FilesResource.GetRequest request = _driveService.Files.Get(id);
            request.Fields = "id, name, modifiedTime";
            File file = await request.ExecuteAsync();
            return GetInfo(file);
        }

        public async Task<FileInfo> FindFileInFolderAsync(string target, string pdfName)
        {
            FilesResource.ListRequest request = _driveService.Files.List();
            request.Q = $"'{target}' in parents and name = '{pdfName}'";
            request.Fields = "files(id, name, modifiedTime)";
            FileList files = await request.ExecuteAsync();
            return files.Files.Count > 0 ? GetInfo(files.Files.First()) : null;
        }

        public async Task<IUploadProgress> CreateAsync(string name, string parent, FileStream stream,
            string contentType)
        {
            var file = new File
            {
                Name = name,
                Parents = new[] { parent }
            };
            FilesResource.CreateMediaUpload request = _driveService.Files.Create(file, stream, contentType);
            return await request.UploadAsync();
        }

        public async Task<IUploadProgress> UpdateAsync(string fileId, Stream stream, string contentType)
        {
            var file = new File();
            FilesResource.UpdateMediaUpload request = _driveService.Files.Update(file, fileId, stream, contentType);
            return await request.UploadAsync();
        }

        private static FileInfo GetInfo(File file) => new FileInfo(file.Id, file.Name, file.ModifiedTime);

        private static readonly string[] Scopes = { DriveService.Scope.Drive };
        private const string ApplicationName = "GoogleApisDriveProvider";

        private readonly DriveService _driveService;
    }
}
