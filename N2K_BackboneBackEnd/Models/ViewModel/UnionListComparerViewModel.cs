using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class UnionListComparerViewModel : IEntityModel
    {
        public string BioRegion { get; set; } = string.Empty;
        public string Sitecode { get; set; } = string.Empty;
        public string? SitenameSourceValue { get; set; }
        public string? SitenameTargetValue { get; set; }
        public Boolean? PrioritySourceValue { get; set; }
        public Boolean? PriorityTargetValue { get; set; }
        public double? AreaSourceValue { get; set; }
        public double? AreaTargetValue { get; set; }
        public double? LengthSourceValue { get; set; }
        public double? LengthTargetValue { get; set; }
        public double? LatitudeSourceValue { get; set; }
        public double? LatitudeTargetValue { get; set; }
        public double? LongitudeSourceValue { get; set; }
        public double? LongitudeTargetValue { get; set; }

        public List<string> Changes { get; set; } = new List<string>();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UnionListComparerViewModel>().HasNoKey();
        }
    }
}
