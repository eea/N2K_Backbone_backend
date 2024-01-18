using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class SectionChangeDetail : IEntityModel
    {
        [NotMapped]
        public List<CategoryChangeDetail> ChangesByCategory { get; set; } = new List<CategoryChangeDetail>();
        [NotMapped]
        public List<CategoryChangeDetail> AddedCodes { get; set; } = new List<CategoryChangeDetail>();
        [NotMapped]
        public List<CategoryChangeDetail> DeletedCodes { get; set; } = new List<CategoryChangeDetail>();

        public SectionChangeDetail()
        {
            ChangesByCategory = new List<CategoryChangeDetail>();
            AddedCodes = new List<CategoryChangeDetail>();
            DeletedCodes = new List<CategoryChangeDetail>();
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SectionChangeDetail>();
        }
    }
}