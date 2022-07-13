using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Models.backbone_db
{

    public class SiteChangeDb : IEntityModel, IEntityModelBackboneDB
    {
      
        [Key]
        public long ChangeId { get; set; }

        public string SiteCode { get; set; } = String.Empty;
        [NotMapped]
        public string SiteName { get; set; } = String.Empty;
        public int Version { get; set; }
        public string? Country { get; set; }

        public SiteChangeStatus? Status { get; set; }

        public string? Tags { get; set; }

        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }


        [NotMapped]
        public int NumChanges { get; set; }

        public string? NewValue { get; set; }
        public string? OldValue { get; set; }

        public string? Detail { get; set; }

        public string? Code { get; set; }
        public string? Section { get; set; }
        public int VersionReferenceId { get; set; }
        public string? FieldName { get; set; }
        public string ReferenceSiteCode { get; set; } = String.Empty;
        [NotMapped]
        public bool? JustificationRequired { get; set;  }
        [NotMapped]
        public bool? JustificationProvided { get; set; }

        [NotMapped]
        public bool? HasGeometry { get; set; }



        public List<SiteChangeView> subRows { get; set; } = new List<SiteChangeView>();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDb>()
                .ToTable("Changes")
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeDb>()
                .ToTable("Changes")
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());


        }
    }


    public class SiteChangeDbNumsperLevel :  IEntityModel, IEntityModelBackboneDB
    {

        public long ChangeId { get; set; }

        public string SiteCode { get; set; } = String.Empty;
        public string SiteName { get; set; } = String.Empty;
        public int Version { get; set; }
        public string? Country { get; set; }

        public SiteChangeStatus? Status { get; set; }

        public string? Tags { get; set; }

        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }

        [NotMapped]
        public int NumChanges { get; set; }

        public string? NewValue { get; set; }
        public string? OldValue { get; set; }

        public string? Detail { get; set; }

        public string? Code { get; set; }
        public string? Section { get; set; }
        public int VersionReferenceId { get; set; }
        public string? FieldName { get; set; }
        public string ReferenceSiteCode { get; set; } = String.Empty;

        public bool? HasGeometry { get; set; }


        public bool? JustificationRequired { get; set; }
        public bool? JustificationProvided { get; set; }


        public List<SiteChangeView> subRows { get; set; } = new List<SiteChangeView>();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangeDbNumsperLevel>()
                .HasNoKey()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.SiteChangeStatus>());

            builder.Entity<SiteChangeDbNumsperLevel>()
                .HasNoKey()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());

        }


    }

}
