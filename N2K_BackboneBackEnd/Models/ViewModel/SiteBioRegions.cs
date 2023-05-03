using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class SiteBioRegions : IEntityModel, IEntityModelBackboneDB
    {
        public string SiteCode { get; set; }
        public int Version { get; set; }
        public string BioRegions { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteBioRegions>();
        }
    }
}
