using N2K_BackboneBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KBackboneContext : DbContext
    {
#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
        public N2KBackboneContext(DbContextOptions<N2KBackboneContext> options) : base(options)
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
        { }

        public DbSet<SiteChange> SiteChanges { get; set; }
        public DbSet<SiteChangeExtended> SiteChangesExtended { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //create the definitions of Model Entities via individuals OnModelCreating event
            var types = Assembly.GetExecutingAssembly().GetTypes()
               .Where(s => s.GetInterfaces().Any(_interface => _interface.Equals(typeof(IEntityModel)) && 
                    s.IsClass && !s.IsAbstract && s.IsPublic));
            foreach (var type in types)
            {
                if (type != null)
                {
                    MethodInfo? v = type.GetMethods().FirstOrDefault(x => x.Name == "OnModelCreating");
                    if (v != null) v.Invoke(type, new object[] { modelBuilder });
                }
            }
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
