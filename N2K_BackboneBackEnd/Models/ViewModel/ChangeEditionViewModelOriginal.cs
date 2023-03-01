using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class ChangeEditionViewModelOriginal : ChangeEditionViewModel, IEntityModel, IEntityModelBackboneDB
    {
        public string? OriginalSiteName { get; set; }
        public string? OriginalSiteType { get; set; }
        public double? OriginalArea { get; set; }
        public double? OriginalLength { get; set; }
        public double? OriginalCentreX { get; set; }
        public double? OriginalCentreY { get; set; }
        public List<int>? OriginalBioRegion { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEditionViewModelOriginal>();
        }

    }
}
