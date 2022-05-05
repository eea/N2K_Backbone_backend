namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public class HarvestSpecies : IHarvestingTables
    {
        public async Task<int> Harvest(string countryCode, int versionId)
        {
            Console.WriteLine("=>Start species harvest...");
            await Task.Delay(8000);
            Console.WriteLine("=>End species harvest...");
            return 1;

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
