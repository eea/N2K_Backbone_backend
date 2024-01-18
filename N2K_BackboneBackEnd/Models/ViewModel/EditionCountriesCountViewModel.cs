using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class EditionCountriesCountViewModel : EditionCountriesCount, IEntityModel
    {
        public bool IsEditable { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EditionCountriesCountViewModel>();
        }
    }
}