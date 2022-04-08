using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public interface IHarvestedService
    {
        Task<List<Harvesting>> GetHarvestedAsync();

        List<Harvesting> GetHarvested();

        Task<Harvesting> GetHarvestedAsyncById(int id);

    }
}
