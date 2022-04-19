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
                EnvelopeId = id,
                Country = "Spain",
                PendingChanges = 0,
                Status = Enumerations.HarvestingStatus.Pending,
                SubmissionDate= DateTime.Today
            });

        }

        public async Task<List<Harvesting>> GetPendingEnvelopes()
        {
            var a = new List<Harvesting>();
            a.Add(
               new Harvesting {
                   EnvelopeId = 25654,
                   Country = "Spain",
                   PendingChanges = 11,
                   SubmissionDate = Convert.ToDateTime("04/05/2021"),
                   Id = 1,
                   Status = Enumerations.HarvestingStatus.Pending
             });
            a.Add(
               new Harvesting
               {
                   EnvelopeId = 25655,
                   Country = "Spain",
                   PendingChanges = 5,
                   SubmissionDate = Convert.ToDateTime("05/05/2021"),
                   Id = 2,
                   Status = Enumerations.HarvestingStatus.Pending
               });
            a.Add(
               new Harvesting
               {
                   EnvelopeId = 25656,
                   Country = "Denmark",
                   PendingChanges = 8,
                   SubmissionDate = Convert.ToDateTime("06/05/2021"),
                   Id = 3,
                   Status = Enumerations.HarvestingStatus.Pending
               });
            a.Add(
               new Harvesting
               {
                   EnvelopeId = 25657,
                   Country = "Austria",
                   PendingChanges = 10,
                   SubmissionDate = Convert.ToDateTime("07/05/2021"),
                   Id = 4,
                   Status = Enumerations.HarvestingStatus.Pending
               });


            return await Task.FromResult(a);



        }

        public async Task<String> Harvest(int[] envelopeIDs)
        {
            var a = "OK";

            return await Task.FromResult(a);
        }

    }
}
