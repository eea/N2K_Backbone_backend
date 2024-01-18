using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class EditionCountriesView : IEntityModel
    {
        [Key]
        public string Code { get; set; } = "";
        public string Country { get; set; } = "";
        public int SiteCount { get; set;}
        public bool IsEditable { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EditionCountriesView>();
        }
    }
}
