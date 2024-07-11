using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace N2K_BackboneBackEnd.Helpers
{
    public class FileSystemHandler : AttachedFileHandler, IAttachedFileHandler
    {
        public FileSystemHandler(AttachedFilesConfig attachedFilesConfig, N2KBackboneContext dataContext) : base(attachedFilesConfig, dataContext)
        {
            _pathToSave = string.IsNullOrEmpty(_attachedFilesConfig.FilesRootPath) ?
                Path.Combine(Directory.GetCurrentDirectory(), _folderName) :
                Path.Combine(_attachedFilesConfig.FilesRootPath, _folderName);
        }

        public async Task<List<JustificationFiles>> UploadFileAsync(AttachedFile files)
        {
            try
            {
                string remoteUrl = "";
                List<JustificationFiles> uploadedFiles = new();
                if (files == null || files.Files == null)
                    return uploadedFiles;
                bool invalidFile = await AllFilesValid(files);

                foreach (var f in files.Files)
                {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                    string? fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                    string? fullPath = Path.Combine(_pathToSave, fileName);

                    FileInfo fi = new FileInfo(fullPath);
                    string newfilename = string.Format("{0}_{1}{2}", System.Guid.NewGuid().ToString(), DateTime.Now.Ticks, fi.Extension);
                    File.Move(fullPath, Path.Combine(_pathToSave, newfilename));

                    JustificationFiles item = new JustificationFiles();
                    remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");

                    item.Path = newfilename;
                    item.OriginalName = fileName;
                    uploadedFiles.Add(item);
                }
                return uploadedFiles;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "FileSystemHandler - UploadFileAsync(AttachedFile)", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<string>> UploadFileAsync(string file)
        {
            try
            {
                string remoteUrl = "";
                List<String> uploadedFiles = new();
                if (String.IsNullOrEmpty(file))
                    return uploadedFiles;

                string? fileName = Path.GetFileName(file);

                remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                uploadedFiles.Add(fileName);

                await Task.Delay(10);

                return uploadedFiles;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "FileSystemHandler - UploadFileAsync(string)", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<int> DeleteFileAsync(string fileName)
        {
            try
            {
                await Task.Delay(1);
                string remoteUrl = _attachedFilesConfig.PublicFilesUrl; //  _pathToSave + "/" +  _attachedFilesConfig.FilesRootPath + (!_attachedFilesConfig.FilesRootPath.EndsWith("/") ? "/" : "");
                string filesUrl = string.Format("{0}/{1}/", remoteUrl, _attachedFilesConfig.JustificationFolder);
                fileName = fileName.Replace(filesUrl, "");

                string? fullPath = Path.Combine(_pathToSave, fileName);
                File.Delete(fullPath);
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "FileSystemHandler - DeleteFileAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<int> DeleteUnionListsFilesAsync()
        {
            try
            {
                await Task.Delay(1);
                string filesUrl = _pathToSave; //  string.Format("{0}{1}/", remoteUrl, _attachedFilesConfig.JustificationFolder);

                string[] files = Directory.GetFiles(filesUrl);
                foreach (string file in files)
                {
                    if (file.EndsWith("_Union List.zip"))
                        File.Delete(file);
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "FileSystemHandler - DeleteUnionListsFilesAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<byte[]> ReadFile(string fileName)
        {
            string _folderName = _pathToSave;
            string path = Path.Combine(_folderName, fileName);

            path = Path.Combine(_folderName, fileName);
            var file_bytes = await System.IO.File.ReadAllBytesAsync(path);
            return file_bytes;
        }
    }
}
