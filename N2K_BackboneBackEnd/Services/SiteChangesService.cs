using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.Reflection;

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
            changeDetailVM.Warning = new  SiteChangesLevelDetail();
            changeDetailVM.Info = new SiteChangesLevelDetail();
            changeDetailVM.Critical = new SiteChangesLevelDetail();


            var site = await _dataContext.Set<Sites>().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion).FirstOrDefaultAsync();
            if (site != null)
            {
#pragma warning disable CS8601 // Posible asignación de referencia nula
                changeDetailVM.Name = site.Name;
                changeDetailVM.Status = (SiteChangeStatus?)site.CurrentStatus;
#pragma warning restore CS8601 // Posible asignación de referencia nula
            }
            var changesDb = await _dataContext.Set<SiteChangeDb>().Where(site => site.SiteCode == pSiteCode).ToListAsync();


            changeDetailVM.Critical = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Critical);
            changeDetailVM.Warning = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Warning);
            changeDetailVM.Info = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Info);

            return changeDetailVM;

        }



        private SiteChangesLevelDetail FillLevelChangeDetailCategory(List<SiteChangeDb> changesDB, string pSiteCode, int pCountryVersion, Level level)
        {

            var changesPerLevel = new SiteChangesLevelDetail();
            changesPerLevel.Level = level;


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
            
            foreach (var _levelDetail in levelDetails)
            {
                switch (_levelDetail.Section)
                {
                    case "Site":
                        /*
                        if (_levelDetail.ChangeType.IndexOf("Added") > -1)
                        {
                            if (string.IsNullOrEmpty(changesPerLevel.SiteInfo.AddedCodes.ChangeCategory)) changesPerLevel.SiteInfo.AddedCodes.ChangeCategory = "Site Added";
                            foreach (var changedItem in _levelDetail.ChangeList.OrderBy(c => c.Code == null ? "" : c.Code))
                            {
                                changesPerLevel.SiteInfo.AddedCodes.CodeList.Add(
                                    new CodeAddedRemovedDetail
                                    {
                                        Code = changedItem.Code,
                                        CodeValues = new Dictionary<string, string>()
                                    }
                                );
                            }
                        }
                        else if (_levelDetail.ChangeType.IndexOf("Deleted") > -1)
                        {
                            if (string.IsNullOrEmpty(changesPerLevel.SiteInfo.AddedCodes.ChangeCategory)) changesPerLevel.SiteInfo.AddedCodes.ChangeCategory = "Site Deleted";
                            foreach (var changedItem in _levelDetail.ChangeList.OrderBy(c => c.Code == null ? "" : c.Code))
                            {
                                changesPerLevel.SiteInfo.AddedCodes.CodeList.Add(
                                    new CodeAddedRemovedDetail
                                    {
                                        Code = changedItem.Code,
                                        CodeValues = new Dictionary<string, string>()
                                    }
                                ); ;
                            }
                        }
                        else
                        {
                            changesPerLevel.SiteInfo.ChangesByCategory.Add(GetChangeCategoryDetail(_levelDetail.ChangeCategory, _levelDetail.ChangeType, _levelDetail.ChangeList));
                        }
                        */
                        changesPerLevel.SiteInfo.ChangesByCategory.Add(GetChangeCategoryDetail(_levelDetail.ChangeCategory, _levelDetail.ChangeType, _levelDetail.ChangeList));
                        break;

                    case "Species" or "Habitats": 
                        Type sectionType = typeof(SiteChangesLevelDetail);                   
                        PropertyInfo sectionPropInfo = sectionType.GetProperty(_levelDetail.Section);
                        SectionChangeDetail _Section = (SectionChangeDetail)sectionPropInfo.GetValue(changesPerLevel, null);

                        if (_Section == null) continue;

                        if (_levelDetail.ChangeType.IndexOf("Added") <= -1)
                        {
                            if (_levelDetail.ChangeType.IndexOf("Deleted") > -1)
                            {
                                if (string.IsNullOrEmpty(_Section.AddedCodes.ChangeCategory)) _Section.AddedCodes.ChangeCategory = String.Format("List of {0} Deleted", _levelDetail.Section);
                                foreach (var changedItem in _levelDetail.ChangeList.OrderBy(c => c.Code == null ? "" : c.Code))
                                {
                                    _Section.DeletedCodes.CodeList.Add(
                                        CodeAddedRemovedDetail(_levelDetail.Section, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.Version)
                                    );
                                }
                            }
                            else
                            {
                                _Section.ChangesByCategory.Add(GetChangeCategoryDetail(_levelDetail.ChangeCategory, _levelDetail.ChangeType, _levelDetail.ChangeList));
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(_Section.AddedCodes.ChangeCategory)) _Section.AddedCodes.ChangeCategory = String.Format("List of {0} Added", _levelDetail.Section);
                            foreach (var changedItem in _levelDetail.ChangeList.OrderBy(c => c.Code == null ? "" : c.Code))
                            {
                                _Section.AddedCodes.CodeList.Add(
                                    CodeAddedRemovedDetail(_levelDetail.Section, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.Version)
                                );
                            }
                        }
                        break;
                    

                }

            }

            return changesPerLevel;
        }




        private CategoryChangeDetail GetChangeCategoryDetail(string changeCategory, string changeType, List<SiteChangeDb> changeList)
        {
            var catChange = new CategoryChangeDetail();
            catChange.ChangeType = changeType;
            catChange.ChangeCategory = changeCategory;
            catChange.ChangedCodesDetail = new List<CodeChangeDetail>();

            foreach (var changedItem in changeList.OrderBy(c => c.Code == null ? "" : c.Code))
            {
                catChange.ChangedCodesDetail.Add(
                new CodeChangeDetail
                {
                    Code = changedItem.Code,
                    Name = GetCodeName(changedItem),
                    ChangeId = changedItem.ChangeId,
                    OlValue = changedItem.OldValue,
                    ReportedValue = changedItem.NewValue
                });
            }

            return catChange;
        }

        private string? GetCodeName(SiteChangeDb change)
        {
            if (change.Code == null) return "";
            var name = "";
            switch (change.Section)
            {
                case "Site":
                    if (_dataContext.Set<Sites>().FirstOrDefault(sp => sp.SiteCode.ToLower() == change.Code.ToLower() && sp.Version == change.Version) != null) {
                        name = _dataContext.Set<Sites>().FirstOrDefault(sp => sp.SiteCode.ToLower() == change.Code.ToLower() && sp.Version == change.Version).Name;
                    }
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
