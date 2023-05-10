using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class ChangeEditionViewModelOriginal : ChangeEditionViewModel, IEntityModel, IEntityModelBackboneDB
    {
        public string? OriginalSiteName { get; set; }
        public string? OriginalSiteType { get; set; }
        public decimal? OriginalArea { get; set; }
        public decimal? OriginalLength { get; set; }
        public decimal? OriginalCentreX { get; set; }
        public decimal? OriginalCentreY { get; set; }
        public List<int>? OriginalBioRegion { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEditionViewModelOriginal>();
        }

    }
}
