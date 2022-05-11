using N2K_BackboneBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;
using N2K_BackboneBackEnd.Models.versioning_db;

namespace N2K_BackboneBackEnd.Data
{
    public class N2K_VersioningContext : BaseContext
    {
        public DbSet<OwnerType> OwnerType { get; set; }



        public N2K_VersioningContext(DbContextOptions<N2K_VersioningContext> options) : base(options) {
          

        }

    }
}
