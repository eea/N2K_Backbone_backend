using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using Microsoft.Extensions.Caching.Memory;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteChangesService
    {
        Task<List<SiteChangeDbEdition>> GetSiteChangesAsync(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, int page = 1, int pageLimit = 0, bool onlyedited = false, bool onlyjustreq = false, bool onlysci = false);
        Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion);
        Task<List<SiteCodeView>> GetNonPendingSiteCodes(string country, Boolean onlyedited, Boolean onlyjustreq = false, Boolean onlysci = false);
        Task<List<SiteCodeView>> GetSiteCodesByStatusAndLevelAndCountry(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, bool refresh = false, bool onlyedited = false, bool onlyjustreq = false, bool onlysci = false);
        Task<int> GetPendingChangesByCountry(string? country, IMemoryCache cache);
        Task<List<SiteChangeViewModel>> GetSiteChangesFromSP();
        Task<List<ModifiedSiteCode>> AcceptChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache);
        Task<List<ModifiedSiteCode>> AcceptChangesBulk(string sitecodes, IMemoryCache cache);
        Task<List<ModifiedSiteCode>> RejectChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache);
        Task<List<ModifiedSiteCode>> RejectChangesBulk(string sitecodes, IMemoryCache cache);
        Task<List<ModifiedSiteCode>> MoveToPending(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache);
        Task<List<ModifiedSiteCode>> MoveToPendingBulk(string sitecodes, IMemoryCache cache);
        Task<List<ModifiedSiteCode>> MarkAsJustificationRequired(JustificationModel[] justification, IMemoryCache cache);
        Task<List<ModifiedSiteCode>> JustificationProvided(JustificationModel[] justification);
        Task<List<SiteCodeVersion>> GetNoChanges(string country, IMemoryCache cache, int page = 1, int pageLimit = 0, bool refresh = false);
        Task<int> GetPendingVersion(string siteCode);
    }
}
