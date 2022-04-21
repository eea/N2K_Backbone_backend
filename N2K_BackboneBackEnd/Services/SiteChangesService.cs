using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public class SiteChangesService : ISiteChangesService
    {
        private readonly N2KBackboneContext _dataContext;

        public SiteChangesService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<List<SiteChange>> GetSiteChangesAsync()
        {
            var changes = await _dataContext.SiteChanges.OrderBy(s => s.SiteCode).ToListAsync();
            var result = new List<SiteChange>();
            var siteCode = string.Empty;
            var siteChange = new SiteChange();
            foreach (var change in changes)
            {
                if (change.SiteCode != siteCode)
                {
                    if (siteCode != String.Empty) result.Add(siteChange);
                    siteChange = new SiteChange();
                    siteChange.Subrows = new List<SiteChange>();
                    siteChange.ChangeId = change.ChangeId;
                    siteChange.SiteCode = change.SiteCode;
                    siteCode = change.SiteCode;
                    siteChange.ChangeCategory = change.ChangeCategory;
                    siteChange.ChangeType = change.ChangeType;
                    siteChange.Action = String.Empty;
                    siteChange.Country = change.Country;
                    siteChange.Level = change.Level;
                    siteChange.Status = change.Status;
                    siteChange.Tags = change.Tags;

                }
                else
                {
                    if (siteChange.Subrows == null) siteChange.Subrows = new List<SiteChange>();
                    siteChange.Subrows.Add(new SiteChange
                    {
                        ChangeId = change.ChangeId,
                        SiteCode = string.Empty,
                        Action = string.Empty,
                        ChangeCategory = change.ChangeCategory,
                        ChangeType = change.ChangeType,
                        Country = change.Country,
                        Level = change.Level,
                        Status = change.Status,
                        Subrows = new List<SiteChange>(),
                        Tags = string.Empty
                    });
                }
            }
            if (siteCode != String.Empty) result.Add(siteChange);
            return result;
        }

        public async Task<List<SiteChangeExtended>> GetSiteChangesFromSP()
        {
            var param1 = new SqlParameter("@param1", 1);
            var param2 = new SqlParameter("@param2", 2);

            var list = await _dataContext.Set<SiteChangeExtended>().FromSqlRaw($"exec dbo.Testing2  @param1, @param2",
                            param1, param2)
                //.AsNoTrackingWithIdentityResolution()
                .ToListAsync();

            return list;

            /*
            //For the time we need to execute a sql (StoredProc) that returns an int 
            // define SqlParameters for the other two params to be passed
            var oidProviderParam = new SqlParameter("@oidProvider", id.OIdProvider);
            var oidParam = new SqlParameter("@oid", string.IsNullOrEmpty(id.OId) ? "" : id.OId);

            // define the output parameter that needs to be retained
            // for the Id created when the Stored Procedure executes 
            // the INSERT command
            var userIdParam = new SqlParameter("@Id", SqlDbType.Int);

            // the direction defines what kind of parameter we're passing
            // it can be one of:
            // Input
            // Output
            // InputOutput -- which does pass a value to Stored Procedure and retains a new state
            userIdParam.Direction = ParameterDirection.Output;

            // we can also use context.Database.ExecuteSqlCommand() or awaitable ExecuteSqlCommandAsync()
            // which also produces the same result - but the method is now marked obselete
            // so we use ExecuteSqlRawAsync() instead

            // we're using the awaitable version since GetOrCreateUserAsync() method is marked async
            await context.Database.ExecuteSqlRawAsync(
                    "exec sp_CreateUser @emailAddress, @passwordHash, @oidProvider, @oid, @Id out", 
                    emailAddressParam, 
                    passwordParam, 
                    oidProviderParam, 
                    oidParam, 
                    userIdParam);

            // the userIdParam which represents the Output param
            // now holds the Id of the new user and is an Object type
            // so we convert it to an Integer and send
            return Convert.ToInt32(userIdParam.Value);

                    */

        }

        private Task<List<SiteChangeExtended>> Task(Func<List<SiteChangeExtended>> p)
        {
            throw new NotImplementedException();
        }


#pragma warning disable CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        public async Task<SiteChangeDb?> GetSiteChangeByIdAsync(int id)
#pragma warning restore CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        {
            return await _dataContext.SiteChanges.SingleOrDefaultAsync(s => s.ChangeId == id);
        }


    }
}
