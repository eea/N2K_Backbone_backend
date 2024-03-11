using Microsoft.AspNetCore.Mvc;

namespace N2K_BackboneBackEnd.Services
{
    public interface IDownloadService
    {
        public Task<ActionResult> DownloadFile(string filename);

        public Task<ActionResult> DownloadFile(string filename, string token);

        public Task<ActionResult> DownloadAsFilename(string filename, string outputname);
                
        public Task<ActionResult> DownloadAsFilename(string filename, string outputname,  string token);
    }
}