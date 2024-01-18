using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class CategoryChangeDetail : IEntityModel
    {
        public string ChangeType { get; set; } = string.Empty;
        public string ChangeCategory { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        [NotMapped]
        public List<CodeChangeDetail>? ChangedCodesDetail { get; set; } = new List<CodeChangeDetail>();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CategoryChangeDetail>();
        }
    }
}