using N2K_BackboneBackEnd.Models;

using N2K_BackboneBackEnd.Models.BackboneDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


using System.Reflection;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KBackboneContext : BaseContext
    {

        public N2KBackboneContext(DbContextOptions<N2KBackboneContext> options) :  base(options) {
            var types = Assembly.GetExecutingAssembly().GetTypes()
.Where(s => s.GetInterfaces().Any(_interface => _interface.Equals(typeof(IEntityModelBackboneDB)) &&
    s.IsClass && !s.IsAbstract && s.IsPublic));
            foreach (var type in types)
            {
                if (type != null)
                {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
#pragma warning disable CS8604 // Posible argumento de referencia nulo
                    Type? entityType = Assembly.GetAssembly(type: typeof(IEntityModelBackboneDB)).GetType(type.FullName);
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

    }
}
