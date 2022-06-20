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


        private class OrderedChanges
        {
            public string SiteCode { get; set; } = "";
            public string SiteName { get; set; } = "";
            public Level? Level { get; set; }
            public List<SiteChangeDbNumsperLevel> ChangeList { get; set; } = new List<SiteChangeDbNumsperLevel>();

        }


        private readonly N2KBackboneContext _dataContext;
        private readonly IEnumerable<SpeciesTypes> _speciesTypes;
        private readonly IEnumerable<HabitatTypes> _habitatTypes;
        private readonly IEnumerable<Countries> _countries;
        private IEnumerable<Habitats>? _siteHabitats;
        private IEnumerable<Species>? _siteSpecies;


        public SiteChangesService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
            _speciesTypes = _dataContext.Set<SpeciesTypes>().AsNoTracking().ToList();
            _habitatTypes = _dataContext.Set<HabitatTypes>().AsNoTracking().ToList();
            _countries = _dataContext.Set<Countries>().AsNoTracking().ToList();
        }


        public async Task<List<SiteChangeDb>> GetSiteChangesAsync(string country, SiteChangeStatus? status, Level? level, int page = 1, int pageLimit = 0)
        {
            //call a stored procedure that returs the site changes that match the given criteria                        
            var startRow = (page - 1) * pageLimit;

            SqlParameter param1 = new SqlParameter("@country", country);
            SqlParameter param2 = new SqlParameter("@status", status.HasValue ? status.ToString() : String.Empty);
            SqlParameter param3 = new SqlParameter("@level", level.HasValue ? level.ToString() : String.Empty);

            IQueryable<SiteChangeDbNumsperLevel> changes = _dataContext.Set<SiteChangeDbNumsperLevel>().FromSqlRaw($"exec dbo.spGetChangesByCountryAndStatusAndLevel  @country, @status, @level",
                            param1, param2, param3);

            IEnumerable<OrderedChanges> orderedChanges;
            //order the changes so that the first codes are the one with the highest Level value (1. Critical 2. Warning 3. Info)
            //It return an enumeration of sitecodes with a nested list of the changes for that sitecode, ordered by level
            IOrderedEnumerable<OrderedChanges> orderedChangesEnum = (from t in await changes.ToListAsync()
                                                                     group t by new { t.SiteCode, t.SiteName }
                                                                     into g
                                                                     select new OrderedChanges
                                                                     {
                                                                         SiteCode = g.Key.SiteCode,
                                                                         SiteName= g.Key.SiteName,
                                                                         Level = (from t2 in g select t2.Level).Max()
                                                                         
                                                                         ,
                                                                         //Nest all changes of each sitecode ordered by Level
                                                                         ChangeList = g.Where(s => s.SiteCode.ToUpper() == g.Key.SiteCode.ToUpper()).OrderByDescending(x => (int)x.Level).ToList()
                                                                     }).OrderByDescending(a => a.Level).ThenBy(b => b.SiteCode);
            if (pageLimit != 0)
            {
                orderedChanges = orderedChangesEnum
                        .Skip(startRow)
                        .Take(pageLimit)
                        .ToList();
            }
            else
                orderedChanges = orderedChangesEnum.ToList();


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
                    if (count == 0)
                    {
                        siteChange.NumChanges = 1;
                        siteChange.ChangeId = 0;
                        siteChange.SiteName = change.SiteName;
                        siteChange.SiteCode = change.SiteCode;
                        siteCode = change.SiteCode;
                        siteChange.ChangeCategory = "";
                        siteChange.ChangeType = "";
                        siteChange.Country = "";
                        siteChange.JustificationProvided = change.JustificationProvided;
                        siteChange.JustificationRequired = change.JustificationRequired;
                        if (change.Country != null)
                        {
                            var countryName = _countries.Where(ctry => ctry.Code.ToLower() == change.Country.ToLower()).FirstOrDefault();
                            siteChange.Country = countryName != null ? countryName.Country : change.Country;
                        }
                        siteChange.Level = null;
                        siteChange.Status = null;
                        siteChange.Tags = "";
                        siteChange.Version = change.Version;
                        var changeView = new SiteChangeView
                        {
                            ChangeId = change.ChangeId,
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
                        if (!siteChange.subRows.Any(ch => ch.ChangeCategory == change.ChangeCategory && ch.ChangeType==change.ChangeType))
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
                                Tags = string.Empty,
                                NumChanges=1
                            });
                        }
                        else
                        {
                            siteChange.subRows.Where(ch => ch.ChangeCategory == change.ChangeCategory && ch.ChangeType == change.ChangeType).FirstOrDefault().NumChanges++;
                        }
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
            return await _dataContext.Set<SiteChangeDb>().AsNoTracking().SingleOrDefaultAsync(s => s.ChangeId == id);
        }


        public async Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion)
        {
            var changeDetailVM = new SiteChangeDetailViewModel();
            changeDetailVM.SiteCode = pSiteCode;
            changeDetailVM.Version = pCountryVersion;
            changeDetailVM.Warning = new SiteChangesLevelDetail();
            changeDetailVM.Info = new SiteChangesLevelDetail();
            changeDetailVM.Critical = new SiteChangesLevelDetail();


            var site = await _dataContext.Set<Sites>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion).FirstOrDefaultAsync();
            if (site != null)
            {
#pragma warning disable CS8601 // Posible asignación de referencia nula
                changeDetailVM.Name = site.Name;
                changeDetailVM.Status = (SiteChangeStatus?)site.CurrentStatus;
                changeDetailVM.JustificationProvided = site.JustificationProvided.HasValue ? site.JustificationProvided.Value : false;
                changeDetailVM.JustificationRequired = site.JustificationRequired.HasValue ? site.JustificationRequired.Value : false;
#pragma warning restore CS8601 // Posible asignación de referencia nula
            }
            var changesDb = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == pSiteCode).ToListAsync();


            _siteHabitats = await _dataContext.Set<Habitats>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion).ToListAsync();
            _siteSpecies = await _dataContext.Set<Species>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion).ToListAsync();

            changeDetailVM.Critical = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Critical);
            changeDetailVM.Warning = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Warning);
            changeDetailVM.Info = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Info);


            _siteHabitats = null;
            _siteSpecies = null;

            return changeDetailVM;
        }


        public async Task<List<SiteCodeView>> GetSiteCodesByStatusAndLevelAndCountry(string country, SiteChangeStatus? status, Level? level)
        {

            var result = new List<SiteCodeView>();
            var siteChangesQuery = _dataContext.Set<SiteChangeDb>().AsNoTracking();
            if (!string.IsNullOrEmpty(country)) siteChangesQuery = siteChangesQuery.Where(s => s.Country == country);
            if (status.HasValue) siteChangesQuery = siteChangesQuery.Where(s => s.Status == status.Value);


            var query = from o in siteChangesQuery
                        group o by new { o.SiteCode, o.Version } into g
                        select new
                        {
                            SiteCode = g.Key.SiteCode,
                            Version = g.Key.Version,
                            NumInfo = g.Sum(d => d.Level == Level.Info ? (Int32?)1 : 0),
                            NumCritical = g.Sum(d => d.Level == Level.Critical ? (Int32?)1 : 0),
                            NumWarning = g.Sum(d => d.Level == Level.Warning ? (Int32?)1 : 0),
                        };

            var list = await query.OrderByDescending(a => a.NumCritical).ThenByDescending(b => b.NumWarning).ThenByDescending(c => c.NumInfo).ToListAsync();
            switch (level)
            {
                case Level.Critical:

                    return list.Where(a => a.NumCritical > 0).Select(x =>
                         new SiteCodeView
                         {
                             //CountryCode = .Country,
                             SiteCode = x.SiteCode,
                             Version = x.Version
                         }
                    ).ToList();

                case Level.Warning:
                    return list.Where(a => a.NumCritical == 0 && a.NumWarning > 0).Select(x =>
                        new SiteCodeView
                        {
                            //CountryCode = .Country,
                            SiteCode = x.SiteCode,
                            Version = x.Version
                        }
                    ).ToList();

                case Level.Info:
                    return list.Where(a => a.NumCritical == 0 && a.NumWarning == 0 && a.NumInfo > 0).Select(x =>
                        new SiteCodeView
                        {
                            //CountryCode = .Country,
                            SiteCode = x.SiteCode,
                            Version = x.Version
                        }
                    ).ToList();

                default:
                    return list.Select(x =>
                        new SiteCodeView
                        {
                            //CountryCode = .Country,
                            SiteCode = x.SiteCode,
                            Version = x.Version
                        }
                    ).ToList();
            }
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
                SectionChangeDetail _Section = null;
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

                    case "Species":
                        _Section = changesPerLevel.Species;
                        break;
                    case "Habitats":
                        _Section = changesPerLevel.Habitats;
                        break;
                }
                if (_Section == null)
                {
                    continue;
                }

                if (_levelDetail.ChangeType.IndexOf("Added") <= -1)
                {
                    if (_levelDetail.ChangeType.IndexOf("Deleted") > -1)
                    {
                        if (_Section.DeletedCodes.Count == 0)
                        {
                            _Section.DeletedCodes.Add(new CategoryChangeDetail
                            {
                                ChangeCategory = _levelDetail.Section,
                                ChangeType = String.Format("List of {0} Deleted", _levelDetail.Section),
                                ChangedCodesDetail = new List<CodeChangeDetail>()
                            });
                        }

                        foreach (var changedItem in _levelDetail.ChangeList.OrderBy(c => c.Code == null ? "" : c.Code))
                        {
                            _Section.DeletedCodes.ElementAt(0).ChangedCodesDetail.Add(
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
                    if (_Section.AddedCodes.Count == 0)
                    {
                        _Section.AddedCodes.Add(new CategoryChangeDetail
                        {
                            ChangeCategory = _levelDetail.Section,
                            ChangeType = String.Format("List of {0} Added", _levelDetail.Section),
                            ChangedCodesDetail = new List<CodeChangeDetail>()
                        });
                    }

                    foreach (var changedItem in _levelDetail.ChangeList.OrderBy(c => c.Code == null ? "" : c.Code))
                    {
                        _Section.AddedCodes.ElementAt(0).ChangedCodesDetail.Add(
                            CodeAddedRemovedDetail(_levelDetail.Section, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.Version)
                        );

                    }
                }

            }

            return changesPerLevel;
        }




        private CategoryChangeDetail GetChangeCategoryDetail(string changeCategory, string changeType, List<SiteChangeDb> changeList)
        {
            var catChange = new CategoryChangeDetail();
            catChange.ChangeType = changeType;
            catChange.ChangeCategory = changeCategory;

            foreach (var changedItem in changeList.OrderBy(c => c.Code == null ? "" : c.Code))
            {
                var fields = new Dictionary<string, string>();
                fields.Add("Reference", changedItem.OldValue);
                fields.Add("Reported", changedItem.NewValue);


                catChange.ChangedCodesDetail.Add(
                    new CodeChangeDetail
                    {
                        Code = changedItem.Code,
                        Name = GetCodeName(changedItem),
                        ChangeId = changedItem.ChangeId,
                        Fields = fields
                    }

                );
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
                    if (_dataContext.Set<Sites>().FirstOrDefault(sp => sp.SiteCode.ToLower() == change.Code.ToLower() && sp.Version == change.Version) != null)
                    {
                        name = _dataContext.Set<Sites>().FirstOrDefault(sp => sp.SiteCode.ToLower() == change.Code.ToLower() && sp.Version == change.Version).Name;
                    }
                    break;

                case "Species":
                    if (_speciesTypes.FirstOrDefault(sp => sp.Code.ToLower() == change.Code.ToLower()) != null)
                    {
                        name = _speciesTypes.FirstOrDefault(sp => sp.Code.ToLower() == change.Code.ToLower()).Name;
                    }
                    break;

                case "Habitats":
                    if (_habitatTypes.FirstOrDefault(hab => hab.Code.ToLower() == change.Code.ToLower()) != null)
                        name = _habitatTypes.FirstOrDefault(hab => hab.Code.ToLower() == change.Code.ToLower()).Name;
                    break;

                default:
                    name = "";
                    break;


            }
            return name;
        }


        private CodeChangeDetail? CodeAddedRemovedDetail(string section, string? code, long changeId, string pSiteCode, int pCountryVersion)
        {
            var fields = new Dictionary<string, string>();
            switch (section)
            {
                case "Species":
                    string? specName = null;
                    string? population = null;
                    string? specType = null;

                    if (code != null)
                    {
                        var spectype = _speciesTypes.FirstOrDefault(s => s.Code.ToLower() == code.ToLower()).Name;
                        if (spectype != null) specName = spectype;

                        var specDetails = _siteSpecies.Where(sp => sp.SpecieCode.ToLower() == code.ToLower())
                            .Select(spc => new
                            {
                                Population = spc.Population,
                                SpecType = spc.SpecieType
                            }).FirstOrDefault();
                        if (specDetails != null)
                        {
                            population = specDetails.Population;
                            specType = specDetails.SpecType;
                        }
                    }
                    fields.Add("Population", population);
                    fields.Add("SpeciesType", specType);

                    return new CodeChangeDetail
                    {
                        ChangeId = changeId,
                        Code = code,
                        Name = specName,
                        Fields = fields

                    };

                case "Habitats":
                    string? habName = null;
                    string? coverHa = null;
                    string? relSurface = null;
                    if (code != null)
                    {

                        var habType = _habitatTypes.Where(s => s.Code.ToLower() == code.ToLower()).Select(spc => spc.Name).FirstOrDefault();
                        if (habType != null) habName = habType;

                        var habDetails = _siteHabitats.Where(sh => sh.HabitatCode.ToLower() == code.ToLower())
                            .Select(hab => new
                            {
                                CoverHA = hab.CoverHA.ToString(),
                                RelativeSurface = hab.RelativeSurface
                            }).FirstOrDefault();
                        if (habDetails != null)
                        {
                            relSurface = habDetails.RelativeSurface;
                            coverHa = habDetails.CoverHA;
                        }
                    }
                    fields.Add("CoverHa", coverHa);
                    fields.Add("RelativeSurface", relSurface);

                    return new CodeChangeDetail
                    {
                        ChangeId = changeId,
                        Code = code,
                        Name = habName,
                        Fields = fields
                    };
                    break;
            }

            return null;
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
                        SqlParameter paramSiteCode = new SqlParameter("@sitecode", modifiedSiteCode.SiteCode);
                        SqlParameter paramVersionId = new SqlParameter("@version", modifiedSiteCode.VersionId);

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
                        SqlParameter paramSiteCode = new SqlParameter("@sitecode", modifiedSiteCode.SiteCode);
                        SqlParameter paramVersionId = new SqlParameter("@version", modifiedSiteCode.VersionId);

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


        public async Task<List<ModifiedSiteCode>> MarkAsJustificationRequired(JustificationModel[] justification)
        {
            List<ModifiedSiteCode> result = new List<ModifiedSiteCode>();
            try
            {
                foreach (var just in justification)
                {
                    ModifiedSiteCode modifiedSiteCode = new ModifiedSiteCode();
                    try
                    {
                        SqlParameter paramSiteCode = new SqlParameter("@sitecode", just.SiteCode);
                        SqlParameter paramVersionId = new SqlParameter("@version", just.VersionId);
                        SqlParameter justificationMarked = new SqlParameter("@justififcation", just.Justification);

                        await _dataContext.Database.ExecuteSqlRawAsync(
                                "exec spMarkJustificationRequired @sitecode, @version, @justififcation",
                                paramSiteCode,
                                paramVersionId,
                                justificationMarked
                                );
                        modifiedSiteCode.SiteCode = just.SiteCode;
                        modifiedSiteCode.VersionId = just.VersionId;
                        modifiedSiteCode.OK = 1;
                        modifiedSiteCode.Error = string.Empty;
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


        public async Task<List<ModifiedSiteCode>> JustificationProvided(JustificationModel[] justification)
        {
            List<ModifiedSiteCode> result = new List<ModifiedSiteCode>();
            try
            {
                foreach (var just in justification)
                {
                    ModifiedSiteCode modifiedSiteCode = new ModifiedSiteCode();
                    try
                    {
                        SqlParameter paramSiteCode = new SqlParameter("@sitecode", just.SiteCode);
                        SqlParameter paramVersionId = new SqlParameter("@version", just.VersionId);
                        SqlParameter justificationProvided = new SqlParameter("@justififcation", just.Justification);

                        await _dataContext.Database.ExecuteSqlRawAsync(
                                "exec spJustificationProvided @sitecode, @version, @justififcation",
                                paramSiteCode,
                                paramVersionId,
                                justificationProvided
                                );
                        modifiedSiteCode.SiteCode = just.SiteCode;
                        modifiedSiteCode.VersionId = just.VersionId;
                        modifiedSiteCode.OK = 1;
                        modifiedSiteCode.Error = string.Empty;
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
