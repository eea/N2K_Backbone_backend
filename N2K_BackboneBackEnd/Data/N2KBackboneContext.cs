using N2K_BackboneBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KBackboneContext : DbContext
    {
#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
        public N2KBackboneContext(DbContextOptions<N2KBackboneContext> options) : base(options)
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
        { }

        public DbSet<SiteChange> SiteChanges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteChange>()
                .ToTable("test_table")
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<Enumerations.Status>());

            modelBuilder.Entity<SiteChange>()
                .ToTable("test_table")
                .Property(e => e.Level)
                .HasConversion(new EnumToStringConverter<Enumerations.Level>());

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
