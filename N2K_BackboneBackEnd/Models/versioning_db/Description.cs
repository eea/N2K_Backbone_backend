using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class Description : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; } = "";
        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; } = "";
        public int OBJECTID { get; set; }
        public string? QUALITY { get; set; }
        public string? VULNARAB { get; set; }
        public string? DESIGNATION { get; set; }
        public string? MANAG_PLAN { get; set; }
        public string? DOCUMENTATION { get; set; }
        public string? OTHERCHARACT { get; set; }
        public string? MANAG_CONSERV_MEASURES { get; set; }
        public string? MANAG_PLAN_URL { get; set; }
        public string? MANAG_STATUS { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Description>()
                .ToTable("DESCRIPTION")
                .HasNoKey();
        }
    }
}