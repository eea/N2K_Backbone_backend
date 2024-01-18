using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class Contact : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; } = String.Empty;
        [Column(TypeName = "decimal(18, 0)")]
        public decimal VERSIONID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal COUNTRYVERSIONID { get; set; }
        public Int32 OBJECTID { get; set; }
        public Int32 CONTACTID { get; set; }
        public string? ORG_NAME { get; set; }
        public string? STREET { get; set; }
        public string? BLDG_NUM { get; set; }
        public string? POSTCODE { get; set; }
        public string? LOCALITY { get; set; }
        public string? STATE { get; set; }
        public string? COUNTRY { get; set; }
        public string? ADMIN_UNIT { get; set; }
        public string? LOCATOR_DESIGNATOR { get; set; }
        public string? LOCATOR_NAME { get; set; }
        public string? ADDRESS_AREA { get; set; }
        public string? POST_NAME { get; set; }
        public string? THOROUGHFARE { get; set; }
        public string? CONTACT_NAME { get; set; }
        public string? TEL { get; set; }
        public string? FAX { get; set; }
        public string? EMAIL { get; set; }
        public string? UNSTRUCTURED_ADD { get; set; }
        public string SITECODE { get; set; } = String.Empty;

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Contact>()
                .ToTable("CONTACT")
                .HasNoKey();
        }
    }
}