using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public class ExtractionService : IExtractionService
    {
        private readonly IOptions<ConfigSettings> _appSettings;
        private readonly N2KBackboneContext _dataContext;

        public ExtractionService(IOptions<ConfigSettings> app, N2KBackboneContext dataContext)
        {
            _appSettings = app;
            _dataContext = dataContext;
        }

        public Task<FileContentResult> DownloadExtractions()
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> UpdateExtractions()
        {
            throw new NotImplementedException();
        }
    }
}

