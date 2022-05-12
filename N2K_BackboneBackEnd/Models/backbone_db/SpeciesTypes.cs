using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SpeciesTypes : IEntityModel, IEntityModelBackboneDB
    {
		[Key]
		public string Code { get; set; }
		public string? Name { get; set; }
		public string? NameLat { get; set; }
		public bool? IsBird { get; set; }
		public bool? isCodeNew { get; set; }
		public SpeciesGroup? Group { get; set; }
		public string? Type { get; set; }


		public static void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<SpeciesGroup>()
				.ToTable("SpeciesGroup")
				.HasKey(c => new { c.Code });
		}

	}
}
