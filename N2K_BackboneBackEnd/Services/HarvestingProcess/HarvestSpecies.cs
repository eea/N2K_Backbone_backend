using N2K_BackboneBackEnd.Data;

namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public class HarvestSpecies : BaseHarvestingProcess, IHarvestingTables
    {
        public HarvestSpecies(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext) : base(dataContext, versioningContext)
        {
        }

        public async Task<int> Harvest(string countryCode, int versionId)
        {
            try
            {
                Console.WriteLine("=>Start species harvest...");
                await Task.Delay(8000);
                var a = 1;
                var b = 1 / (a - 1);
                Console.WriteLine("=>End species harvest...");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("=>End species harvest with error...");
                return 0;
            }

        }

        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start species validate...");
            await Task.Delay(2000);
            Console.WriteLine("==>ENd speciesvalidate...");
            return 1;
        }

    }
}
