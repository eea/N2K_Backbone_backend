using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.Data;
using NuGet.Protocol;
using N2K_BackboneBackEnd.Helpers;
using System.Security.Policy;
using System.Diagnostics;
using N2K_BackboneBackEnd.Models.BackboneDB;
using Microsoft.AspNetCore.Http;

namespace N2K_BackboneBackEnd.Services
{


    public class SiteChangesService : ISiteChangesService
    {

        private struct MyStruct
        {
            public string SiteCode { get; set; }
            public string Name { get; set; }
            public int Version { get; set; }
            public N2K_BackboneBackEnd.Enumerations.Level Level { get; set; }
        }

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
        private IEnumerable<SpeciesOther>? _siteSpeciesOther;
        private IEnumerable<Habitats>? _siteHabitatsReference;
        private IEnumerable<Species>? _siteSpeciesReference;
        private IEnumerable<SpeciesOther>? _siteSpeciesOtherReference;


        public SiteChangesService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
            _speciesTypes = _dataContext.Set<SpeciesTypes>().AsNoTracking().ToList();
            _habitatTypes = _dataContext.Set<HabitatTypes>().AsNoTracking().ToList();
            _countries = _dataContext.Set<Countries>().AsNoTracking().ToList();
        }


        public async Task<List<SiteChangeDbEdition>> GetSiteChangesAsync(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, int page = 1, int pageLimit = 0)
        {
            var startRow = (page - 1) * pageLimit;
            var sitesList = (await GetSiteCodesByStatusAndLevelAndCountry(country, status, level, cache));
            if (pageLimit > 0)
            {
                sitesList = sitesList
                    .Skip(startRow)
                    .Take(pageLimit)
                    .ToList();
            }
            var sitecodesfilter = new DataTable("sitecodesfilter");
            sitecodesfilter.Columns.Add("SiteCode", typeof(string));
            sitecodesfilter.Columns.Add("Version", typeof(int));

            foreach (var sc in sitesList)
            {
                sitecodesfilter.Rows.Add(new Object[] { sc.SiteCode, sc.Version });
            }


            //call a stored procedure that returs the site changes that match the given criteria                        
            SqlParameter param1 = new SqlParameter("@country", country);
            SqlParameter param2 = new SqlParameter("@status", status.HasValue ? status.ToString() : String.Empty);
            SqlParameter param3 = new SqlParameter("@level", level.HasValue ? level.ToString() : String.Empty);
            SqlParameter param4 = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
            param4.Value = sitecodesfilter;
            param4.TypeName = "[dbo].[SiteCodeFilter]";

            IQueryable<SiteChangeDbNumsperLevel> changes = _dataContext.Set<SiteChangeDbNumsperLevel>().FromSqlRaw($"exec dbo.spGetChangesByCountryAndStatusAndLevel  @country, @status, @level, @siteCodes",
                            param1, param2, param3, param4);

            IEnumerable<OrderedChanges> orderedChanges;
            //order the changes so that the first codes are the one with the highest Level value (1. Critical 2. Warning 3. Info)
            //It return an enumeration of sitecodes with a nested list of the changes for that sitecode, ordered by level
            IOrderedEnumerable<OrderedChanges> orderedChangesEnum = (from t in await changes.ToListAsync()
                                                                     group t by new { t.SiteCode, t.SiteName }
                                                                     into g
                                                                     select new OrderedChanges
                                                                     {
                                                                         SiteCode = g.Key.SiteCode,
                                                                         SiteName = g.Key.SiteName,
                                                                         Level = (from t2 in g select t2.Level).Max(),
                                                                         //Nest all changes of each sitecode ordered by Level
                                                                         ChangeList = g.Where(s => s.SiteCode.ToUpper() == g.Key.SiteCode.ToUpper()).OrderByDescending(x => (int)x.Level).ToList()
                                                                     }).OrderByDescending(a => a.Level).ThenBy(b => b.SiteCode);

            /*
            if (pageLimit != 0)
            {
                orderedChanges = orderedChangesEnum
                        .Skip(startRow)
                        .Take(pageLimit)
                        .ToList();
            }
            else
            */
            orderedChanges = orderedChangesEnum.ToList();


            var result = new List<SiteChangeDbEdition>();
            var siteCode = string.Empty;
            List<SiteActivities> activities = await _dataContext.Set<SiteActivities>().FromSqlRaw($"exec dbo.spGetSiteActivitiesUserEditionByCountry  @country",
                            param1).ToListAsync();
            foreach (var sCode in orderedChanges)
            {
                //load all the changes for each of the site codes ordered by level
                var siteChange = new SiteChangeDbEdition();
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
                        siteChange.HasGeometry = change.HasGeometry;
                        if (change.Country != null)
                        {
                            var countryName = _countries.Where(ctry => ctry.Code.ToLower() == change.Country.ToLower()).FirstOrDefault();
                            siteChange.Country = countryName != null ? countryName.Country : change.Country;
                        }
                        siteChange.Level = null;
                        siteChange.Status = null;
                        siteChange.Tags = "";
                        siteChange.Version = change.Version;
                        SiteActivities activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version).FirstOrDefault();
                        if (activity == null)
                        {
                            SiteChangeDb editionChange = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version && e.ChangeType == "User edition").FirstOrDefaultAsync();
                            if (editionChange != null)
                                activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == editionChange.VersionReferenceId).FirstOrDefault();
                            if (activity == null)
                            {
                                activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Action == "User edition after rejection of version " + change.Version).FirstOrDefault();
                            }
                        }
                        siteChange.EditedBy = activity is null ? null : activity.Author;
                        siteChange.EditedDate = activity is null ? null : activity.Date;
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
                            Tags = change.Tags,
                            NumChanges = 1
                        };
                        siteChange.subRows = new List<SiteChangeView>();
                        siteChange.subRows.Add(changeView);
                    }
                    else
                    {
                        if (!siteChange.subRows.Any(ch => ch.ChangeCategory == change.ChangeCategory && ch.ChangeType == change.ChangeType && ch.Level == change.Level))
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
                                NumChanges = 1
                            });
                        }
                        else
                        {
                            siteChange.subRows.Where(ch => ch.ChangeCategory == change.ChangeCategory && ch.ChangeType == change.ChangeType && ch.Level == change.Level).FirstOrDefault().NumChanges++;
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
                changeDetailVM.HasGeometry = false;
                SqlParameter param1 = new SqlParameter("@SiteCode", site.SiteCode);
                SqlParameter param2 = new SqlParameter("@Version", site.Version);

                //var geometries = await _dataContext.Set<SiteGeometry>().FromSqlRaw($"exec dbo.spGetSiteVersionGeometry  @SiteCode, @Version",
                //                param1, param2).ToArrayAsync();

                //if (geometries.Length > 0 && !string.IsNullOrEmpty(geometries[0].GeoJson)) 
                changeDetailVM.HasGeometry = true;


#pragma warning disable CS8601 // Posible asignación de referencia nula
                changeDetailVM.Name = site.Name;
                changeDetailVM.Status = (SiteChangeStatus?)site.CurrentStatus;
                changeDetailVM.JustificationProvided = site.JustificationProvided.HasValue ? site.JustificationProvided.Value : false;
                changeDetailVM.JustificationRequired = site.JustificationRequired.HasValue ? site.JustificationRequired.Value : false;
#pragma warning restore CS8601 // Posible asignación de referencia nula
            }
            ProcessedEnvelopes harvestedEnvelope = await _dataContext.Set<ProcessedEnvelopes>().AsNoTracking().Where(envelope => envelope.Country == site.CountryCode && envelope.Status == HarvestingStatus.Harvested).FirstOrDefaultAsync();
            var changesDb = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(changes => changes.SiteCode == pSiteCode && changes.N2KVersioningVersion == harvestedEnvelope.Version).ToListAsync();
            changesDb = changesDb.OrderByDescending(m => m.Version).DistinctBy(m => new { m.SiteCode, m.Country, m.Status, m.Tags, m.Level, m.ChangeCategory, m.ChangeType, m.NewValue, m.OldValue, m.Detail, m.Code, m.Section, m.VersionReferenceId, m.FieldName, m.ReferenceSiteCode, m.N2KVersioningVersion }).ToList();
            if (changesDb != null)
            {
                if (changesDb.FirstOrDefault().ChangeType == "Site Deleted")
                {
                    changeDetailVM.Status = changesDb.FirstOrDefault().Status;
                }
            }

            _siteHabitats = await _dataContext.Set<Habitats>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion).ToListAsync();
            _siteSpecies = await _dataContext.Set<Species>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion).ToListAsync();
            _siteSpeciesOther = await _dataContext.Set<SpeciesOther>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion && site.SpecieCode != null).ToListAsync();
            _siteHabitatsReference = await _dataContext.Set<Habitats>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version != pCountryVersion).ToListAsync();
            _siteSpeciesReference = await _dataContext.Set<Species>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version != pCountryVersion).ToListAsync();
            _siteSpeciesOtherReference = await _dataContext.Set<SpeciesOther>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version != pCountryVersion && site.SpecieCode != null).ToListAsync();

            changeDetailVM.Critical = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Critical);
            changeDetailVM.Warning = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Warning);
            changeDetailVM.Info = FillLevelChangeDetailCategory(changesDb, pSiteCode, pCountryVersion, Level.Info);


            _siteHabitats = null;
            _siteSpecies = null;
            _siteSpeciesOther = null;

            return changeDetailVM;
        }

        public async Task<List<SiteCodeView>> GetNonPendingSiteCodes(string country)
        {
            SqlParameter param1 = new SqlParameter("@country", country);
            IQueryable<SiteCodeVersion> changes = _dataContext
                .Set<SiteCodeVersion>()
                .FromSqlRaw($"exec dbo.[spGetActiveSiteCodesByCountryNonPending]  @country", param1);
            List<SiteCodeView> result = new List<SiteCodeView>();
            List<SiteActivities> activities = await _dataContext.Set<SiteActivities>().FromSqlRaw($"exec dbo.spGetSiteActivitiesUserEditionByCountry  @country",
                            param1).ToListAsync();
            foreach (var change in (await changes.ToListAsync()))
            {
                SiteActivities activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version).FirstOrDefault();
                if (activity == null)
                {
                    SiteChangeDb editionChange = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version && e.ChangeType == "User edition").FirstOrDefaultAsync();
                    if (editionChange != null)
                        activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == editionChange.VersionReferenceId).FirstOrDefault();
                }
                SiteCodeView temp = new SiteCodeView
                {
                    SiteCode = change.SiteCode,
                    Version = change.Version,
                    Name = change.Name,
                    EditedBy = activity is null ? null : activity.Author,
                    EditedDate = activity is null ? null : activity.Date
                };
                result.Add(temp);
            }

            return result;
        }

        public async Task<List<SiteCodeView>> GetSiteCodesByStatusAndLevelAndCountry(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, bool refresh = false)
        {
            string listName = string.Format("{0}_{1}_{2}_{3}", "listcodes",
                    country,
                    status.ToString(),
                    level.ToString()
                   );
            //if there has been a change in the status refresh the changed sitecodes cache
            if (refresh) cache.Remove(listName);

            var result = new List<SiteCodeView>();
            if (cache.TryGetValue(listName, out List<SiteCodeView> cachedList))
            {
                result = cachedList;
            }
            else
            {
                SqlParameter param1 = new SqlParameter("@country", country);
                SqlParameter param2 = new SqlParameter("@status", status.ToString());
                SqlParameter param3 = new SqlParameter("@level", level.ToString());

                IQueryable<SiteCodeVersion> changes = _dataContext.Set<SiteCodeVersion>().FromSqlRaw($"exec dbo.[spGetActiveSiteCodesByCountryAndStatusAndLevel]  @country, @status, @level",
                            param1, param2, param3);

                result = (await changes.ToListAsync()).Select(x =>
                     new SiteCodeView
                     {
                         SiteCode = x.SiteCode,
                         Version = x.Version,
                         Name = x.Name
                     }
                ).ToList();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(2500))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(40000);
                cache.Set(listName, result, cacheEntryOptions);
            }
            return result.OrderBy(o => o.SiteCode).ToList();
        }

        public async Task<int> GetPendingChangesByCountry(string? country, IMemoryCache cache)
        {
            //return (await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, null,cache)).Count;
            SqlParameter param1 = new SqlParameter("@country", country);

            IQueryable<PendingSites> changes = _dataContext.Set<PendingSites>().FromSqlRaw($"exec dbo.[spGetPendingSiteCodesByCountry] @country ",
                        param1);

            var result = (await changes.ToListAsync());
            if (result != null && result.Count > 0) return result[0].NumSites;
            return 0;
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
                    case "BioRegions":
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
                        if (_levelDetail.ChangeType != "User edition")
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
                            if (_levelDetail.ChangeType == "Other Species Deleted")
                            {
                                _Section.DeletedCodes.Add(new CategoryChangeDetail
                                {
                                    ChangeCategory = _levelDetail.Section,
                                    ChangeType = String.Format("List of {0}", _levelDetail.ChangeType),
                                    ChangedCodesDetail = new List<CodeChangeDetail>()
                                });
                            }
                            else
                            {
                                _Section.DeletedCodes.Add(new CategoryChangeDetail
                                {
                                    ChangeCategory = _levelDetail.Section,
                                    ChangeType = String.Format("List of {0} Deleted", _levelDetail.Section),
                                    ChangedCodesDetail = new List<CodeChangeDetail>()
                                });
                            }
                        }

                        foreach (var changedItem in _levelDetail.ChangeList.OrderBy(c => c.Code == null ? "" : c.Code))
                        {
                            _Section.DeletedCodes.ElementAt(0).ChangedCodesDetail.Add(
                                CodeAddedRemovedDetail(_levelDetail.Section, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.Version, changedItem.VersionReferenceId)
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
                            CodeAddedRemovedDetail(_levelDetail.Section, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.Version, changedItem.VersionReferenceId)
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
                string nullCase = "";
                if (changedItem.OldValue != null && changedItem.OldValue.ToUpper() != "NULL")
                {
                    fields.Add("Reference", changedItem.OldValue);
                }
                else
                {
                    fields.Add("Reference", nullCase);
                }

                if (changedItem.NewValue != null && changedItem.NewValue.ToUpper() != "NULL")
                {
                    fields.Add("Reported", changedItem.NewValue);
                }
                else
                {
                    fields.Add("Reported", nullCase);
                }

                if (changeCategory == "Habitats" || changeCategory == "Species")
                {
                    if (GetCodeName(changedItem) != String.Empty)
                    {
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
                    else
                    {
                        catChange.ChangedCodesDetail.Add(
                                new CodeChangeDetail
                                {
                                    Code = "-",
                                    Name = changedItem.Code,
                                    ChangeId = changedItem.ChangeId,
                                    Fields = fields
                                }

                            );
                    }
                }
                else
                {
                    catChange.ChangedCodesDetail.Add(
                            new CodeChangeDetail
                            {
                                ChangeId = changedItem.ChangeId,
                                Fields = fields
                            }
                        );
                }
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
                case "BioRegions":
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


        private CodeChangeDetail? CodeAddedRemovedDetail(string section, string? code, long changeId, string pSiteCode, int pCountryVersion, int versionReferenceId)
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
                        SpeciesTypes? _spectype = _speciesTypes.FirstOrDefault(s => s.Code.ToLower() == code.ToLower());
                        if (_spectype != null) specName = _spectype.Name;

                        var specDetails = _siteSpecies.Where(sp => sp.SpecieCode.ToLower() == code.ToLower())
                            .Select(spc => new
                            {
                                Population = spc.Population,
                                SpecType = spc.SpecieType
                            }).FirstOrDefault();
                        if (specDetails == null)
                        {
                            specDetails = _siteSpeciesOther.Where(sp => sp.SpecieCode.ToLower() == code.ToLower())
                            .Select(spc => new
                            {
                                Population = spc.Population,
                                SpecType = spc.SpecieType
                            }).FirstOrDefault();
                        }
                        if (specDetails == null)
                        {
                            specDetails = _siteSpeciesReference.Where(sp => sp.SpecieCode.ToLower() == code.ToLower() && sp.Version == versionReferenceId)
                            .Select(spc => new
                            {
                                Population = spc.Population,
                                SpecType = spc.SpecieType
                            }).FirstOrDefault();
                        }
                        if (specDetails == null)
                        {
                            specDetails = _siteSpeciesOtherReference.Where(sp => sp.SpecieCode.ToLower() == code.ToLower() && sp.Version == versionReferenceId)
                            .Select(spc => new
                            {
                                Population = spc.Population,
                                SpecType = spc.SpecieType
                            }).FirstOrDefault();
                        }
                        if (specDetails != null)
                        {
                            population = specDetails.Population;
                            specType = specDetails.SpecType;
                        }
                    }
                    fields.Add("Population", population);
                    fields.Add("SpeciesType", specType);

                    if (specName != String.Empty && specName != null)
                    {
                        return new CodeChangeDetail
                        {
                            ChangeId = changeId,
                            Code = code,
                            Name = specName,
                            Fields = fields

                        };
                    }
                    else
                    {
                        return new CodeChangeDetail
                        {
                            ChangeId = changeId,
                            Code = "-",
                            Name = code,
                            Fields = fields

                        };
                    }

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

                        if (habDetails == null)
                        {
                            habDetails = _siteHabitatsReference.Where(sh => sh.HabitatCode.ToLower() == code.ToLower() && sh.Version == versionReferenceId)
                            .Select(hab => new
                            {
                                CoverHA = hab.CoverHA.ToString(),
                                RelativeSurface = hab.RelativeSurface
                            }).FirstOrDefault();
                        }
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
            }

            return null;
        }


        public async Task<List<ModifiedSiteCode>> AcceptChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache)
        {
            List<SiteActivities> siteActivities = new List<SiteActivities>();
            List<ModifiedSiteCode> result = new List<ModifiedSiteCode>();
            try
            {
                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));

                changedSiteStatus.ToList().ForEach(cs =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { cs.SiteCode, cs.VersionId });

                    cs.OK = 1;
                    cs.Error = string.Empty;
                    cs.Status = SiteChangeStatus.Accepted;
                    result.Add(cs);

                    siteActivities.Add(new SiteActivities
                    {
                        SiteCode = cs.SiteCode,
                        Version = cs.VersionId,
                        Author = GlobalData.Username,
                        Date = DateTime.Now,
                        Action = "Accept Changes",
                        Deleted = false
                    });
                });

                string queryString = @" 
                        select  Changes.SiteCode,Changes.Version, Sites.Name as SiteName, Max(
	                        case
		                        when Level='Critical' then 2
		                        when Level='Warning' then 1
		                        when Level='Info' then 0
                            end
	                        ) as Level
                        from 
	                        [dbo].[Changes] inner join 
	                        Sites ON   changes.sitecode= sites.sitecode and Changes.version=Sites.version 
	                        inner join
	                        @siteCodes T on  Changes.SiteCode= T.SiteCode and Changes.Version= T.Version

                        group by 
	                        changes.SiteCode, Changes.version, Sites.name";

                SqlConnection backboneConn = null;
                SqlCommand command = null;
                SqlDataReader reader = null;
                try
                {

                    backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
                    backboneConn.Open();
                    command = new SqlCommand(queryString, backboneConn);
                    SqlParameter paramTable1 = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                    paramTable1.Value = sitecodesfilter;
                    paramTable1.TypeName = "[dbo].[SiteCodeFilter]";
                    command.Parameters.Add(paramTable1);
                    reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        SiteCodeView mySiteView = new SiteCodeView();
                        mySiteView.SiteCode = reader["SiteCode"].ToString();
                        mySiteView.Version = int.Parse(reader["Version"].ToString());
                        mySiteView.Name = reader["SiteName"].ToString();
                        Level level;
                        Enum.TryParse<Level>(reader["Level"].ToString(), out level);
                        //Alter cached listd. They come from pendign and goes to accepted
                        await swapSiteInListCache(cache, SiteChangeStatus.Accepted, level, SiteChangeStatus.Pending, mySiteView);
                    }
                }
                catch (Exception ex)
                {
                    SystemLog.write(SystemLog.errorLevel.Error, ex, "AcceptChanges", "");
                }
                finally
                {
                    if (reader != null) await reader.DisposeAsync();
                    if (command != null) command.Dispose();
                    if (backboneConn != null) backboneConn.Dispose();
                }

                try
                {

                    SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                    paramTable.Value = sitecodesfilter;
                    paramTable.TypeName = "[dbo].[SiteCodeFilter]";

                    await _dataContext.Database.ExecuteSqlRawAsync(
                            "exec spAcceptSiteCodeChangesBulk @siteCodes",
                            paramTable);

                    SiteActivities.SaveBulkRecord(_dataContext.Database.GetConnectionString(), siteActivities);

                }
                catch
                {
                }

                //Refresh site codes cache
                if (result.Count > 0)
                {
                    var country = (result.First().SiteCode).Substring(0, 2);
                    var site = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == result.First().SiteCode && site.Version == result.First().VersionId).ToListAsync();
                    Level level = (Level)site.Max(a => a.Level);
                    //var status = site.FirstOrDefault().Status;

                    //refresh the cache of site codes
                    List<SiteCodeView> mockresult = null;
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Accepted, level, cache, true);
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, level, cache, true);
                }
                return result;
            }
            catch
            {
                throw;
            }


        }


        public async Task<List<ModifiedSiteCode>> RejectChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache)
        {
            List<SiteActivities> siteActivities = new List<SiteActivities>();
            List<ModifiedSiteCode> result = new List<ModifiedSiteCode>();
            try
            {
                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));

                changedSiteStatus.ToList().ForEach(cs =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { cs.SiteCode, cs.VersionId });

                    cs.OK = 1;
                    cs.Error = string.Empty;
                    cs.Status = SiteChangeStatus.Rejected;
                    result.Add(cs);

                    siteActivities.Add(new SiteActivities
                    {
                        SiteCode = cs.SiteCode,
                        Version = cs.VersionId,
                        Author = GlobalData.Username,
                        Date = DateTime.Now,
                        Action = "Reject Changes",
                        Deleted = false
                    });
                });
                string queryString = @" 
                        select  Changes.SiteCode,Changes.Version, Sites.Name as SiteName, Max(
	                        case
		                        when Level='Critical' then 2
		                        when Level='Warning' then 1
		                        when Level='Info' then 0
                            end
	                        ) as Level
                        from 
	                        [dbo].[Changes] inner join 
	                        Sites ON   changes.sitecode= sites.sitecode and Changes.version=Sites.version 
	                        inner join
	                        @siteCodes T on  Changes.SiteCode= T.SiteCode and Changes.Version= T.Version

                        group by 
	                        changes.SiteCode, Changes.version, Sites.name";
                SqlConnection backboneConn = null;
                SqlCommand command = null;
                SqlDataReader reader = null;
                try
                {
                    backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
                    backboneConn.Open();
                    command = new SqlCommand(queryString, backboneConn);
                    SqlParameter paramTable1 = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                    paramTable1.Value = sitecodesfilter;
                    paramTable1.TypeName = "[dbo].[SiteCodeFilter]";
                    command.Parameters.Add(paramTable1);
                    reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        SiteCodeView mySiteView = new SiteCodeView();
                        mySiteView.SiteCode = reader["SiteCode"].ToString();
                        mySiteView.Version = int.Parse(reader["Version"].ToString());
                        mySiteView.Name = reader["SiteName"].ToString();
                        Level level;
                        Enum.TryParse<Level>(reader["Level"].ToString(), out level);
                        //Alter cached listd. They come from pendign and goes to rejected
                        await swapSiteInListCache(cache, SiteChangeStatus.Rejected, level, SiteChangeStatus.Pending, mySiteView);
                    }
                }
                catch (Exception ex)
                {
                    SystemLog.write(SystemLog.errorLevel.Error, ex, "RejectChanges", "");

                }
                finally
                {
                    if (reader != null) await reader.DisposeAsync();
                    if (command != null) command.Dispose();
                    if (backboneConn != null) backboneConn.Dispose();
                }




                try
                {
                    SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                    paramTable.Value = sitecodesfilter;
                    paramTable.TypeName = "[dbo].[SiteCodeFilter]";

                    await _dataContext.Database.ExecuteSqlRawAsync(
                            "exec spRejectSiteCodeChangesBulk @siteCodes",
                            paramTable);

                    SiteActivities.SaveBulkRecord(_dataContext.Database.GetConnectionString(), siteActivities);
                }
                catch
                {

                }

                //refresh the chache
                if (result.Count > 0)
                {
                    var country = (result.First().SiteCode).Substring(0, 2);
                    var site = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == result.First().SiteCode && site.Version == result.First().VersionId).ToListAsync();
                    Level level = (Level)site.Max(a => a.Level);
                    var status = site.FirstOrDefault().Status;

                    //refresh the cache of site codes
                    List<SiteCodeView> mockresult = null;
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Rejected, level, cache, true);
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, level, cache, true);
                }
                return result;


            }
            catch
            {
                throw;
            }

        }


        public async Task<List<ModifiedSiteCode>> MoveToPending(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache)
        {
            //var country = (changedSiteStatus.First().SiteCode).Substring(0, 2);
            //var site = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == changedSiteStatus.First().SiteCode && site.Version == changedSiteStatus.First().VersionId).ToListAsync();
            Level level = Level.Critical;
            SiteChangeStatus? status = SiteChangeStatus.Accepted;

            List<SiteActivities> siteActivities = new List<SiteActivities>();
            List<ModifiedSiteCode> result = new List<ModifiedSiteCode>();
            try
            {
                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));

                var sitecodesdelete = new DataTable("sitecodesdelete");
                sitecodesdelete.Columns.Add("SiteCode", typeof(string));
                sitecodesdelete.Columns.Add("Version", typeof(int));

                foreach (var modifiedSiteCode in changedSiteStatus)
                {
                    sitecodesfilter.Rows.Add(new Object[] { modifiedSiteCode.SiteCode, modifiedSiteCode.VersionId });

                    siteActivities.Add(new SiteActivities
                    {
                        SiteCode = modifiedSiteCode.SiteCode,
                        Version = modifiedSiteCode.VersionId,
                        Author = GlobalData.Username,
                        Date = DateTime.Now,
                        Action = "Back to Pending",
                        Deleted = false
                    });

                    try
                    {
                        List<SiteChangeDb> changes = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == modifiedSiteCode.VersionId).ToListAsync();

                        //Create the listView for the cached lists. By deafult this values
                        SiteCodeView mySiteView = new SiteCodeView();
                        mySiteView.SiteCode = modifiedSiteCode.SiteCode;
                        mySiteView.Version = modifiedSiteCode.VersionId;
                        mySiteView.Name = changes.First().SiteName;

                        SqlParameter paramSiteCode = new SqlParameter("@sitecode", modifiedSiteCode.SiteCode);
                        SqlParameter paramVersionId = new SqlParameter("@version", modifiedSiteCode.VersionId);
                        SqlParameter paramOldVersion = new SqlParameter("@oldVersion", modifiedSiteCode.VersionId);
                        SqlParameter paramNewVersion2 = null;

                        Sites siteToDelete = null;
                        int previousCurrent = -1;//The 0 value can be a version

                        #region In case of user edition

                        List<SiteActivities> activities = await _dataContext.Set<SiteActivities>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Action.StartsWith("User edition") && e.Deleted == false).ToListAsync();

                        //Was this site edited after being accepted?
                        SiteChangeDb? change = changes.Where(e => e.ChangeType == "User edition").FirstOrDefault();
                        if (change != null)
                        {
                            //Select the max version for the site with the currentsatatus accepted, but not the version of the change and the referenced version
                            previousCurrent = _dataContext.Set<Sites>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version != modifiedSiteCode.VersionId && e.Version != change.VersionReferenceId && e.CurrentStatus == SiteChangeStatus.Accepted).Max(e => e.Version);
                            //Search the previous activities
                            List<SiteActivities> activityDelete = activities.Where(e => (e.Version == modifiedSiteCode.VersionId || e.Version == change.VersionReferenceId) && e.Action == "User edition").ToList();

                            //mark the result as activities deleted
                            activityDelete.ForEach(s => s.Deleted = true);


                            //Add comments and docs to the soon to be pending version (the previous version referenced in the change)
                            SqlParameter paramNewVersion1 = new SqlParameter("@newVersion", change.VersionReferenceId);
                            await _dataContext.Database.ExecuteSqlRawAsync(
                                "exec spCopyJustificationFilesAndStatusChanges @sitecode, @oldVersion, @newVersion",
                                paramSiteCode, paramOldVersion, paramNewVersion1);

                            //Find edited version in order to remove from the sites entity
                            siteToDelete = await _dataContext.Set<Sites>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == modifiedSiteCode.VersionId).FirstOrDefaultAsync();

                            //Change the version and the name for the previous version
                            paramVersionId = new SqlParameter("@version", change.VersionReferenceId);
                            mySiteView.Version = change.VersionReferenceId; //points to the final version
                            string previousName = _dataContext.Set<Sites>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == change.VersionReferenceId).Select(x => x.Name).First().ToString();
                            mySiteView.Name = previousName;
                        }
                        //Was this site edited after being rejected?
                        List<SiteActivities> activityCheck = activities.Where(e => e.Action == "User edition after rejection of version " + modifiedSiteCode.VersionId).ToList();
                        if (activityCheck != null && activityCheck.Count > 0)
                        {
                            //Get the site max accepted version for the last package but not the current nor the present version 
                            Sites previousSite = await _dataContext.Set<Sites>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version != modifiedSiteCode.VersionId && e.CurrentStatus == SiteChangeStatus.Accepted && e.Current == false).OrderByDescending(x => x.N2KVersioningVersion).ThenByDescending(x => x.Version).FirstOrDefaultAsync();
                            previousCurrent = previousSite.Version;

                            //mark the result as activities deleted
                            activityCheck.ForEach(s => s.Deleted = true);

                            //Find the current site
                            siteToDelete = await _dataContext.Set<Sites>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Current == true).FirstOrDefaultAsync();
                        }
                        //In both cases
                        if (change != null || (activityCheck != null && activityCheck.Count > 0))
                        {
                            paramNewVersion2 = new SqlParameter("@newVersion", previousCurrent);

                            //Add comments and docs to the previous current version
                            await _dataContext.Database.ExecuteSqlRawAsync(
                                "exec spCopyJustificationFilesAndStatusChanges @sitecode, @oldVersion, @newVersion",
                                paramSiteCode, paramOldVersion, paramNewVersion2);

                            //Delete edited version
                            sitecodesdelete.Rows.Add(new Object[] { siteToDelete.SiteCode, siteToDelete.Version });

                            //Save activities changes
                            await _dataContext.SaveChangesAsync();
                        }
                        #endregion

                        //Get the previous level and status to find the proper cached lists
                        level = (Level)changes.Max(a => a.Level);
                        status = (SiteChangeStatus)changes.FirstOrDefault().Status;

                        //Alter cached list. It comes from Removed or Accepted list and goes to Pending list
                        await swapSiteInListCache(cache, SiteChangeStatus.Pending, level, status, mySiteView);


                        modifiedSiteCode.OK = 1;
                        modifiedSiteCode.Error = string.Empty;
                        modifiedSiteCode.Status = SiteChangeStatus.Pending;
                        modifiedSiteCode.VersionId = change is null ? modifiedSiteCode.VersionId : change.VersionReferenceId;
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

                try
                {
                    SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                    paramTable.Value = sitecodesdelete;
                    paramTable.TypeName = "[dbo].[SiteCodeFilter]";

                    if (sitecodesdelete.Rows.Count > 0)
                    {
                        await _dataContext.Database.ExecuteSqlRawAsync(
                                "exec spDeleteSitesBulk @siteCodes",
                                paramTable);
                    }

                    paramTable.Value = sitecodesfilter;

                    await _dataContext.Database.ExecuteSqlRawAsync(
                            "exec spMoveSiteCodeToPendingBulk @siteCodes",
                            paramTable);

                    SiteActivities.SaveBulkRecord(_dataContext.Database.GetConnectionString(), siteActivities);
                }
                catch
                {

                }

                ////GetSiteCodesByStatusAndLevelAndCountry
                ////get the country and the level of the first site code. The other codes will have the same level
                ////refresh the cache
                if (result.Count > 0)
                {
                    var country = (result.First().SiteCode).Substring(0, 2);

                    //refresh the cache of site codes
                    List<SiteCodeView> mockresult = null;
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, status, level, cache, true);
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, level, cache, true);
                }
                return result;
            }
            catch
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
            catch
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
            catch
            {
                throw;
            }

        }


        /// <summary>
        /// Moves the site from one cached list to anotherone
        /// </summary>
        /// <param name="pCache">Lists strored in the chache</param>
        /// <param name="pStatus">Status of the Site</param>
        /// <param name="pLevel"> Level of the list</param>
        /// <param name="pListNameFrom"></param>
        /// <param name="pSite"></param>
        /// <returns></returns>
        private async Task<List<SiteCodeView>> swapSiteInListCache(IMemoryCache pCache, SiteChangeStatus? pStatus, Level? pLevel, SiteChangeStatus? pListNameFrom, SiteCodeView pSite)
        {

            List<SiteCodeView> cachedlist = new List<SiteCodeView>();


            //Site comes from this list
            string listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", pSite.SiteCode.Substring(0, 2), pListNameFrom.ToString(), pLevel.ToString());
            if (pCache.TryGetValue(listName, out cachedlist))
            {
                SiteCodeView element = cachedlist.Where(cl => cl.SiteCode == pSite.SiteCode).FirstOrDefault();
                if (element != null)
                {
                    cachedlist.Remove(element);
                }
            }
            //Site goes to that list
            listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", pSite.SiteCode.Substring(0, 2), pStatus.ToString(), pLevel.ToString());
            if (pCache.TryGetValue(listName, out cachedlist))
            {
                SiteCodeView element = cachedlist.Where(cl => cl.SiteCode == pSite.SiteCode).FirstOrDefault();
                if (element != null)
                {
                    element.Version = pSite.Version;
                    element.Name = pSite.Name;
                }
                else
                {
                    cachedlist.Add(pSite);
                }

            }
            else
            {
                //If the destination list doesn't exist, create it.
                List<SiteCodeView> mockresult = null;
                mockresult = await GetSiteCodesByStatusAndLevelAndCountry(pSite.SiteCode.Substring(0, 2), pStatus, pLevel, pCache, true);
            }
            return null;
        }


    }
}
