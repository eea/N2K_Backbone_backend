using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{

   

    [Keyless]
    public class CodeAddedRemovedDetail : IEntityModel
    {

        public long ChangeId { get; set; }
        public string Code { get; set; } = "";

        public Dictionary<string, string>? CodeValues { get; set; } = new Dictionary<string, string>();


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeAddedRemovedDetail>();
        }

    }
}
