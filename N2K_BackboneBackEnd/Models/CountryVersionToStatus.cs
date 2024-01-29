using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models
{
    public class CountryVersionToStatus
    {
        public CountryVersion[] countryVersion { get; set; }
        public HarvestingStatus toStatus { get; set; }
    }
}