using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SpeciesGroup : IEntityModel, IEntityModelBackboneDB
	{
		[Key]
		public string Code { get; set; }
		public string? Name { get; set; }
		public string? Speciehabitat { get; set; }

		public static void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<SpeciesGroup>()
				.ToTable("SpeciesGroup")
				.HasKey(c => new { c.Code });
		}


	}
}
