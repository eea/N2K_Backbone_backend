using Azure.Storage.Blobs;

namespace N2K_BackboneBackEnd.Helpers
{
    public class AzureBlobHandler
    {
        private static BlobContainerClient ConnectToAzureBlob()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=n2kbackbonesharedfiles;AccountKey=eL27qbK6uh620SFY8Z/Ij40FEnAog4NfST8r32cBhK8exsK+rAW+82KlTayk1G70RbHrlxb50Dn2+AStsBAZ3g==;EndpointSuffix=core.windows.net";

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            return  blobServiceClient.GetBlobContainerClient("justificationfiles");
                
        }


        public async static Task<int> UploadFileToBlob(string fullPath, string fileName)
        {


            BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);

            // Upload data from the local file
            await blobClient.UploadAsync(fullPath, true);

            return 1;
        }


        public async static Task<int> DeleteFileFromBlob( string fileName)
        {

            var filesUrl = "https://n2kbackbonesharedfiles.blob.core.windows.net/justificationfiles/";
            fileName = fileName.Replace(filesUrl, "");

            BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
            // Upload data from the local file
            await blobClient.DeleteIfExistsAsync();

            return 1;
        }

    }
}
