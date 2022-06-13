using N2K_BackboneBackEnd.Models;
using System.Net.Http.Headers;

namespace N2K_BackboneBackEnd.Helpers
{
    public class FileSystemHandler : IAttachedFileHandler
    {

        private readonly AttachedFilesConfig _attachedFilesConfig;
        public FileSystemHandler(AttachedFilesConfig attachedFilesConfig)
        {
            _attachedFilesConfig = attachedFilesConfig;
        }



        public async Task<string> UploadFileAsync(AttachedFile file)
        {
            var folderName = _attachedFilesConfig.JustificationFolder;
            var pathToSave = string.IsNullOrEmpty(_attachedFilesConfig.FilesRootPath) ?
                Path.Combine(Directory.GetCurrentDirectory(), folderName) :
                Path.Combine(_attachedFilesConfig.FilesRootPath, folderName);

            var fileName = ContentDispositionHeaderValue.Parse(file.File.ContentDisposition).FileName.Trim('"');
            var fullPath = Path.Combine(pathToSave, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.File.CopyTo(stream);
            }
            var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
            return string.Format("{0}{1}/{2}", remoteUrl, folderName, fileName);
        }


        public async Task<int> DeleteFileAsync(string fileName)
        {
            var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
            var filesUrl = string.Format("{0}{1}/", remoteUrl, _attachedFilesConfig.JustificationFolder);
            fileName = fileName.Replace(filesUrl, "");

            var folderName = _attachedFilesConfig.JustificationFolder;
            var pathToSave = string.IsNullOrEmpty(_attachedFilesConfig.FilesRootPath) ?
                Path.Combine(Directory.GetCurrentDirectory(), folderName) :
                Path.Combine(_attachedFilesConfig.FilesRootPath, folderName);

            var fullPath = Path.Combine(pathToSave, fileName);
            File.Delete(fullPath);
            return 1;

        }

    }
}
