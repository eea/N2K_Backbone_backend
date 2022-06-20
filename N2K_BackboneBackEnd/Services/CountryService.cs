using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.backbone_db;
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

        public async Task<List<Countries>> GetCountriesWithDataAsync()
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

        public async Task<List<Countries>> GetCountriesAsync()
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
    }
}
