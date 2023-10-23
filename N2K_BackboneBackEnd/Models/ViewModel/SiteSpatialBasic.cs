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
    public class SiteSpatialBasic : IEntityModel
    {
        public string SiteCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public Boolean? data { get; set; }
        public decimal? area { get; set; }
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteSpatialBasic>();
        }
    }
}