using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteLineageService
    {
        Task<List<SiteLineage>> GetSiteLineageAsync(string siteCode);

        Task<List<LineageChanges>> GetChanges(string country, LineageStatus status, IMemoryCache cache, int page = 1, int pageLimit = 0, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true);

        Task<List<string>> GetLineageReferenceSites(string country);

        Task<List<Lineage>> ConsolidateChanges(int changeId, string type, List<string> predecessors, List<string> successors);

        Task<List<ModifiedSiteCode>> SetChangesBackToPending(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache);
    }
}
