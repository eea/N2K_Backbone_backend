using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;

namespace N2K_BackboneBackEnd.Services
{
    public interface IHarvestedService
    {
        Task<List<Harvesting>> GetHarvestedAsync();

        List<Harvesting> GetHarvested();

        Task<Harvesting> GetHarvestedAsyncById(int id);

        Task<List<Harvesting>> GetPendingEnvelopes();

        Task<List<HarvestingExpanded>> GetEnvelopesByStatus(HarvestingStatus status);

        Task<List<EnvelopesToHarvest>> GetPreHarvestedEnvelopes();

        Task<List<HarvestedEnvelope>> Validate(EnvelopesToProcess[] envelopeIDs);

        Task<List<HarvestedEnvelope>> ValidateSingleSite(string siteCode, int versionId);

        Task<List<HarvestedEnvelope>> ValidateSingleSiteObject(SiteToHarvest harvestingSite);

        Task<List<HarvestedEnvelope>> Harvest(EnvelopesToProcess[] envelopeIDs);

        Task<List<HarvestedEnvelope>> FullHarvest();
        Task<ProcessedEnvelopes> ChangeStatus(string country, int version, HarvestingStatus toStatus);
    }
}
