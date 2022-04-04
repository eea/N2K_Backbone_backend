using N2K_BackboneBackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KBackboneContext : DbContext
    {

        public N2KBackboneContext(DbContextOptions<N2KBackboneContext> options) : base(options)
        {
        }


        public DbSet<SiteChange> SiteChanges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteChange>().ToTable("test_table");
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("N2K_BackboneBackEndContext");
            optionsBuilder.UseSqlServer(connectionString);
        }

    }
}
