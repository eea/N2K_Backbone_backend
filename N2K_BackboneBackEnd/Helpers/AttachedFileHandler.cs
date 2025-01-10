using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Net.Http.Headers;

namespace N2K_BackboneBackEnd.Helpers
{
    public class AttachedFileHandler
    {
        protected readonly AttachedFilesConfig _attachedFilesConfig;
        protected readonly string _folderName;
        protected string _pathToSave;
        protected readonly N2KBackboneContext _dataContext;

        public AttachedFileHandler(AttachedFilesConfig attachedFilesConfig, N2KBackboneContext dataContext)
        {
            _attachedFilesConfig = attachedFilesConfig;
            _folderName = _attachedFilesConfig.JustificationFolder;
            _pathToSave = Path.Combine(Directory.GetCurrentDirectory(), _folderName);
            _dataContext = dataContext;
        }

        public bool CheckExtensions(string fileName)
        {
            List<String> extensionWhiteList = _attachedFilesConfig.ExtensionWhiteList;
            string[] fileArray = fileName.Split(".");
            string fileExtension = fileArray[fileArray.Length - 1];
            return extensionWhiteList.Any(x => x.ToLower() == fileExtension.ToLower());
        }

        public bool CheckCompressionFormats(string fileName)
        {
            List<String> compressionFormats = _attachedFilesConfig.CompressionFormats;
            string[] fileArray = fileName.Split(".");
            string fileExtension = fileArray[fileArray.Length - 1];
            return compressionFormats.Any(x => x.ToLower() == fileExtension.ToLower());
        }

        protected List<string> ExtractCompressedFiles(string fileName)
        {
            List<(string, MemoryStream)> files = GetAllCompressedFiles(fileName).Result;
            files.ForEach(f =>
            {
                using (FileStream fs = new FileStream(Path.Combine(_pathToSave, f.Item1), FileMode.Create, FileAccess.Write))
                {
                    f.Item2.WriteTo(fs);
                }
            });
            return files.Select(f => f.Item1).ToList();
        }

        protected bool CheckCompressedFiles(string fileName)
        {
            List<(string, MemoryStream)> files = GetAllCompressedFiles(fileName).Result;
            return files.Any(f => !CheckExtensions(f.Item1));
        }

        // recursively returns a flat list of all compressed files and their subfiles
        // fileName needs to be a compressed file
        protected async Task<List<(string, MemoryStream)>> GetAllCompressedFiles(string fileName)
        {
            List<(string, MemoryStream)> files;
            using (Stream stream = File.OpenRead(fileName))
            {
                using (var reader = ReaderFactory.Open(stream, new ReaderOptions { LeaveStreamOpen = true }))
                {
                    files = await SeekFiles(fileName, reader, 3);
                }
            }
            return files;
        }

        private async Task<List<(string, MemoryStream)>> SeekFiles(string fileName, IReader reader, int? recurse = 0)
        {
            List<(string, MemoryStream)> files = new();
            if (recurse < 1)
            {
                throw new Exception("Limit for recursion reached");
            }
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    string name = reader.Entry.Key;
                    if (CheckCompressionFormats(name))
                    {
                        var auxReader = ReaderFactory.Open(reader.OpenEntryStream(), new ReaderOptions { LeaveStreamOpen = true });
                        var auxFiles = await SeekFiles(name, auxReader, recurse - 1);
                        auxFiles.ForEach(files.Add);
                    }
                    else
                    {
                        using (var entryStream = reader.OpenEntryStream())
                        {
                            MemoryStream currentFile = new MemoryStream();
                            await currentFile.FlushAsync();
                            currentFile.Position = 0;
                            await entryStream.CopyToAsync(currentFile);
                            files.Add((name, currentFile));
                        }
                    }
                }
            }
            return files;
        }

        protected async Task<bool> CopyCompressedFileToTempFolder(IFormFile file, string fileName)
        {
            using (FileStream stream = new(fileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return true;
        }

        protected async Task<bool> AllFilesValid(AttachedFile files)
        {
            if (files == null || files.Files == null) return true;
            bool invalidFile = false;
            foreach (var f in files.Files)
            {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                string? fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                string? fullPath = Path.Combine(_pathToSave, fileName);
                bool res = await CopyCompressedFileToTempFolder(f, fullPath);
                if (!CheckExtensions(fileName) || invalidFile == true)
                {
                    if (!invalidFile) invalidFile = true;
                    File.Delete(fullPath);
                }
                /*
                if (CheckCompressionFormats(fileName))
                {
                    //copy file to temp repository                    
                    //check compressed files types
                    if (!invalidFile && CheckCompressedFiles(fullPath)) invalidFile = true;
                }
                */
            }
            if (invalidFile)
                throw new Exception("some of the attached file(s) have invalid extensions");

            return invalidFile;
        }
    }
}
