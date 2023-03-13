using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.ServiceResponse;

namespace N2K_BackboneBackEnd.Services
{
    public class CountryService : ICountryService
    {
        private class OrderedChanges
        {
            public string Code { get; set; } = "";
            public string Country { get; set; } = "";

        }
        private class CountryVersion
        {
            public string Country { get; set; } = "";
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

            var changes = _dataContext.Set<SiteChangeDb>().AsNoTracking().Select(ch => new CountryVersion { Country = ch.Country, Version = ch.N2KVersioningVersion.Value }).Distinct();
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

        public async Task<List<Countries>> GetAsync()
        {

            //_dataContext.ChangeTracker.AutoDetectChangesEnabled = false;
            List<Respondents> items = new List<Respondents>();
            //SqlConnection conn = new SqlConnection(this._dataContext.Database.GetConnectionString());
            //conn.Open();

            DateTime start1 = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    Respondents respondent = new Respondents( this._dataContext.Database.GetConnectionString());
                    DateTime start = DateTime.Now;
                    respondent.SiteCode = string.Format("123{0}", i);
                    respondent.Version = 0;
                    respondent.locatorName = "";
                    respondent.addressArea = "";
                    respondent.postName = "";
                    respondent.postCode = "fdgfdkjshdf";
                    respondent.thoroughfare = "";
                    respondent.addressUnstructured = "";
                    respondent.name = "";
                    respondent.Email = "";
                    respondent.AdminUnit = "";
                    respondent.LocatorDesignator = "";

                    respondent.SaveRecord();
                    items.Add(respondent);

                    //_dataContext.Set<Respondents>().AddRange(items);
                    //await _dataContext.SaveChangesAsync();
                    //_dataContext.SaveChanges();
                    items.Clear();
                    var diff = DateTime.Now - start;
                    Console.WriteLine(string.Format("{0}=> {1}", i, diff.TotalMilliseconds.ToString()));
                    Console.WriteLine("****************");

                }
                catch (Exception ex)
                {
                    SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - HarvestRespondents", "");
                    //return null;
                    Console.WriteLine(string.Format("{0} Error ", i));
                    items.Clear();
                }
            }
            //conn.Close();
            //conn.Dispose();
            Console.WriteLine("////////////");
            var diff1 = DateTime.Now - start1;
            Console.WriteLine(string.Format("Total=> {0}", diff1.TotalMilliseconds.ToString()));
            Console.WriteLine("/////");

            _dataContext.ChangeTracker.AutoDetectChangesEnabled = true;

            return null;

            List<Countries> result = new List<Countries>();

            return await _dataContext.Set<Countries>()
                .AsNoTracking()
                .Select(c => new Countries { 
                           Code = c.Code.ToUpper(),
                           Country = c.Country,
                           isEUCountry = c.isEUCountry
                           })
                .ToListAsync();
        }
        
        public async Task<List<CountriesWithDataView>> GetWithDataAsync(SiteChangeStatus? status, Level? level)
        {

            var param2 = new SqlParameter("@status", status.HasValue ? status.ToString() : string.Empty);
            var param3 = new SqlParameter("@level", level.HasValue ? level.ToString() : string.Empty);


            List<CountriesWithDataView> countries = await _dataContext
                .Set<CountriesWithDataView>()
                .FromSqlRaw($"exec dbo.spGetCountriesByStatusAndLevel @status, @level", param2, param3)
                .AsNoTracking()
                .ToListAsync();

           return countries;
        }

        public async Task<List<CountriesChangesView>> GetPendingLevelAsync()
        {
            var param1 = new SqlParameter("@status", "Pending");
            var countries = await _dataContext
                .Set<CountriesChangesView>()
                .FromSqlRaw($"exec dbo.spGetCountriesCountLevelByStatus @status", param1)
                .AsNoTracking()
                .ToListAsync();
            return countries;
        }

        public async Task<List<CountriesChangesView>> GetConsolidatedCountries()
        {
            var countries = await _dataContext
                .Set<CountriesChangesView>()
                .FromSqlRaw($"exec dbo.spGetCountriesWithOnlyConsolidatedSumbmisions")
                .AsNoTracking()
                .ToListAsync();
            return countries;
        }

        public async Task<List<ClosedCountriesView>> GetClosedAndDiscardedCountriesAsync()
        {
            var countries = await _dataContext
                .Set<ClosedCountriesView>()
                .FromSqlRaw($"exec dbo.spGetCountriesWithClosedOrDiscardedSubmissions")
                .AsNoTracking()
                .ToListAsync();
            return countries;
        }

        public async Task<List<SitesWithChangesView>> GetSiteLevelAsync(SiteChangeStatus? status)
        {
            var param1 = new SqlParameter("@status", status.HasValue ? status.ToString() : string.Empty);
            var countries = await _dataContext
                .Set<SitesWithChangesView>()
                .FromSqlRaw($"exec dbo.spGetSiteCountLevelByStatus @status", param1)
                .AsNoTracking()
                .ToListAsync();
            return countries;
        }

        public async Task<List<CountriesSiteCountView>> GetSiteCountAsync()
        {
            var param1 = new SqlParameter("@country", DBNull.Value);
            var countries = await _dataContext
                .Set<CountriesSiteCountView>()
                .FromSqlRaw($"exec dbo.spGetSiteStatusCountByCountry @country", param1)
                .AsNoTracking()
                .ToListAsync();
            return countries;
        }

    }
}
