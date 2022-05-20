using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class CodeChangeDetail : IEntityModel
    {
        public string Code { get; set; } = "";
        public long ChangeId { get; set; }
        public string ReportedValue { get; set; } = "";
        public string OlValue { get; set; } = "";

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeChangeDetail>();
        }
    }
}
