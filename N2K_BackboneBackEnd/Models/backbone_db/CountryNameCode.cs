using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class CountryNameCode : IEntityModel
    {
        public string? Country { get; set; }
        public string? Code { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CountryNameCode>()
                .ToTable("COUNTRYNAMECODE")
                .HasKey(c => new {c.Code});
        }
    }
}
