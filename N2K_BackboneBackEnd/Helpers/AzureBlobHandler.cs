using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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

            BlobServiceClient blobServiceClient = new(connectionString);

            // Create the container and return a container client object
            return blobServiceClient.GetBlobContainerClient(_attachedFilesConfig.JustificationFolder);
        }

        public async Task<List<string>> UploadFileAsync(AttachedFile files)
        {
            string remoteUrl = "";
            List<String> uploadedFiles = new();
            if (files == null || files.Files == null) return uploadedFiles;

            bool invalidFile = await AllFilesValid(files);

            foreach (var f in files.Files)
            {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                var fileName = (ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"') + DateTime.Now).GetHashCode().ToString();
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

        public async Task<List<string>> UploadFileAsync(string file)
        {
            string remoteUrl = "";
            List<String> uploadedFiles = new();
            if (String.IsNullOrEmpty(file))
                return uploadedFiles;
            var fileName = Path.GetFileName(file);
            var fullPath = Path.Combine(_pathToSave, fileName);

            BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
            await blobClient.UploadAsync(fullPath, true);
            remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
            uploadedFiles.Add(string.Format("{0}{1}/{2}", remoteUrl, _folderName, fileName));

            File.Delete(file);

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

        public async Task<int> DeleteUnionListsFilesAsync()
        {
            BlobContainerClient blobContainerClient = ConnectToAzureBlob();
            var remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
            string filesUrl = string.Format("{0}{1}", remoteUrl, _attachedFilesConfig.JustificationFolder);

            blobContainerClient.GetBlobs();
            foreach (BlobItem blob in blobContainerClient.GetBlobs())
            {
                if (blob.Name.EndsWith("_Union List.zip"))
                {
                    await blobContainerClient.GetBlobClient(blob.Name).DeleteIfExistsAsync();
                }
            }
            return 1;
        }
    }
}