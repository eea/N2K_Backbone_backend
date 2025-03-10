using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISDFService
    {
        Task<SDF> GetExtraData(string SiteCode, int submission);
        Task<SDF> GetData(string SiteCode, int Version = -1);
        Task<ReleaseSDF> GetReleaseData(string SiteCode, int ReleaseId, Boolean initialValidation, Boolean internalViewers, Boolean internalBarometer, Boolean internalPortalSDFSensitive, Boolean publicViewers, Boolean publicBarometer, Boolean sdfPublic, Boolean naturaOnlineList, Boolean productsCreated, Boolean jediDimensionCreated, bool showSensitive = true);
    }
}
