using N2K_BackboneBackEnd.Models;
using System.Net.Http.Headers;

namespace N2K_BackboneBackEnd.Helpers
{
    public class FileSystemHandler : AttachedFileHandler, IAttachedFileHandler
    {

        public FileSystemHandler(AttachedFilesConfig attachedFilesConfig) : base(attachedFilesConfig)
        {
            _pathToSave = string.IsNullOrEmpty(_attachedFilesConfig.FilesRootPath) ?
                Path.Combine(Directory.GetCurrentDirectory(), _folderName) :
                Path.Combine(_attachedFilesConfig.FilesRootPath, _folderName);
        }


        public async Task<List<string>> UploadFileAsync(AttachedFile files)
        {
            List<String> uploadedFiles = new List<string>();
            if (files == null || files.Files == null) return uploadedFiles;
            var invalidFile = await AllFilesValid(files);

            foreach (var f in files.Files)
            {


#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                string? fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                var fullPath = Path.Combine(_pathToSave, fileName);

                if (CheckCompressionFormats(fileName))
                {
                    List<string> uncompressedFiles = ExtractCompressedFiles(fullPath);
                    foreach (var uncompressed in uncompressedFiles)
                    {
                        var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                        uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, uncompressed));
                    }
                    File.Delete(fullPath);
                }

                else
                {
                    var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                    uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, fileName));
                }

            }
            return uploadedFiles;
        }

        public async Task<List<string>> UploadFileAsync(string file)
        {
            List<String> uploadedFiles = new List<string>();
            if (String.IsNullOrEmpty(file)) return uploadedFiles;

#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            var fileName = Path.GetFileName(file);

            var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
            uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, fileName));

            await Task.Delay(10);

            return uploadedFiles;
        }


        public async Task<int> DeleteFileAsync(string fileName)
        {
            await Task.Delay(1);
            var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
            var filesUrl = string.Format("{0}{1}/", remoteUrl, _attachedFilesConfig.JustificationFolder);
            fileName = fileName.Replace(filesUrl, "");

            var fullPath = Path.Combine(_pathToSave, fileName);
            File.Delete(fullPath);
            return 1;

        }


        public async Task<int> DeleteUnionListsFilesAsync()
        {
            await Task.Delay(1);
            var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
            var filesUrl = string.Format("{0}{1}/", remoteUrl, _attachedFilesConfig.JustificationFolder);

            string[] files = Directory.GetFiles(filesUrl);
            foreach (string file in files)
            {
                if (file.EndsWith("_Union List.zip"))
                    File.Delete(file);
            }

            return 1;

        }

    }
}
