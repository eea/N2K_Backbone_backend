using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class CountriesChangesView : IEntityModel
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public string? Country { get; set; }
        public int NumInfo { get; set; }
        public int NumWarning { get; set; }
        public int NumCritical { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CountriesChangesView>();
        }
    }
}