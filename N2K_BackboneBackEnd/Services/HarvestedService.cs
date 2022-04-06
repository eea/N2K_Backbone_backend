using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public class HarvestedService : IHarvestedService
    {
        private readonly N2KBackboneContext _dataContext;

        public HarvestedService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
        }
        public  async Task<List<Harvesting>> GetHarvestedAsync()
        {
            var a= new  List<Harvesting>();
            a.Add(
                new Harvesting
                {
                    Date = DateTime.Now.AddDays(-3),
                    TemperatureC = 100
                });
            return await Task.FromResult(a);

        }

#pragma warning disable CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        public async Task<Harvesting> GetHarvestedAsyncById(int id)
#pragma warning restore CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        {
            return await Task.FromResult(new Harvesting
                {
                    Date = DateTime.Now,
                    TemperatureC = 123
                });

        }
    }
}
