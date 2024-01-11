using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class SiteCodeView : IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = "";
        public int Version { get; set; }

        public string Name { get; set; } = "";

        [NotMapped]
        public string CountryCode { get; set; } = "";
        //public Level Level { get; set; }

        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }

        public LineageTypes? LineageChangeType { get; set; }

        public string? Type { get; set; }

    }
}
