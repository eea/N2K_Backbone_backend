using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class EditionCountriesCount : IEntityModel
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int SiteCount { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EditionCountriesCount>();
        }
    }
}