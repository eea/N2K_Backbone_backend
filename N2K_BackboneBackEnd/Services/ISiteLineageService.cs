using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteLineageService
    {
        Task<List<SiteLineage>> GetSiteLineageAsync(string siteCode);

        Task<List<LineageChanges>> GetChanges(string country, LineageStatus status, IMemoryCache cache, int page = 1, int pageLimit = 0, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true);

        Task<List<LineageConsolidate>> ConsolidateChanges( List<LineageConsolidate> consolidateChanges);

        Task<List<Lineage>> SetChangesBackToPropose(List<Lineage> ChangeId);
    }
}
