using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.Collections.Concurrent;

namespace N2K_BackboneBackEnd.Services
{
    public interface IHarvestedService
    {
        Task<List<Harvesting>> GetHarvestedAsync();

        List<Harvesting> GetHarvested();

        Task<Harvesting> GetHarvestedAsyncById(int id);

        Task<List<Harvesting>> GetPendingEnvelopes();

        Task<List<HarvestingExpanded>> GetEnvelopesByStatus(HarvestingStatus status);
        Task<List<HarvestingExpanded>> GetOnlyClosedEnvelopes();

        Task<List<EnvelopesToHarvest>> GetPreHarvestedEnvelopes();

        Task<List<HarvestedEnvelope>> ChangeDetection(EnvelopesToProcess[] envelopeIDs, N2KBackboneContext? ctx=null);

        Task<List<HarvestedEnvelope>> ChangeDetectionSingleSite(string siteCode, int versionId, string connectionString);

        Task<List<HarvestedEnvelope>> Harvest(EnvelopesToProcess[] envelopeIDs);

        Task HarvestSpatialData(EnvelopesToProcess[] envelopeIDs, IMemoryCache cache);

        Task<List<HarvestedEnvelope>> FullHarvest(IMemoryCache cache);
        Task<List<ProcessedEnvelopes>> ChangeStatus(CountryVersionToStatus envelopesToStatus, IMemoryCache cache);
        Task CompleteFMESpatial(string message);

    }
}
