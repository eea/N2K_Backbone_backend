using Azure.Storage.Blobs;
using N2K_BackboneBackEnd.Models;
<<<<<<< HEAD
using SevenZip;
=======
using System.Diagnostics;
>>>>>>> 37ef51dec9ee70fc9ea220b2f945a8d53acf0222
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;

namespace N2K_BackboneBackEnd.Helpers
{
    public class AzureBlobHandler : AttachedFileHandler, IAttachedFileHandler
    {

        public AzureBlobHandler(AttachedFilesConfig attachedFilesConfig) : base(attachedFilesConfig)
        {
        }



        private BlobContainerClient ConnectToAzureBlob()
        {
            string connectionString = _attachedFilesConfig.AzureConnectionString;

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            return blobServiceClient.GetBlobContainerClient(_attachedFilesConfig.JustificationFolder);

        }


        public async Task<List<string>> UploadFileAsync(AttachedFile files)
        {
            var remoteUrl = "";
            List<String> uploadedFiles = new List<string>();
            var invalidFile =await AllFilesValid(files);
           
            foreach (var f in files.Files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(_pathToSave, fileName);

                //upload the file to AzureBlob container
                if (CheckCompressionFormats(fileName))
                {
                    //if the file is compressed(extract all the content)
                    List<string> uncompressed= ExtractCompressedFiles(fullPath);

                    foreach (var uncompressedFile in uncompressed)
                    {
<<<<<<< HEAD
                        BlobClient blobClient1 = ConnectToAzureBlob().GetBlobClient(uncompressedFile);
                        await blobClient1.UploadAsync(Path.Combine(_pathToSave, uncompressedFile), true);
                        remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                        uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, uncompressedFile));

                        File.Delete(Path.Combine(_pathToSave, uncompressedFile));
=======
                        await f.CopyToAsync(stream);
                    }
                    if (fileName.EndsWith("zip"))
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(fullPath))
                        {
                            archive.ExtractToDirectory(pathToSave);
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (!CheckExtensions(entry.Name) && !invalidFile)
                                {
                                    invalidFile = true;
                                }
                                File.Delete(Path.Combine(pathToSave, entry.Name));
                            }
                        }
                    }
                    else if (fileName.EndsWith("7z"))
                    {
                        ProcessStartInfo p = new ProcessStartInfo();
                        p.FileName = "7za.exe";
                        p.Arguments = "x \"" + fullPath + "\" -o\"" + pathToSave + "\"";
                        p.WindowStyle = ProcessWindowStyle.Hidden;
                        Process x = Process.Start(p);
                        x.WaitForExit();

                        System.IO.DirectoryInfo di = new DirectoryInfo(pathToSave);
                        foreach (FileInfo file in di.EnumerateFiles())
                        {
                            if (!CheckExtensions(file.Name) && !invalidFile)
                            {
                                invalidFile = true;
                            }
                            file.Delete();
                        }
                    }
                    File.Delete(fullPath);
                }
            }

            if (invalidFile)
                throw new Exception("some of the file(s) attached has invalid extension");

            foreach (var f in files.Files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    f.CopyTo(stream);
                }

                //if the file is compressed (extract all the content)
                if (CheckCompressionFormats(fileName))
                {
                    if (fileName.EndsWith("zip"))
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(fullPath))
                        {
                            archive.ExtractToDirectory(pathToSave);
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                BlobClient blobClient1 = ConnectToAzureBlob().GetBlobClient(entry.Name);
                                await blobClient1.UploadAsync(Path.Combine(pathToSave, entry.Name), true);
                                remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                                uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, folderName, entry.Name));

                                File.Delete(Path.Combine(pathToSave, entry.Name));
                            }
                        }
                    }
                    else if (fileName.EndsWith("7z"))
                    {
                        ProcessStartInfo p = new ProcessStartInfo();
                        p.FileName = "7za.exe";
                        p.Arguments = "x \"" + fullPath + "\" -o\"" + pathToSave + "\"";
                        p.WindowStyle = ProcessWindowStyle.Hidden;
                        Process x = Process.Start(p);
                        x.WaitForExit();

                        System.IO.DirectoryInfo di = new DirectoryInfo(pathToSave);
                        foreach (FileInfo file in di.EnumerateFiles())
                        {
                            BlobClient blobClient1 = ConnectToAzureBlob().GetBlobClient(file.Name);
                            await blobClient1.UploadAsync(Path.Combine(pathToSave, file.Name), true);
                            remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                            uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, folderName, file.Name));

                            file.Delete();
                        }
>>>>>>> 37ef51dec9ee70fc9ea220b2f945a8d53acf0222
                    }
                }
                else
                {
                    BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
                    await blobClient.UploadAsync(fullPath, true);
                    remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                    uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, fileName));
                }
                File.Delete(fullPath);
            }
            return uploadedFiles;

        }


        public async Task<int> DeleteFileAsync(string fileName)
        {
            var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
            var filesUrl = string.Format("{0}{1}", remoteUrl, _attachedFilesConfig.JustificationFolder);
            fileName = fileName.Replace(filesUrl, "");

            BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
            // Upload data from the local file
            await blobClient.DeleteIfExistsAsync();

            return 1;
        }

    }
}
