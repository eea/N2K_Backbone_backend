using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly N2KBackboneContext _dataContext;

        public MasterDataService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<BioRegionTypes>> GetBioRegionTypes()
        {
            try
            {
                return await _dataContext.Set<BioRegionTypes>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "MasterDataService - GetBioRegionTypes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SiteTypes>> GetSiteTypes()
        {
            try
            {
                return await _dataContext.Set<SiteTypes>().FromSqlRaw($"exec dbo.spGetSiteTypes").ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "MasterDataService - GetSiteTypes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}
