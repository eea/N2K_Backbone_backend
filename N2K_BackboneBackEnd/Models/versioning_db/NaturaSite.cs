using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    public class NaturaSite : VersioningBase, IEntityModel
    {
        public int _OBJECTID { get; set; }
        public string COUNTRYCODE { get; set; }
        public float VERSIONID { get; set; }
        public float COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public string SITENAME { get; set; }
        public string? COUNTRY_CODE { get; set; }
        public DateTime? DATE_LOADED { get; set; }
        public string? SITETYPE { get; set; }
        public int? ALTITUDE_MIN { get; set; }
        public int? ALTITUDE_MAX { get; set; }
        public int? ALTITUDE_MEAN { get; set; }
        public DateTime? DATE_COMPILATION { get; set; }
        public DateTime? DATE_UPDATE { get; set; }
        public DateTime? DATE_PROP_SCI { get; set; }
        public DateTime? DATE_CONF_SCI { get; set; }
        public DateTime? DATE_SPA { get; set; }
        public DateTime? DATE_SAC { get; set; }
        public float? AREAHA { get; set; }
        public float? LENGTHKM { get; set; }
        public float? LATITUDE { get; set; }
        public float? LONGITUDE { get; set; }
        public int? PUBLISHED { get; set; }
        public int? SENSITIVE { get; set; }
        public string? SPA_LEGAL_REFERENCE { get; set; }
        public string? SAC_LEGAL_REFERENCE { get; set; }
        public string? EXPLANATIONS { get; set; }
        public float? MARINEAREA { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NaturaSite>()
                .ToTable("NATURASITE");
        }

    }
}
