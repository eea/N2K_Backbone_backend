using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public class HarvestedService : IHarvestedService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;

        public HarvestedService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
        }
        public  async Task<List<Harvesting>> GetHarvestedAsync()
        {
            var a= new  List<Harvesting>();
            return await Task.FromResult(a);

        }

        public List<Harvesting> GetHarvested()
        {
            var a = new List<Harvesting>();
            return a;

        }



#pragma warning disable CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        public async Task<Harvesting> GetHarvestedAsyncById(int id)
#pragma warning restore CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        {
            return await Task.FromResult(new Harvesting
            {
                Id = id,
                Country = "ES",
                Status = Enumerations.HarvestingStatus.Pending,
                SubmissionDate= DateTime.Today
            });

        }

        public async Task<List<Harvesting>> GetPendingEnvelopes()
        {
            var result = new List<Harvesting>();
            var processed = await _dataContext.ProcessedEnvelopes.ToListAsync();
            foreach (var procCountry in processed) {
                var param1 = new SqlParameter("@country", procCountry.Country );
                var param2 = new SqlParameter("@version", procCountry.Version);
                var param3 = new SqlParameter("@importdate", procCountry.ImportDate);

                var list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.spGetPendingCountryVersion  @country, @version,@importdate",
                                param1, param2,param3)
                    .ToListAsync();
                if (list.Count>0 ) result.AddRange(list);
            }
            return await Task.FromResult(result);
        }

        public async Task<String> Harvest(EnvelopesToProcess[] envelopeIDs)
        {
            var a = "OK";
            return await Task.FromResult(a);
        }

    }
}
