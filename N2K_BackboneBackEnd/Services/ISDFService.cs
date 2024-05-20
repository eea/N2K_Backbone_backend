using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISDFService
    {
        Task<SDF> GetExtraData(string SiteCode, int submission);
        Task<SDF> GetData(string SiteCode, int Version = -1);
        Task<ReleaseSDF> GetReleaseData(string SiteCode, int ReleaseId = -1);
    }
}