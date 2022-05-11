using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;

namespace N2K_BackboneBackEnd.Services
{
    public interface IHarvestedService
    {
        Task<List<Harvesting>> GetHarvestedAsync();

        List<Harvesting> GetHarvested();

        Task<Harvesting> GetHarvestedAsyncById(int id);

        Task<List<Harvesting>> GetPendingEnvelopes();


        Task<List<HarvestedEnvelope>> Harvest(EnvelopesToProcess[] envelopeIDs);

        Task<List<HarvestedEnvelope>> Start(EnvelopesToProcess[] envelopeIDs);
    }
}
