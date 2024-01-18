using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public class CountryService : ICountryService
    {
        private class OrderedChanges
        {
            public string Code { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;

        }

        private class CountryVersion
        {
            public string Country { get; set; } = string.Empty;
            public int Version { get; set; }

        }

        private readonly N2KBackboneContext _dataContext;
        private readonly IEnumerable<Countries> _countries;

        public CountryService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
            _countries = _dataContext.Set<Countries>().AsNoTracking().ToList();
        }

        public async Task<List<CountriesWithDataView>> GetWithDataAsync()
        {
            try
            {
                var changes = _dataContext.Set<SiteChangeDb>().Select(ch => new CountryVersion { Country = ch.Country, Version = ch.N2KVersioningVersion.Value }).Distinct();
                var countries = _dataContext.Set<CountriesWithDataView>();

                var aux = (from ch in await changes.ToListAsync()
                           join ctr in countries
                           on ch.Country.ToUpper() equals ctr.Code.ToUpper()
                           select new CountriesWithDataView
                           {
                               Code = ch.Country.ToUpper(),
                               Country = ctr.Country,
                               isEUCountry = ctr.isEUCountry,
                               Version = ch.Version
                           }).ToList();

                return aux;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CountryService - GetWithDataAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<Countries>> GetAsync()
        {
            try
            {
                List<Countries> result = new();

                return await _dataContext.Set<Countries>()
                    .AsNoTracking()
                    .Select(c => new Countries
                    {
                        Code = c.Code.ToUpper(),
                        Country = c.Country,
                        isEUCountry = c.isEUCountry
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CountryService - GetAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<CountriesWithDataView>> GetWithDataAsync(SiteChangeStatus? status, Level? level)
        {
            try
            {
                SqlParameter param2 = new("@status", status.HasValue ? status.ToString() : string.Empty);
                SqlParameter param3 = new("@level", level.HasValue ? level.ToString() : string.Empty);

                List<CountriesWithDataView> countries = await _dataContext
                    .Set<CountriesWithDataView>()
                    .FromSqlRaw($"exec dbo.spGetCountriesByStatusAndLevel @status, @level", param2, param3)
                    .AsNoTracking()
                    .ToListAsync();

                return countries;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CountryService - GetWithDataAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<CountriesChangesView>> GetPendingLevelAsync()
        {
            try
            {
                SqlParameter param1 = new("@status", "Pending");
                List<CountriesChangesView> countries = await _dataContext
                    .Set<CountriesChangesView>()
                    .FromSqlRaw($"exec dbo.spGetCountriesCountLevelByStatus @status", param1)
                    .AsNoTracking()
                    .ToListAsync();
                return countries;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CountryService - GetPendingLevelAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<CountriesChangesView>> GetConsolidatedCountries()
        {
            try
            {
                List<CountriesChangesView> countries = await _dataContext
                .Set<CountriesChangesView>()
                .FromSqlRaw($"exec dbo.spGetCountriesWithOnlyConsolidatedSumbmisions")
                .AsNoTracking()
                .ToListAsync();
                return countries;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CountryService - GetConsolidatedCountries", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<ClosedCountriesView>> GetClosedAndDiscardedCountriesAsync()
        {
            try
            {
                List<ClosedCountriesView> countries = await _dataContext
                .Set<ClosedCountriesView>()
                .FromSqlRaw($"exec dbo.spGetCountriesWithClosedOrDiscardedSubmissions")
                .AsNoTracking()
                .ToListAsync();
                return countries;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CountryService - GetClosedAndDiscardedCountriesAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SitesWithChangesView>> GetSiteLevelAsync(SiteChangeStatus? status)
        {
            try
            {
                SqlParameter param1 = new("@status", status.HasValue ? status.ToString() : string.Empty);
                List<SitesWithChangesView> countries = await _dataContext
                    .Set<SitesWithChangesView>()
                    .FromSqlRaw($"exec dbo.spGetSiteCountLevelByStatus @status", param1)
                    .AsNoTracking()
                    .ToListAsync();
                return countries;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CountryService - GetSiteLevelAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<CountriesSiteCountView>> GetSiteCountAsync()
        {
            try
            {
                SqlParameter param1 = new("@country", DBNull.Value);
                List<CountriesSiteCountView> countries = await _dataContext
                    .Set<CountriesSiteCountView>()
                    .FromSqlRaw($"exec dbo.spGetSiteStatusCountByCountry @country", param1)
                    .AsNoTracking()
                    .ToListAsync();
                return countries;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CountryService - GetSiteCountAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}