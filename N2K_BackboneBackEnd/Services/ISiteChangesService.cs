using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteChangesService
    {
        Task<List<SiteChangeDb>> GetSiteChangesAsync(SiteChangeStatus? status=null );

        Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion);
        

        Task<List<SiteChangeViewModel>> GetSiteChangesFromSP();

        Task<List<ModifiedSiteCode>> AcceptChanges(ModifiedSiteCode[] changedSiteStatus );

        Task<List<ModifiedSiteCode>> RejectChanges(ModifiedSiteCode[] changedSiteStatus);

    }
}
