using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models
{
    public class HarvestedEnvelope
    {
        public int VersionId { get; set; }
        public string CountryCode { get; set; } = "";
        public int NumChanges { get; set; }

        public HarvestingStatus Status { get; set; }
    }
}
