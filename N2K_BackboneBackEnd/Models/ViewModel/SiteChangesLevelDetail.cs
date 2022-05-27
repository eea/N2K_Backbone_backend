using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class SiteChangesLevelDetail
    {
        [NotMapped]
        public Dictionary<string, SectionChangeDetail> SectionChanges { get; set; }= new Dictionary<string, SectionChangeDetail>();


        [NotMapped]
        public SectionChangeDetail SiteInfo { get; set; } = new SectionChangeDetail();
        [NotMapped]
        public SectionChangeDetail  Species { get; set; } = new SectionChangeDetail();
        [NotMapped]
        public SectionChangeDetail Habitats { get; set; } = new SectionChangeDetail();

        public Level? Level { get; set; } = Enumerations.Level.Info;


        public SiteChangesLevelDetail()
        {
            SectionChanges.Add("SiteInfo", new SectionChangeDetail());
            SectionChanges.Add("Species", new SectionChangeDetail());
            SectionChanges.Add("Habitats", new SectionChangeDetail());

        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteChangesLevelDetail>()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }
    }



}
