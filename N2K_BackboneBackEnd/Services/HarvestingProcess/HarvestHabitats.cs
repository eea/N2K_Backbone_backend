namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public class HarvestHabitats : IHarvestingTables
    {
        public async Task<int> Harvest(string countryCode, int versionId)
        {

            Console.WriteLine("Start HarvestHabitats harvest...");
            await Task.Delay(5000);
            Console.WriteLine("End HarvestHabitatsharvest...");
            return 1;


        }

        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("Start HarvestHabitats validate...");
            await Task.Delay(5000);
            Console.WriteLine("End HarvestHabitats validate...");
            return 1; 
        }
    }
}
