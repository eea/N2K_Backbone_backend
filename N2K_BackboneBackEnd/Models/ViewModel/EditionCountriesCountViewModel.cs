using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class EditionCountriesCountViewModel : EditionCountriesCount
    {
        public bool IsEditable { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EditionCountriesCountViewModel>();
        }
    }
}
