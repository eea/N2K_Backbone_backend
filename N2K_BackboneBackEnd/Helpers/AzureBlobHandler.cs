using Azure.Storage.Blobs;
using N2K_BackboneBackEnd.Models;
using System.IO.Compression;
using System.Net.Http.Headers;

namespace N2K_BackboneBackEnd.Helpers
{
    public class AzureBlobHandler : IAttachedFileHandler
    {

        private readonly AttachedFilesConfig _attachedFilesConfig;
        public AzureBlobHandler(AttachedFilesConfig attachedFilesConfig)
        {
            _attachedFilesConfig = attachedFilesConfig;
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
            var folderName = _attachedFilesConfig.JustificationFolder;
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            List<String> uploadedFiles = new List<string>();

            foreach (var f in files.Files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    f.CopyTo(stream);
                }

                //if the file is compressed (extract all the content)
                if (fullPath.EndsWith("zip"))
                {
                    using (ZipArchive archive = ZipFile.OpenRead(fullPath))
                    {
                        archive.ExtractToDirectory(pathToSave);
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {

                            var aaa = 1;
                            BlobClient blobClient1 = ConnectToAzureBlob().GetBlobClient(entry.Name);
                            await blobClient1.UploadAsync(Path.Combine(pathToSave, entry.Name), true);
                        }
                    }

                }
                else
                {
                    BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
                    await blobClient.UploadAsync(fullPath, true);
                }
                //File.Delete(fullPath);
                //var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                //uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, folderName, fileName));
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
