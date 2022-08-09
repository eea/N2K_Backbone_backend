using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class ChangeEditionViewModel : ChangeEdition, IEntityModel, IEntityModelBackboneDB
    {
        public List<int> BioRegion { get; set; } = new List<int>();
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ChangeEditionViewModel>();
        }

    }
}
