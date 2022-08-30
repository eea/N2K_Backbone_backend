using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class SitesWithChangesView : IEntityModel
    {
        [Key]
        public string Country { get; set; }
        public int ModifiedSites { get; set; }
        public string Level { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SitesWithChangesView>();
        }
    }
}
