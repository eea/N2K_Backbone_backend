using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace N2K_BackboneBackEnd.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly IOptions<ConfigSettings> _appSettings;
        private readonly N2KBackboneContext _dataContext;
        

        public DownloadService(IOptions<ConfigSettings> app, N2KBackboneContext dataContext)
        {
            _appSettings = app;
            _dataContext = dataContext;
        }

        private string decode64(string encoded)
        {
            return Base64UrlEncoder.Decode(encoded);
        }

        private bool IsValid(string token)
        {
            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = new JwtSecurityToken(token);
            }
            catch (Exception)
            {
                return false;
            }
            return jwtSecurityToken.ValidTo > DateTime.UtcNow;
        }

        private async Task<byte[]> readfile(string filename)
        {
            IAttachedFileHandler? fileHandler  = _appSettings.Value.AttachedFiles.AzureBlob ?            
                new AzureBlobHandler(_appSettings.Value.AttachedFiles, _dataContext) :
                new FileSystemHandler(_appSettings.Value.AttachedFiles, _dataContext);

            byte[] file_bytes =await fileHandler.ReadFile(filename);
            return file_bytes;
        }

        private async Task<(string?, string?)>  GetFileName(int id, int docuType)
        {
            string filename = string.Empty;
            string outputname = string.Empty;
            if (docuType == 0)
            {
                JustificationFiles temp = await _dataContext.Set<JustificationFiles>().Where(f => f.Id == id).AsNoTracking().FirstOrDefaultAsync();
                filename = temp.Path;
                outputname = temp.OriginalName;
            }
            else if (docuType == 1)
            {
                JustificationFilesRelease temp = await _dataContext.Set<JustificationFilesRelease>().Where(f => f.ID == id).AsNoTracking().FirstOrDefaultAsync();
                filename = temp.Path;
                outputname = temp.OriginalName;
            }


            return (filename, outputname);
        }


        public async Task<FileContentResult> DownloadFile(int id, int docuType)
        {
            (string? filename, string? outputname) = await GetFileName(id, docuType);

            var file_bytes = await readfile(filename);
            return new FileContentResult(file_bytes, "application/octet-stream")
            {
                FileDownloadName = outputname
            };
        }

        public async Task<FileContentResult> DownloadFile(int id, int docuType, string token)
        {
            //validate token
            var handler = new JwtSecurityTokenHandler();
            var decodedValue = handler.ReadJwtToken(decode64(token));

            if (!IsValid(decode64(token)))
                throw new UnauthorizedAccessException("Token has expired!");
            
            (string? filename, string? outputname) = await GetFileName(id, docuType);
            var file_bytes = await readfile(filename);
            return new FileContentResult(file_bytes, "application/octet-stream")
            {
                FileDownloadName = outputname
            };
        }

        public async Task<FileContentResult> DownloadExtractionsFile()
        {
            DirectoryInfo files = new DirectoryInfo("ExtractionFiles");
            FileInfo latest = files.GetFiles("*.zip").OrderBy(f => f.CreationTime).Last();
            IAttachedFileHandler fileHandler = new FileSystemHandler(_appSettings.Value.AttachedFiles, _dataContext);
            //byte[] file_bytes = await fileHandler.ReadFile(latest.FullName);
            byte[] file_bytes = File.ReadAllBytes(latest.FullName);
            return new FileContentResult(file_bytes, "application/octet-stream")
            {
                FileDownloadName = latest.Name
            };
        }
    }
}
