using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Enumerations;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.ViewModel
{
    [Keyless]
    public class RelatedSites : IEntityModel
    {
        public string? PreviousSiteCode { get; set; }
        public int? PreviousVersion { get; set; }
        public string? NewSiteCode { get; set; }
        public int? NewVersion { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RelatedSites>();
        }
    }
}