using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models
{
    [Keyless]
    public class LineageCountry : IEntityModel
    {
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public int Creation { get; set; }
        public int Deletion { get; set; }
        public int Split { get; set; }
        public int Merge { get; set; }
        public int Recode { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LineageCountry>();
        }
    }
}
