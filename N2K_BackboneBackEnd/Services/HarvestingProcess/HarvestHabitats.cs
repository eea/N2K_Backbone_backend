namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public class HarvestHabitats : IHarvestingTables
    {
        public async Task<int> Harvest(string countryCode, int versionId)
        {
            return await Task.Run(() => 1);

            //throw new NotImplementedException();
        }

        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            return await Task.Run(() => 1);
        }
    }
}
