using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Services
{
    public class ReportingPeriodService : IReportingPeriodService
    {
        private readonly N2KBackboneContext _dataContext;
        private IEnumerable<RepPeriod> _rpContext;

        public ReportingPeriodService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
            _rpContext = _dataContext.Set<RepPeriod>();
        }

        public async Task<List<RepPeriodView>> Get()
        {
            try
            {
                List<RepPeriodView> result = new();
                List<RepPeriod> periods = _rpContext.ToList();
                foreach (RepPeriod period in periods)
                {
                    List<UnionListHeader>? releases = _dataContext.Set<UnionListHeader>().Where(r => r.Date >= period.InitDate && r.Date <= period.EndDate)?.ToList();
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

        public async Task<List<RepPeriodView>> Edit(long id, DateTime newEndDate)
        {
            try
            {
                RepPeriod? period = _rpContext.SingleOrDefault(r => r.Id == id);
                if(period  == null)
                {
                    throw new Exception("Reporting Period not found");
                }
                period.EndDate = newEndDate;
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

        public async Task<List<RepPeriodView>> Create(DateTime init, DateTime end)
        {
            try
            {
                RepPeriod? active = _rpContext.SingleOrDefault(r => r.Active);
                if (active != null)
                {
                    throw new Exception("An active reporting period already exists");
                }

                RepPeriod newRp = new RepPeriod
                {
                    InitDate = init,
                    EndDate = end,
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
                RepPeriod? active = _rpContext.SingleOrDefault(r => r.Active);
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
