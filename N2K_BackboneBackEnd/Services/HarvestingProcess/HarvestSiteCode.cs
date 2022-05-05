namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public  class HarvestSiteCode: IHarvestingTables
    {
        public  async Task<int> Harvest(string countryCode, int versionId)
        {
            Console.WriteLine("Start Site Code harvest...");
            await Task.Delay(5000);
            Console.WriteLine("End Site Code harvest...");
            return 1;

        }

        public  async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("Start Site Code validate...");
            await Task.Delay(10000);
            Console.WriteLine("ENd Site Code validate...");
            return 1;
        }
    }
}
