using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public interface ILongRunningService
    {
        Task<int> TestLongRun();
    }
}
