using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    public class CountriesSiteCountView : IEntityModel
    {
        [Key]
        public string? Code { get; set; }
        public string? Country { get; set; }
        public int NumAccepted { get; set; }
        public int NumPending { get; set; }
        public int NumRejected { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CountriesSiteCountView>();
        }
    }
}