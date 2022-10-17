using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Enumerations;
using Microsoft.Extensions.Caching.Memory;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteChangesService
    {
        Task<List<SiteChangeDb>> GetSiteChangesAsync(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache,  int page = 1, int pageLimit = 0);

        Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion);

        Task<List<SiteCodeView>> GetReferenceSiteCodes(string country);

        Task<List<SiteCodeView>> GetSiteCodesByStatusAndLevelAndCountry(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, bool refresh = false);

        Task<int> GetPendingChangesByCountry(string? country, IMemoryCache cache);

        Task<List<SiteChangeViewModel>> GetSiteChangesFromSP();

        Task<List<ModifiedSiteCode>> AcceptChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache);

        Task<List<ModifiedSiteCode>> RejectChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache);

        Task<List<ModifiedSiteCode>> MoveToPending(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache);
        
        Task<List<ModifiedSiteCode>> MarkAsJustificationRequired(JustificationModel[] justification);

        Task<List<ModifiedSiteCode>> JustificationProvided(JustificationModel[] justification);
    }
}
