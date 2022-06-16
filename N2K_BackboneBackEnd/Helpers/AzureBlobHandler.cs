using Azure.Storage.Blobs;
using N2K_BackboneBackEnd.Models;
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


        public async Task<string> UploadFileAsync(AttachedFile file)
        {
            var folderName = _attachedFilesConfig.JustificationFolder;
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            var fileName = ContentDispositionHeaderValue.Parse(file.File.ContentDisposition).FileName.Trim('"');
            var fullPath = Path.Combine(pathToSave, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.File.CopyTo(stream);
            }
            BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
            await blobClient.UploadAsync(fullPath, true);

            File.Delete(fullPath);
            var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/":"");

            return string.Format("{0}{1}/{2}", remoteUrl, folderName, fileName);
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
