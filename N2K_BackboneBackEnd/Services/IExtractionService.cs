using Microsoft.AspNetCore.Mvc;

namespace N2K_BackboneBackEnd.Services
{
    public interface IExtractionService
    {
        Task UpdateExtraction();
        Task<FileContentResult> DownloadExtraction();
    }
}
