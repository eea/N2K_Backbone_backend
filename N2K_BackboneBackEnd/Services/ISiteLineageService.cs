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

        Task<LineageCount> GetCodesCount(string country, IMemoryCache cache, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true);

        Task<List<long>> ConsolidateChanges(LineageConsolidation[] consolidateChanges);

        Task<List<long>> SetChangesBackToProposed(long[] ChangeId);

        Task<List<LineageEditionInfo>> GetPredecessorsInfo(long ChangeId);

        Task<LineageEditionInfo> GetLineageChangesInfo(long ChangeId);

        Task<List<string>> GetLineageReferenceSites(string country);
    }
}
