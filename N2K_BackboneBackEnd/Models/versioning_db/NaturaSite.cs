using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class NaturaSite : VersioningBase, IEntityModel
    {
        public Int32 OBJECTID { get; set; }
        public string COUNTRYCODE { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public string SITENAME { get; set; }
        public string? COUNTRY_CODE { get; set; }
        public DateTime? DATE_LOADED { get; set; }
        public string? SITETYPE { get; set; }
        public Int32? ALTITUDE_MIN { get; set; }
        public Int32? ALTITUDE_MAX { get; set; }
        public Int32? ALTITUDE_MEAN { get; set; }
        public DateTime? DATE_COMPILATION { get; set; }
        public DateTime? DATE_UPDATE { get; set; }
        public DateTime? DATE_PROP_SCI { get; set; }
        public DateTime? DATE_CONF_SCI { get; set; }
        public DateTime? DATE_SPA { get; set; }
        public DateTime? DATE_SAC { get; set; }

        [Column(TypeName = "decimal(38, 4)")]
        public decimal? AREAHA { get; set; }
        [Column(TypeName = "decimal(38, 2)")]
        public decimal? LENGTHKM { get; set; }
        [Column(TypeName = "decimal(38, 6)")]
        public decimal? LATITUDE { get; set; }
        [Column(TypeName = "decimal(38, 6)")]
        public decimal? LONGITUDE { get; set; }
        public Int16? PUBLISHED { get; set; }
        public Int16? SENSITIVE { get; set; }
        public string? SPA_LEGAL_REFERENCE { get; set; }
        public string? SAC_LEGAL_REFERENCE { get; set; }
        public string? EXPLANATIONS { get; set; }
        [Column(TypeName = "decimal(38, 4)")]
        public decimal? MARINEAREA { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NaturaSite>()
                .ToTable("NATURASITE")
                .HasNoKey();
        }

    }
}
