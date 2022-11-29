using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class Respondents : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long ID { get; }
        public string? SiteCode { get; set; }
        public int Version { get; set; }
        public string? locatorName { get; set; }
        public string? addressArea { get; set; }
        public string? postName { get; set; }
        public string? postCode { get; set; }
        public string? thoroughfare { get; set; }
        public string? addressUnstructured { get; set; }
        public string? name { get; set; }
        public string? Email { get; set; }
        public string? AdminUnit { get; set; }
        public string? LocatorDesignator { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Respondents>()
                .ToTable("Respondents")
                .HasKey("ID");

        }
       
    }
}
