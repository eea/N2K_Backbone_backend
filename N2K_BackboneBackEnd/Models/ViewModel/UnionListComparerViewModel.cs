using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [NotMapped]
    public class UnionListValues<T>
    {
        public object? Source { get; set; } = null;
        public object? Target { get; set; } = null;
        public string? Change { get; set; } = null;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UnionListValues<string>>().HasNoKey();
        }
    }

    [NotMapped]
    public class UnionListComparerBioReg : IEntityModelBackboneDB
    {
        public string BioRegion { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    [NotMapped]
    public class UnionListComparerSummaryViewModel : IEntityModelBackboneDB
    {
        public List<BioRegionSiteCode> BioRegSiteCodes { get; set; } = new List<BioRegionSiteCode>();
        public List<UnionListComparerBioReg> BioRegionSummary { get; set; } = new List<UnionListComparerBioReg>();
    }

    [NotMapped]
    public class UnionListComparerCodesViewModel : IEntityModelBackboneDB
    {
        public string BioRegion { get; set; } = string.Empty;
        public string Sitecode { get; set; } = string.Empty;
    }

    [NotMapped]
    [Keyless]
    public class UnionListComparerDetailedViewModel : IEntityModelBackboneDB
    {
        public string BioRegion { get; set; } = string.Empty;
        public string Sitecode { get; set; } = string.Empty;
        public UnionListValues<string>? SiteName { get; set; } = null;
        public UnionListValues<bool>? Priority { get; set; } = null;
        public UnionListValues<double>? Area { get; set; } = null;
        public UnionListValues<double>? Length { get; set; } = null;
        public UnionListValues<double>? Latitude { get; set; } = null;
        public UnionListValues<double>? Longitude { get; set; } = null;
        public string? Changes { get; set; }
    }
}