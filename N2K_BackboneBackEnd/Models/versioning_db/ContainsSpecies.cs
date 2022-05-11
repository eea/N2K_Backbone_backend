using Microsoft.EntityFrameworkCore;
namespace N2K_BackboneBackEnd.Models.versioning_db
{
    [Keyless]
    public class ContainsSpecies : VersioningBase, IEntityModel
    {
        public int RID { get; set; }
        public string COUNTRYCODE { get; set; }
        public decimal VERSIONID { get; set; }
        public decimal COUNTRYVERSIONID { get; set; }
        public string SITECODE { get; set; }
        public string? SPECIESCODE { get; set; }
        public string? SPECIESNAME { get; set; } 
        public string? SPECIESNAMECLEAN { get; set; }
        public string? POPULATION { get; set; }
        public string? CONSERVATION { get; set; }
        public string? ISOLATIONFACTOR { get; set; }
        public string? GLOBALIMPORTANCE { get; set; }
        public string? RESIDENT { get; set; }
        public string? BREEDING { get; set; }
        public string? WINTER { get; set; }
        public string? STAGING { get; set; }
        public string? MOTIVATION { get; set; }
        public DateTime? STARTDATE { get; set; }
        public DateTime? ENDDATE { get; set; }
        public Int16? OTHERSPECIES { get; set; }
        public Int16? SENSITIVE { get; set; }
        public Int16? NONPRESENCEINSITE { get; set; }
        public string? LOWERBOUND { get; set; }
        public string? UPPERBOUND { get; set; }
        public string? COUNTINGUNIT { get; set; }
        public string? ABUNDANCECATEGORY { get; set; }
        public string? GLOBALASSESMENT { get; set; }
        public string? DATAQUALITY { get; set; }
        public string? SPTYPE { get; set; }
        public string? POPULATION_TYPE { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ContainsSpecies>()
                .ToTable("CONTAINSSPECIES")
                .HasNoKey();
        }
    }
}
