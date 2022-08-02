using N2K_BackboneBackEnd.Models;
using SevenZip;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;

namespace N2K_BackboneBackEnd.Helpers
{
    public class AttachedFileHandler
    {
        protected readonly AttachedFilesConfig _attachedFilesConfig;
        protected readonly string _folderName;
        protected string _pathToSave;


        public AttachedFileHandler(AttachedFilesConfig attachedFilesConfig)
        {
            _attachedFilesConfig = attachedFilesConfig;
            _folderName = _attachedFilesConfig.JustificationFolder;
            _pathToSave = Path.Combine(Directory.GetCurrentDirectory(), _folderName);
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


        private bool checkZipCompressedFiles(string fileName)
        {
            bool invalidFile = false;
            using (ZipArchive archive = ZipFile.OpenRead(fileName))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!CheckExtensions(entry.Name))
                    {
                        if (!invalidFile) invalidFile = true;
                    }
                }
            }
            if (invalidFile) File.Delete(fileName);
            return invalidFile;
        }

        private string returnSevenZipDllPath()
        {
            return  Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
        }


        private bool checkSevenZipCompressedFiles(string fileName)
        {
            bool invalidFile = false;


            using (Stream stream = File.OpenRead(fileName))
            {                
                SevenZipBase.SetLibraryPath(returnSevenZipDllPath());
                using (SevenZipExtractor extr = new SevenZipExtractor(stream))
                {
                    foreach (ArchiveFileInfo archiveFileInfo in extr.ArchiveFileData)
                    {
                        if (!archiveFileInfo.IsDirectory)
                        {
                            string shortFileName = Path.GetFileName(archiveFileInfo.FileName);
                            if (!CheckExtensions(shortFileName))
                            {
                                if (!invalidFile) invalidFile = true;
                            }
                        }
                    }
                }
            }


            if (invalidFile)   File.Delete(fileName);
            return invalidFile;
        }

        private List<string> extractZipCompressedFiles(string fileName)
        {
            var fileList = new List<string>();
            using (ZipArchive archive = ZipFile.OpenRead(fileName))  {
                archive.ExtractToDirectory(_pathToSave);
                return archive.Entries.Select(entry => entry.Name).ToList();
            }
        }

        private List<string> extractSevenZipCompressedFiles(string fileName)
        {
            var fileList = new List<string>();
            using (Stream stream = File.OpenRead(fileName))
            {

                SevenZipBase.SetLibraryPath(returnSevenZipDllPath());
                using (SevenZipExtractor extr = new SevenZipExtractor(stream))
                {
                    foreach (ArchiveFileInfo archiveFileInfo in extr.ArchiveFileData)
                    {
                        if (!archiveFileInfo.IsDirectory)
                        {
                            using (var mem = new MemoryStream())
                            {
                                extr.ExtractFile(archiveFileInfo.Index, mem);
                                string shortFileName = Path.GetFileName(archiveFileInfo.FileName);
                                using (FileStream file = new FileStream(Path.Combine(_pathToSave, shortFileName), FileMode.Create, System.IO.FileAccess.Write))
                                {
                                    byte[] bytes = new byte[mem.Length];
                                    mem.Read(bytes, 0, (int)mem.Length);
                                    file.Write(bytes, 0, bytes.Length);
                                    fileList.Add(shortFileName);
                                }
                            }
                        }
                    }
                }
            }

            return fileList;
        }

        protected bool CheckCompressedFiles(string fileName)
        {

            if (fileName.ToLower().EndsWith(".zip"))
            {
                return checkZipCompressedFiles(fileName);
            }
            else return checkSevenZipCompressedFiles(fileName);
        }

        protected async Task<bool> CopyCompressedFileToTempFolder(IFormFile file, string fileName) { 
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return true;
        }



        protected List<string> ExtractCompressedFiles(string fileName)
        {
            if (fileName.ToLower().EndsWith(".zip"))
            {
                return extractZipCompressedFiles(fileName);
            }
            else return extractSevenZipCompressedFiles(fileName);
        }


        protected async Task<bool> AllFilesValid(AttachedFile files)
        {
            var invalidFile = false;
            foreach (var f in files.Files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(_pathToSave, fileName);
                if (!CheckExtensions(fileName) || invalidFile == true)
                {
                    if (!invalidFile) invalidFile = true;
                    File.Delete(fullPath);
                }
                if (CheckCompressionFormats(fileName))
                {
                    //copy file to temp repository
                    bool res= await CopyCompressedFileToTempFolder(f, fullPath);
                    //check compressed files types
                    if (!invalidFile && CheckCompressedFiles(fullPath)) invalidFile = true;
                }
            }
            if (invalidFile)
                throw new Exception("some of the attached file(s) have invalid extensions");

            return invalidFile;
        }


    }
}
