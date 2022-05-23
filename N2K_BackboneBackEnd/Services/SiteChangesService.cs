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


        public async Task<List<SiteChangeDb>> GetSiteChangesAsync(SiteChangeStatus? status)
        {
            List<SiteChangeDb> changes = await _dataContext.Set<SiteChangeDb>().ToListAsync();
            if (status!= null)
                changes = changes.Where(s=> s.Status==status).ToList();

            //order the changes so that the first codes are the one with the hisgest Level value (1. Critical 2. Warning 3. Info)
            var orderedChanges = (from t in changes
                                group t by t.SiteCode
                                into g
                                select new 
                                {                                          
                                    SiteCode = g.Key,
                                    Level = (from t2 in g select t2.Level).Max(),
                                    //Nest all changes of each sitecode ordered by Level
                                    ChangeList = g.Where(s => s.SiteCode == g.Key).OrderByDescending(x => (int)x.Level).ToList()
                                }).OrderByDescending(a => a.Level).ToList();

            var result = new List<SiteChangeDb>();
            var siteCode = string.Empty;
            foreach (var sCode in orderedChanges)
            {
                //load all the changes for each of the site codes ordered by level
                var siteChange = new SiteChangeDb();
                var count = 0;
                if (sCode.ChangeList == null) continue;
                foreach (var change in sCode.ChangeList)
                {

                    if (count==0)
                    {                        
                        siteChange.NumChanges = 1;
                        siteChange.ChangeId = 0;
                        siteChange.SiteCode = change.SiteCode;
                        siteCode = change.SiteCode;
                        siteChange.ChangeCategory = "";
                        siteChange.ChangeType = "";
                        siteChange.Country = "Austria"; // change.Country;
                        siteChange.Level = null;
                        siteChange.Status = null;
                        siteChange.Tags = "";
                        siteChange.Version = change.Version;
                        var changeView = new SiteChangeView
                        {
                             Action ="",
                             SiteCode= "",
                             ChangeCategory= change.ChangeCategory,
                             ChangeType = change.ChangeType,
                             Country = "",                            
                             Level = change.Level,
                             Status = change.Status,
                             Tags = change.Tags
                        };
                        siteChange.subRows = new List<SiteChangeView>( );
                        siteChange.subRows.Add(changeView);
                    }
                    else
                    {
                        siteChange.subRows.Add(new SiteChangeView
                        {
                            ChangeId = change.ChangeId,
                            SiteCode = string.Empty,
                            Action = string.Empty,
                            ChangeCategory = change.ChangeCategory,
                            ChangeType = change.ChangeType,
                            Country = "",
                            Level = change.Level,
                            Status = change.Status,
                            Tags = string.Empty
                        });
                        siteChange.NumChanges++;
                    }
                    count++;
                }
                result.Add(siteChange);
            }
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
                changeDetailVM.Status = (SiteChangeStatus?) site.CurrentStatus;
#pragma warning restore CS8601 // Posible asignación de referencia nula
            }

            var detectedChanges = await _dataContext.Set<SiteChangeDb>().Where(site => site.SiteCode == pSiteCode  && site.Version== pCountryVersion  ).ToListAsync();
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

        


        public async Task<SiteChangeDetailViewModelAdvanced> GetSiteChangesDetailExtended(string pSiteCode, int pCountryVersion)
        {

            var changeDetailVM = new SiteChangeDetailViewModelAdvanced();
            changeDetailVM.SiteCode = pSiteCode;
            changeDetailVM.CountryVersion = pCountryVersion;
            changeDetailVM.Warning = new CategorisedSiteChangeDetail();
            changeDetailVM.Info = new CategorisedSiteChangeDetail();
            changeDetailVM.Critical = new CategorisedSiteChangeDetail();


            var site = await _dataContext.Set<Sites>().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion).FirstOrDefaultAsync();
            if (site != null)
            {
#pragma warning disable CS8601 // Posible asignación de referencia nula
                changeDetailVM.Name = site.Name;
                changeDetailVM.Status = (SiteChangeStatus?)site.CurrentStatus;
#pragma warning restore CS8601 // Posible asignación de referencia nula
            }

            var changesDb = await _dataContext.Set<SiteChangeDb>().Where(site => site.SiteCode == pSiteCode).ToListAsync();


            #region Critical
            changeDetailVM.Critical = FillChangeDetailCategory(changesDb, Level.Critical);
            changeDetailVM.Warning = FillChangeDetailCategory(changesDb, Level.Warning);
            changeDetailVM.Info = FillChangeDetailCategory(changesDb, Level.Info);


            /*
            var levelSites = changesDb.Where(s => s.Level == Level.Critical).ToList();
            foreach (var levelSite in levelSites)
            {
                var CriticalChangeDetail = new CategorisedSiteChangeDetail
                {

                    ChangeCategory = levelSite.ChangeCategory,
                    ChangeId = levelSite.ChangeId,
                    ChangeType = levelSite.ChangeType,
                    Description = "",
                    Level = Level.Critical,
                    ReportedValue = levelSite.NewValue ?? null,
                    OlValue = levelSite.OldValue ?? null,
                };
                changeDetailVM.Critical.Add(CriticalChangeDetail);
            }
            */
            #endregion

            
            #region Warning
            /*
            var levelSitesSiteInfo = changesDb.Where(s => s.Level == Level.Warning && 
                (s.ChangeCategory== "Site General Info" ||  s.ChangeCategory== "Change of area" || s.ChangeCategory == "Site Added" || s.ChangeCategory =="Site Deleted" )
            ).ToList();
            */
            /*



            changeDetailVM.Warning.SiteInfo = new List<CategoryChangeDetail>();


            changeDetailVM.Warning.Habitats = new List<CategoryChangeDetail>();
            var levelHabitats = (from t in changesDb
                                  where t.ChangeCategory == "Habitats" && t.Level == Level.Warning
                                  group t by t.ChangeType
                                  into g
                                  select new
                                  {
                                      ChangeType = g.Key,
                                      ChangeList = g.Where(s => s.ChangeType == g.Key).ToList()
                                  }).ToList();
            foreach (var changeCat in levelHabitats)
            {

                var warnHabitats = new CategoryChangeDetail();
                warnHabitats.ChangeCategory = "Habitats";
                warnHabitats.FieldName = "";
                warnHabitats.ChangeType = changeCat.ChangeType;
                warnHabitats.AddedCodes = new List<CodeAddedDetail>();
                warnHabitats.ChangedCodes = new List<CodeChangeDetail>();
                foreach (var changedItem in changeCat.ChangeList) {

                    if (changeCat.ChangeType.IndexOf("Added") > -1)
                    {
                        warnHabitats.AddedCodes.Add(new CodeAddedDetail
                        {
                            Code = changedItem.NewValue,
                            ChangeId = changedItem.ChangeId
                        });    
                    }
                    else
                    {
                        warnHabitats.ChangedCodes.Add(
                            new CodeChangeDetail
                            {
                                 ChangeId= changedItem.ChangeId,
                                 OlValue=changedItem.OldValue,
                                 ReportedValue= changedItem.NewValue
                            }    
                        );
                    }
                }
                changeDetailVM.Warning.Habitats.Add(warnHabitats);
            }



            changeDetailVM.Warning.Species = new List<CategoryChangeDetail>();


            */

            /*
            foreach (var levelSite in levelSitesSiteInfo)
            {
                var changeDetail = new CategoryChangeDetail();
                changeDetail.ChangeType = levelSite.ChangeType;
                changeDetail.ChangeCategory = levelSite.ChangeCategory;
                changeDetail.FieldName = "";
                changeDetail.AddedCodes = new List<CodeAddedDetail>();
                changeDetail.ChangedCodes = new List<CodeChangeDetail>();
            }
            */
            #endregion

            /*


            #region Info
            levelSites = changesDb.Where(s => s.Level == Level.Info).ToList();
            changeDetailVM.Info.Habitats = new List<CategoryChangeDetail>();
            changeDetailVM.Info.Species = new List<CategoryChangeDetail>();
            changeDetailVM.Info.SiteInfo = new List<CategoryChangeDetail>();
            changeDetailVM.Warning.Habitats = new List<CategoryChangeDetail>();
            var levelHabitats = (from t in changesDb
                                  where t.ChangeCategory == "Habitats" && t.Level == Level.Info
                                  group t by t.ChangeType
                                  into g
                                  select new
                                  {
                                      ChangeType = g.Key,
                                      ChangeList = g.Where(s => s.ChangeType == g.Key).ToList()
                                  }).ToList();
            foreach (var changeCat in levelHabitats)
            {

                var warnHabitats = new CategoryChangeDetail();
                warnHabitats.ChangeCategory = "Habitats";
                warnHabitats.FieldName = "";
                warnHabitats.ChangeType = changeCat.ChangeType;
                warnHabitats.ChangedCodes = new List<CodeChangeDetail>();
                if 


                warnHabitats.ChangedCodes = new List<CodeChangeDetail>();






                //changeDetailVM.Warning.Habitats.Add(changeCat);
            }



            #endregion




            var oldSite = await _dataContext.Set<Sites>().Where(site => site.SiteCode == pSiteCode && site.Current == true).FirstOrDefaultAsync();


            var detectedChanges = await _dataContext.Set<SiteChangeDb>().Where(site => site.SiteCode == pSiteCode).ToListAsync();
            if (detectedChanges != null)
            {
                foreach (var change in detectedChanges)
                {
                    var changeDetail = new ChangeDetail();
                    changeDetail.ChangeId = change.ChangeId;
                    changeDetail.Level = change.Level;
                    changeDetail.ChangeType = change.ChangeType != null ? change.ChangeType.ToString() : String.Empty;
                    changeDetail.ChangeCategory = change.ChangeCategory != null ? change.ChangeCategory.ToString() : String.Empty;
                    changeDetail.FieldName = String.Empty;
                    changeDetail.ReportedValue = String.Empty;
                    changeDetail.OlValue = String.Empty;
                    changeDetail.Description = String.Empty;
                    FillChangeDetail(site, oldSite, change, ref changeDetail);
                    changeDetailVM.ChangesList.Add(changeDetail);
                }
            }
            */
            return changeDetailVM;

        }
            

        private CategorisedSiteChangeDetail FillChangeDetailCategory(List<SiteChangeDb> changesDB, Level level) 
        {

            var changedPerCategories = new CategorisedSiteChangeDetail();
            changedPerCategories.SiteInfo = new List<CategoryChangeDetail>();
            changedPerCategories.Species = new List<CategoryChangeDetail>();
            changedPerCategories.Habitats = new List<CategoryChangeDetail>();


            var levelDetails = (from t in changesDB
                                 where t.Level == level
                                 group t by  new { t.ChangeCategory, t.ChangeType }
                                 into g
                                 select new
                                 {
                                     ChangeCategory = g.Key.ChangeCategory,
                                     ChangeType = g.Key.ChangeType,
                                     ChangeList = g.Where(s => s.ChangeType == g.Key.ChangeType  && s.ChangeCategory== g.Key.ChangeCategory ).ToList()
                                 }).ToList();
            foreach (var changeCat in levelDetails)
            {

                var warnHabitats = new CategoryChangeDetail();
                warnHabitats.ChangeCategory = changeCat.ChangeCategory;
                warnHabitats.FieldName = "";
                warnHabitats.ChangeType = changeCat.ChangeType ;
                warnHabitats.AddedCodes = new List<CodeAddedDetail>();
                warnHabitats.ChangedCodes = new List<CodeChangeDetail>();
                foreach (var changedItem in changeCat.ChangeList)
                {

                    if (changeCat.ChangeType.IndexOf("Added") > -1)
                    {
                        warnHabitats.AddedCodes.Add(new CodeAddedDetail
                        {
                            Code = changedItem.NewValue,
                            ChangeId = changedItem.ChangeId
                        });
                    }
                    else
                    {
                        warnHabitats.ChangedCodes.Add(
                            new CodeChangeDetail
                            {
                                ChangeId = changedItem.ChangeId,
                                OlValue = changedItem.OldValue,
                                ReportedValue = changedItem.NewValue
                            }
                        );
                    }
                }
                
            }

            return changedPerCategories;
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
                                detail.ReportedValue =siteChangeDb.NewValue;
                                detail.OlValue = siteChangeDb.OldValue;
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
                        detail.ReportedValue = siteChangeDb.SiteCode;
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
                detail.ReportedValue = siteChangeDb.NewValue ?? null;
                detail.OlValue = siteChangeDb.OldValue ?? null;
            }
        }



        public async Task<List<ModifiedSiteCode>> AcceptChanges(ModifiedSiteCode[] changedSiteStatus)
        {

            List<ModifiedSiteCode> result = new List<ModifiedSiteCode>();
            try
            {
                foreach (var modifiedSiteCode in changedSiteStatus)
                {

                    try
                    {
                        var paramSiteCode = new SqlParameter("@sitecode", modifiedSiteCode.SiteCode);
                        var paramVersionId = new SqlParameter("@version", modifiedSiteCode.VersionId );

                        await  _dataContext.Database.ExecuteSqlRawAsync(
                                "exec spAcceptSiteCodeChanges @sitecode, @version",
                                paramSiteCode,
                                paramVersionId);
                        modifiedSiteCode.OK = 1;
                        modifiedSiteCode.Error = string.Empty;
                        modifiedSiteCode.Status = SiteChangeStatus.Accepted;
                    }
                    catch (Exception ex)
                    {
                        modifiedSiteCode.OK = 0;
                        modifiedSiteCode.Error = ex.Message;
                    }
                    finally
                    {
                        result.Add(modifiedSiteCode);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ;
            }


        }


        public async Task<List<ModifiedSiteCode>> RejectChanges(ModifiedSiteCode[] changedSiteStatus)
        {
            List<ModifiedSiteCode> result = new List<ModifiedSiteCode>();
            try
            {
                foreach (var modifiedSiteCode in changedSiteStatus)
                {

                    try
                    {
                        var paramSiteCode = new SqlParameter("@sitecode", modifiedSiteCode.SiteCode);
                        var paramVersionId = new SqlParameter("@version", modifiedSiteCode.VersionId);

                        await _dataContext.Database.ExecuteSqlRawAsync(
                                "exec spRejectSiteCodeChanges @sitecode, @version",
                                paramSiteCode,
                                paramVersionId);
                        modifiedSiteCode.OK = 1;
                        modifiedSiteCode.Error = string.Empty;
                        modifiedSiteCode.Status = SiteChangeStatus.Rejected;
                    }
                    catch (Exception ex)
                    {
                        modifiedSiteCode.OK = 0;
                        modifiedSiteCode.Error = ex.Message;
                    }
                    finally
                    {
                        result.Add(modifiedSiteCode);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }


        }



    }
}
