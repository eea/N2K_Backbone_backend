using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;

namespace N2K_BackboneBackEnd.Models.ViewModel
{

    [Keyless]
    public class CodeAddedRemovedDetail : IEntityModel
    {
        public long ChangeId { get; set; }

        public string? Code { get; set; } = "";

        public Dictionary<string, string>? CodeValues { get; set; } = new Dictionary<string, string>();


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeAddedRemovedDetail>();
        }

    }
}
