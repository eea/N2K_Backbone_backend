using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Services
{
    public class ReportingPeriodService : IReportingPeriodService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public ReportingPeriodService(N2KBackboneContext dataContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _appSettings = app;
        }

        public async Task<List<RepPeriodView>> Get()
        {
            try
            {
                List<RepPeriodView> result = new();
                List<RepPeriod> periods = await _dataContext.Set<RepPeriod>().ToListAsync();
                foreach (RepPeriod period in periods)
                {
                    List<UnionListHeader>? releases = await _dataContext.Set<UnionListHeader>().Where(r => r.Date >= period.InitDate && r.Date <= period.EndDate && r.Name != _appSettings.Value.current_ul_name).ToListAsync();
                    result.Add(new RepPeriodView
                    {
                        Id = period.Id,
                        Active = period.Active,
                        InitDate = period.InitDate,
                        EndDate = period.EndDate,
                        Releases = releases
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReportingPeriodService - Get", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<RepPeriodView>> Edit(RepPeriod rp)
        {
            try
            {
                RepPeriod? period = await _dataContext.Set<RepPeriod>().Where(r => r.Id == rp.Id).FirstOrDefaultAsync();
                if(period  == null)
                {
                    throw new Exception("Reporting Period not found");
                }
                period.EndDate = rp.EndDate;
                _dataContext.Set<RepPeriod>().Update(period);
                await _dataContext.SaveChangesAsync();
                return await Get();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReportingPeriodService - Edit", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<RepPeriodView>> Create(RepPeriod rp)
        {
            try
            {
                RepPeriod? active = await _dataContext.Set<RepPeriod>().FirstOrDefaultAsync(r => r.Active);
                if (active != null)
                {
                    throw new Exception("An active reporting period already exists");
                }

                RepPeriod newRp = new RepPeriod
                {
                    InitDate = rp.InitDate,
                    EndDate = rp.EndDate,
                    Active = true
                };

                _dataContext.Set<RepPeriod>().Add(newRp);
                await _dataContext.SaveChangesAsync();

                return await Get();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReportingPeriodService - Create", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<RepPeriodView>> Close()
        {
            try
            {
                RepPeriod? active = await _dataContext.Set<RepPeriod>().FirstOrDefaultAsync(r => r.Active);
                if (active != null)
                {
                    active.Active = false;
                    _dataContext.Set<RepPeriod>().Update(active);
                    await _dataContext.SaveChangesAsync();
                }
                return await Get();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReportingPeriodService - Close", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}
