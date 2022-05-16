using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.VersioningDB;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Services
{
    public class SiteChangesService : ISiteChangesService
    {
        private readonly N2KBackboneContext _dataContext;


        public SiteChangesService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<List<SiteChangeDb>> GetSiteChangesAsync()
        {

            var changes = await _dataContext.Set<SiteChangeDb>().ToListAsync();
            changes = changes.OrderBy(s=>s.SiteCode).ThenByDescending(x => (int)(x.Level)).ToList();

            var result = new List<SiteChangeDb>();
            var siteCode = string.Empty;
            var siteChange = new SiteChangeDb();
            foreach (var change in changes)
            {
                if (change.SiteCode != siteCode)
                {
                    if (siteCode != String.Empty) result.Add(siteChange);
                    siteChange = new SiteChangeDb();
                    siteChange.NumChanges = 1;
                    siteChange.ChangeId = change.ChangeId;
                    siteChange.SiteCode = change.SiteCode;
                    siteCode = change.SiteCode;
                    siteChange.ChangeCategory = change.ChangeCategory;
                    siteChange.ChangeType = change.ChangeType;
                    siteChange.Country = change.Country;
                    siteChange.Level = change.Level;
                    siteChange.Status = change.Status;
                    siteChange.Tags = change.Tags;
                    //result.Add(siteChange);
                    siteCode = change.SiteCode;

                }
                else
                {
                    if (siteChange.Subrows == null) siteChange.Subrows = new List<SiteChangeView>();
                    siteChange.Subrows.Add(new SiteChangeView
                    {
                        ChangeId = change.ChangeId,
                        SiteCode = string.Empty,
                        Action = string.Empty,
                        ChangeCategory = change.ChangeCategory,
                        ChangeType = change.ChangeType,
                        Country = change.Country,
                        Level = change.Level,
                        Status = change.Status,
                        Tags = string.Empty
                    });
                    siteChange.NumChanges++;
                }
                
            }
            if (siteCode != String.Empty) result.Add(siteChange);
            return result;
        }

        public async Task<List<SiteChangeViewModel>> GetSiteChangesFromSP()
        {
            var param1 = new SqlParameter("@param1", 1);
            var param2 = new SqlParameter("@param2", 2);

            var list = await _dataContext.Set<SiteChangeViewModel>().FromSqlRaw($"exec dbo.Testing2  @param1, @param2",
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



#pragma warning disable CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        public async Task<SiteChangeDb?> GetSiteChangeByIdAsync(int id)
#pragma warning restore CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        {
            var result = new List<Harvesting>();
            return await _dataContext.Set<SiteChangeDb>().SingleOrDefaultAsync(s => s.ChangeId == id);
        }


        public async Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion) {

            var changeDetailVM = new SiteChangeDetailViewModel();
            changeDetailVM.SiteCode = pSiteCode;
            changeDetailVM.CountryVersion= pCountryVersion;
            changeDetailVM.ChangesList = new List<ChangeDetail>();


            var site = await _dataContext.Set<Sites>().Where(site => site.SiteCode == pSiteCode  && site.Version == pCountryVersion ).FirstOrDefaultAsync();
            var oldSite = await _dataContext.Set<Sites>().Where(site => site.SiteCode == pSiteCode && site.Current == true).FirstOrDefaultAsync();

            if (site != null)
            {
#pragma warning disable CS8601 // Posible asignación de referencia nula
                changeDetailVM.Name = site.Name;
                changeDetailVM.Status = SiteChangeStatus.Pending; //(SiteChangeStatus?) site.CurrentStatus;
#pragma warning restore CS8601 // Posible asignación de referencia nula
            }

            var detectedChanges = await _dataContext.Set<SiteChangeDb>().Where(site => site.SiteCode == pSiteCode).ToListAsync();
            if (detectedChanges != null)
            {
                foreach (var change in detectedChanges)
                {
                    var changeDetail = new ChangeDetail();
                    changeDetail.ChangeId = change.ChangeId;
                    changeDetail.Level = change.Level;
                    changeDetail.ChangeType = change.ChangeType != null ? change.ChangeType.ToString() : String.Empty;
                    changeDetail.ChangeCategory = change.ChangeCategory != null? change.ChangeCategory.ToString() : String.Empty;
                    changeDetail.FieldName = String.Empty;
                    changeDetail.ReportedValue = String.Empty;
                    changeDetail.OlValue = String.Empty;
                    changeDetail.Description = String.Empty;
                    FillChangeDetail(site, oldSite, change, ref changeDetail);
                    changeDetailVM.ChangesList.Add(changeDetail);
                }
            }
            return changeDetailVM;

        }



        private void FillChangeDetail(Sites? site, Sites? oldSite, SiteChangeDb siteChangeDb,  ref ChangeDetail detail)
        {
            if (detail != null)
            {
                switch (detail.ChangeCategory)
                {
                    case "Site General Info":
                       switch (detail.ChangeType)
                        {
                            case "SiteName Changed":
                                detail.FieldName = "SiteName";
                                detail.ReportedValue = "New name";
                                detail.OlValue = oldSite.Name;
                                break;

                            case "SiteType Changed":
                                detail.FieldName = "SiteType";
                                detail.ReportedValue = "NewSiteType";
                                detail.OlValue = oldSite.SiteType.ToString();
                                break;

                            case "Length Changed":
                                detail.FieldName = "Length";
                                detail.ReportedValue = "New Length";
                                detail.OlValue = oldSite.Length.ToString();
                                break;
                        }
                        break;

                    case "Change of area":
                        detail.FieldName = "Area";
                        detail.ReportedValue = "NewSite Area";
                        detail.OlValue = oldSite.Area.ToString();
                        break;

                    case "Site Added":
                        detail.FieldName = "SiteCode";
                        detail.ReportedValue = site.SiteCode;
                        detail.OlValue = "";
                        break;


                    case "Site Deleted":
                        detail.FieldName = "SiteCode";
                        detail.ReportedValue = site.SiteCode;
                        detail.OlValue = site.SiteCode;
                        break;


                    case "Species and habitats":
                        switch (detail.ChangeType)
                        {
                            case "Relative surface Decrease":
                                detail.FieldName = "RelSurface";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;

                            case "Relative surface Increase":
                                detail.FieldName = "RelSurface";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;

                            case "Relative surface Change":
                                detail.FieldName = "RelSurface";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;

                            case "Representativity Decrease":
                                detail.FieldName = "Representativity";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;

                            case "Representativity Increase":
                                detail.FieldName = "Representativity";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;

                            case "Representativity Change":
                                detail.FieldName = "Representativity";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;

                            case "Cover_ha Decrease":
                                detail.FieldName = "Cover_ha";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;

                            case "Cover_ha Increase":
                                detail.FieldName = "Cover_ha";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;

                            case "Cover_ha Change":
                                detail.FieldName = "Cover_ha";
                                detail.ReportedValue = "New Value";
                                detail.OlValue = "Old Value";
                                break;


                        }
                        break;

                    case "Species Added":
                        detail.FieldName = "Species";
                        detail.ReportedValue = "New Value";
                        detail.OlValue = "";
                        break;

                    case "Species Deleted":
                        detail.FieldName = "Species";
                        detail.ReportedValue = "";
                        detail.OlValue = "Deleted";
                        break;


                    case "Habitat Added":
                        detail.FieldName = "Habitats";
                        detail.ReportedValue = "New Habitat";
                        detail.OlValue = "";
                        break;

                    case "Habitat Deleted":
                        detail.FieldName = "Species";
                        detail.ReportedValue = "";
                        detail.OlValue = "Deleted";
                        break;


                }

            }
        }





    }
}
