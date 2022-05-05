using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;


namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public class BaseHarvestingProcess 
    {
        protected readonly N2KBackboneContext _dataContext;
        protected readonly N2K_VersioningContext _versioningContext;


        public BaseHarvestingProcess (N2KBackboneContext dataContext, N2K_VersioningContext versioningContext)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
        }

        protected void SaveChanges(List<SiteChangeDb> changes)
        {
            _dataContext.SiteChanges.AddRange(changes);
            _dataContext.SaveChanges();
        }

    }
}
