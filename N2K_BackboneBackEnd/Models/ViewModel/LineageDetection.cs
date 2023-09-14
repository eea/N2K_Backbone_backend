using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class LineageDetection : IEntityModel
    {
        public string op { get; set; } = string.Empty;
        public string? old_sitecode { get; set; }
        public int? old_version { get; set; }
        public string? new_sitecode { get; set; }
        public int? new_version { get; set; }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<LineageDetection>();
        }
    }
}
