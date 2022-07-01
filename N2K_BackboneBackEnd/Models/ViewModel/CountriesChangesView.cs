using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class CountriesChangesView : IEntityModel
    {
        [Key]
        public string Code { get; set; } = "";
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
