using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.Data;
using NuGet.Protocol;
using N2K_BackboneBackEnd.Helpers;
using System.Security.Policy;
using System.Collections.Generic;

namespace N2K_BackboneBackEnd.Services
{


    public class SiteLineageService : ISiteLineageService
    {
        private readonly N2KBackboneContext _dataContext;


        public SiteLineageService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
        }


        public async Task<List<SiteLineage>> GetSiteLineageAsync(string siteCode)
        {
            List<SiteLineage> result;
            return result;
        }

        

    }
}
