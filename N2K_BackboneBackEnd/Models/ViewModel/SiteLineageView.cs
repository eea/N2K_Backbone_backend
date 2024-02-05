using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class SiteLineageView : IEntityModel
    {
        public string? SiteCode { get; set; }
        public string? Release { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteLineageView>();
        }
    }
}