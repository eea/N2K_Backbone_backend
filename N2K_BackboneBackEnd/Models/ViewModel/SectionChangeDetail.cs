using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{

    [Keyless]
    public class SectionChangeDetail : IEntityModel
    {
        [NotMapped]
        public List<CategoryChangeDetail> ChangesByCategory { get; set; } = new List<CategoryChangeDetail>();
        [NotMapped]
        public AddedRemovedDetail AddedCodes { get; set; } = new AddedRemovedDetail();
        [NotMapped]
        public AddedRemovedDetail DeletedCodes { get; set; } = new AddedRemovedDetail();


        public SectionChangeDetail()
        {
            ChangesByCategory = new List<CategoryChangeDetail>();
            AddedCodes = new AddedRemovedDetail();
            DeletedCodes = new AddedRemovedDetail();
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SectionChangeDetail>();
        }

    }
}
