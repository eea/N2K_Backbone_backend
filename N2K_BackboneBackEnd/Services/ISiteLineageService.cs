using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using Microsoft.Extensions.Caching.Memory;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteLineageService
    {
        Task<List<SiteLineage>> GetSiteLineageAsync(string siteCode);
    }
}
