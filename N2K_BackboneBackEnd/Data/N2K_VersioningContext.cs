using N2K_BackboneBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace N2K_BackboneBackEnd.Data
{
    public class N2K_VersioningContext : DbContext
    {

        public N2K_VersioningContext(DbContextOptions<N2K_VersioningContext> options) : base(options) { }


        //here define the DB<Entities> only for the actual tables in the DB
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
