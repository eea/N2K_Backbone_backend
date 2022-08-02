using N2K_BackboneBackEnd.Models;
using System.IO.Compression;
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
            var invalidFile =await AllFilesValid(files);

            foreach (var f in files.Files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(_pathToSave, fileName);


                if (CheckCompressionFormats(fileName))
                {

                    //if the file is compressed(extract all the content)
                    List<string> uncompressed = ExtractCompressedFiles(fullPath);
                    foreach (var uncompressedFile in uncompressed)
                    {
                        var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                        uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, uncompressedFile));
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

    }
}
