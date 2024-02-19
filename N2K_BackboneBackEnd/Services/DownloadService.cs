using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace N2K_BackboneBackEnd.Services
{
    public class DownloadService : IDownloadService
    {

        private readonly IOptions<ConfigSettings> _appSettings;

        private string decode64(string encoded)
        {


            return  Base64UrlEncoder.Decode(encoded);

            var decode = System.Convert.FromBase64String(encoded);
            return Encoding.UTF8.GetString(decode);
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
            string _folderName = _appSettings.Value.AttachedFiles.JustificationFolder;
            string path = Path.Combine(_folderName, filename);

            //_folderName = @"C:\Proyectos\N2kBackbone\Code";
            //filename = "sites_par.csv";
            path = Path.Combine(_folderName, filename);

            var file_bytes = await System.IO.File.ReadAllBytesAsync(path);
            return file_bytes;
        }


        public DownloadService(IOptions<ConfigSettings> app)
        {
            _appSettings = app;
        }


        public async Task<ActionResult> DownloadAsFilename(string filename, string outputname)
        {
            var file_bytes = await readfile(filename);
            return new FileContentResult(file_bytes, "application/octet-stream")
            {
                FileDownloadName = decode64(outputname)
            };
        }

        public async Task<ActionResult> DownloadAsFilename(string filename, string outputname, string token)
        {
            //validate token


            var handler = new JwtSecurityTokenHandler();
            var decodedValue = handler.ReadJwtToken(decode64(token));

            if (!IsValid(token))
                throw new UnauthorizedAccessException("Token has expired!");


            var file_bytes = await readfile(decode64(filename));
            return new FileContentResult(file_bytes, "application/octet-stream")
            {
                FileDownloadName = decode64(outputname)
            };
        }

        public async Task<ActionResult> DownloadFile(string filename)
        {

            var file_bytes = await readfile(decode64(filename));
            return new FileContentResult(file_bytes, "application/octet-stream")
            {
                FileDownloadName = decode64(filename)
            };
        }

        public async Task<ActionResult> DownloadFile(string filename, string token)
        {
            //validate token

            var handler = new JwtSecurityTokenHandler();
            var decodedValue = handler.ReadJwtToken(decode64(token));

            if (!IsValid(token))
                throw new UnauthorizedAccessException("Token has expired!" );


            var file_bytes = await readfile(decode64(filename));
            return new FileContentResult(file_bytes, "application/octet-stream")
            {
                FileDownloadName = filename
            };
        }
    }
}
