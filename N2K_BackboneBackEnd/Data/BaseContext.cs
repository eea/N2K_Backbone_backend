using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Models;
using System.Reflection;

namespace N2K_BackboneBackEnd.Data
{
    public class BaseContext : DbContext
    {


        public BaseContext(DbContextOptions options ) : base(options)
        {
        }


        public BaseContext(DbContextOptions options , string Interface ) : base(options)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(s => s.GetInterfaces().Any(_interface => _interface.Name == Interface) && 
                            s.IsClass && !s.IsAbstract && s.IsPublic);
            foreach (var type in types)
            {
                if (type != null)
                {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
#pragma warning disable CS8604 // Posible argumento de referencia nulo
                    Type? entityType = Assembly.GetAssembly(type).GetType(type.FullName);
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
            }
        }



        //here define the DB<Entities> only for the existing tables in the DB
        //public DbSet<SiteChange> SiteChanges { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
