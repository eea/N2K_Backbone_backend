using Microsoft.AspNetCore.Mvc;

namespace N2K_BackboneBackEnd.Services
{
    public interface IExtractionService
    {
        Task<ActionResult> UpdateExtractions();
        Task<FileContentResult> DownloadExtractions();
    }
}
