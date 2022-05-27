using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class AddedRemovedDetail : IEntityModel
    {
        public string ChangeType { get; set; } = "";
        public string ChangeCategory { get; set; } = "";

        public string FieldName { get; set; } = "";

        [NotMapped]
        public List<CodeAddedRemovedDetail> CodeList { get; set; } = new List<CodeAddedRemovedDetail>();

        public AddedRemovedDetail()
        {
            ChangeCategory = "";
        }


        public AddedRemovedDetail(string? Category)
        {
            ChangeCategory = Category != null ? Category : string.Empty;
        }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeAddedRemovedDetail>();
        }

    }
}
