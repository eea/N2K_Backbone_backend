using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class ChangeEditionViewModelOriginalExtended : ChangeEditionViewModelOriginal, IEntityModel, IEntityModelBackboneDB
    {
        public DateTime? ReleaseDate { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEditionViewModelOriginal>();
        }
    }
}