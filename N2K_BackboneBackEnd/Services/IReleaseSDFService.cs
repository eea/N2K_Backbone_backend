using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public interface IReleaseSDFService
    {
        Task<ReleaseSDF> GetData(string SiteCode, int Version = -1);
    }
}
