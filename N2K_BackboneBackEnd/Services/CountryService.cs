using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
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

        private readonly N2KBackboneContext _dataContext;
        private readonly IEnumerable<Countries> _countries;

        public CountryService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
            _countries = _dataContext.Set<Countries>().AsNoTracking().ToList();
        }

        public async Task<List<Countries>> GetWithDataAsync()
        {

            var changes = _dataContext.Set<SiteChangeDb>().Select(c => c.Country).Distinct();
            var countries = _dataContext.Set<Countries>();

            var aux = (from ch in await changes.ToListAsync()
                       join ctr in countries
                       on ch.ToUpper() equals ctr.Code.ToUpper()
                       select new Countries
                       {
                           Code = ch.ToUpper(),
                           Country = ctr.Country,
                           isEUCountry = ctr.isEUCountry
                       }).ToList();

            return aux;
        }

        public async Task<List<Countries>> GetAsync()
        {
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
        
        public async Task<List<Countries>> GetWithDataAsync(SiteChangeStatus? status, Level? level)
        {
            var param1 = new SqlParameter("@country", DBNull.Value);
            var param2 = new SqlParameter("@status", status.HasValue ? status.ToString() : DBNull.Value);
            var param3 = new SqlParameter("@level", level.HasValue ? level.ToString() : DBNull.Value);

            var countries = await _dataContext
                .Set<Countries>()
                .FromSqlRaw($"exec dbo.spGetCountriesByStatusAndLevel @status, @level", param2, param3)
                .AsNoTracking()
                .ToListAsync();

            return countries;
        }

    }
}
