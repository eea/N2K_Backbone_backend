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
using System.Runtime.CompilerServices;
using System.Globalization;

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
        private readonly IEnumerable<SpeciesPriority> _speciesPriority;
        private readonly IEnumerable<HabitatPriority> _habitatPriority;
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
            _speciesPriority = _dataContext.Set<SpeciesPriority>().AsNoTracking().ToList();
            _habitatPriority = _dataContext.Set<HabitatPriority>().AsNoTracking().ToList();
            _countries = _dataContext.Set<Countries>().AsNoTracking().ToList();
        }


        public async Task<List<SiteChangeDbEdition>> GetSiteChangesAsync(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, int page = 1, int pageLimit = 0, bool onlyedited = false)
        {
            try
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
                SqlParameter param5 = new SqlParameter("@status", DBNull.Value);
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
                List<SiteChangeDb> editionChanges = await _dataContext.Set<SiteChangeDb>().FromSqlRaw($"exec dbo.spGetActiveEnvelopeSiteChangesUserEditionByCountry  @country",
                                param1).ToListAsync();
                List<Lineage> lineageChanges = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetLineageData @country, @status",
                                param1, param5).ToListAsync();
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
                                SiteChangeDb editionChange = editionChanges.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version && e.ChangeType == "User edition").FirstOrDefault();
                                if (editionChange != null)
                                    activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == editionChange.VersionReferenceId).FirstOrDefault();
                                if (activity == null)
                                {
                                    activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Action == "User edition after rejection of version " + change.Version).FirstOrDefault();
                                }
                            }
                            SiteChangeDb recoded = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version && e.ChangeType == "Site Recoded").FirstOrDefaultAsync();
                            siteChange.EditedBy = activity is null ? null : activity.Author;
                            siteChange.EditedDate = activity is null ? null : activity.Date;
                            siteChange.Recoded = recoded is null ? false : true;
                            Lineage lineageChangeType = lineageChanges.FirstOrDefault(e => e.SiteCode == change.SiteCode && e.Version == change.Version);
                            siteChange.LineageChangeType = lineageChangeType is null ? LineageTypes.NoChanges : lineageChangeType.Type;
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
                if (onlyedited)
                    result = result.Where(x => x.EditedDate != null).ToList();
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetSiteChangesAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SiteChangeViewModel>> GetSiteChangesFromSP()
        {
            try
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
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetSiteChangesFromSP", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }



#pragma warning disable CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        public async Task<SiteChangeDb?> GetSiteChangeByIdAsync(int id)
#pragma warning restore CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        {
            try
            {
                var result = new List<Harvesting>();
                return await _dataContext.Set<SiteChangeDb>().AsNoTracking().SingleOrDefaultAsync(s => s.ChangeId == id);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetSiteChangeByIdAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }


        public async Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion)
        {
            try
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
                    changeDetailVM.Type = await _dataContext.Set<SiteTypes>().AsNoTracking().Where(t => t.Code == site.SiteType).Select(t => t.Classification).FirstOrDefaultAsync();
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
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetSiteChangesDetail", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SiteCodeView>> GetNonPendingSiteCodes(string country)
        {
            try
            {
                SqlParameter param1 = new SqlParameter("@country", country);
                IQueryable<SiteCodeVersion> changes = _dataContext
                    .Set<SiteCodeVersion>()
                    .FromSqlRaw($"exec dbo.[spGetActiveSiteCodesByCountryNonPending]  @country", param1);
                List<SiteCodeView> result = new List<SiteCodeView>();
                List<SiteActivities> activities = await _dataContext.Set<SiteActivities>().FromSqlRaw($"exec dbo.spGetSiteActivitiesUserEditionByCountry  @country",
                                param1).ToListAsync();
                List<Lineage> lineageChanges = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetLineageData @country, @status",
                                param1, new SqlParameter("@status", DBNull.Value)).ToListAsync();
                foreach (var change in (await changes.ToListAsync()))
                {
                    SiteActivities activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version).FirstOrDefault();
                    if (activity == null)
                    {
                        SiteChangeDb editionChange = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version && e.ChangeType == "User edition").FirstOrDefaultAsync();
                        if (editionChange != null)
                            activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == editionChange.VersionReferenceId).FirstOrDefault();
                    }

                    // Get Lineage change type from lineageChanges
                    LineageTypes? changeLineage = lineageChanges.FirstOrDefault(
                        l => l.SiteCode == change.SiteCode
                            && l.Version == change.Version
                        )?.Type ?? LineageTypes.NoChanges;

                    SiteCodeView temp = new SiteCodeView
                    {
                        SiteCode = change.SiteCode,
                        Version = change.Version,
                        Name = change.Name,
                        EditedBy = activity is null ? null : activity.Author,
                        EditedDate = activity is null ? null : activity.Date,
                        LineageChangeType = changeLineage
                    };
                    result.Add(temp);
                }

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetNonPendingSiteCodes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SiteCodeView>> GetSiteCodesByStatusAndLevelAndCountry(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, bool refresh = false, bool onlyedited = false)
        {
            try
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

                    List<SiteActivities> activities = await _dataContext.Set<SiteActivities>().FromSqlRaw($"exec dbo.spGetSiteActivitiesUserEditionByCountry  @country",
                                param1).ToListAsync();
                    List<SiteChangeDb> editionChanges = await _dataContext.Set<SiteChangeDb>().FromSqlRaw($"exec dbo.spGetActiveEnvelopeSiteChangesUserEditionByCountry  @country",
                                    param1).ToListAsync();
                    List<Lineage> lineageChanges = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetLineageData @country, @status",
                                    param1, new SqlParameter("@status", DBNull.Value)).ToListAsync();
                    foreach (var change in (await changes.ToListAsync()))
                    {
                        SiteActivities activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version).FirstOrDefault();
                        if (activity == null)
                        {
                            SiteChangeDb editionChange = editionChanges.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version && e.ChangeType == "User edition").FirstOrDefault();
                            if (editionChange != null)
                                activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == editionChange.VersionReferenceId).FirstOrDefault();
                        }

                        // Get Lineage change type from lineageChanges
                        LineageTypes? changeLineage = lineageChanges.FirstOrDefault(
                            l => l.SiteCode == change.SiteCode
                                && l.Version == change.Version
                            )?.Type ?? LineageTypes.NoChanges;

                        SiteCodeView temp = new SiteCodeView
                        {
                            SiteCode = change.SiteCode,
                            Version = change.Version,
                            Name = change.Name,
                            EditedBy = activity is null ? null : activity.Author,
                            EditedDate = activity is null ? null : activity.Date,
                            LineageChangeType = changeLineage
                        };
                        result.Add(temp);
                    }
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(2500))
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(40000);
                    cache.Set(listName, result, cacheEntryOptions);
                }
                if (onlyedited)
                    result = result.Where(x => x.EditedDate != null).ToList();
                return result.OrderBy(o => o.SiteCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetSiteCodesByStatusAndLevelAndCountry", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<int> GetPendingChangesByCountry(string? country, IMemoryCache cache)
        {
            try
            {
                //return (await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, null,cache)).Count;
                SqlParameter param1 = new SqlParameter("@country", country);

                IQueryable<PendingSites> changes = _dataContext.Set<PendingSites>().FromSqlRaw($"exec dbo.[spGetPendingSiteCodesByCountry] @country ",
                            param1);

                var result = (await changes.ToListAsync());
                if (result != null && result.Count > 0) return result[0].NumSites;
                return 0;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetPendingChangesByCountry", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }


        private SiteChangesLevelDetail FillLevelChangeDetailCategory(List<SiteChangeDb> changesDB, string pSiteCode, int pCountryVersion, Level level)
        {
            try
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
                                    CodeAddedRemovedDetail(_levelDetail.Section, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.VersionReferenceId, changedItem.VersionReferenceId)
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
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteChangesService - FillLevelChangeDetailCategory", "");
                throw ex;
            }
        }


        private CategoryChangeDetail GetChangeCategoryDetail(string changeCategory, string changeType, List<SiteChangeDb> changeList)
        {
            try
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
                        fields.Add("Submission", changedItem.NewValue);
                    }
                    else
                    {
                        fields.Add("Submission", nullCase);
                    }
                    if (catChange.ChangeCategory == "Change of area" || catChange.ChangeType == "Length Changed"
                        || catChange.ChangeType == "Change of spatial area")
                    {
                        string? reportedString = nullCase;
                        string? referenceString = nullCase;
                        if (fields.TryGetValue("Submission", out reportedString) && fields.TryGetValue("Reference", out referenceString)
                            && reportedString != "" && referenceString != "")
                        {
                            var culture = new CultureInfo("en-US");
                            var reported = decimal.Parse(reportedString, CultureInfo.InvariantCulture);
                            var reference = decimal.Parse(referenceString, CultureInfo.InvariantCulture);
                            fields.Add("Difference", Math.Round((reported - reference), 4).ToString("F4", culture));
                            if (reference != 0)
                            {
                                fields.Add("Percentage", Math.Round((((reported - reference) / reference) * 100), 4).ToString("F4", culture));
                            }
                            else
                            {
                                fields.Add("Percentage", Math.Round((reported - reference), 4).ToString("F4", culture));
                            }
                        }
                        else
                        {
                            fields.Add("Difference", nullCase);
                            fields.Add("Percentage", nullCase);
                        }
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
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetChangeCategoryDetail", "");
                throw ex;
            }
        }

        private string? GetCodeName(SiteChangeDb change)
        {
            try
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
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetCodeName", "");
                throw ex;
            }
        }


        private CodeChangeDetail? CodeAddedRemovedDetail(string section, string? code, long changeId, string pSiteCode, int pCountryVersion, int versionReferenceId)
        {
            try
            {
                var fields = new Dictionary<string, string>();
                switch (section)
                {
                    case "Species":
                        string? specName = null;
                        string? priorityS = "-";
                        string? population = null;
                        string? specType = null;

                        if (code != null)
                        {
                            SpeciesTypes? _spectype = _speciesTypes.FirstOrDefault(s => s.Code.ToLower() == code.ToLower());
                            if (_spectype != null)
                            {
                                specName = _spectype.Name;
                                SpeciesPriority? _specpriority = _speciesPriority.FirstOrDefault(s => s.SpecieCode.ToLower() == code.ToLower());
                                priorityS = (_specpriority == null) ? priorityS : "*";
                            }

                            var specDetails = _siteSpecies.Where(sp => sp.SpecieCode.ToLower() == code.ToLower() && sp.Version == pCountryVersion)
                                .Select(spc => new
                                {
                                    Population = spc.Population,
                                    SpecType = spc.SpecieType
                                }).FirstOrDefault();
                            if (specDetails == null)
                            {
                                specDetails = _siteSpeciesOther.Where(sp => sp.SpecieCode.ToLower() == code.ToLower() && sp.Version == pCountryVersion)
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
                        fields.Add("Priority", priorityS);
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
                        string? priorityH = "-";
                        string? coverHa = null;
                        string? relSurface = null;
                        if (code != null)
                        {

                            var habType = _habitatTypes.Where(s => s.Code.ToLower() == code.ToLower()).Select(spc => spc.Name).FirstOrDefault();
                            if (habType != null) habName = habType;

                            HabitatPriority? _habpriority = _habitatPriority.FirstOrDefault(h => h.HabitatCode.ToLower() == code.ToLower());

                            var habDetails = _siteHabitats.Where(sh => sh.HabitatCode.ToLower() == code.ToLower() && sh.Version == pCountryVersion)
                                .Select(hab => new
                                {
                                    CoverHA = hab.CoverHA.ToString(),
                                    RelativeSurface = hab.RelativeSurface,
                                    PriorityForm = hab.PriorityForm
                                }).FirstOrDefault();

                            if (habDetails == null)
                            {
                                habDetails = _siteHabitatsReference.Where(sh => sh.HabitatCode.ToLower() == code.ToLower() && sh.Version == versionReferenceId)
                                .Select(hab => new
                                {
                                    CoverHA = hab.CoverHA.ToString(),
                                    RelativeSurface = hab.RelativeSurface,
                                    PriorityForm = hab.PriorityForm
                                }).FirstOrDefault();
                            }
                            if (habDetails != null)
                            {
                                relSurface = habDetails.RelativeSurface;
                                coverHa = habDetails.CoverHA;
                                priorityH = (_habpriority == null) ? priorityH : ((_habpriority.Priority == 1 || (_habpriority.Priority == 2 && habDetails.PriorityForm == true)) ? "*" : priorityH);
                            }
                        }
                        fields.Add("Priority", priorityH);
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
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteChangesService - CodeAddedRemovedDetail", "");
                throw ex;
            }
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
                        mySiteView.CountryCode = mySiteView.SiteCode.Substring(0, 2);

                        Level level;
                        Enum.TryParse<Level>(reader["Level"].ToString(), out level);
                        //Alter cached listd. They come from pendign and goes to accepted
                        await swapSiteInListCache(cache, SiteChangeStatus.Accepted, level, SiteChangeStatus.Pending, mySiteView);
                    }
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "AcceptChanges", "", _dataContext.Database.GetConnectionString());
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

                    await SiteActivities.SaveBulkRecord(_dataContext.Database.GetConnectionString(), siteActivities);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "AcceptChanges", "", _dataContext.Database.GetConnectionString());
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
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Accepted, level, cache, false);
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, level, cache, false);
                }

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - AcceptChanges", "", _dataContext.Database.GetConnectionString());
                throw ex;
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
                        mySiteView.CountryCode = mySiteView.SiteCode.Substring(0, 2);

                        Level level;
                        Enum.TryParse<Level>(reader["Level"].ToString(), out level);
                        //Alter cached listd. They come from pendign and goes to rejected
                        await swapSiteInListCache(cache, SiteChangeStatus.Rejected, level, SiteChangeStatus.Pending, mySiteView);
                    }
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "RejectChanges", "", _dataContext.Database.GetConnectionString());
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

                    await SiteActivities.SaveBulkRecord(_dataContext.Database.GetConnectionString(), siteActivities);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "RejectChanges", "", _dataContext.Database.GetConnectionString());
                }

                //refresh the cache
                if (result.Count > 0)
                {
                    var country = (result.First().SiteCode).Substring(0, 2);
                    var site = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == result.First().SiteCode && site.Version == result.First().VersionId).ToListAsync();
                    Level level = (Level)site.Max(a => a.Level);
                    var status = site.FirstOrDefault().Status;

                    //refresh the cache of site codes
                    List<SiteCodeView> mockresult = null;
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Rejected, level, cache, false);
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, level, cache, false);
                }

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - RejectChanges", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }

        }


        private async Task<List<SiteActivities>> GetSiteActivities(DataTable sitecodesfilter)
        {
            List<SiteActivities> activities = new List<SiteActivities>();
            string queryString = @" 
                        select SiteActivities.SiteCode, SiteActivities.Version,Author, Date, Action,Deleted
                        from 
	                        [dbo].[SiteActivities] inner join 
	                        @siteCodes T on  SiteActivities.SiteCode= T.SiteCode 
                        where 
                           SiteActivities.deleted=0 and SiteActivities.Action like 'User edition%'
                        ";

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
                    SiteActivities act = new SiteActivities();
                    act.SiteCode = reader["SiteCode"] is null ? null : reader["SiteCode"].ToString();
                    act.Version = int.Parse(reader["Version"].ToString());
                    act.Author = reader["Author"] is null ? null : reader["Author"].ToString();
                    act.Date = DateTime.Parse(reader["Date"].ToString());
                    act.Action = reader["Action"] is null ? null : reader["Action"].ToString();
                    act.Deleted = bool.Parse(reader["Deleted"].ToString());
                    activities.Add(act);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetSiteActivities", "", _dataContext.Database.GetConnectionString());
            }
            finally
            {
                if (reader != null) await reader.DisposeAsync();
                if (command != null) command.Dispose();
                if (backboneConn != null) backboneConn.Dispose();
            }
            return activities;
        }
        private async Task<List<SiteChangeDb>> GetChanges(DataTable sitecodesfilter)
        {
            //List<SiteChangeDb> changes = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == modifiedSiteCode.VersionId).ToListAsync();

            List<SiteChangeDb> changes = new List<SiteChangeDb>();
            string queryString = @" 
                        select Changes.[SiteCode],Changes.[Version],Changes.[Country],[Status],[Tags],[Level],[ChangeCategory],[ChangeType],[NewValue],[OldValue],[Detail],[Code],[Section],[VersionReferenceId],[FieldName],[ReferenceSiteCode],[N2KVersioningVersion]
                        from 
	                        [dbo].[Changes]
	                        inner join
	                        @siteCodes T on  Changes.SiteCode= T.SiteCode and Changes.Version=T.Version
                        ";

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

                    SiteChangeDb change = new SiteChangeDb
                    {
                        SiteCode = reader["SiteCode"] is null ? null : reader["SiteCode"].ToString(),
                        Version = int.Parse(reader["Version"].ToString()),
                        Country = reader["Country"].ToString(),
                        Tags = reader["Tags"].ToString(),
                        ChangeCategory = reader["ChangeCategory"].ToString(),
                        ChangeType = reader["ChangeType"].ToString(),
                        NewValue = reader["NewValue"].ToString(),
                        OldValue = reader["OldValue"].ToString(),
                        Detail = reader["Detail"].ToString(),
                        Code = reader["Code"].ToString(),
                        Section = reader["Section"].ToString(),
                        VersionReferenceId = int.Parse(reader["VersionReferenceId"].ToString()),
                        FieldName = reader["FieldName"].ToString(),
                        ReferenceSiteCode = reader["ReferenceSiteCode"] is null ? reader["SiteCode"].ToString() : reader["ReferenceSiteCode"].ToString(),
                        N2KVersioningVersion = int.Parse(reader["N2KVersioningVersion"].ToString())
                    };
                    Level level;
                    Enum.TryParse<Level>(reader["Level"].ToString(), out level);
                    change.Level = level;

                    SiteChangeStatus status;
                    Enum.TryParse<SiteChangeStatus>(reader["Status"].ToString(), out status);
                    change.Status = status;
                    changes.Add(change);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetChanges", "", _dataContext.Database.GetConnectionString());
            }
            finally
            {
                if (reader != null) await reader.DisposeAsync();
                if (command != null) command.Dispose();
                if (backboneConn != null) backboneConn.Dispose();
            }
            return changes;
        }

        private async Task<List<Sites>> GetSites(DataTable sitecodesfilter)
        {
            List<Sites> sites = new List<Sites>();
            string queryString = @" SELECT Sites.[SiteCode],Sites.[Version],[Current],[Name],[CompilationDate],[ModifyTS],[CurrentStatus],[CountryCode],[SiteType],[AltitudeMin],[AltitudeMax],[N2KVersioningVersion],[N2KVersioningRef],[Area],[Length],[JustificationRequired],[JustificationProvided],[DateConfSCI],[SCIOverwriten],[Priority],[DatePropSCI],[DateSpa],[DateSac]  
                                    FROM [dbo].[Sites]
	                                inner join
	                                @siteCodes T on  Sites.SiteCode= T.SiteCode and Sites.Version=T.Version
                                 ";

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
                    Sites site = new Sites
                    {
                        SiteCode = reader["SiteCode"] is null ? null : reader["SiteCode"].ToString(),
                        Version = int.Parse(reader["Version"].ToString()),
                        Current = bool.Parse(reader["Current"].ToString()),
                        Name = reader["Name"].ToString(),
                        CountryCode = reader["CountryCode"].ToString(),
                        SiteType = reader["SiteType"].ToString(),
                        //AltitudeMin = double.Parse(reader["AltitudeMin"].ToString()),
                        //AltitudeMax = double.Parse(reader["AltitudeMax"].ToString()),
                        N2KVersioningVersion = int.Parse(reader["N2KVersioningVersion"].ToString()),
                        N2KVersioningRef = int.Parse(reader["N2KVersioningRef"].ToString()),
                        Area = decimal.Parse(reader["Area"].ToString()),
                        Length = decimal.Parse(reader["Length"].ToString()),
                        JustificationRequired = bool.Parse(reader["JustificationRequired"].ToString()),
                        //JustificationProvided = bool.Parse(reader["JustificationProvided"].ToString()),
                        Priority = bool.Parse(reader["Priority"].ToString())
                    };
                    if (reader["CompilationDate"].ToString() != "")
                    {
                        site.CompilationDate = DateTime.Parse(reader["CompilationDate"].ToString());
                    }
                    if (reader["ModifyTS"].ToString() != "")
                    {
                        site.ModifyTS = DateTime.Parse(reader["ModifyTS"].ToString());
                    }
                    if (reader["DateConfSCI"].ToString() != "")
                    {
                        site.DateConfSCI = DateTime.Parse(reader["DateConfSCI"].ToString());
                    }
                    if (reader["DatePropSCI"].ToString() != "")
                    {
                        site.DatePropSCI = DateTime.Parse(reader["DatePropSCI"].ToString());
                    }
                    if (reader["DateSpa"].ToString() != "")
                    {
                        site.DateSpa = DateTime.Parse(reader["DateSpa"].ToString());
                    }
                    if (reader["DateSac"].ToString() != "")
                    {
                        site.DateSac = DateTime.Parse(reader["DateSac"].ToString());
                    }
                    sites.Add(site);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "GetSites", "", _dataContext.Database.GetConnectionString());
            }
            finally
            {
                if (reader != null) await reader.DisposeAsync();
                if (command != null) command.Dispose();
                if (backboneConn != null) backboneConn.Dispose();
            }
            return sites;
        }

        public DataSet GetDataSet(string storedProcName, DataTable param)
        {
            try
            {
                SqlConnection backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
                var command = new SqlCommand(storedProcName, backboneConn) { CommandType = CommandType.StoredProcedure };
                SqlParameter paramTable1 = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                paramTable1.Value = param;
                paramTable1.TypeName = "[dbo].[SiteCodeFilter]";
                command.Parameters.Add(paramTable1);
                var result = new DataSet();
                var dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(result);

                dataAdapter.Dispose();
                command.Dispose();
                backboneConn.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetDataSet", "");
                throw ex;
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

                var sitecodeschanges = new DataTable("sitecodeschanges");
                sitecodeschanges.Columns.Add("SiteCode", typeof(string));
                sitecodeschanges.Columns.Add("Version", typeof(int));

                var sitecodesdelete = new DataTable("sitecodesdelete");
                sitecodesdelete.Columns.Add("SiteCode", typeof(string));
                sitecodesdelete.Columns.Add("Version", typeof(int));

                var iddelete = new DataTable("iddelete");
                iddelete.Columns.Add("ID", typeof(long));

                var JustificationFiles = new DataTable("JustificationFiles");
                JustificationFiles.Columns.Add("SiteCode", typeof(string));
                JustificationFiles.Columns.Add("OldVersion", typeof(int));
                JustificationFiles.Columns.Add("NewVersion", typeof(int));


                changedSiteStatus.ToList().ForEach(cs =>
                {
                    sitecodeschanges.Rows.Add(new Object[] { cs.SiteCode, cs.VersionId });

                    siteActivities.Add(new SiteActivities
                    {
                        SiteCode = cs.SiteCode,
                        Version = cs.VersionId,
                        Author = GlobalData.Username,
                        Date = DateTime.Now,
                        Action = "Back to Pending",
                        Deleted = false
                    });
                });


                //List<SiteChangeDb> changes = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == modifiedSiteCode.VersionId).ToListAsync();
                //get the activities already saved in the DB
                //List<SiteActivities> _lstActivities = await GetSiteActivities(sitecodeschanges);
                //get the changes already saved in the DB
                //List<SiteChangeDb> _lstChanges = await GetChanges(sitecodeschanges);

                //get the sites already saved in the DB
                // List<Sites> _lstSites = await GetSites(sitecodeschanges);


                //GET ALL FROM DB
                var dataSet = GetDataSet("spGetMoveToPendingTables", sitecodeschanges);

                //GET SITEACTIVITIES
                var siteActivitiesTable = dataSet?.Tables?[0];
                List<SiteActivities> activitiesDB = new List<SiteActivities>();
                foreach (DataRow row in siteActivitiesTable.Rows)
                {
                    SiteActivities act = new SiteActivities();
                    act.ID = long.Parse(row["ID"] is null ? null : row["ID"].ToString());
                    act.SiteCode = row["SiteCode"] is null ? null : row["SiteCode"].ToString();
                    act.Version = int.Parse(row["Version"].ToString());
                    act.Author = row["Author"] is null ? null : row["Author"].ToString();
                    act.Date = DateTime.Parse(row["Date"].ToString());
                    act.Action = row["Action"] is null ? null : row["Action"].ToString();
                    act.Deleted = bool.Parse(row["Deleted"].ToString());
                    activitiesDB.Add(act);
                }


                //GET SITES
                var sitesTable = dataSet?.Tables?[2];
                List<Sites> sitesDB = new List<Sites>();
                foreach (DataRow row in sitesTable.Rows)
                {
                    Sites site = new Sites
                    {
                        SiteCode = row["SiteCode"] is null ? null : row["SiteCode"].ToString(),
                        Version = int.Parse(row["Version"].ToString()),
                        CountryCode = row["CountryCode"].ToString(),
                        SiteType = row["SiteType"].ToString(),
                        //AltitudeMin = double.Parse(row["AltitudeMin"].ToString()),
                        //AltitudeMax = double.Parse(row["AltitudeMax"].ToString()),
                        //JustificationProvided = bool.Parse(row["JustificationProvided"].ToString())
                    };
                    if (row["Current"].ToString() != "")
                    {
                        site.Current = bool.Parse(row["Current"].ToString());
                    }
                    if (row["Name"].ToString() != "")
                    {
                        site.Name = row["Name"].ToString();
                    }
                    if (row["CompilationDate"].ToString() != "")
                    {
                        site.CompilationDate = DateTime.Parse(row["CompilationDate"].ToString());
                    }
                    if (row["ModifyTS"].ToString() != "")
                    {
                        site.ModifyTS = DateTime.Parse(row["ModifyTS"].ToString());
                    }
                    if (row["N2KVersioningVersion"].ToString() != "")
                    {
                        site.N2KVersioningVersion = int.Parse(row["N2KVersioningVersion"].ToString());
                    }
                    if (row["N2KVersioningRef"].ToString() != "")
                    {
                        site.N2KVersioningRef = int.Parse(row["N2KVersioningRef"].ToString());
                    }
                    if (row["Area"].ToString() != "")
                    {
                        site.Area = decimal.Parse(row["Area"].ToString());
                    }
                    if (row["Length"].ToString() != "")
                    {
                        site.Length = decimal.Parse(row["Length"].ToString());
                    }
                    if (row["JustificationRequired"].ToString() != "")
                    {
                        site.JustificationRequired = bool.Parse(row["JustificationRequired"].ToString());
                    }
                    if (row["Priority"].ToString() != "")
                    {
                        site.Priority = bool.Parse(row["Priority"].ToString());
                    }
                    if (row["DateConfSCI"].ToString() != "")
                    {
                        site.DateConfSCI = DateTime.Parse(row["DateConfSCI"].ToString());
                    }
                    if (row["DatePropSCI"].ToString() != "")
                    {
                        site.DatePropSCI = DateTime.Parse(row["DatePropSCI"].ToString());
                    }
                    if (row["DateSpa"].ToString() != "")
                    {
                        site.DateSpa = DateTime.Parse(row["DateSpa"].ToString());
                    }
                    if (row["DateSac"].ToString() != "")
                    {
                        site.DateSac = DateTime.Parse(row["DateSac"].ToString());
                    }
                    SiteChangeStatus statusChange;
                    Enum.TryParse<SiteChangeStatus>(row["CurrentStatus"].ToString(), out statusChange);
                    site.CurrentStatus = statusChange;
                    sitesDB.Add(site);
                }


                //GET CHANGES
                var siteChangesTable = dataSet?.Tables?[1];
                List<SiteChangeDb> changesDB = new List<SiteChangeDb>();
                foreach (DataRow row in siteChangesTable.Rows)
                {

                    SiteChangeDb change = new SiteChangeDb
                    {
                        SiteCode = row["SiteCode"] is null ? null : row["SiteCode"].ToString(),
                        Version = int.Parse(row["Version"].ToString()),
                        Country = row["Country"].ToString(),
                        Tags = row["Tags"].ToString(),
                        ChangeCategory = row["ChangeCategory"].ToString(),
                        ChangeType = row["ChangeType"].ToString(),
                        NewValue = row["NewValue"].ToString(),
                        OldValue = row["OldValue"].ToString(),
                        Detail = row["Detail"].ToString(),
                        Code = row["Code"].ToString(),
                        Section = row["Section"].ToString(),
                        VersionReferenceId = int.Parse(row["VersionReferenceId"].ToString()),
                        FieldName = row["FieldName"].ToString(),
                        ReferenceSiteCode = row["ReferenceSiteCode"] is null ? row["SiteCode"].ToString() : row["ReferenceSiteCode"].ToString(),
                        N2KVersioningVersion = int.Parse(row["N2KVersioningVersion"].ToString())
                    };
                    Level levelChange = 0;
                    Enum.TryParse<Level>(row["Level"].ToString(), out levelChange);
                    change.Level = levelChange;

                    //take the name of the site from the list created in the previous step
                    var _site = sitesDB.Where(s => s.SiteCode == change.SiteCode && s.Version == change.Version).FirstOrDefault();
                    if (_site != null) change.SiteName = _site.Name;

                    SiteChangeStatus statusChange;
                    Enum.TryParse<SiteChangeStatus>(row["Status"].ToString(), out statusChange);
                    change.Status = statusChange;
                    changesDB.Add(change);
                }

                foreach (var modifiedSiteCode in changedSiteStatus)
                {
                    try
                    {
                        List<SiteChangeDb> changes = changesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == modifiedSiteCode.VersionId).ToList();
                        if (changes == null || changes.Count == 0) continue;
                        //Create the listView for the cached lists. By deafult this values
                        SiteCodeView mySiteView = new SiteCodeView();
                        mySiteView.SiteCode = modifiedSiteCode.SiteCode;
                        mySiteView.Version = modifiedSiteCode.VersionId;
                        mySiteView.Name = changes.First().SiteName;
                        mySiteView.CountryCode = modifiedSiteCode.SiteCode.Substring(0, 2);

                        Sites siteToDelete = null;
                        int previousCurrent = -1;//The 0 value can be a version

                        #region In case of user edition

                        List<SiteActivities> activities = activitiesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode).ToList();

                        #region Was this site edited after being accepted?
                        SiteChangeDb? change = changes.Where(e => e.ChangeType == "User edition").FirstOrDefault();
                        if (change != null)
                        {
                            Lineage temp = await _dataContext.Set<Lineage>().Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == change.VersionReferenceId).FirstOrDefaultAsync();
                            LineageAntecessors temp1 = await _dataContext.Set<LineageAntecessors>().Where(e => e.LineageID == temp.ID).FirstOrDefaultAsync();
                            //Select the max version for the site with the currentsatatus accepted, but not the version of the change and the referenced version
                            previousCurrent = await _dataContext.Set<Sites>().Where(e => e.SiteCode == temp1.SiteCode && e.Version == temp1.Version && e.CurrentStatus == SiteChangeStatus.Accepted).Select(e => e.Version).FirstOrDefaultAsync();
                            //Search the previous activities
                            List<SiteActivities> activityDelete = activities.Where(e => (e.Version == modifiedSiteCode.VersionId || e.Version == change.VersionReferenceId) && e.Action == "User edition").ToList();

                            //mark the result as activities deleted
                            activityDelete.ForEach(s => iddelete.Rows.Add(new Object[] { s.ID }));


                            //Add comments and docs to the soon to be pending version (the previous version referenced in the change)
                            //SqlParameter paramNewVersion1 = new SqlParameter("@newVersion", change.VersionReferenceId);
                            //await _dataContext.Database.ExecuteSqlRawAsync(
                            //    "exec spCopyJustificationFilesAndStatusChanges @sitecode, @oldVersion, @newVersion",
                            //    paramSiteCode, paramOldVersion, paramNewVersion1);

                            JustificationFiles.Rows.Add(new Object[] { modifiedSiteCode.SiteCode, modifiedSiteCode.VersionId, change.VersionReferenceId });

                            //Find edited version in order to remove from the sites entity
                            siteToDelete = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == modifiedSiteCode.VersionId).FirstOrDefault();

                            //Change the version and the name for the previous version
                            //paramVersionId = new SqlParameter("@version", change.VersionReferenceId);
                            mySiteView.Version = change.VersionReferenceId; //points to the final version
                            string previousName = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == change.VersionReferenceId).Select(x => x.Name).First().ToString();
                            mySiteView.Name = previousName;
                        }
                        #endregion

                        #region Was this site edited after being rejected?
                        List<SiteActivities> activityCheck = activities.Where(e => e.Action == "User edition after rejection of version " + modifiedSiteCode.VersionId).ToList();
                        if (activityCheck != null && activityCheck.Count > 0)
                        {
                            SiteChangeDb siteDeleted = changes.Where(e => e.ChangeType == "Site Deleted").FirstOrDefault();
                            Sites previousSite = new Sites();
                            if (siteDeleted != null)
                            {
                                //Get the site max accepted version for the last package but not the current
                                previousSite = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.CurrentStatus == SiteChangeStatus.Accepted && e.Current == false).OrderByDescending(x => x.N2KVersioningVersion).ThenByDescending(x => x.Version).FirstOrDefault();
                            }
                            else
                            {
                                //Get the site max accepted version for the last package but not the current nor the present version 
                                previousSite = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version != modifiedSiteCode.VersionId && e.CurrentStatus == SiteChangeStatus.Accepted && e.Current == false).OrderByDescending(x => x.N2KVersioningVersion).ThenByDescending(x => x.Version).FirstOrDefault();
                            }
                            previousCurrent = previousSite.Version;

                            //mark the result as activities deleted
                            activityCheck.ForEach(s => iddelete.Rows.Add(new Object[] { s.ID }));

                            //Find the current site
                            siteToDelete = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Current == true).FirstOrDefault();
                        }
                        #endregion

                        #region In both cases
                        if (change != null || (activityCheck != null && activityCheck.Count > 0))
                        {
                            //paramNewVersion2 = new SqlParameter("@newVersion", previousCurrent);

                            //Add comments and docs to the previous current version
                            //await _dataContext.Database.ExecuteSqlRawAsync(
                            //    "exec spCopyJustificationFilesAndStatusChanges @sitecode, @oldVersion, @newVersion",
                            //    paramSiteCode, paramOldVersion, paramNewVersion2);
                            JustificationFiles.Rows.Add(new Object[] { modifiedSiteCode.SiteCode, modifiedSiteCode.VersionId, previousCurrent });

                            //Delete edited version
                            if (siteToDelete != null)
                            {
                                sitecodesdelete.Rows.Add(new Object[] { siteToDelete.SiteCode, siteToDelete.Version });
                            }
                        }
                        #endregion

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

                        sitecodesfilter.Rows.Add(new Object[] { modifiedSiteCode.SiteCode, modifiedSiteCode.VersionId });
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
                    SqlParameter paramTable2 = new SqlParameter("@iddelete", System.Data.SqlDbType.Structured);
                    paramTable2.Value = iddelete;
                    paramTable2.TypeName = "[dbo].[IdDelete]";
                    await _dataContext.Database.ExecuteSqlRawAsync(
                        "exec spMarkActivitiesAsDeleted @iddelete",
                        paramTable2);

                    SqlParameter paramTable1 = new SqlParameter("@justificationFiles", System.Data.SqlDbType.Structured);
                    paramTable1.Value = JustificationFiles;
                    paramTable1.TypeName = "[dbo].[JustificationFilesAndStatusChanges]";
                    await _dataContext.Database.ExecuteSqlRawAsync(
                        "exec spCopyJustificationFilesAndStatusChangesBulk @justificationFiles",
                        paramTable1);
                    //Save activities changes
                    await _dataContext.SaveChangesAsync();

                    SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                    paramTable.Value = sitecodesdelete;
                    paramTable.TypeName = "[dbo].[SiteCodeFilter]";

                    //Delete the clones of manual editions
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

                    //Save new avtivities
                    await SiteActivities.SaveBulkRecord(_dataContext.Database.GetConnectionString(), siteActivities);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                ////GetSiteCodesByStatusAndLevelAndCountry
                ////get the country and the level of the first site code. The other codes will have the same level
                ////refresh the cache
                if (result.Count > 0)
                {
                    var country = (result.First().SiteCode).Substring(0, 2);

                    //refresh the cache of site codes
                    List<SiteCodeView> mockresult = null;
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, status, level, cache, false);
                    mockresult = await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, level, cache, false);
                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - MoveToPending", "", _dataContext.Database.GetConnectionString());
                throw ex;
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - MarkAsJustificationRequired", "", _dataContext.Database.GetConnectionString());
                throw ex;
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - JustificationProvided", "", _dataContext.Database.GetConnectionString());
                throw ex;
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
            await Task.Delay(10);
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
            return null;
        }


    }
}
