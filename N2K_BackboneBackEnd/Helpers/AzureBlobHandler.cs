using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using System;
using System.Drawing;
using System.Net.Http.Headers;


namespace N2K_BackboneBackEnd.Helpers
{
    public class AzureBlobHandler : AttachedFileHandler, IAttachedFileHandler
    {
        public AzureBlobHandler(AttachedFilesConfig attachedFilesConfig, N2KBackboneContext dataContext) : base(attachedFilesConfig, dataContext)
        {
        }

        private BlobContainerClient ConnectToAzureBlob()
        {
            string connectionString = _attachedFilesConfig.AzureConnectionString;

            BlobServiceClient blobServiceClient = new(connectionString);

            // Create the container and return a container client object
            return blobServiceClient.GetBlobContainerClient(_attachedFilesConfig.JustificationFolder);
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
                    //string? fileName = (ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"') + DateTime.Now).GetHashCode().ToString();
                    string? fileName = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim('"');
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                    string? fullPath = Path.Combine(_pathToSave, fileName);

                    //if the file is compressed (extract all the content)
                    if (CheckCompressionFormats(fileName))
                    {
                        List<string> uncompressedFiles = ExtractCompressedFiles(fullPath);
                        foreach (var uncompressed in uncompressedFiles)
                        {
                            JustificationFiles item = new JustificationFiles();
                            FileInfo fi = new FileInfo(Path.Combine(_pathToSave, uncompressed));
                            string newfilename= string.Format("{0}_{1}{2}", System.Guid.NewGuid().ToString(), DateTime.Now.Ticks, fi.Extension);
                            BlobClient blobClient1 = ConnectToAzureBlob().GetBlobClient(newfilename);
                            await blobClient1.UploadAsync(Path.Combine(_pathToSave, uncompressed), true);
                            remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                            item.Path = newfilename;
                            item.OriginalName = uncompressed;
                            uploadedFiles.Add(item);
                            File.Delete(Path.Combine(_pathToSave, uncompressed));
                        }
                    }
                    else
                    {
                        JustificationFiles item = new JustificationFiles();
                        FileInfo fi = new FileInfo(fullPath);
                        string newfilename = string.Format("{0}_{1}{2}", System.Guid.NewGuid().ToString(), DateTime.Now.Ticks, fi.Extension);
                        BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
                        await blobClient.UploadAsync(fullPath, true);
                        remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                        item.Path = newfilename;
                        item.OriginalName = fileName;
                        uploadedFiles.Add(item);
                    }
                    File.Delete(fullPath);
                }
                return uploadedFiles;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "AzureBlobHandler - UploadFileAsync(AttachedFile)", "", _dataContext.Database.GetConnectionString());
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
                string? fullPath = Path.Combine(_pathToSave, fileName);

                BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
                await blobClient.UploadAsync(fullPath, true);
                remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                uploadedFiles.Add(fileName);

                File.Delete(file);

                return uploadedFiles;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "AzureBlobHandler - UploadFileAsync(string)", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<int> DeleteFileAsync(string fileName)
        {
            try
            {
                string remoteUrl = _attachedFilesConfig.PublicFilesUrl + (!_attachedFilesConfig.PublicFilesUrl.EndsWith("/") ? "/" : "");
                string filesUrl = string.Format("{0}{1}", remoteUrl, _attachedFilesConfig.JustificationFolder);
                fileName = fileName.Replace(filesUrl, "");

                BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "AzureBlobHandler - DeleteFileAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<int> DeleteUnionListsFilesAsync()
        {
            try
            {
                BlobContainerClient blobContainerClient = ConnectToAzureBlob();

                blobContainerClient.GetBlobs();
                foreach (BlobItem blob in blobContainerClient.GetBlobs())
                {
                    if (blob.Name.EndsWith("_Union List.zip"))
                        await blobContainerClient.GetBlobClient(blob.Name).DeleteIfExistsAsync();
                }
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "AzureBlobHandler - DeleteUnionListsFilesAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<byte[]> ReadFile(string fileName)
        {

            try
            {
                BlobClient blobClient = ConnectToAzureBlob().GetBlobClient(fileName);
                if (await blobClient.ExistsAsync())
                {
                    using (var ms = new MemoryStream())
                    {
                        blobClient.DownloadTo(ms);
                        return ms.ToArray();
                    }
                }
                return new byte[0];  // returns empty array
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "AzureBlobHandler - ReadFile", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}