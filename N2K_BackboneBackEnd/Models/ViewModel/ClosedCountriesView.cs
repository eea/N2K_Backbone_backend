using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class ClosedCountriesView : IEntityModel
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public string? Country { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ClosedCountriesView>();
        }
    }
}