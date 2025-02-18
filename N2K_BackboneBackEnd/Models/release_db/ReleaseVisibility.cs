using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.release_db
{
    public class ReleaseVisibility : IEntityModel, IEntityModelReleasesDB
    {
        public long ReleaseID { get; set; }
        public Boolean? InitialValidation { get; set; }
        public Boolean? InternalViewers { get; set; }
        public Boolean? InternalBarometer { get; set; }
        public Boolean? InternalPortalSDFSensitive { get; set; }
        public Boolean? PublicViewers { get; set; }
        public Boolean? PublicBarometer { get; set; }
        public Boolean? SDFPublic { get; set; }
        public Boolean? NaturaOnlineList { get; set; }
        public Boolean? ProductsCreated { get; set; }
        public Boolean? JediDimensionCreated { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReleaseVisibility>()
                .ToTable("ReleaseVisibility")
                .HasKey(c => new { c.ReleaseID });
        }
    }
}