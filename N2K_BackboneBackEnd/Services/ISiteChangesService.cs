using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteChangesService
    {
        Task<List<SiteChangeDb>> GetSiteChangesAsync(string country, SiteChangeStatus? status, Level? level,  int page = 1, int pageLimit = 0);

        Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion);

        Task<List<SiteCodeView>> GetSiteCodesByStatusAndLevelAndCountry(string country,SiteChangeStatus? status, Level? level);
        Task<int> GetPendingChangesByCountry(string? country);


        Task<List<SiteChangeViewModel>> GetSiteChangesFromSP();

        Task<List<ModifiedSiteCode>> AcceptChanges(ModifiedSiteCode[] changedSiteStatus);

        Task<List<ModifiedSiteCode>> RejectChanges(ModifiedSiteCode[] changedSiteStatus);

        Task<List<ModifiedSiteCode>> MoveToPending(ModifiedSiteCode[] changedSiteStatus);

        
        Task<List<ModifiedSiteCode>> MarkAsJustificationRequired(JustificationModel[] justification);

        Task<List<ModifiedSiteCode>> JustificationProvided(JustificationModel[] justification);
        Task<string> SaveSiteChangeEdition(string sitecode, string sitename, string sitetype, string[] biogeographicRegion, float area, float length, float centreX, float centreY);
    }
}
