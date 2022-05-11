using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class DescribesSites : VersioningBase, IEntityModel
    {
        public string COUNTRYCODE { get; set; }
        public decimal VERSIONID { get; set; }
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public string HABITATCODE { get; set; }
        public decimal? PERCENTAGECOVER { get; set; }
        public int? RID { get; set; }
       

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DescribesSites>()
                .ToTable("DESCRIBESSITES")
                .HasNoKey();
        }
    }
}
