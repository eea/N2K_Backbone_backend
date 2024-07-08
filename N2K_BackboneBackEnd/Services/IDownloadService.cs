using Microsoft.AspNetCore.Mvc;

namespace N2K_BackboneBackEnd.Services
{
    public interface IDownloadService
    {
        Task<FileContentResult> DownloadFile(int id, int docuType);

        Task<FileContentResult> DownloadFile(int id, int docuType, string token);

        Task<FileContentResult> DownloadExtractionsFile();
    }
}
