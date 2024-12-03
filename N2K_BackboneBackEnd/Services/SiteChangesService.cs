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
using N2K_BackboneBackEnd.Helpers;
using System.Globalization;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Vml.Office;
using N2K_BackboneBackEnd.Models.BackboneDB;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Razor;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            public string SiteCode { get; set; } = string.Empty;
            public string SiteName { get; set; } = string.Empty;
            public Level? Level { get; set; }
            public List<SiteChangeDbNumsperLevel> ChangeList { get; set; } = new List<SiteChangeDbNumsperLevel>();

        }

        private readonly N2KBackboneContext _dataContext;
        private readonly IEnumerable<SpeciesTypes> _speciesTypes;
        private readonly IEnumerable<HabitatTypes> _habitatTypes;
        private readonly IEnumerable<SpeciesPriority> _speciesPriority;
        private readonly IEnumerable<HabitatPriority> _habitatPriority;
        private readonly IEnumerable<Countries> _countries;
        private readonly IEnumerable<Lineage> _lineage;
        private IEnumerable<Habitats>? _siteHabitats;
        private IEnumerable<Species>? _siteSpecies;
        private IEnumerable<SpeciesOther>? _siteSpeciesOther;
        private IEnumerable<Habitats>? _siteHabitatsReference;
        private IEnumerable<Species>? _siteSpeciesReference;
        private IEnumerable<SpeciesOther>? _siteSpeciesOtherReference;

        // this list is used to check if the affected sites field should return null or not
        private List<LineageTypes> lineageCases = new() { LineageTypes.Recode, LineageTypes.Split, LineageTypes.Merge };

        public SiteChangesService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
            _speciesTypes = _dataContext.Set<SpeciesTypes>().AsNoTracking().ToList();
            _habitatTypes = _dataContext.Set<HabitatTypes>().AsNoTracking().ToList();
            _speciesPriority = _dataContext.Set<SpeciesPriority>().AsNoTracking().ToList();
            _habitatPriority = _dataContext.Set<HabitatPriority>().AsNoTracking().ToList();
            _countries = _dataContext.Set<Countries>().AsNoTracking().ToList();
            _lineage = _dataContext.Set<Lineage>().AsNoTracking().ToList();
        }

        public async Task<List<SiteChangeDbEdition>> GetSiteChangesAsync(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, int page = 1, int pageLimit = 0, bool onlyedited = false, bool onlyjustreq = false, bool onlysci = false)
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
                SqlParameter param1 = new("@country", country);
                SqlParameter param2 = new("@status", status.HasValue ? status.ToString() : String.Empty);
                SqlParameter param3 = new("@level", level.HasValue ? level.ToString() : String.Empty);
                SqlParameter param4 = new("@siteCodes", System.Data.SqlDbType.Structured);
                SqlParameter param5 = new("@status", DBNull.Value);
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
                List<Lineage> lineageChanges = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetLineageData @country, @status",
                                param1, param5).ToListAsync();

                List<SiteChangeDb> siteChanges = new();
                if (status != null)
                {
                    siteChanges = await _dataContext.Set<SiteChangeDb>().Where(e => e.Country == country
                                 && e.Status == status).ToListAsync();
                }
                else
                {
                    siteChanges = await _dataContext.Set<SiteChangeDb>().Where(e => e.Country == country).ToListAsync();
                }

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

                            Lineage? lineageChange = lineageChanges.FirstOrDefault(e => e.SiteCode == change.SiteCode && e.Version == change.Version);
                            siteChange.LineageChangeType = lineageChange?.Type ?? LineageTypes.NoChanges;

                            if (siteChange.LineageChangeType == LineageTypes.NoChanges)
                            {
                                SiteChangeDb lineageSiteChange = siteChanges.FirstOrDefault(e => e.SiteCode == change.SiteCode && e.Version == change.Version
                                    && (e.ChangeType == "Site Added" || e.ChangeType == "Site Merged" || e.ChangeType == "Site Recoded"
                                    || e.ChangeType == "Site Split" || e.ChangeType == "Site Deleted"));
                                if (lineageSiteChange != null)
                                {
                                    switch (lineageSiteChange.ChangeType)
                                    {
                                        case "Site Added":
                                            siteChange.LineageChangeType = LineageTypes.Creation;
                                            break;
                                        case "Site Deleted":
                                            siteChange.LineageChangeType = LineageTypes.Deletion;
                                            break;
                                        case "Site Split":
                                            siteChange.LineageChangeType = LineageTypes.Split;
                                            break;
                                        case "Site Merged":
                                            siteChange.LineageChangeType = LineageTypes.Merge;
                                            break;
                                        case "Site Recoded":
                                            siteChange.LineageChangeType = LineageTypes.Recode;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }

                            if (lineageCases.Contains((LineageTypes)siteChange.LineageChangeType))
                                siteChange.AffectedSites = GetAffectedSites(siteCode, lineageChange).Result;

                            if (change.ReferenceSiteCode != null && change.ReferenceSiteCode != "" && !siteChange.LineageChangeType.Equals(LineageTypes.Creation))
                                siteChange.ReferenceSiteCode = change.ReferenceSiteCode;
                            siteChange.Version = change.Version;
                            SiteActivities activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version).FirstOrDefault();
                            if (activity == null)
                            {
                                SiteChangeDb editionChange = siteChanges.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version && e.ChangeType == "User edition").FirstOrDefault();
                                if (editionChange != null)
                                    activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == editionChange.VersionReferenceId).FirstOrDefault();
                                if (activity == null)
                                {
                                    activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Action == "User edition after rejection of version " + change.Version).FirstOrDefault();
                                }
                            }

                            siteChange.EditedBy = activity is null ? null : activity.Author;
                            siteChange.EditedDate = activity is null ? null : activity.Date;

                            siteChange.SiteType = sitesList.Find(s => s.SiteCode == siteChange.SiteCode && s.Version == siteChange.Version)?.Type;

                            if (lineageCases.Contains((LineageTypes)siteChange.LineageChangeType))
                                siteChange.AffectedSites = GetAffectedSites(siteCode, lineageChange).Result;

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
                                NumChanges = 1,
                                OldValue = change.OldValue,
                                NewValue = change.NewValue
                            };
                            siteChange.subRows = new List<SiteChangeView>
                            {
                                changeView
                            };
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
                                    NumChanges = 1,
                                    OldValue = change.OldValue,
                                    NewValue = change.NewValue
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
                if (onlyjustreq)
                    result = result.Where(x => x.JustificationRequired != null && x.JustificationRequired != false).ToList();
                if (onlysci)
                    result = result.Where(x => x.SiteType == "B" || x.SiteType == "C").ToList();
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

        public async Task<SiteChangeDb?> GetSiteChangeByIdAsync(int id)
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

        private async Task<String>? GetAffectedSites(string pSiteCode, Lineage? lineageChange)
        {
            if (lineageChange == null)
                return "";

            // Get antecessors from LineageAntecessors table matching lineageID
            List<String>? antecessorsSiteCodes = await _dataContext.Set<LineageAntecessors>().AsNoTracking()
                .Where(l => l.LineageID == lineageChange.ID).Select(x => x.SiteCode).ToListAsync();
            lineageChange.AntecessorsSiteCodes = String.Join(",", antecessorsSiteCodes);

            // Get successor siteCodes by searching for successor lineage IDs
            List<long> successorsIDs = await _dataContext.Set<LineageAntecessors>().AsNoTracking()
                .Where(l => l.SiteCode == pSiteCode).Select(l => l.LineageID).ToListAsync();
            List<String>? successorSiteCodes = await _dataContext.Set<Lineage>().AsNoTracking()
                .Where(l => successorsIDs.Contains(l.ID)).Select(l => l.SiteCode).ToListAsync();

            return String.Join(",",
                new List<String> { pSiteCode }
                .Concat(antecessorsSiteCodes
                .Concat(successorSiteCodes))
            .ToHashSet());
        }

        public async Task<SiteChangeDetailViewModel> GetSiteChangesDetail(string pSiteCode, int pCountryVersion)
        {
            try
            {
                SiteChangeDetailViewModel changeDetailVM = new()
                {
                    SiteCode = pSiteCode,
                    Version = pCountryVersion,
                    Warning = new SiteChangesLevelDetail(),
                    Info = new SiteChangesLevelDetail(),
                    Critical = new SiteChangesLevelDetail()
                };

                // Get the harvested envelope
                ProcessedEnvelopes harvestedEnvelope = await _dataContext.Set<ProcessedEnvelopes>().AsNoTracking().Where(envelope => envelope.Country == pSiteCode.Substring(0, 2) && envelope.Status == HarvestingStatus.Harvested).FirstOrDefaultAsync();

                // Get lineage change type from Lineage table
                Lineage? lineageChange = await _dataContext.Set<Lineage>().AsNoTracking().FirstOrDefaultAsync(l => l.SiteCode == pSiteCode && l.Version == pCountryVersion && l.N2KVersioningVersion == harvestedEnvelope.Version);
                changeDetailVM.LineageChangeType = lineageChange?.Type ?? LineageTypes.NoChanges;

                // Get affected sites list only in certain lineage change types
                if (lineageCases.Contains((LineageTypes)changeDetailVM.LineageChangeType))
                    changeDetailVM.AffectedSites = GetAffectedSites(pSiteCode, lineageChange).Result;

                var site = await _dataContext.Set<Sites>().AsNoTracking().Where(site => site.SiteCode == pSiteCode && site.Version == pCountryVersion).FirstOrDefaultAsync();
                if (site != null)
                {
                    changeDetailVM.HasGeometry = false;
                    SqlParameter param1 = new("@SiteCode", site.SiteCode);
                    SqlParameter param2 = new("@Version", site.Version);

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
                var changesDb = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(changes => changes.SiteCode == pSiteCode && changes.N2KVersioningVersion == harvestedEnvelope.Version).ToListAsync();
                changesDb = changesDb.OrderByDescending(m => m.Version).DistinctBy(m => new { m.SiteCode, m.Country, m.Status, m.Tags, m.Level, m.ChangeCategory, m.ChangeType, m.NewValue, m.OldValue, m.Detail, m.Code, m.Section, m.VersionReferenceId, m.FieldName, m.ReferenceSiteCode, m.N2KVersioningVersion }).ToList();
                if (changesDb != null)
                {
                    if (!changeDetailVM.LineageChangeType.Equals(LineageTypes.Creation))
                    {
                        changeDetailVM.ReferenceSiteCode = changesDb.FirstOrDefault()?.ReferenceSiteCode;
                    }
                    if (changesDb.FirstOrDefault()?.ChangeType == "Site Deleted")
                    {
                        changeDetailVM.Status = changesDb.FirstOrDefault()?.Status;
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

        public async Task<List<SiteCodeView>> GetNonPendingSiteCodes(string country, Boolean onlyedited = false, Boolean onlyjustreq = false, Boolean onlysci = false)
        {
            try
            {
                SqlParameter param1 = new("@country", country);
                IQueryable<SiteCodeVersion> changes = _dataContext
                    .Set<SiteCodeVersion>()
                    .FromSqlRaw($"exec dbo.[spGetActiveSiteCodesByCountryNonPending]  @country", param1);
                List<SiteCodeView> result = new();
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

                    SiteCodeView temp = new()
                    {
                        SiteCode = change.SiteCode,
                        Version = change.Version,
                        Name = change.Name,
                        EditedBy = activity is null ? null : activity.Author,
                        EditedDate = activity is null ? null : activity.Date,
                        LineageChangeType = changeLineage,
                        Type = change.Type,
                        JustificationRequired = await _dataContext.Set<Sites>().Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version).Select(s => s.JustificationRequired).FirstOrDefaultAsync()
                    };
                    result.Add(temp);
                }
                if (onlyedited)
                    result = result.Where(x => x.EditedDate != null).ToList();
                if (onlyjustreq)
                    result = result.Where(x => x.JustificationRequired != null && x.JustificationRequired != false).ToList();
                if (onlysci)
                    result = result.Where(x => x.Type == "B" || x.Type == "C").ToList();
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetNonPendingSiteCodes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task PopulateCacheByByStatusAndLevelAndCountry(List<SiteChangesSummary> changes, string country, SiteChangeStatus? status, Level? level, IMemoryCache cache)
        {
            await Task.Delay(10);

            string listName = string.Format("{0}_{1}_{2}_{3}", "listcodes",
                country,
                status.ToString(),
                level.ToString()
            );
            if (cache.TryGetValue(listName, out List<SiteCodeView> cachedList)) return;
            
            List<SiteCodeView> result = new();
            switch (level)
            {
                case Level.Critical:
                    result = changes.Where(c => c.SiteCode.StartsWith(country) && c.Status == status && c.NumCritical > 0).
                        Select(c1 => new SiteCodeView
                        {
                            SiteCode = c1.SiteCode,
                            Version = c1.Version,
                            Name = c1.Name,
                            EditedBy = c1.Author,
                            EditedDate = c1.Date,
                            LineageChangeType =  c1.LineageType,
                            Type = c1.SiteType,
                            JustificationRequired = c1.JustificationRequired
                    }).ToList();
                    break;

                case Level.Warning:
                    result = changes.Where(c => c.SiteCode.StartsWith(country) && c.Status == status && c.NumCritical == 0 && c.NumWarning>0).
                        Select(c1 => new SiteCodeView
                        {
                            SiteCode = c1.SiteCode,
                            Version = c1.Version,
                            Name = c1.Name,
                            EditedBy = c1.Author,
                            EditedDate = c1.Date,
                            LineageChangeType = c1.LineageType,
                            Type = c1.SiteType,
                            JustificationRequired = c1.JustificationRequired
                        }).ToList();
                    break;

                case Level.Info:
                    result = changes.Where(c => c.SiteCode.StartsWith(country) && c.Status == status && c.NumCritical == 0 && c.NumWarning == 0 && c.NumInfo>0).
                        Select(c1 => new SiteCodeView
                        {
                            SiteCode = c1.SiteCode,
                            Version = c1.Version,
                            Name = c1.Name,
                            EditedBy = c1.Author,
                            EditedDate = c1.Date,
                            LineageChangeType = c1.LineageType,
                            Type = c1.SiteType,
                            JustificationRequired = c1.JustificationRequired
                        }).ToList();

                    break;

                default:
                    break;
            }


            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(2500))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(40000);
            cache.Set(listName, result, cacheEntryOptions);
        }


        private async Task BuildSitecodesCaches(string country, IMemoryCache cache)
        {
            try
            {
                SqlParameter param1 = new("@country", country);
                List<SiteChangesSummary> changes = await _dataContext.Set<SiteChangesSummary>().FromSqlRaw($"exec dbo.[spGetSiteChangesSummary]  @country",
                                param1).ToListAsync();


                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Pending, Level.Critical, cache);
                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Pending, Level.Warning, cache);
                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Pending, Level.Info, cache);

                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Accepted, Level.Critical, cache);
                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Accepted, Level.Warning, cache);
                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Accepted, Level.Info, cache);

                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Rejected, Level.Critical, cache);
                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Rejected, Level.Warning, cache);
                await PopulateCacheByByStatusAndLevelAndCountry(changes, country, SiteChangeStatus.Rejected, Level.Info, cache);


            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - BuildSitecodesCaches", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }

        }


        public async Task<List<SiteCodeView>> GetSiteCodesByStatusAndLevelAndCountry(string country, SiteChangeStatus? status, Level? level, IMemoryCache cache, bool refresh = false, bool onlyedited = false, bool onlyjustreq = false, bool onlysci = false)
        {
            //DuckDBLoader duckDBLoader=null;
            try
            {

                //duckDBLoader = new Helpers.DuckDBLoader(_dataContext);

                string listName = string.Format("{0}_{1}_{2}_{3}", "listcodes",
                    country,
                    status.ToString(),
                    level.ToString()
                );
                //if there has been a change in the status refresh the changed sitecodes cache
                if (refresh) cache.Remove(listName);

                List<SiteCodeView> result = new();
                if (cache.TryGetValue(listName, out List<SiteCodeView> cachedList))
                {
                    result = cachedList;
                }
                else
                {
                    await BuildSitecodesCaches(country, cache);
                    cache.TryGetValue(listName, out List<SiteCodeView> _cachedList);
                    result = _cachedList;


                    //SqlParameter param1 = new("@country", country);
                    //SqlParameter param2 = new("@status", status.ToString());
                    //SqlParameter param3 = new("@level", level.ToString());

                    //List<SiteCodeVersion> changedSiteCodes = new List<SiteCodeVersion>();
                    //List<SiteActivities> activities = new List<SiteActivities>();
                    //List<Lineage> lineageChanges = new List<Lineage>();
                    //List<SiteChangeDb> siteChanges = new List<SiteChangeDb>();
                    //changedSiteCodes = await duckDBLoader.GetActiveSitesByCountryAndStatusAndLevel(country, status, level);
                    //activities = await duckDBLoader.LoadSiteActivitiesUserEdition(country);
                    //lineageChanges = await duckDBLoader.LoadLineageChanges(country);
                    //siteChanges = await duckDBLoader.LoadChanges(country, status);




                    ////IQueryable<SiteCodeVersion> changes = _dataContext.Set<SiteCodeVersion>().FromSqlRaw($"exec dbo.[spGetActiveSiteCodesByCountryAndStatusAndLevel]  @country, @status, @level",
                    ////            param1, param2, param3);

                    ////List<SiteActivities> activities = await _dataContext.Set<SiteActivities>().FromSqlRaw($"exec dbo.spGetSiteActivitiesUserEditionByCountry  @country",
                    ////            param1).ToListAsync();

                    ////List <Lineage> lineageChanges = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetLineageData @country, @status",
                    ////            param1, new SqlParameter("@status", DBNull.Value)).ToListAsync();

                    ///*
                    //List<SiteChangeDb> siteChanges = new();
                    //if (status != null)
                    //{
                    //    siteChanges = await _dataContext.Set<SiteChangeDb>().Where(e => e.Country == country
                    //                 && e.Status == status).ToListAsync();
                    //}
                    //else
                    //{
                    //    siteChanges = await _dataContext.Set<SiteChangeDb>().Where(e => e.Country == country).ToListAsync();
                    //}
                    //*/

                    ////foreach (var change in (await changes.ToListAsync()))
                    //foreach (var change in changedSiteCodes)
                    //    {
                    //    SiteActivities activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version).FirstOrDefault();
                    //    if (activity == null)
                    //    {
                    //        SiteChangeDb editionChange = siteChanges.Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version && e.ChangeType == "User edition").FirstOrDefault();
                    //        if (editionChange != null)
                    //            activity = activities.Where(e => e.SiteCode == change.SiteCode && e.Version == editionChange.VersionReferenceId).FirstOrDefault();
                    //    }

                    //    // Get Lineage change type from lineageChanges
                    //    LineageTypes? changeLineage = lineageChanges.FirstOrDefault(
                    //        l => l.SiteCode == change.SiteCode
                    //            && l.Version == change.Version
                    //        )?.Type ?? LineageTypes.NoChanges;

                    //    if (changeLineage == LineageTypes.NoChanges)
                    //    {
                    //        SiteChangeDb lineageChange = siteChanges.FirstOrDefault(e => e.SiteCode == change.SiteCode && e.Version == change.Version
                    //            && (e.ChangeType == "Site Added" || e.ChangeType == "Site Merged" || e.ChangeType == "Site Recoded"
                    //            || e.ChangeType == "Site Split" || e.ChangeType == "Site Deleted"));
                    //        if (lineageChange != null)
                    //        {
                    //            switch (lineageChange.ChangeType)
                    //            {
                    //                case "Site Added":
                    //                    changeLineage = LineageTypes.Creation;
                    //                    break;
                    //                case "Site Deleted":
                    //                    changeLineage = LineageTypes.Deletion;
                    //                    break;
                    //                case "Site Split":
                    //                    changeLineage = LineageTypes.Split;
                    //                    break;
                    //                case "Site Merged":
                    //                    changeLineage = LineageTypes.Merge;
                    //                    break;
                    //                case "Site Recoded":
                    //                    changeLineage = LineageTypes.Recode;
                    //                    break;
                    //                default:
                    //                    break;
                    //            }
                    //        }
                    //    }



                    //    SiteCodeView temp = new()
                    //    {
                    //        SiteCode = change.SiteCode,
                    //        Version = change.Version,
                    //        Name = change.Name,
                    //        EditedBy = activity is null ? null : activity.Author,
                    //        EditedDate = activity is null ? null : activity.Date,
                    //        LineageChangeType = changeLineage,
                    //        Type = change.Type,
                    //        //JustificationRequired = await _dataContext.Set<Sites>().Where(e => e.SiteCode == change.SiteCode && e.Version == change.Version).Select(s => s.JustificationRequired).FirstOrDefaultAsync()
                    //        JustificationRequired = await duckDBLoader.SiteJustificationRequired(change.SiteCode, change.Version)
                    //    };
                    //    result.Add(temp);
                    //}
                    //var cacheEntryOptions = new MemoryCacheEntryOptions()
                    //        .SetSlidingExpiration(TimeSpan.FromSeconds(2500))
                    //        .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    //       .SetPriority(CacheItemPriority.Normal)
                    //        .SetSize(40000);
                    //cache.Set(listName, result, cacheEntryOptions);
                }
                if (onlyedited)
                    result = result.Where(x => x.EditedDate != null).ToList();
                if (onlyjustreq)
                    result = result.Where(x => x.JustificationRequired != null && x.JustificationRequired != false).ToList();
                if (onlysci)
                    result = result.Where(x => x.Type == "B" || x.Type == "C").ToList();
                return result.OrderBy(o => o.SiteCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetSiteCodesByStatusAndLevelAndCountry", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            /*
            finally
            {
                if (duckDBLoader!=null)
                    await duckDBLoader.DisposeAsync();
            }
            */

        }

        public async Task<int> GetPendingChangesByCountry(string? country, IMemoryCache cache)
        {
            try
            {
                //return (await GetSiteCodesByStatusAndLevelAndCountry(country, SiteChangeStatus.Pending, null,cache)).Count;
                SqlParameter param1 = new("@country", country);

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
                SiteChangesLevelDetail changesPerLevel = new()
                {
                    Level = level
                };

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
                            if (_levelDetail.ChangeType != "User edition")
                                changesPerLevel.SiteInfo.ChangesByCategory.Add(GetChangeCategoryDetail(_levelDetail.ChangeCategory, _levelDetail.ChangeType, _levelDetail.ChangeList));
                            break;
                        case "BioRegions":
                            if (_levelDetail.ChangeType != "User edition")
                                changesPerLevel.SiteInfo.ChangesByCategory.Add(GetBioregionChangeDetail(_levelDetail.ChangeCategory, _levelDetail.ChangeType, _levelDetail.ChangeList));
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
                                if (_levelDetail.ChangeType.Contains("Species"))
                                {
                                    _Section.DeletedCodes.Add(new CategoryChangeDetail
                                    {
                                        ChangeCategory = _levelDetail.Section,
                                        ChangeType = _levelDetail.ChangeType,
                                        ChangedCodesDetail = new List<CodeChangeDetail>()
                                    });
                                }
                                else
                                {
                                    _Section.DeletedCodes.Add(new CategoryChangeDetail
                                    {
                                        ChangeCategory = _levelDetail.Section,
                                        ChangeType = String.Format("{0} Deleted", _levelDetail.Section),
                                        ChangedCodesDetail = new List<CodeChangeDetail>()
                                    });
                                }
                            }

                            foreach (var changedItem in _levelDetail.ChangeList.OrderBy(c => c.Code == null ? "" : c.Code))
                            {
                                _Section.DeletedCodes.ElementAt(0).ChangedCodesDetail.Add(
                                    CodeAddedRemovedDetail(_levelDetail.Section, _levelDetail.ChangeType, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.VersionReferenceId, changedItem.VersionReferenceId)
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
                            if (_levelDetail.ChangeType.Contains("Species"))
                            {
                                _Section.AddedCodes.Add(new CategoryChangeDetail
                                {
                                    ChangeCategory = _levelDetail.Section,
                                    ChangeType = _levelDetail.ChangeType,
                                    ChangedCodesDetail = new List<CodeChangeDetail>()
                                });
                            }
                            else
                            {
                                _Section.AddedCodes.Add(new CategoryChangeDetail
                                {
                                    ChangeCategory = _levelDetail.Section,
                                    ChangeType = String.Format("{0} Added", _levelDetail.Section),
                                    ChangedCodesDetail = new List<CodeChangeDetail>()
                                });
                            }
                        }

                        if (_levelDetail.ChangeList.Where(c => !c.ChangeType.Contains("Other Species")).Any())
                        {
                            foreach (var changedItem in _levelDetail.ChangeList.Where(c => c.Code != "" || c.Code != null))
                            {
                                _Section.AddedCodes.ElementAt(0).ChangedCodesDetail.Add(
                                    CodeAddedRemovedDetail(_levelDetail.Section, _levelDetail.ChangeType, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.Version, changedItem.VersionReferenceId)
                                );
                            }
                        }

                        if (_levelDetail.ChangeList.Where(c => c.ChangeType.Contains("Other Species Added")).Any())
                        {
                            _Section.AddedCodes.Add(new CategoryChangeDetail
                            {
                                ChangeCategory = _levelDetail.Section,
                                ChangeType = _levelDetail.ChangeType,
                                ChangedCodesDetail = new List<CodeChangeDetail>()
                            });
                            foreach (var changedItem in _levelDetail.ChangeList)
                            {
                                CategoryChangeDetail otherSpeciesDetail = _Section.AddedCodes.First(c => c.ChangeType == "Other Species Added");
                                otherSpeciesDetail.ChangedCodesDetail.Add(
                                    CodeAddedRemovedDetail(_levelDetail.Section, _levelDetail.ChangeType, changedItem.Code, changedItem.ChangeId, changedItem.SiteCode, changedItem.Version, changedItem.VersionReferenceId)
                                );
                            }
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
                CategoryChangeDetail catChange = new()
                {
                    ChangeType = changeType,
                    ChangeCategory = changeCategory
                };

                foreach (SiteChangeDb? changedItem in changeList.OrderBy(c => c.Code == null ? "" : c.Code))
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
                        || catChange.ChangeType == "Change of spatial area" || catChange.ChangeType == "Spatial Area Decrease"
                        || catChange.ChangeType == "Spatial Area Increase" || catChange.ChangeType.StartsWith("Cover_ha"))
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
                            if (!catChange.ChangeType.StartsWith("Cover_ha"))
                            {
                                if (reference != 0)
                                {
                                    fields.Add("Percentage", Math.Round((((reported - reference) / reference) * 100), 4).ToString("F4", culture));
                                }
                                else
                                {
                                    fields.Add("Percentage", Math.Round((reported - reference), 4).ToString("F4", culture));
                                }
                            }
                        }
                        else
                        {
                            fields.Add("Difference", nullCase);
                            fields.Add("Percentage", nullCase);
                        }
                    }
                    if (catChange.ChangeType == "Deletion of Spatial Area" ||
                        catChange.ChangeType == "Addition of Spatial Area")
                    {
                        string? reportedString = nullCase;
                        string? referenceString = nullCase;
                        string? detail = changedItem.Detail;
                        bool deleted = false;

                        switch (catChange.ChangeType)
                        {
                            case "Deletion of Spatial Area":
                                fields.Add("Cumulative deleted spatial area (ha)", "");
                                deleted = true;
                                break;
                            case "Addition of Spatial Area":
                                fields.Add("Cumulative added spatial area (ha)", "");
                                break;
                        }

                        if (fields.TryGetValue("Submission", out reportedString)
                            && reportedString != "" && !string.IsNullOrEmpty(detail))
                        {
                            var culture = new CultureInfo("en-US");
                            var reported = decimal.Parse(reportedString, CultureInfo.InvariantCulture);
                            var totalArea = decimal.Parse(detail, CultureInfo.InvariantCulture);

                            if (totalArea != 0)
                            {
                                fields[deleted ? "Cumulative deleted spatial area (ha)" : "Cumulative added spatial area (ha)"] = string.Format("{0}", Math.Round(reported, 4).ToString("F4", culture));
                                fields.Add("Percentage", string.Format("{0}{1}", deleted ? "-" : "", Math.Round(((reported * 100) / totalArea), 4).ToString("F4", culture)));
                            }
                            else
                            {
                                fields.Add("Percentage", "0.0");
                            }
                        }
                        else
                        {
                            fields.Add("Percentage", nullCase);
                        }
                        fields.Remove("Reference");
                        fields.Remove("Submission");
                    }

                    if (changeCategory == "Habitats")
                    {
                        CodeChangeDetail changeDetail;

                        if (changeType.Contains("Representativity")
                            || changeType.Contains("Cover_ha")
                            || changeType.Contains("Relative surface")
                            || changeType.Contains("PriorityForm"))
                        {
                            string? priorityH = "-";
                            HabitatPriority? _habpriority = _habitatPriority.FirstOrDefault(h => h.HabitatCode.ToLower() == changedItem.Code?.ToLower());
                            var habDetails = _siteHabitats.Where(sh => sh.HabitatCode.ToLower() == changedItem.Code?.ToLower())
                                .Select(hab => new
                                {
                                    CoverHA = hab.CoverHA.ToString(),
                                    RelativeSurface = hab.RelativeSurface,
                                    PriorityForm = hab.PriorityForm
                                }).FirstOrDefault();
                            priorityH = (_habpriority == null) ? priorityH : ((_habpriority.Priority == 1 || (_habpriority.Priority == 2 && habDetails?.PriorityForm == true)) ? "*" : priorityH);

                            fields = (Dictionary<string, string>)new Dictionary<string, string>() { { "Priority", priorityH } }.Concat(fields).ToDictionary(x => x.Key, x => x.Value);
                        }

                        if (GetCodeName(changedItem) != String.Empty)
                        {
                            changeDetail =
                                new CodeChangeDetail
                                {
                                    Code = changedItem.Code,
                                    Name = GetCodeName(changedItem),
                                    ChangeId = changedItem.ChangeId,
                                    Fields = fields
                                };
                        }
                        else
                        {
                            changeDetail =
                                new CodeChangeDetail
                                {
                                    Code = "-",
                                    Name = changedItem.Code,
                                    ChangeId = changedItem.ChangeId,
                                    Fields = fields
                                };
                        }
                        catChange.ChangedCodesDetail.Add(changeDetail);
                    }
                    else if (changeCategory == "Species")
                    {
                        CodeChangeDetail changeDetail;
                        string? speciesName = GetCodeName(changedItem);
                        if (!String.IsNullOrEmpty(speciesName))
                        {
                            changeDetail = new CodeChangeDetail
                            {
                                Code = changedItem.Code,
                                Name = speciesName,
                                ChangeId = changedItem.ChangeId,
                                Fields = fields
                            };
                        }
                        else
                        {
                            changeDetail = new CodeChangeDetail
                            {
                                Code = "-",
                                Name = changedItem.Code,
                                ChangeId = changedItem.ChangeId,
                                Fields = fields
                            };
                        }

                        if (changeType == "Population Change"
                            || changeType == "Population Increase"
                            || changeType == "Population Decrease")
                        {
                            SpeciesPriority? sp = _dataContext.Set<SpeciesPriority>().Where(s => s.SpecieCode == changedItem.Code).FirstOrDefault();
                            string? priorityS = sp == null ? "-" : "*";
                            changeDetail.Fields = new Dictionary<string, string> { { "Priority", priorityS } }.Concat(changeDetail.Fields).ToDictionary(k => k.Key, v => v.Value);
                        }

                        catChange.ChangedCodesDetail.Add(changeDetail);
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

        private CategoryChangeDetail GetBioregionChangeDetail(string changeCategory, string changeType, List<SiteChangeDb> changeList)
        {
            try
            {
                CategoryChangeDetail catChange = new()
                {
                    ChangeType = changeType,
                    ChangeCategory = changeCategory
                };

                List<BioRegions> referenceBioregions = _dataContext.Set<BioRegions>().Where(bgr => bgr.SiteCode == changeList.FirstOrDefault().SiteCode && bgr.Version == changeList.FirstOrDefault().VersionReferenceId).ToList();
                List<BioRegions> submissionBioregions = _dataContext.Set<BioRegions>().Where(bgr => bgr.SiteCode == changeList.FirstOrDefault().SiteCode && bgr.Version == changeList.FirstOrDefault().Version).ToList();
                List<BioRegionTypes> bioregionTypes = _dataContext.Set<BioRegionTypes>().ToList();

                if (changeType == "Sites added due to a change of BGR")
                {
                    foreach (BioRegions changedItem in submissionBioregions)
                    {
                        var fields = new Dictionary<string, string>();
                        string nullCase = "-";
                        BioRegions reference = referenceBioregions.Where(r => r.BGRID == changedItem.BGRID).FirstOrDefault();
                        if (reference != null)
                        {
                            fields.Add("Reference", bioregionTypes.Where(t => t.Code == reference.BGRID).FirstOrDefault().RefBioGeoName + " " + ((reference.Percentage != null) ? reference.Percentage : "-") + "%");
                        }
                        else
                        {
                            fields.Add("Reference", nullCase);
                        }

                        if (changedItem != null)
                        {
                            fields.Add("Submission", bioregionTypes.Where(t => t.Code == changedItem.BGRID).FirstOrDefault().RefBioGeoName + " " + ((changedItem.Percentage != null) ? changedItem.Percentage : "-") + "%");
                        }
                        else
                        {
                            fields.Add("Submission", nullCase);
                        }

                        catChange.ChangedCodesDetail.Add(
                                    new CodeChangeDetail
                                    {
                                        ChangeId = changeList.FirstOrDefault().ChangeId,
                                        Fields = fields
                                    }
                                );
                    }
                }
                else if (changeType == "Sites deleted due to a change of BGR")
                {
                    foreach (BioRegions reference in referenceBioregions)
                    {
                        var fields = new Dictionary<string, string>();
                        string nullCase = "-";
                        BioRegions changedItem = submissionBioregions.Where(r => r.BGRID == reference.BGRID).FirstOrDefault();
                        if (reference != null)
                        {
                            fields.Add("Reference", bioregionTypes.Where(t => t.Code == reference.BGRID).FirstOrDefault().RefBioGeoName + " " + ((reference.Percentage != null) ? reference.Percentage : "-") + "%");
                        }
                        else
                        {
                            fields.Add("Reference", nullCase);
                        }

                        if (changedItem != null)
                        {
                            fields.Add("Submission", bioregionTypes.Where(t => t.Code == changedItem.BGRID).FirstOrDefault().RefBioGeoName + " " + ((changedItem.Percentage != null) ? changedItem.Percentage : "-") + "%");
                        }
                        else
                        {
                            fields.Add("Submission", nullCase);
                        }

                        catChange.ChangedCodesDetail.Add(
                                    new CodeChangeDetail
                                    {
                                        ChangeId = changeList.FirstOrDefault().ChangeId,
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

        private CodeChangeDetail? CodeAddedRemovedDetail(string section, string changeType, string? code, long changeId, string pSiteCode, int pCountryVersion, int versionReferenceId)
        {
            try
            {
                var fields = new Dictionary<string, string>();
                switch (section)
                {
                    case "Species":
                        string? specName = null;
                        string? annexII = "-";
                        string? priorityS = "-";
                        string? population = null;
                        string? popType = null;
                        string? specType = null;

                        if (code != null)
                        {
                            SpeciesTypes? _spectype = _speciesTypes.FirstOrDefault(s => s.Code.ToLower() == code.ToLower());
                            if (_spectype != null)
                            {
                                specName = _spectype.Name;
                                annexII = (_spectype.AnnexII == null) ? annexII : _spectype.AnnexII;
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
                                popType = _siteSpecies.FirstOrDefault(a => a.SiteCode == pSiteCode && a.SpecieCode == code && a.Version == pCountryVersion)?.PopulationType;
                            }
                        }

                        // don't add annexII and priority fields if change affects Other species
                        if (!changeType.Contains("Other"))
                        {
                            fields.Add("AnnexII", annexII);
                            fields.Add("Priority", priorityS);
                        }
                        fields.Add("Pop. Size", population);
                        fields.Add("Pop. Type", popType);
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

        public async Task<ModifiedSiteCode[]> BulkStatusCoverter(string sitecodes)
        {
            string[] sitecodeList = sitecodes.Split(',');
            sitecodeList = sitecodeList.Distinct().ToArray();
            List<SiteBasicBulk> queryResults = new();
            ModifiedSiteCode[] result = new ModifiedSiteCode[sitecodeList.Length];

            string queryString = @" 
                        SELECT DISTINCT [SiteCode],
	                        MAX([Changes].[Version]) AS 'Version',
	                        [N2KVersioningVersion]
                        FROM [dbo].[Changes]
                        INNER JOIN [dbo].[ProcessedEnvelopes] PE ON [Changes].[Country] = PE.[Country]
	                        AND [Changes].[N2KVersioningVersion] = PE.[Version]
	                        AND PE.[Status] = 3
                        GROUP BY [SiteCode],
	                        [N2KVersioningVersion]

                        UNION

                        SELECT DISTINCT [SiteCode],
	                        MAX([Sites].[Version]) AS 'Version',
	                        [N2KVersioningVersion]
                        FROM [dbo].[Sites]
                        INNER JOIN [dbo].[ProcessedEnvelopes] PE ON [Sites].[CountryCode] = PE.[Country]
	                        AND [Sites].[N2KVersioningVersion] = PE.[Version]
	                        AND PE.[Status] = 3
                        GROUP BY [SiteCode],
	                        [N2KVersioningVersion]
                        ORDER BY [SiteCode]";

            SqlConnection backboneConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
                backboneConn.Open();
                command = new SqlCommand(queryString, backboneConn);
                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    SiteBasicBulk mySiteView = new()
                    {
                        SiteCode = reader["SiteCode"].ToString(),
                        Version = int.Parse(reader["Version"].ToString()),
                        N2KVersioningVersion = int.Parse(reader["N2KVersioningVersion"].ToString())
                    };
                    queryResults.Add(mySiteView);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - BulkStatusCoverter", "", _dataContext.Database.GetConnectionString());
            }
            finally
            {
                if (reader != null) await reader.DisposeAsync();
                if (command != null) command.Dispose();
                if (backboneConn != null) backboneConn.Dispose();
            }

            for (int counter = 0; counter < sitecodeList.Length; counter++)
            {
                ModifiedSiteCode temp = new()
                {
                    SiteCode = sitecodeList[counter],
                    VersionId = queryResults.Where(w => w.SiteCode == sitecodeList[counter]).Select(s => s.Version).FirstOrDefault(),
                    Status = SiteChangeStatus.Pending,
                    OK = 1,
                    Error = string.Empty
                };
                result[counter] = temp;
            }
            return result;
        }

        public async Task<List<ModifiedSiteCode>> AcceptChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache)
        {
            List<SiteActivities> siteActivities = new();
            List<ModifiedSiteCode> result = new();
            try
            {
                DataTable sitecodesfilter = new("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));

                DataTable sitecodesfilter2 = new("sitecodesfilter");
                sitecodesfilter2.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter2.Columns.Add("Version", typeof(int));

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
                        SELECT Changes.SiteCode,
	                        Changes.Version,
	                        Sites.Name AS SiteName,
	                        Max(CASE 
			                        WHEN LEVEL = 'Critical'
				                        THEN 2
			                        WHEN LEVEL = 'Warning'
				                        THEN 1
			                        WHEN LEVEL = 'Info'
				                        THEN 0
			                        END) AS LEVEL,
	                        Sites.SiteType,
	                        Sites.JustificationRequired
                        FROM [dbo].[Changes]
                        INNER JOIN Sites ON changes.sitecode = sites.sitecode
	                        AND Changes.version = Sites.version
                        INNER JOIN @siteCodes T ON Changes.SiteCode = T.SiteCode
	                        AND Changes.Version = T.Version
                        WHERE Changes.Status = 'Pending'
                        GROUP BY changes.SiteCode,
	                        Changes.version,
	                        Sites.name,
	                        Sites.SiteType,
	                        Sites.JustificationRequired";

                SqlConnection backboneConn = null;
                SqlCommand command = null;
                SqlDataReader reader = null;
                try
                {
                    backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
                    backboneConn.Open();
                    command = new SqlCommand(queryString, backboneConn);
                    SqlParameter paramTable1 = new("@siteCodes", System.Data.SqlDbType.Structured)
                    {
                        Value = sitecodesfilter,
                        TypeName = "[dbo].[SiteCodeFilter]"
                    };
                    command.Parameters.Add(paramTable1);
                    reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        SiteCodeView mySiteView = new()
                        {
                            SiteCode = reader["SiteCode"].ToString(),
                            Version = int.Parse(reader["Version"].ToString()),
                            Name = reader["SiteName"].ToString(),
                            Type = reader["SiteType"].ToString(),
                            JustificationRequired = (bool)reader["JustificationRequired"]
                        };
                        mySiteView.CountryCode = mySiteView.SiteCode[..2];

                        mySiteView.LineageChangeType = _lineage.Where(l => l.SiteCode == mySiteView.SiteCode && l.Version == mySiteView.Version).First()?.Type
                            ?? LineageTypes.NoChanges;
                        Level level;
                        Enum.TryParse<Level>(reader["Level"].ToString(), out level);

                        //Alter cached listd. They come from pendign and goes to accepted
                        await swapSiteInListCache(cache, SiteChangeStatus.Accepted, level, SiteChangeStatus.Pending, mySiteView);

                        sitecodesfilter2.Rows.Add(new Object[] { mySiteView.SiteCode, mySiteView.Version });
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
                    SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                    {
                        Value = sitecodesfilter2,
                        TypeName = "[dbo].[SiteCodeFilter]"
                    };

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
                if (result.Count > 0 && sitecodesfilter2.Rows.Count > 0)
                {
                    string country = (result.First().SiteCode)[..2];
                    List<SiteChangeDb> site = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == result.First().SiteCode && site.Version == result.First().VersionId).ToListAsync();
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

        public async Task<List<ModifiedSiteCode>> AcceptChangesBulk(string sitecodes, IMemoryCache cache)
        {
            return await AcceptChanges(await BulkStatusCoverter(sitecodes), cache);
        }

        public async Task<List<ModifiedSiteCode>> RejectChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache)
        {
            List<SiteActivities> siteActivities = new();
            List<ModifiedSiteCode> result = new();
            try
            {
                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));

                DataTable sitecodesfilter2 = new("sitecodesfilter");
                sitecodesfilter2.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter2.Columns.Add("Version", typeof(int));

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
                        SELECT Changes.SiteCode,
	                        Changes.Version,
	                        Sites.Name AS SiteName,
	                        Max(CASE 
			                        WHEN LEVEL = 'Critical'
				                        THEN 2
			                        WHEN LEVEL = 'Warning'
				                        THEN 1
			                        WHEN LEVEL = 'Info'
				                        THEN 0
			                        END) AS LEVEL,
	                        Sites.SiteType,
	                        Sites.JustificationRequired
                        FROM [dbo].[Changes]
                        INNER JOIN Sites ON changes.sitecode = sites.sitecode
	                        AND Changes.version = Sites.version
                        INNER JOIN @siteCodes T ON Changes.SiteCode = T.SiteCode
	                        AND Changes.Version = T.Version
                        WHERE Changes.Status = 'Pending'
                        GROUP BY changes.SiteCode,
	                        Changes.version,
	                        Sites.name,
	                        Sites.SiteType,
	                        Sites.JustificationRequired";

                SqlConnection backboneConn = null;
                SqlCommand command = null;
                SqlDataReader reader = null;
                try
                {
                    backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
                    backboneConn.Open();
                    command = new SqlCommand(queryString, backboneConn);
                    SqlParameter paramTable1 = new("@siteCodes", System.Data.SqlDbType.Structured)
                    {
                        Value = sitecodesfilter,
                        TypeName = "[dbo].[SiteCodeFilter]"
                    };
                    command.Parameters.Add(paramTable1);
                    reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        SiteCodeView mySiteView = new()
                        {
                            SiteCode = reader["SiteCode"].ToString(),
                            Version = int.Parse(reader["Version"].ToString()),
                            Name = reader["SiteName"].ToString(),
                            Type = reader["SiteType"].ToString(),
                            JustificationRequired = (bool)reader["JustificationRequired"]
                        };
                        mySiteView.CountryCode = mySiteView.SiteCode[..2];

                        mySiteView.LineageChangeType = _lineage.Where(l => l.SiteCode == mySiteView.SiteCode && l.Version == mySiteView.Version).First()?.Type
                            ?? LineageTypes.NoChanges;

                        Level level;
                        Enum.TryParse<Level>(reader["Level"].ToString(), out level);
                        //Alter cached listd. They come from pendign and goes to rejected
                        await swapSiteInListCache(cache, SiteChangeStatus.Rejected, level, SiteChangeStatus.Pending, mySiteView);

                        sitecodesfilter2.Rows.Add(new Object[] { mySiteView.SiteCode, mySiteView.Version });
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
                    SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                    {
                        Value = sitecodesfilter2,
                        TypeName = "[dbo].[SiteCodeFilter]"
                    };

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
                if (result.Count > 0 && sitecodesfilter2.Rows.Count > 0)
                {
                    string country = (result.First().SiteCode)[..2];
                    List<SiteChangeDb> site = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == result.First().SiteCode && site.Version == result.First().VersionId).ToListAsync();
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

        public async Task<List<ModifiedSiteCode>> RejectChangesBulk(string sitecodes, IMemoryCache cache)
        {
            return await RejectChanges(await BulkStatusCoverter(sitecodes), cache);
        }

        private async Task<List<SiteActivities>> GetSiteActivities(DataTable sitecodesfilter)
        {
            List<SiteActivities> activities = new();
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
                SqlParameter paramTable1 = new("@siteCodes", System.Data.SqlDbType.Structured)
                {
                    Value = sitecodesfilter,
                    TypeName = "[dbo].[SiteCodeFilter]"
                };
                command.Parameters.Add(paramTable1);
                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    SiteActivities act = new()
                    {
                        SiteCode = reader["SiteCode"] is null ? null : reader["SiteCode"].ToString(),
                        Version = int.Parse(reader["Version"].ToString()),
                        Author = reader["Author"] is null ? null : reader["Author"].ToString(),
                        Date = DateTime.Parse(reader["Date"].ToString()),
                        Action = reader["Action"] is null ? null : reader["Action"].ToString(),
                        Deleted = bool.Parse(reader["Deleted"].ToString())
                    };
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

            List<SiteChangeDb> changes = new();
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
                SqlParameter paramTable1 = new("@siteCodes", System.Data.SqlDbType.Structured)
                {
                    Value = sitecodesfilter,
                    TypeName = "[dbo].[SiteCodeFilter]"
                };
                command.Parameters.Add(paramTable1);
                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    SiteChangeDb change = new()
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
            List<Sites> sites = new();
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
                SqlParameter paramTable1 = new("@siteCodes", System.Data.SqlDbType.Structured)
                {
                    Value = sitecodesfilter,
                    TypeName = "[dbo].[SiteCodeFilter]"
                };
                command.Parameters.Add(paramTable1);
                reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    Sites site = new()
                    {
                        SiteCode = reader["SiteCode"]?.ToString(),
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
                SqlConnection backboneConn = new(_dataContext.Database.GetConnectionString());
                var command = new SqlCommand(storedProcName, backboneConn) { CommandType = CommandType.StoredProcedure };
                SqlParameter paramTable1 = new("@siteCodes", System.Data.SqlDbType.Structured)
                {
                    Value = param,
                    TypeName = "[dbo].[SiteCodeFilter]"
                };
                command.Parameters.Add(paramTable1);
                DataSet result = new();
                SqlDataAdapter dataAdapter = new(command);
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

            List<SiteActivities> siteActivities = new();
            List<ModifiedSiteCode> result = new();
            try
            {
                DataTable sitecodesfilter = new("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));

                DataTable sitecodeschanges = new("sitecodeschanges");
                sitecodeschanges.Columns.Add("SiteCode", typeof(string));
                sitecodeschanges.Columns.Add("Version", typeof(int));

                DataTable sitecodesdelete = new("sitecodesdelete");
                sitecodesdelete.Columns.Add("SiteCode", typeof(string));
                sitecodesdelete.Columns.Add("Version", typeof(int));

                DataTable iddelete = new("iddelete");
                iddelete.Columns.Add("ID", typeof(long));

                DataTable JustificationFiles = new("JustificationFiles");
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
                List<SiteActivities> activitiesDB = new();
                foreach (DataRow row in siteActivitiesTable.Rows)
                {
                    SiteActivities act = new()
                    {
                        ID = long.Parse(row["ID"] is null ? null : row["ID"].ToString()),
                        SiteCode = row["SiteCode"] is null ? null : row["SiteCode"].ToString(),
                        Version = int.Parse(row["Version"].ToString()),
                        Author = row["Author"] is null ? null : row["Author"].ToString(),
                        Date = DateTime.Parse(row["Date"].ToString()),
                        Action = row["Action"] is null ? null : row["Action"].ToString(),
                        Deleted = bool.Parse(row["Deleted"].ToString())
                    };
                    activitiesDB.Add(act);
                }

                //GET SITES
                var sitesTable = dataSet?.Tables?[2];
                List<Sites> sitesDB = new();
                foreach (DataRow row in sitesTable.Rows)
                {
                    Sites site = new()
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
                List<SiteChangeDb> changesDB = new();
                foreach (DataRow row in siteChangesTable.Rows)
                {
                    SiteChangeDb change = new()
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
                    Sites? _site = sitesDB.Where(s => s.SiteCode == change.SiteCode && s.Version == change.Version).FirstOrDefault();
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
                        //Create the listView for the cached lists. By default this values
                        SiteCodeView mySiteView = new()
                        {
                            SiteCode = modifiedSiteCode.SiteCode,
                            Version = modifiedSiteCode.VersionId,
                            Name = changes.First().SiteName,
                            CountryCode = modifiedSiteCode.SiteCode[..2]
                        };
                        mySiteView.Type = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == modifiedSiteCode.VersionId).Select(x => x.SiteType).First().ToString();
                        mySiteView.JustificationRequired = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == modifiedSiteCode.VersionId).Select(x => x.JustificationRequired).First();
                        mySiteView.LineageChangeType = _lineage.Where(l => l.SiteCode == mySiteView.SiteCode && l.Version == mySiteView.Version && l.N2KVersioningVersion == changes.First().N2KVersioningVersion).First()?.Type
                            ?? LineageTypes.NoChanges;

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
                            if (temp1 != null)
                            {
                                //Select the max version for the site with the currentstatus accepted, but not the version of the change and the referenced version
                                previousCurrent = await _dataContext.Set<Sites>().Where(e => e.SiteCode == temp1.SiteCode && e.Version == temp1.Version && e.CurrentStatus == SiteChangeStatus.Accepted).Select(e => e.Version).FirstOrDefaultAsync();
                            }
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
                            mySiteView.Name = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == change.VersionReferenceId).Select(x => x.Name).First().ToString();
                            mySiteView.Type = sitesDB.Where(e => e.SiteCode == modifiedSiteCode.SiteCode && e.Version == change.VersionReferenceId).Select(x => x.SiteType).First().ToString();
                        }
                        #endregion

                        #region Was this site edited after being rejected? (Unused)
                        List<SiteActivities> activityCheck = activities.Where(e => e.Action == "User edition after rejection of version " + modifiedSiteCode.VersionId).ToList();
                        if (activityCheck != null && activityCheck.Count > 0)
                        {
                            SiteChangeDb siteDeleted = changes.Where(e => e.ChangeType == "Site Deleted").FirstOrDefault();
                            Sites previousSite = new();
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
                            if (previousCurrent != -1)
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
                    SqlParameter paramTable2 = new("@iddelete", System.Data.SqlDbType.Structured)
                    {
                        Value = iddelete,
                        TypeName = "[dbo].[IdDelete]"
                    };
                    await _dataContext.Database.ExecuteSqlRawAsync(
                        "exec spMarkActivitiesAsDeleted @iddelete",
                        paramTable2);

                    SqlParameter paramTable1 = new("@justificationFiles", System.Data.SqlDbType.Structured)
                    {
                        Value = JustificationFiles,
                        TypeName = "[dbo].[JustificationFilesAndStatusChanges]"
                    };
                    await _dataContext.Database.ExecuteSqlRawAsync(
                        "exec spCopyJustificationFilesAndStatusChangesBulk @justificationFiles",
                        paramTable1);
                    //Save activities changes
                    await _dataContext.SaveChangesAsync();

                    SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                    {
                        Value = sitecodesdelete,
                        TypeName = "[dbo].[SiteCodeFilter]"
                    };

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
                    var country = (result.First().SiteCode)[..2];
                    List<SiteChangeDb> site = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == result.First().SiteCode && site.Version == result.First().VersionId).ToListAsync();
                    level = (Level)site.Max(a => a.Level);
                    status = site.FirstOrDefault().Status;

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

        public async Task<List<ModifiedSiteCode>> MoveToPendingBulk(string sitecodes, IMemoryCache cache)
        {
            return await MoveToPending(await BulkStatusCoverter(sitecodes), cache);
        }

        public async Task<List<ModifiedSiteCode>> MarkAsJustificationRequired(JustificationModel[] justification, IMemoryCache cache)
        {
            List<ModifiedSiteCode> result = new();
            try
            {
                foreach (var just in justification)
                {
                    ModifiedSiteCode modifiedSiteCode = new();
                    try
                    {
                        SqlParameter paramSiteCode = new("@sitecode", just.SiteCode);
                        SqlParameter paramVersionId = new("@version", just.VersionId);
                        SqlParameter justificationMarked = new("@justififcation", just.Justification);

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

                        //Change site in cache
                        List<SiteChangeDb> sites = await _dataContext.Set<SiteChangeDb>().AsNoTracking().Where(site => site.SiteCode == just.SiteCode && site.Version == just.VersionId).ToListAsync();
                        Level level = (Level)sites.Max(a => a.Level);

                        List<SiteCodeView> cachedlist = new();
                        string listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", just.SiteCode[..2], sites.FirstOrDefault().Status, level.ToString());
                        if (cache.TryGetValue(listName, out cachedlist))
                        {
                            SiteCodeView element = cachedlist.Where(cl => cl.SiteCode == just.SiteCode).FirstOrDefault();
                            if (element != null)
                            {
                                element.JustificationRequired = just.Justification;
                            }
                        }
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
            List<ModifiedSiteCode> result = new();
            try
            {
                foreach (var just in justification)
                {
                    ModifiedSiteCode modifiedSiteCode = new();
                    try
                    {
                        SqlParameter paramSiteCode = new("@sitecode", just.SiteCode);
                        SqlParameter paramVersionId = new("@version", just.VersionId);
                        SqlParameter justificationProvided = new("@justififcation", just.Justification);

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
            List<SiteCodeView> cachedlist = new();

            //Site comes from this list
            string listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", pSite.SiteCode[..2], pListNameFrom.ToString(), pLevel.ToString());
            if (pCache.TryGetValue(listName, out cachedlist))
            {
                SiteCodeView element = cachedlist.Where(cl => cl.SiteCode == pSite.SiteCode).FirstOrDefault();
                if (element != null)
                {
                    cachedlist.Remove(element);
                }
            }

            //Site goes to that list
            listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", pSite.SiteCode[..2], pStatus.ToString(), pLevel.ToString());
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

        public async Task<List<SiteCodeVersion>> GetNoChanges(string country, IMemoryCache cache, int page = 1, int pageLimit = 0, bool refresh = false)
        {
            try
            {
                string listNameNoChanges = string.Format("{0}_{1}", "listcodes", country);

                //if there has been a change in the status refresh the changed sitecodes cache
                if (refresh) cache.Remove(listNameNoChanges);

                List<SiteCodeVersion> result = new();
                List<SiteCodeVersion> sitelist = new();
                if (cache.TryGetValue(listNameNoChanges, out List<SiteCodeVersion> cachedList))
                {
                    sitelist = cachedList;
                }
                else
                {
                    SqlParameter param1 = new("@country", country);
                    IQueryable<SiteCodeVersion> sites = _dataContext.Set<SiteCodeVersion>().FromSqlRaw($"exec dbo.[spGetSitesWithNoChanges]  @country",
                                param1);
                    sitelist = await sites.ToListAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(2500))
                            .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(40000);
                    cache.Set(listNameNoChanges, sitelist, cacheEntryOptions);
                }

                var startRow = (page - 1) * pageLimit;
                if (pageLimit > 0)
                {
                    sitelist = sitelist.OrderBy(s => s.SiteCode)
                        .Skip(startRow)
                        .Take(pageLimit)
                        .ToList();
                }

                foreach (var site in sitelist)
                {
                    SiteCodeVersion temp = new()
                    {
                        SiteCode = site.SiteCode,
                        Version = site.Version,
                        Name = site.Name,
                        Type = site.Type
                    };
                    result.Add(temp);
                }
                return result.OrderBy(o => o.SiteCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetNoChanges", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<int> GetPendingVersion(string siteCode)
        {
            try
            {
                Sites result = await _dataContext.Set<Sites>().AsNoTracking().Where(site => site.SiteCode == siteCode && site.CurrentStatus == SiteChangeStatus.Pending).FirstOrDefaultAsync();
                return result != null ? result.Version : -1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangesService - GetPendingVersion", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}
