using Azure.Storage.Blobs;
using N2K_BackboneBackEnd.Models;
using SevenZip;
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
                        BlobClient blobClient1 = ConnectToAzureBlob().GetBlobClient(uncompressedFile);
                        await blobClient1.UploadAsync(Path.Combine(_pathToSave, uncompressedFile), true);
                        remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                        uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, uncompressedFile));

                        File.Delete(Path.Combine(_pathToSave, uncompressedFile));
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
