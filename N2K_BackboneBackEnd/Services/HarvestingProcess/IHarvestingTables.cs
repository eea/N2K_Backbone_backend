namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public interface IHarvestingTables
    {

       Task<int> Harvest(string countryCode, int versionId);
       Task<int> ValidateChanges(string countryCode, int newVersionId, int referenceVersionID);
    }
}
