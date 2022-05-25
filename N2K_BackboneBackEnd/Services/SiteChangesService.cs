using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;

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
            if (status != null)
                changes = changes.Where(s => s.Status == status).ToList();

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
            var countries = await _dataContext.Set<Countries>().ToListAsync();
            var siteCode = string.Empty;
            foreach (var sCode in orderedChanges)
            {
                //load all the changes for each of the site codes ordered by level
                var siteChange = new SiteChangeDb();
                var count = 0;
                if (sCode.ChangeList == null) continue;
                foreach (var change in sCode.ChangeList)
                {

                    if (count == 0)
                    {
                        siteChange.NumChanges = 1;
                        siteChange.ChangeId = 0;
                        siteChange.SiteCode = change.SiteCode;
                        siteCode = change.SiteCode;
                        siteChange.ChangeCategory = "";
                        siteChange.ChangeType = "";
                        siteChange.Country = "";
                        if (change.Country != null)
                        {
                            var countryName = countries.Where(ctry => ctry.Code.ToLower() == change.Country.ToLower()).FirstOrDefault();
                            siteChange.Country = countryName != null ? countryName.Country : change.Country;
                        }
                        siteChange.Level = null;
                        siteChange.Status = null;
                        siteChange.Tags = "";
                        siteChange.Version = change.Version;
                        var changeView = new SiteChangeView
                        {
                            Action = "",
                            SiteCode = "",
                            ChangeCategory = change.ChangeCategory,
                            ChangeType = change.ChangeType,
                            Country = "",
                            Level = change.Level,
                            Status = change.Status,
                            Tags = change.Tags
                        };
                        siteChange.subRows = new List<SiteChangeView>();
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


        public async Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion)
        {
            var changeDetailVM = new SiteChangeDetailViewModel();
            changeDetailVM.SiteCode = pSiteCode;
            changeDetailVM.Version = pCountryVersion;
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


            changeDetailVM.Critical = FillChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Critical);
            changeDetailVM.Warning = FillChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Warning);
            changeDetailVM.Info = FillChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Info);

            return changeDetailVM;

        }


        private CategorisedSiteChangeDetail FillChangeDetailCategory(List<SiteChangeDb> changesDB, string pSiteCode, int pCountryVersion, Level level)
        {

            var changedPerCategories = new CategorisedSiteChangeDetail();
            changedPerCategories.Level = level;
            changedPerCategories.SiteInfo = new List<CategoryChangeDetail>();
            changedPerCategories.Species = new List<CategoryChangeDetail>();
            changedPerCategories.Habitats = new List<CategoryChangeDetail>();


            var levelDetails = (from t in changesDB
                                where t.Level == level
                                group t by new { t.Section, t.ChangeCategory, t.ChangeType }
                                 into g
                                select new
                                {
                                    ChangeCategory = g.Key.ChangeCategory,
                                    ChangeType = g.Key.ChangeType,
                                    Section = g.Key.Section,
                                    ChangeList = g.Where(s => s.Section == g.Key.Section && s.ChangeType == g.Key.ChangeType && s.ChangeCategory == g.Key.ChangeCategory).ToList()
                                }).ToList();
            foreach (var changeCat in levelDetails)
            {

                var changeDetail = new CategoryChangeDetail();
                changeDetail.ChangeCategory = changeCat.ChangeCategory;
                changeDetail.FieldName = "";
                changeDetail.ChangeType = changeCat.ChangeType;
                changeDetail.AddedCodes = new List<CodeAddedRemovedDetail>();
                changeDetail.DeletedCodes = new List<CodeAddedRemovedDetail>();
                changeDetail.ChangedCodes = new List<CodeChangeDetail>();
                foreach (var changedItem in changeCat.ChangeList.OrderBy(c=> c.Code==null?"":c.Code ))
                {
                    if (changeCat.ChangeType.IndexOf("Added") > -1)
                    {
                        changeDetail.AddedCodes.Add(
                            CodeAddedRemovedDetail(changeCat.Section, changedItem.Code, changedItem.ChangeId, pSiteCode, pCountryVersion)
                        );
                    }
                    else if (changeCat.ChangeType.IndexOf("Deleted") > -1)
                    {

                        //it needs amending to catch the record value it was deleted 
                        changeDetail.DeletedCodes.Add(
                            CodeAddedRemovedDetail(changeCat.Section, changedItem.Code, changedItem.ChangeId, pSiteCode, pCountryVersion)
                        );
                    }
                    else
                    {

                        changeDetail.ChangedCodes.Add(
                            new CodeChangeDetail
                            {
                                Code = changedItem.Code,
                                Name = GetCodeName(changedItem) ,
                                ChangeId = changedItem.ChangeId,
                                OlValue = changedItem.OldValue,
                                ReportedValue = changedItem.NewValue
                            }
                        ) ;
                    }
                }
                switch (changeCat.Section)
                {

                    case "Site":
                        changedPerCategories.SiteInfo.Add(changeDetail);
                        break;

                    case "Species":
                        changedPerCategories.Species.Add(changeDetail);
                        break;

                    case "Habitats":
                        changedPerCategories.Habitats.Add(changeDetail);
                        break;
                }
            }

            return changedPerCategories;
        }



        private string? GetCodeName(SiteChangeDb change)
        {
            if (change.Code == null) return "";
            var name = "";
            switch (change.Section)
            {
                case "Site":
                    name = "";
                    break;

                case "Species":
                    if (_dataContext.Set<SpeciesTypes>().FirstOrDefault(sp => sp.Code.ToLower() == change.Code.ToLower()) != null)
                    {
                        name = _dataContext.Set<SpeciesTypes>().FirstOrDefault(sp => sp.Code.ToLower() == change.Code.ToLower()).Name;
                    }
                    break;

                case "Habitats":
                    if (_dataContext.Set<HabitatTypes>().FirstOrDefault(hab => hab.Code.ToLower() == change.Code.ToLower()) != null)
                        name = _dataContext.Set<HabitatTypes>().FirstOrDefault(hab => hab.Code.ToLower() == change.Code.ToLower()).Name;
                    break;

                default:
                    name = "";
                    break;


            }
            return name;
        }


        private CodeAddedRemovedDetail CodeAddedRemovedDetail(string section, string? code, long changeId, string pSiteCode, int pCountryVersion)
        {
            var codeValues = new Dictionary<string, string>();
            switch (section)
            {
                case "Species":
                    if (code != null)
                    {
                        var specName = "";
                        var spectype = _dataContext.Set<SpeciesTypes>().FirstOrDefault(s => s.Code.ToLower() == code.ToLower()).Name;
                        if (spectype != null) specName = spectype;

                        var specDetails = _dataContext.Set<Species>().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion && site.SpecieCode.ToLower() == code.ToLower())
                            .Select(spc => new
                            {
                                Population = spc.Population,
                                SpecType = spc.SpecieType
                            });
                        if (specDetails != null && specDetails.FirstOrDefault() != null)
                        {
                            codeValues.Add("Name", specName);
                            codeValues.Add("Population", specDetails.FirstOrDefault().Population);
                            codeValues.Add("SpeciesType", specDetails.FirstOrDefault().SpecType);
                        }
                    }
                    break;

                case "Habitats":
                    if (code != null)
                    {

                        var habName = "";
                        var habType = _dataContext.Set<HabitatTypes>().Where(s => s.Code.ToLower() == code.ToLower()).Select(spc => spc.Name).FirstOrDefault();
                        if (habType != null) habName = habType;

                        var habDetails = _dataContext.Set<Habitats>().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion && site.HabitatCode.ToLower() == code.ToLower())
                            .Select(hab => new
                            {
                                CoverHA = hab.CoverHA.ToString(),
                                RelativeSurface = hab.RelativeSurface
                            });
                        if (habDetails != null)
                        {
                            codeValues.Add("C", habName);
                            codeValues.Add("RelSurface", habDetails.FirstOrDefault().RelativeSurface);
                            codeValues.Add("Cover", habDetails.FirstOrDefault().CoverHA);
                        }
                    }
                    break;
            }

            return new CodeAddedRemovedDetail
            {
                Code = code,
                ChangeId = changeId,
                CodeValues = codeValues
            };

        }


        private void FillChangeDetail(Sites? site, Sites? oldSite, SiteChangeDb siteChangeDb, ref ChangeDetail detail)
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
                                detail.ReportedValue = siteChangeDb.NewValue;
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
                        var paramVersionId = new SqlParameter("@version", modifiedSiteCode.VersionId);

                        await _dataContext.Database.ExecuteSqlRawAsync(
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
                throw;
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
