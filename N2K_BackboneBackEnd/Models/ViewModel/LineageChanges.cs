using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class LineageChanges : IEntityModel
    {
        public long ChangeId { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public LineageTypes? Type { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Reported { get; set; } = string.Empty;


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LineageChanges>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<Enumerations.LineageTypes>());
        }
    }
}
