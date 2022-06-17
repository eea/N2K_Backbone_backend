using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Models.backbone_db
{

    public class CountryChangeDb : IEntityModel, IEntityModelBackboneDB
    {

        // TODO
        public string Country { get; set; }

        public string Code { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            // TODO
            builder.Entity<CountryChangeDb>()
                .ToTable("Changes");
        }
    }
}
