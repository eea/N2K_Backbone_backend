using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class EnvelopesToHarvest:IEntityModel, IEntityModelBackboneDB
    {
        public string CountryCode { get; set; } = "";

        public string Country { get; set; } = "";
        public int Version { get; set; }
        public DateTime ImportDate { get; set; }

        public bool CanHarvest { get; set; } = false;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EnvelopesToHarvest>()
                .HasNoKey();
        }


    }
}
