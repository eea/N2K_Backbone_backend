using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Services
{
    public class ControllerSiteService
    {
        private class OrderedChanges
        {
            public string Code { get; set; } = "";
            public string Country { get; set; } = "";
            public List<CountryChangeDb> ChangeList { get; set; } = new List<CountryChangeDb>();

        }

        private readonly N2KBackboneContext _dataContext;
        private readonly IEnumerable<Countries> _countries;

        public ControllerSiteService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
            _countries = _dataContext.Set<Countries>().AsNoTracking().ToList();
        }

        public async Task<List<CountryNameCode>> GetCountriesWithDataAsync()
        {
            SqlParameter param1 = new SqlParameter("@country", "NULL");
            SqlParameter param2 = new SqlParameter("@status", "NULL");
            SqlParameter param3 = new SqlParameter("@level", "NULL");

            IQueryable<CountryNameCode> countries = _dataContext.Set<CountryNameCode>()
                .FromSqlRaw($"exec dbo.spGetChangesByCountryAndStatusAndLevel @country, @status, @level", param1, param2, param3);

            //returns the country codes of the countries that have had changes
            IOrderedEnumerable<OrderedChanges> orderedChangesEnum = (from t in await countries.ToListAsync()
                                                                     group t by t.Code
                                                                     into g
                                                                     select new OrderedChanges
                                                                     {
                                                                         Code = g.Key
                                                                         //Nest all changes of each sitecode ordered by Level
                                                                     }).OrderByDescending(a => a.Code);

            // TODO: get country code and country name
            var result = new List<CountryNameCode>();

            foreach (var country in countries)
            {
                result.Add(country);
            }

            return result;
        }
    }
}
