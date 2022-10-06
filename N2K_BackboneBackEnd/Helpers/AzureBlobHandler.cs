using Azure.Storage.Blobs;
using N2K_BackboneBackEnd.Models;
using System.Net.Http.Headers;


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
            if (files == null || files.Files == null) return uploadedFiles;

            var invalidFile =await AllFilesValid(files);
                       
            foreach (var f in files.Files)
            {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                var fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                var fullPath = Path.Combine(_pathToSave, fileName);


                //if the file is compressed (extract all the content)
                if (CheckCompressionFormats(fileName))
                {
                    List<string> uncompressedFiles = ExtractCompressedFiles(fullPath);
                    foreach (var uncompressed in uncompressedFiles)
                    {
                        BlobClient blobClient1 = ConnectToAzureBlob().GetBlobClient(uncompressed);
                        await blobClient1.UploadAsync(Path.Combine(_pathToSave, uncompressed), true);
                        remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                        uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, uncompressed));

                        File.Delete(Path.Combine(_pathToSave, uncompressed));
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
