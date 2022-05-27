using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{

   

    [Keyless]
    public class CodeAddedRemovedDetail : IEntityModel
    {

        public long ChangeId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";

        [NotMapped]
        public List<CodeChangeDetailedValues> DetailedValues { get; set; } = new List<CodeChangeDetailedValues>();


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeAddedRemovedDetail>();
        }

    }
}
