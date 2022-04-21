using N2K_BackboneBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KBackboneContext : DbContext
    {
#pragma warning disable CS8618 // Accept NULL value.
        public N2KBackboneContext(DbContextOptions<N2KBackboneContext> options) : base(options)
#pragma warning restore CS8618 // Accept NULL value
        { }

        //here define the DB<Entities> only for the existing tables in the DB
        //public DbSet<SiteChange> SiteChanges { get; set; }
        public DbSet<SiteChangeDb> SiteChanges { get; set; }
        public DbSet<ProcessedEnvelopes> ProcessedEnvelopes { get; set; }
        
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
