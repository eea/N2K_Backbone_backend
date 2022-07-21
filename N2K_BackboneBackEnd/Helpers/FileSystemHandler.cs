using N2K_BackboneBackEnd.Models;
using System.IO.Compression;
using System.Net.Http.Headers;

namespace N2K_BackboneBackEnd.Helpers
{
    public class FileSystemHandler : AttachedFileHandler, IAttachedFileHandler
    {

        public FileSystemHandler(AttachedFilesConfig attachedFilesConfig) : base(attachedFilesConfig)
        {
        }



        public async Task<List<string>> UploadFileAsync(AttachedFile files)
        {
            var folderName = _attachedFilesConfig.JustificationFolder;
            var pathToSave = string.IsNullOrEmpty(_attachedFilesConfig.FilesRootPath) ?
                Path.Combine(Directory.GetCurrentDirectory(), folderName) :
                Path.Combine(_attachedFilesConfig.FilesRootPath, folderName);
            List<String> uploadedFiles = new List<string>();
            var invalidFile = false;

            foreach (var f in files.Files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await f.CopyToAsync(stream);
                }
                if (!CheckExtensions(fileName) || invalidFile == true)
                {
                    invalidFile = true;
                    File.Delete(fullPath);
                    break;
                }
                if (fileName.EndsWith("zip"))
                {
                    using (ZipArchive archive = ZipFile.OpenRead(fullPath))
                    {
                        archive.ExtractToDirectory(pathToSave);
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (!CheckExtensions(entry.Name))
                            {
                                invalidFile = true;
                                File.Delete(Path.Combine(pathToSave, entry.Name));
                                break;
                            }
                            File.Delete(Path.Combine(pathToSave, entry.Name));
                        }
                    }
                }
                File.Delete(fullPath);
            }

            if (!invalidFile)
            {
                foreach (var f in files.Files)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await f.CopyToAsync(stream);
                    }
                    if (fullPath.EndsWith("zip"))
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(fullPath))
                        {
                            archive.ExtractToDirectory(pathToSave);
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                                uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, folderName, entry.Name));
                            }
                        }
                    }
                    else
                    {
                        var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                        uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, folderName, fileName));
                    }
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
