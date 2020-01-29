using System;
using System.Collections.Generic;
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

        public Task<IDownloadProgress> DownloadFileAsync(string id, Stream stream)
        {
            return _driveService.Files.Get(id).DownloadAsync(stream);
        }

        public Task<IDownloadProgress> ExportFileAsync(string id, string targetMimeType, Stream stream)
        {
            return _driveService.Files.Export(id, targetMimeType).DownloadAsync(stream);
        }

        public async Task<FileInfo> GetFileInfoAsync(string id)
        {
            FilesResource.GetRequest request = _driveService.Files.Get(id);
            request.Fields = GetFields;
            File file = await request.ExecuteAsync();
            return GetInfo(file);
        }

        public Task<IEnumerable<FileInfo>> FindFilesInFolderAsync(string parentId, string name)
        {
            return ListFilesAsync($"'{parentId}' in parents and name = '{name}'");
        }

        public Task<IEnumerable<FileInfo>> GetFilesInFolder(string parentId)
        {
            return ListFilesAsync($"'{parentId}' in parents");
        }

        public Task<IUploadProgress> CreateAsync(string name, string parentId, FileStream stream, string contentType)
        {
            var file = new File
            {
                Name = name,
                Parents = new[] { parentId }
            };
            return _driveService.Files.Create(file, stream, contentType).UploadAsync();
        }

        public Task<IUploadProgress> UpdateAsync(string fileId, Stream stream, string contentType)
        {
            var file = new File();
            return _driveService.Files.Update(file, fileId, stream, contentType).UploadAsync();
        }

        private static FileInfo GetInfo(File file) => new FileInfo(file.Id, file.Name, file.ModifiedTime);

        private async Task<IEnumerable<FileInfo>> ListFilesAsync(string query)
        {
            FilesResource.ListRequest request = _driveService.Files.List();
            request.Q = query;
            request.Fields = ListFields;
            FileList fileList = await request.ExecuteAsync();
            return fileList.Files.Select(GetInfo);
        }

        private const string ApplicationName = "GoogleApisDriveProvider";
        private const string GetFields = "id, name, modifiedTime";

        private static readonly string[] Scopes = { DriveService.Scope.Drive };
        private static readonly string ListFields = $"files({GetFields})";

        private readonly DriveService _driveService;
    }
}
