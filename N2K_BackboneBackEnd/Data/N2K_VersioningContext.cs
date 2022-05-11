using N2K_BackboneBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;
using N2K_BackboneBackEnd.Models.versioning_db;

namespace N2K_BackboneBackEnd.Data
{
    public class N2K_VersioningContext : DbContext
    {
       /* public DbSet<NaturaSite> NaturaSite { get; set; }
        public DbSet<BelongsToBioregion> BelongsToBioRegions { get; set; }
        public DbSet<NutsRegion> NutsRegion { get; set; } 
        public DbSet<IsImpactedBy> IsImpactedBy { get; set; }
        public DbSet<HasNationalProtection> HasNationalProtection { get; set; }
        public DbSet<DetailedProtectionStatus> DetailedProtectionStatus { get; set; }
        public DbSet<Description> Description { get; set; }
        public DbSet<OwnerType> OwnerType { get; set; }*/

        public N2K_VersioningContext(DbContextOptions<N2K_VersioningContext> options) : base(options) {
           /*  var types = Assembly.GetExecutingAssembly().GetTypes()
        .Where(s => s.GetInterfaces().Any(_interface => _interface.Equals(typeof(IEntityModelVersioningDB)) &&
             s.IsClass && !s.IsAbstract && s.IsPublic));
             foreach (var type in types)
             {
                 if (type != null)
                 {
 #pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
 #pragma warning disable CS8604 // Posible argumento de referencia nulo
                     Type? entityType = Assembly.GetAssembly(type: typeof(IEntityModelVersioningDB)).GetType(type.FullName);
 #pragma warning restore CS8604 // Posible argumento de referencia nulo
 #pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                     if (entityType != null)
                     {
                         // create an instance of that type
 #pragma warning disable CS8600 // Se va a convertir un literal nulo o un posible valor nulo en un tipo que no acepta valores NULL
                         object instance = Activator.CreateInstance(entityType);
 #pragma warning restore CS8600 // Se va a convertir un literal nulo o un posible valor nulo en un tipo que no acepta valores NULL
                         if (instance != null) this.Add(instance);
                     }
                 }
            
                
            }*/

           

        }

        //here define the DB<Entities> only for the existing tables in the DB
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<BelongsToBioregion>().HasNoKey(); // No key
            //modelBuilder.ApplyConfigurationsFromAssembly(typeof(N2K_VersioningContext).Assembly);

            
                //create the definitions of Model Entities via OnModelCreating individuals in each Entity.cs file
                var types = Assembly.GetExecutingAssembly().GetTypes()
                   .Where(s => s.GetInterfaces().Any(_interface => _interface.Equals(typeof(IEntityModel)) &&
                        s.IsClass && !s.IsAbstract && s.IsPublic));
                foreach (var type in types)
                {
                    if (type != null)
                    {
                        MethodInfo? v = type.GetMethods().FirstOrDefault(x => x.Name == "OnModelCreating");
                        if (v != null)
                            v.Invoke(type, new object[] { modelBuilder });
                        else
                            throw new Exception(String.Format("static OnModelCreating of entitity {0} not implemented!!"));
                    }
                }
            
        }

    }
}
