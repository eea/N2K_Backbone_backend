using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Models;
using System.Reflection;

namespace N2K_BackboneBackEnd.Data
{
    public class BaseContext : DbContext
    {


        public BaseContext(DbContextOptions options ) : base(options)
        {
            SaveChangesFailed += mySaveChangesFailed;
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
            SaveChangesFailed += mySaveChangesFailed;
        }

        private void mySaveChangesFailed(object sender, SaveChangesFailedEventArgs e)
        {
            Console.WriteLine($"Save Chagnes Failed at {DateTime.Now}");
            try
            {
                SystemLog.write(SystemLog.errorLevel.Error, ((DbUpdateException)e.Exception).InnerException.Message + " in following Entries:", "mySaveChangesFailed", "Entityframework");
                foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entity in ((DbUpdateException)e.Exception).Entries)
                {
                    string entityName = entity.Entity.GetType().Name + "; ";
                    foreach (System.Reflection.PropertyInfo info in entity.Entity.GetType().GetProperties())
                    {
                        entityName += "; " + info.Name + "=" + info.GetValue(entity.Entity).ToString();
                    }
                    SystemLog.write(SystemLog.errorLevel.Error, entityName, "mySaveChangesFailed", "Entityframework");

                }
            }
            catch (Exception ex)
            {

            }
            //SystemLog.write(SystemLog.errorLevel.Error, ((DbUpdateException)e.Exception).Entries[0].Entity.GetType().Name, "", "");



        }

        private void myStateChanged(object sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs e)
        {
            // YOU CAN USE AN INTERFACE OR A BASE CLASS
            // But, for this demo, we are directly typecasting to Student model
            switch (e.Entry.State)
            {
                case EntityState.Deleted:
                    Console.WriteLine($"Marked for delete: {e.Entry.Entity}");
                    break;
                case EntityState.Modified:
                    Console.WriteLine($"Marked for update: {e.Entry.Entity}");
                    break;
                case EntityState.Added:
                    Console.WriteLine($"Marked for insert: {e.Entry.Entity}");
                    break;
            }
        }

        private void myTracked(object sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs e)
        {
            Console.WriteLine($"Marked for Tracking: {e.Entry.Entity}");
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
                    Console.WriteLine(type.FullName);
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
