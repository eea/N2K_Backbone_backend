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
    public class LineageEditionInfo : IEntityModelBackboneDB
    {
        public string SiteCode { get; set; } = string.Empty;
        public string SiteName { get; set; } = string.Empty;
        public string SiteType { get; set; } = string.Empty;
        public string? BioRegion { get; set; }
        public double? AreaSDF { get; set; }
        public double? AreaGEO { get; set; }
        public double? Length { get; set; }
        public string? Status { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
