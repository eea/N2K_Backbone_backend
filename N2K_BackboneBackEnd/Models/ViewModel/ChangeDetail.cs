using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class ChangeDetail : IEntityModel
    {
        public long ChangeId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public string ChangeCategory { get; set; } = string.Empty;
        public string ReportedValue { get; set; } = string.Empty;
        public string OlValue { get; set; } = string.Empty;
        public Level? Level { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeDetail>()
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());
        }
    }
}