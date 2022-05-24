using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.versioning_db;

namespace N2K_BackboneBackEnd.Services
{
    public interface IHarvestedService
    {
        Task<List<Harvesting>> GetHarvestedAsync();

        List<Harvesting> GetHarvested();

        Task<Harvesting> GetHarvestedAsyncById(int id);

        Task<List<Harvesting>> GetPendingEnvelopes();

        Task<List<HarvestedEnvelope>> Validate(EnvelopesToProcess[] envelopeIDs);

        Task<List<HarvestedEnvelope>> Harvest(EnvelopesToProcess[] envelopeIDs);

    }
}
