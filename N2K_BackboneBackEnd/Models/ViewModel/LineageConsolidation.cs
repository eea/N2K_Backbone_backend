using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class LineageConsolidation : IEntityModel
    {
        public long ChangeId { get; set; }
        public LineageTypes Type { get; set; }
        public string? Predecessors { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LineageConsolidation>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<Enumerations.LineageTypes>());
        }
    }
}