using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Services.HarvestingProcess;
using N2K_BackboneBackEnd.Enumerations;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

using System.Data;
using N2K_BackboneBackEnd.Helpers;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using N2K_BackboneBackEnd.Hubs;

namespace N2K_BackboneBackEnd.Services
{
    public class HarvestedService : IHarvestedService
    {
        private N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private bool _ThereAreChanges = false;
        private IBackgroundSpatialHarvestJobs _fmeHarvestJobs;

        private IList<SpeciesTypes> _speciesTypes = new List<SpeciesTypes>();
        private IList<DataQualityTypes> _dataQualityTypes = new List<DataQualityTypes>();
        private IList<Models.backbone_db.OwnerShipTypes> _ownerShipTypes = new List<Models.backbone_db.OwnerShipTypes>();
        private IList<Models.backbone_db.SpecieBase> _countrySpecies = new List<Models.backbone_db.SpecieBase>();

        private readonly IHubContext<ChatHub> _hubContext;
        //private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        //private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(initialCount:1);

        private IDictionary<Type, object> _siteItems = new Dictionary<Type, object>(); private struct SiteVersion
        {
            public string SiteCode; 
            public int MaxVersion;
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="dataContext">Context for the BackBone database</param>
        /// <param name="versioningContext">Context for the Versioning database</param>
        /// <param name="app">Configuration options</param>
        /// <param name="harvestJobs">Queue of FME harvest spatial processes </param>
        public HarvestedService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext, IHubContext<ChatHub> hubContext, IOptions<ConfigSettings> app, IBackgroundSpatialHarvestJobs harvestJobs)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
            _appSettings = app;
            InitialiseBulkItems();
            _fmeHarvestJobs = harvestJobs;
            _hubContext = hubContext;
        }

        /// <summary>
        /// To define
        /// </summary>
        /// <returns></returns>
        public async Task<List<Harvesting>> GetHarvestedAsync()
        {
            return await Task.FromResult(new List<Harvesting>());
        }

        /// <summary>
        /// To define
        /// </summary>
        /// <returns></returns>
        public List<Harvesting> GetHarvested()
        {
            List<Harvesting> a = new();
            return a;
        }

        private String GetLineageOPType(LineageTypes type)
        {
            return Enum.GetName(type);
        }

        private CountryVersionToStatus GetCountryVersionToStatusFromSingleEnvelope(string countryCode, int version, HarvestingStatus status)
        {
            return new CountryVersionToStatus
            {
                countryVersion = new CountryVersion[]
                {
                    new() {
                        CountryCode = countryCode,
                        VersionId = version
                    }
                },
                toStatus = status
            };
        }

        private async Task<RepPeriod?> GetActiveRepPeriodAsync()
        {
            return await _dataContext.Set<RepPeriod>().Where(a => a.Active).FirstOrDefaultAsync();
        }

        private Boolean IsInRepPeriodRange(DateTime? date, RepPeriod? period)
        {
            if (date == null || period == null)
                return false;
            return period.InitDate <= date && period.EndDate >= date;
        }

        private void InitialiseBulkItems()
        {
            _siteItems.Add(typeof(List<Respondents>), new List<Respondents>());
            _siteItems.Add(typeof(List<BioRegions>), new List<BioRegions>());
            _siteItems.Add(typeof(List<NutsBySite>), new List<NutsBySite>());
            _siteItems.Add(typeof(List<N2K_BackboneBackEnd.Models.backbone_db.IsImpactedBy>), new List<N2K_BackboneBackEnd.Models.backbone_db.IsImpactedBy>());
            _siteItems.Add(typeof(List<N2K_BackboneBackEnd.Models.backbone_db.HasNationalProtection>), new List<N2K_BackboneBackEnd.Models.backbone_db.HasNationalProtection>());
            _siteItems.Add(typeof(List<N2K_BackboneBackEnd.Models.backbone_db.DetailedProtectionStatus>), new List<N2K_BackboneBackEnd.Models.backbone_db.DetailedProtectionStatus>());
            _siteItems.Add(typeof(List<SiteLargeDescriptions>), new List<SiteLargeDescriptions>());
            _siteItems.Add(typeof(List<SiteOwnerType>), new List<SiteOwnerType>());
            _siteItems.Add(typeof(List<N2K_BackboneBackEnd.Models.backbone_db.Ownership>), new List<N2K_BackboneBackEnd.Models.backbone_db.Ownership>());
            _siteItems.Add(typeof(List<Habitats>), new List<Habitats>());
            _siteItems.Add(typeof(List<DescribeSites>), new List<DescribeSites>());
            _siteItems.Add(typeof(List<SpeciesOther>), new List<SpeciesOther>());
            _siteItems.Add(typeof(List<Species>), new List<Species>());
        }

        private void ClearBulkItems()
        {
            _siteItems[typeof(List<Respondents>)] = new List<Respondents>();
            _siteItems[typeof(List<BioRegions>)] = new List<BioRegions>();
            _siteItems[typeof(List<NutsBySite>)] = new List<NutsBySite>();
            _siteItems[typeof(List<N2K_BackboneBackEnd.Models.backbone_db.IsImpactedBy>)] = new List<N2K_BackboneBackEnd.Models.backbone_db.IsImpactedBy>();
            _siteItems[typeof(List<N2K_BackboneBackEnd.Models.backbone_db.HasNationalProtection>)] = new List<N2K_BackboneBackEnd.Models.backbone_db.HasNationalProtection>();
            _siteItems[typeof(List<N2K_BackboneBackEnd.Models.backbone_db.DetailedProtectionStatus>)] = new List<N2K_BackboneBackEnd.Models.backbone_db.DetailedProtectionStatus>();
            _siteItems[typeof(List<SiteLargeDescriptions>)] = new List<SiteLargeDescriptions>();
            _siteItems[typeof(List<SiteOwnerType>)] = new List<SiteOwnerType>();
            _siteItems[typeof(List<Habitats>)] = new List<Habitats>();
            _siteItems[typeof(List<DescribeSites>)] = new List<DescribeSites>();
            _siteItems[typeof(List<SpeciesOther>)] = new List<SpeciesOther>();
            _siteItems[typeof(List<Species>)] = new List<Species>();
        }

        private async Task<int> SaveBulkItems(DateTime startTime)
        {
            string db = _dataContext.Database.GetConnectionString();
            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Start saving sites in bulk mode  {0}", (DateTime.Now - startTime).TotalSeconds), "HarvestedService - SaveBulkItems", "", db);

            try
            {
                try
                {
                    List<Respondents> _listed = (List<Respondents>)_siteItems[typeof(List<Respondents>)];
                    await Respondents.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - Respondents.SaveBulkRecord", "", db);
                }
                try
                {
                    List<BioRegions> _listed = (List<BioRegions>)_siteItems[typeof(List<BioRegions>)];
                    await BioRegions.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - BioRegions.SaveBulkRecord", "", db);
                }
                try
                {
                    List<NutsBySite> _listed = (List<NutsBySite>)_siteItems[typeof(List<NutsBySite>)];
                    await NutsBySite.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - NutsBySite.SaveBulkRecord", "", db);
                }
                try
                {
                    List<N2K_BackboneBackEnd.Models.backbone_db.IsImpactedBy> _listed = (List<N2K_BackboneBackEnd.Models.backbone_db.IsImpactedBy>)_siteItems[typeof(List<N2K_BackboneBackEnd.Models.backbone_db.IsImpactedBy>)];
                    await N2K_BackboneBackEnd.Models.backbone_db.IsImpactedBy.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - IsImpactedBy.SaveBulkRecord", "", db);
                }
                try
                {
                    List<N2K_BackboneBackEnd.Models.backbone_db.HasNationalProtection> _listed = (List<N2K_BackboneBackEnd.Models.backbone_db.HasNationalProtection>)_siteItems[typeof(List<N2K_BackboneBackEnd.Models.backbone_db.HasNationalProtection>)];
                    await N2K_BackboneBackEnd.Models.backbone_db.HasNationalProtection.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - HasNationalProtection.SaveBulkRecord", "", db);
                }
                try
                {
                    List<N2K_BackboneBackEnd.Models.backbone_db.DetailedProtectionStatus> _listed = (List<N2K_BackboneBackEnd.Models.backbone_db.DetailedProtectionStatus>)_siteItems[typeof(List<N2K_BackboneBackEnd.Models.backbone_db.DetailedProtectionStatus>)];
                    await N2K_BackboneBackEnd.Models.backbone_db.DetailedProtectionStatus.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - DetailedProtectionStatus.SaveBulkRecord", "", db);
                }
                try
                {
                    List<N2K_BackboneBackEnd.Models.backbone_db.Ownership> _listed = (List<N2K_BackboneBackEnd.Models.backbone_db.Ownership>)_siteItems[typeof(List<N2K_BackboneBackEnd.Models.backbone_db.Ownership>)];
                    await N2K_BackboneBackEnd.Models.backbone_db.Ownership.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - Ownership.SaveBulkRecord", "", db);
                }
                try
                {
                    List<SiteLargeDescriptions> _listed = (List<SiteLargeDescriptions>)_siteItems[typeof(List<SiteLargeDescriptions>)];
                    await SiteLargeDescriptions.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - SiteLargeDescriptions.SaveBulkRecord", "", db);
                }
                try
                {
                    List<SiteOwnerType> _listed = (List<SiteOwnerType>)_siteItems[typeof(List<SiteOwnerType>)];
                    await SiteOwnerType.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - SiteOwnerType.SaveBulkRecord", "", db);
                }
                try
                {
                    List<Habitats> _listed = (List<Habitats>)_siteItems[typeof(List<Habitats>)];
                    await Habitats.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - Habitats.SaveBulkRecord", "", db);
                }
                try
                {
                    List<DescribeSites> _listed = (List<DescribeSites>)_siteItems[typeof(List<DescribeSites>)];
                    await DescribeSites.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - DescribeSites.SaveBulkRecord", "", db);
                }
                try
                {
                    List<SpeciesOther> _listed = (List<SpeciesOther>)_siteItems[typeof(List<SpeciesOther>)];
                    await SpeciesOther.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - SpeciesOther.SaveBulkRecord", "", db);
                }
                try
                {
                    List<Species> _listed = (List<Species>)_siteItems[typeof(List<Species>)];
                    await Species.SaveBulkRecord(db, _listed);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - Species.SaveBulkRecord", "", db);
                }
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("End saving sites in bulk mode  {0}", (DateTime.Now - startTime).TotalSeconds), "HarvestedService - SaveBulkItems", "", db);
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - SaveBulkItems", "", db);
                return 0;
            }
            finally
            {
                ClearBulkItems();
            }
        }

        /// <summary>
        /// To define
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Harvesting> GetHarvestedAsyncById(int id)
        {
            return await Task.FromResult(new Harvesting
            {
                Id = id,
                Country = "ES",
                Status = Enumerations.HarvestingStatus.Pending,
                SubmissionDate = DateTime.Today
            });

        }

        public async Task<List<HarvestingExpanded>> GetEnvelopesByStatus(HarvestingStatus status)
        {
            try
            {
                SqlParameter param1 = new("@status", (int)status);

                List<HarvestingExpanded> result = await _dataContext.Set<HarvestingExpanded>().FromSqlRaw($"exec dbo.spGetEnvelopesByStatus  @status",
                                param1).AsNoTracking().ToListAsync();

                List<ProcessedEnvelopes> envelopes = await _dataContext.Set<ProcessedEnvelopes>().Where(e => e.Status == HarvestingStatus.DataLoaded).AsNoTracking().ToListAsync();

                result.ForEach(r =>
                {
                    r.DataLoaded = envelopes.Where(e => e.Country == r.Country).Count();
                });

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - GetEnvelopesByStatus - " + status, "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<HarvestingExpanded>> GetOnlyClosedEnvelopes()
        {
            try
            {
                List<HarvestingExpanded> result = await _dataContext.Set<HarvestingExpanded>().FromSqlRaw($"exec dbo.spGetOnlyClosedEnvelopes").AsNoTracking().ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - GetOnlyClosedEnvelopes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<EnvelopesToHarvest>> GetPreHarvestedEnvelopes()
        {
            try
            {
                IQueryable<EnvelopesToHarvest> changes = _dataContext.Set<EnvelopesToHarvest>().FromSqlRaw($"exec dbo.spGetPreHarvestedEnvelopes");
                return await changes.ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - GetPreHarvestedEnvelopes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<Harvesting>> GetPendingEnvelopes()
        {
            try
            {
                List<Harvesting> result = new();
                List<Countries> countries = await _dataContext.Set<Countries>().ToListAsync();
                List<ProcessedEnvelopes> processed = await _dataContext.Set<ProcessedEnvelopes>().FromSqlRaw($"select * from dbo.[vHighVersionProcessedEnvelopes]").AsNoTracking().ToListAsync();
                List<ProcessedEnvelopes> allEnvs = await _dataContext.Set<ProcessedEnvelopes>().AsNoTracking().ToListAsync();
                foreach (ProcessedEnvelopes procCountry in processed)
                {
                    SqlParameter param1 = new("@country", procCountry.Country);
                    SqlParameter param2 = new("@version", procCountry.Version);
                    SqlParameter param3 = new("@importdate", procCountry.ImportDate);

                    List<Harvesting> list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.spGetPendingCountryVersion  @country, @version,@importdate",
                                    param1, param2, param3).AsNoTracking().ToListAsync();

                    RepPeriod? dateRange = await GetActiveRepPeriodAsync();

                    if (list.Count > 0)
                    {
                        foreach (Harvesting pendEnv in list)
                        {
                            if (!result.Contains(pendEnv) && IsInRepPeriodRange(pendEnv.SubmissionDate, dateRange))
                            {
                                if (allEnvs.Where(e => e.Version == pendEnv.Id && e.Country == pendEnv.Country && e.Status == HarvestingStatus.Harvesting).ToList().Count == 0)
                                {
                                    result.Add(
                                        new Harvesting
                                        {
                                            Country = pendEnv.Country, //countries.Where(ct => ct.Code.ToLower() == pendEnv.Country.ToLower()).FirstOrDefault().Country,
                                            Name = pendEnv.Name,
                                            Status = pendEnv.Status,
                                            Id = pendEnv.Id,
                                            SubmissionDate = pendEnv.SubmissionDate
                                        }
                                     );
                                }
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - GetPendingEnvelopes", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        /// <summary>
        /// Method to return Pending status when status is Harvested
        /// </summary>
        /// <param name="envelopeStatus">Status from the processed envelope</param>
        /// <returns>The SiteChange status based on the envelope status</returns>
        public async Task<HarvestingStatus> GetSiteChangeStatus(HarvestingStatus envelopeStatus, N2KBackboneContext? ctx = null)
        {
            try
            {
                await Task.Delay(1);
                return envelopeStatus == HarvestingStatus.Harvested ? HarvestingStatus.Pending : envelopeStatus;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - GetSiteChangeStatus", "", ctx.Database.GetConnectionString());
                throw ex;
            }
        }

        /// <summary>
        /// Method to validate the quality and the main rules of the data harvested
        /// </summary>
        /// <param name="envelopeIDs">List of the envelops to process</param>
        /// <returns>A list of the envelops with the result of the process</returns>
        public async Task<List<HarvestedEnvelope>> ChangeDetection(EnvelopesToProcess[] envelopeIDs, N2KBackboneContext? ctx = null)
        {
            try
            {
                //check if ChangeDetection has been called from the specific end point 
                //this will determine the "DELETE FROM dbo.Changes" sentences in the end of the execution
                bool from_end_point = false;
                if (ctx == null)
                {
                    from_end_point = true;
                    ctx = this._dataContext;
                }

                List<HarvestedEnvelope> result = new();
                List<SiteChangeDb> changes = new();
                //List<ProcessedEnvelopes> latestVersions = await ctx.Set<ProcessedEnvelopes>().ToListAsync();
                //await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.Changes");

                //Get the lists of priority habitats and species
                List<HabitatPriority> habitatPriority = await ctx.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
                List<SpeciesPriority> speciesPriority = await ctx.Set<SpeciesPriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

                //from the view vLatest//processedEnvelopes (backbonedb) load the sites with the latest versionid of the countries

                //Load all sites with the CountryVersionID-CountryCode from Versioning
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    try
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Start ChangeDetection {0} - {1}", envelope.CountryCode, envelope.VersionId), "ChangeDetection", "", ctx.Database.GetConnectionString());

                        SqlParameter param1 = new("@country", envelope.CountryCode);
                        SqlParameter param2 = new("@version", envelope.VersionId);
                        SqlParameter param3 = new("@last_envelop", envelope.VersionId);

                        //Get the changes status from ProcessedEnvelopes
                        List<ProcessedEnvelopes> processedEnvelopes = await ctx.Set<ProcessedEnvelopes>().FromSqlRaw($"exec dbo.spGetProcessedEnvelopesByCountryAndVersion  @country, @version",
                                        param1, param2).ToListAsync();
                        ProcessedEnvelopes? processedEnvelope = processedEnvelopes.FirstOrDefault();

                        List<RelatedSites>? sitesRelation = await ctx.Set<RelatedSites>().FromSqlRaw($"exec dbo.spGetSitesToDetectChanges  @last_envelop, @country",
                                        param3, param1).ToListAsync();
                        DataTable previoussitecodesfilter = new("sitecodesfilter");
                        previoussitecodesfilter.Columns.Add("SiteCode", typeof(string));
                        previoussitecodesfilter.Columns.Add("Version", typeof(int));
                        DataTable newsitecodesfilter = new("sitecodesfilter");
                        newsitecodesfilter.Columns.Add("SiteCode", typeof(string));
                        newsitecodesfilter.Columns.Add("Version", typeof(int));
                        foreach (RelatedSites sc in sitesRelation)
                        {
                            if (sc.PreviousSiteCode != null && sc.PreviousVersion != null)
                                previoussitecodesfilter.Rows.Add(new Object[] { sc.PreviousSiteCode, sc.PreviousVersion });
                            if (sc.NewSiteCode != null && sc.NewVersion != null)
                                newsitecodesfilter.Rows.Add(new Object[] { sc.NewSiteCode, sc.NewVersion });
                        }

                        SqlParameter paramDetection1 = new("@reported_envelop", envelope.VersionId);
                        SqlParameter paramDetection2 = new("@country", envelope.CountryCode);
                        SqlParameter paramDetection3 = new("@tol", 5);

                        List<LineageDetection>? detectedLineageChanges = await ctx.Set<LineageDetection>().FromSqlRaw($"exec dbo.spGetSitesToDetectChangesWithLineage  @reported_envelop, @country, @tol",
                                        paramDetection1, paramDetection2, paramDetection3).ToListAsync();

                        DataTable lineageInsertion = new("LineageInsertion");
                        lineageInsertion.Columns.Add("SiteCode", typeof(string));
                        lineageInsertion.Columns.Add("Version", typeof(int));
                        lineageInsertion.Columns.Add("N2KVersioningVersion", typeof(int));
                        lineageInsertion.Columns.Add("Type", typeof(int));
                        lineageInsertion.Columns.Add("Status", typeof(int));
                        lineageInsertion.Columns.Add("AntecessorSiteCode", typeof(string));
                        lineageInsertion.Columns.Add("AntecessorVersion", typeof(int));
                        detectedLineageChanges.ForEach(c =>
                        {
                            LineageTypes type = LineageTypes.NoChanges;
                            if (c.op == "ADDED")
                            {
                                type = LineageTypes.Creation;
                            }
                            else if (c.op == "DELETED")
                            {
                                type = LineageTypes.Deletion;
                            }
                            else if (c.op == "SPLIT")
                            {
                                type = LineageTypes.Split;
                            }
                            else if (c.op == "MERGE")
                            {
                                type = LineageTypes.Merge;
                            }
                            else if (c.op == "RECODING")
                            {
                                type = LineageTypes.Recode;
                            }
                            if (c.op == "DELETED")
                            {
                                lineageInsertion.Rows.Add(new Object[] { c.old_sitecode, c.old_version, envelope.VersionId, type, LineageStatus.Proposed, c.old_sitecode, c.old_version });
                            }
                            else
                            {
                                lineageInsertion.Rows.Add(new Object[] { c.new_sitecode, c.new_version, envelope.VersionId, type, LineageStatus.Proposed, c.old_sitecode, c.old_version });
                            }
                        });
                        sitesRelation.ForEach(r =>
                        {
                            LineageDetection temp = detectedLineageChanges.Where(c => c.new_sitecode == r.NewSiteCode || (c.old_sitecode == r.NewSiteCode && c.op == "DELETED")).FirstOrDefault();
                            if (r.NewSiteCode == r.PreviousSiteCode && temp == null)
                            {
                                lineageInsertion.Rows.Add(new Object[] { r.NewSiteCode, r.NewVersion, envelope.VersionId, LineageTypes.NoChanges, LineageStatus.Proposed, r.PreviousSiteCode, r.PreviousVersion });
                            }
                        });

                        SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                        {
                            Value = lineageInsertion,
                            TypeName = "[dbo].[LineageInsertion]"
                        };
                        await _dataContext.Database.ExecuteSqlRawAsync($"exec dbo.spInsertIntoLineageBulk  @siteCodes", paramTable);

                        //get the information of the sites in submission and reported to compare them
                        //we do this to improve the performance: load all in memory first
                        SqlParameter param4 = new("@siteCodes", System.Data.SqlDbType.Structured)
                        {
                            Value = previoussitecodesfilter,
                            TypeName = "[dbo].[SiteCodeFilter]"
                        };
                        List<SiteToHarvest>? previoussites = await ctx.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetSitesBySiteCodeFilter  @siteCodes",
                                        param4).ToListAsync();

                        #region Load values in memory
                        //get the habitats of all the sites in versioning 
                        List<HabitatsToHarvestPerEnvelope> habitatsReferenceEnvelope = await ctx.Set<HabitatsToHarvestPerEnvelope>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodes @siteCodes",
                                        param4).ToListAsync();
                        //get the species of all the sites in reference 
                        List<SpeciesToHarvestPerEnvelope>? speciesReferenceEnvelope = await ctx.Set<SpeciesToHarvestPerEnvelope>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodes @siteCodes",
                                        param4).ToListAsync();
                        //get the species other of all the sites in reference
                        List<SpeciesToHarvestPerEnvelope>? speciesOtherReferenceEnvelope = await ctx.Set<SpeciesToHarvestPerEnvelope>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesOtherBySiteCodes @siteCodes ",
                                        param4).ToListAsync();
                        //get the bioregions of all the sites in reference
                        List<BioRegions> bioRegionsRefereceEnvelope = await ctx.Set<BioRegions>().FromSqlRaw($"exec dbo.spGetReferenceBioRegionsBySiteCodes  @siteCodes",
                                        param4).ToListAsync();

                        //Submission data (versioning)
                        param4.Value = newsitecodesfilter;
                        List<SiteToHarvest>? newsites = await ctx.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetSitesBySiteCodeFilter  @siteCodes",
                                        param4).ToListAsync();
                        //get the habitats of all the sites in submission 
                        List<HabitatsToHarvestPerEnvelope> habitatsVersioningEnvelope = await ctx.Set<HabitatsToHarvestPerEnvelope>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodes @siteCodes",
                                        param4).ToListAsync();
                        //get the species of all the sites in submission
                        List<SpeciesToHarvestPerEnvelope>? speciesVersioningEnvelope = await ctx.Set<SpeciesToHarvestPerEnvelope>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodes @siteCodes ",
                                        param4).ToListAsync();
                        //get the species other of all the sites in submission
                        List<SpeciesToHarvestPerEnvelope>? speciesOtherVersioningEnvelope = await ctx.Set<SpeciesToHarvestPerEnvelope>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesOtherBySiteCodes @siteCodes ",
                                        param4).ToListAsync();
                        //get the bioregions of all the sites in submission
                        List<BioRegions> bioRegionsVersioningEnvelope = await ctx.Set<BioRegions>().FromSqlRaw($"exec dbo.spGetReferenceBioRegionsBySiteCodes  @siteCodes",
                                        param4).ToListAsync();
                        #endregion

                        List<SiteToHarvest> newsitestest = newsites; // .Take(numsites_test).Where(s=> s.SiteCode== "SE0110389").ToList();
                        //For each site in Versioning compare it with that site in backboneDB
                        //Parallel change detection (10 parallel threads)
                        //Create a ConcurrentBag to avoid sync errors with shared variables
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("START parallel change detection {0}-{1}", envelope.CountryCode, envelope.VersionId), "Sites tabular change detection", "", ctx.Database.GetConnectionString());

                        ConcurrentBag<List<SiteChangeDb>> concurrentSitesChanges = new();

                        await newsites.AsyncParallelForEach(
                            async harvestingSite =>
                            {
                                concurrentSitesChanges.Add(await ParallelSiteChangeDetection(detectedLineageChanges, previoussites, harvestingSite,
                                    envelope, habitatPriority, speciesPriority,
                                    processedEnvelope, sitesRelation, false, ctx,
                                    habitatsVersioningEnvelope, habitatsReferenceEnvelope,
                                    speciesVersioningEnvelope, speciesReferenceEnvelope,
                                    speciesOtherVersioningEnvelope, speciesOtherReferenceEnvelope,
                                    bioRegionsVersioningEnvelope, bioRegionsRefereceEnvelope
                                ));
                            }, 10
                        );
                        //create changes list from ConcurrentBag items (concurrentSitesChanges)
                        foreach (var item in concurrentSitesChanges)
                        {
                            changes.AddRange(item.ToList<SiteChangeDb>());
                        }
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("END parallel change detection {0}-{1}", envelope.CountryCode, envelope.VersionId), "Sites tabular change detection", "", ctx.Database.GetConnectionString());

                        /*
                        var ss_par = changes.Select(c=> c.SiteCode).DistinctBy(c => c).ToList();

                        var tt = 43654;
                        var i = 0;
                        //Sequential change detection
                        changes = new List<SiteChangeDb>();                        
                        foreach (SiteToHarvest? harvestingSite in newsites)
                        {
                            changes = await SiteChangeDetection(changes, detectedLineageChanges, previoussites, harvestingSite,
                                envelope, habitatPriority, speciesPriority,
                                processedEnvelope, sitesRelation, false, ctx,
                                habitatsVersioningEnvelope, habitatsReferenceEnvelope,
                                speciesVersioningEnvelope, speciesReferenceEnvelope,
                                speciesOtherVersioningEnvelope,speciesOtherReferenceEnvelope,
                                bioRegionsVersioningEnvelope, bioRegionsRefereceEnvelope                                
                                );

                            //if (i > 300) break;
                            //if (i % 5000 ==0 )
                            //    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Change detection {0} - {1}:{2}", envelope.CountryCode, envelope.VersionId,i.ToString()), "ChangeDetection", "", ctx.Database.GetConnectionString());
                            i = i + 1;
                            //List<SiteChangeDb> a = changes.Where(c => c.LineageChangeType != null).ToList();
                        }
                        var ss_seq = changes.Select(c => c.SiteCode).DistinctBy(c => c).ToList();
                        var dif = ss_seq.Except(ss_seq.Where(o => ss_par.Select(s => s).ToList().Contains(o))).ToList();
                        */

                        //clean memory lists
                        habitatsVersioningEnvelope.Clear();
                        habitatsReferenceEnvelope.Clear();
                        speciesVersioningEnvelope.Clear();
                        speciesReferenceEnvelope.Clear();
                        speciesOtherVersioningEnvelope.Clear();
                        speciesOtherReferenceEnvelope.Clear();
                        bioRegionsVersioningEnvelope.Clear();
                        bioRegionsRefereceEnvelope.Clear();

                        //For each site in backboneDB check if the site still exists in Versioning
                        foreach (SiteToHarvest? storedSite in previoussites)
                        {
                            RelatedSites? siteRelation = sitesRelation.Where(s => s.PreviousSiteCode == storedSite.SiteCode && s.PreviousVersion == storedSite.VersionId).FirstOrDefault();
                            SiteToHarvest? harvestingSite = null;
                            if (siteRelation != null)
                                harvestingSite = newsites.Where(s => s.SiteCode == siteRelation.NewSiteCode && s.VersionId == siteRelation.NewVersion).FirstOrDefault();
                            if (siteRelation != null && harvestingSite == null)
                            {
                                Lineage? lineage = await ctx.Set<Lineage>().FirstOrDefaultAsync(l => l.SiteCode == storedSite.SiteCode && l.Version == storedSite.VersionId && l.N2KVersioningVersion == envelope.VersionId);
                                if (lineage?.Type == LineageTypes.Deletion) //the site is not been recoded, split or merge
                                {
                                    SiteChangeDb siteChange = new()
                                    {
                                        SiteCode = storedSite.SiteCode,
                                        Version = storedSite.VersionId,
                                        ChangeCategory = "Lineage",
                                        ChangeType = "Site Deleted",
                                        Country = envelope.CountryCode,
                                        Level = Enumerations.Level.Critical,
                                        Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status, ctx),
                                        Tags = string.Empty,
                                        NewValue = null,
                                        OldValue = storedSite.SiteCode,
                                        Code = storedSite.SiteCode,
                                        Section = "Site",
                                        VersionReferenceId = storedSite.VersionId,
                                        ReferenceSiteCode = storedSite.SiteCode,
                                        N2KVersioningVersion = envelope.VersionId
                                    };
                                    changes.Add(siteChange);
                                }
                            }
                        }

                        result.Add(new HarvestedEnvelope
                        {
                            CountryCode = envelope.CountryCode,
                            VersionId = envelope.VersionId,
                            NumChanges = changes.Count,
                            Status = HarvestingStatus.PreHarvested
                        });

                        try
                        {
                            await SiteChangeDb.SaveBulkRecord(ctx.Database.GetConnectionString(), changes);
                        }
                        catch (Exception ex)
                        {
                            await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetection - SaveBulkRecord", "", ctx.Database.GetConnectionString());
                            throw ex;
                        }

                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetection - Envelope " + envelope.CountryCode + "/" + envelope.VersionId.ToString(), "", ctx.Database.GetConnectionString());
                        throw ex;

                    }
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("End ChangeDetection {0} - {1}", envelope.CountryCode, envelope.VersionId), "ChangeDetection", "", ctx.Database.GetConnectionString());
                }

                //execute "DELETE FROM dbo.Changes ..." if it has been called directly from the endpoint
                if (from_end_point) await DeleteUnrelatedChanges(ctx);

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetection", "", ctx.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<HarvestedEnvelope>> ChangeDetectionSpatialData(EnvelopesToProcess[] envelopeIDs, N2KBackboneContext? ctx = null)
        {
            try
            {
                if (ctx == null) ctx = _dataContext;
                List<HarvestedEnvelope> result = new();

                //for each envelope to process
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Start  Geospatial changes detection {0}-{1}", envelope.CountryCode, envelope.VersionId), "ChangeDetection - Geospatial change", "", ctx.Database.GetConnectionString());

                    HttpClient client = new();
                    String serverUrl = String.Format(_appSettings.Value.fme_service_spatialchanges, envelope.VersionId, envelope.CountryCode, _appSettings.Value.Environment, _appSettings.Value.fme_security_token);
                    try
                    {
                        //TimeLog.setTimeStamp("Geospatial changes for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString(), "Starting");
                        client.Timeout = TimeSpan.FromHours(5);
                        Task<HttpResponseMessage> response = client.GetAsync(serverUrl);
                        string content = await response.Result.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "Geospatial changes", "", ctx.Database.GetConnectionString());
                    }
                    finally
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("End Geospatial changes detection {0}-{1}", envelope.CountryCode, envelope.VersionId), "ChangeDetection - Geospatial change", "", ctx.Database.GetConnectionString());
                        client.Dispose();
                        //TimeLog.setTimeStamp("Geospatial changes for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString().ToString(), "End");
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetectionSpatialData", "", ctx.Database.GetConnectionString());
                return new List<HarvestedEnvelope>();
            }
            finally
            {
                //TimeLog.setTimeStamp("ChangeDetection spatial changes ", "End");
            }
        }

        public async Task<int> ChangeDetectionSingleSiteSpatialData(string siteCode, int versionId)
        {
            int result = 0;

            HttpClient client = new();
            String serverUrl = String.Format(_appSettings.Value.fme_service_singlesite_spatialchanges, siteCode, versionId.ToString(), _appSettings.Value.fme_security_token);
            try
            {
                //TimeLog.setTimeStamp("Spatial ChangeDetection for site " + siteCode + " - " + versionId.ToString(), "Starting");
                Task<HttpResponseMessage> response = client.GetAsync(serverUrl);
                string content = await response.Result.Content.ReadAsStringAsync();
                result = 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetectionSingleSiteSpatialData", "", _dataContext.Database.GetConnectionString());
            }
            finally
            {
                client.Dispose();
                client = null;
                //TimeLog.setTimeStamp("ChangeDetection spatial site " + siteCode + " - " + versionId.ToString().ToString(), "End");
            }
            return result;
        }

        public async Task<List<HarvestedEnvelope>> ChangeDetectionSingleSite(string siteCode, int versionId, string connectionString)
        {
            List<HarvestedEnvelope> result = new();
            var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(connectionString,
                    opt => opt.EnableRetryOnFailure()).Options;
            using (N2KBackboneContext ctx = new(options))
            {
                SiteToHarvest? harvestingSite;
                try
                {
                    SqlParameter param1 = new("@sitecode", siteCode);
                    SqlParameter param2 = new("@version", versionId);

                    List<SiteToHarvest>? sitesVersioning = await ctx.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSitesBySitecodeAndVersion  @sitecode, @version",
                                            param1, param2).ToListAsync();

                    harvestingSite = sitesVersioning.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetectionSingleSite - GetReferenceSitesBySitecodeAndVersion", "", connectionString);
                    throw ex;
                }

                EnvelopesToProcess envelope = new()
                {
                    CountryCode = harvestingSite.CountryCode,
                    VersionId = (int)harvestingSite.N2KVersioningVersion
                };

                SqlParameter countryParam1 = new("@country", envelope.CountryCode);
                SqlParameter countryParam3 = new("@last_envelop", envelope.VersionId);

                List<SiteChangeDb> changes = new();

                try
                {
                    //Get the changes status from ProcessedEnvelopes
                    ProcessedEnvelopes? processedEnvelope = await ctx.Set<ProcessedEnvelopes>().Where(c => c.Country == harvestingSite.CountryCode && c.Version == (int)harvestingSite.N2KVersioningVersion).AsNoTracking().FirstOrDefaultAsync();

                    Lineage lineage = await ctx.Set<Lineage>().Where(c => c.SiteCode == harvestingSite.SiteCode && c.Version == harvestingSite.VersionId).AsNoTracking().FirstOrDefaultAsync();

                    List<LineageAntecessors> antecessors = await ctx.Set<LineageAntecessors>().AsNoTracking().Where(c => c.LineageID == lineage.ID).ToListAsync();

                    //Get the lists of priority habitats and species
                    List<HabitatPriority> habitatPriority = await ctx.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
                    List<SpeciesPriority> speciesPriority = await ctx.Set<SpeciesPriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

                    SiteToHarvest storedSite = null;

                    //Delete existing changes
                    await ctx.Database.ExecuteSqlRawAsync("DELETE FROM dbo.Changes WHERE [SiteCode] = '" + harvestingSite.SiteCode + "' AND [N2KVersioningVersion] = " + harvestingSite.N2KVersioningVersion);

                    if ((lineage.Type == LineageTypes.NoChanges || lineage.Type == LineageTypes.Recode || lineage.Type == LineageTypes.Deletion) && antecessors.Count == 1)
                    {
                        SqlParameter param1 = new("@sitecode", antecessors.FirstOrDefault().SiteCode);
                        SqlParameter param2 = new("@version", antecessors.FirstOrDefault().Version);

                        List<SiteToHarvest>? referencedSites = await ctx.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSitesBySitecodeAndVersion  @sitecode, @version",
                                            param1, param2).ToListAsync();

                        storedSite = referencedSites.FirstOrDefault();
                    }

                    if (lineage.Type == LineageTypes.Deletion && antecessors.Count == 1)
                    {
                        harvestingSite = null;
                    }

                    changes = await SingleSiteChangeDetection(changes, storedSite, harvestingSite, envelope, habitatPriority, speciesPriority, processedEnvelope, ctx);
                    changes.ForEach(cs => cs.Status = SiteChangeStatus.Pending);
                    result.Add(new HarvestedEnvelope
                    {
                        CountryCode = envelope.CountryCode,
                        VersionId = envelope.VersionId,
                        NumChanges = changes.Count,
                        Status = processedEnvelope.Status
                    });

                    try
                    {
                        await SiteChangeDb.SaveBulkRecord(connectionString, changes);
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetectionSingleSite - SaveBulkRecord", "", connectionString);
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetectionSingleSite - Site " + siteCode + "/" + versionId.ToString(), "", connectionString);
                    throw ex;
                }
            }
            return result;
        }

        public async Task<List<SiteChangeDb>> ParallelSiteChangeDetection(List<LineageDetection>? detectedLineageChanges, List<SiteToHarvest> referencedSites, SiteToHarvest harvestingSite, EnvelopesToProcess envelope, List<HabitatPriority> habitatPriority, List<SpeciesPriority> speciesPriority, ProcessedEnvelopes? processedEnvelope, List<RelatedSites>? sitesRelation, bool manualEdition = false, N2KBackboneContext? _ctx = null,
            List<HabitatsToHarvestPerEnvelope>? habitatsVersioningEnvelope = null, List<HabitatsToHarvestPerEnvelope>? habitatsReferenceEnvelope = null,
            List<SpeciesToHarvestPerEnvelope>? speciesVersioningEnvelope = null, List<SpeciesToHarvestPerEnvelope>? speciesReferenceEnvelope = null,
            List<SpeciesToHarvestPerEnvelope>? speciesOtherVersioningEnvelope = null, List<SpeciesToHarvestPerEnvelope>? speciesOtherReferenceEnvelope = null,
            List<BioRegions>? bioRegionsVersioningEnvelope = null, List<BioRegions>? bioRegionsRefereceEnvelope = null)
        {
            //Tolerance values. If the difference between reference and versioning values is bigger than these numbers, then they are notified.
            //If the tolerance is at 0, then it registers ALL changes, no matter how small they are.
            double siteAreaHaTolerance = 0.0;
            double siteLengthKmTolerance = 0.0;
            double habitatCoverHaTolerance = 0.0;
            List<SiteChangeDb> changes = new();

            try
            {
                if (_ctx == null) _ctx = _dataContext;
                var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_dataContext.Database.GetConnectionString(),
                        opt => opt.EnableRetryOnFailure()).Options;
                using (N2KBackboneContext ctx = new(options))
                {
                    processedEnvelope.Status = await GetSiteChangeStatus(processedEnvelope.Status, ctx);
                    RelatedSites? siteRelation = sitesRelation.Where(s => s.NewSiteCode == harvestingSite.SiteCode && s.NewVersion == harvestingSite.VersionId).FirstOrDefault();
                    SiteToHarvest? storedSite = null;
                    LineageDetection ld = null;
                    if (siteRelation != null)
                    {
                        storedSite = referencedSites.Where(s => s.SiteCode == siteRelation.PreviousSiteCode && s.VersionId == siteRelation.PreviousVersion).FirstOrDefault();
                        if (storedSite != null)
                            ld = detectedLineageChanges.FirstOrDefault(e => e.new_sitecode == storedSite.SiteCode && e.new_version == storedSite.VersionId);
                    }
                    if (siteRelation != null && storedSite != null)
                    {
                        //SiteAttributesChecking
                        HarvestSiteCode siteCode = new(ctx, _versioningContext);
                        changes = await siteCode.ChangeDetectionSiteAttributes(changes, envelope, harvestingSite, storedSite, siteAreaHaTolerance, siteLengthKmTolerance, processedEnvelope, ctx);

                        SqlParameter param3 = new("@site", harvestingSite.SiteCode);
                        int maxVersionSite = harvestingSite.VersionId;
                        SqlParameter param4 = new("@versionId", maxVersionSite);
                        int previousVersionSite = storedSite.VersionId;
                        SqlParameter param5 = new("@versionId", previousVersionSite);

                        List<BioRegions> bioRegionsVersioning = null;
                        if (bioRegionsVersioningEnvelope != null)
                        {
                            bioRegionsVersioning = bioRegionsVersioningEnvelope
                                .Where(spEnv => spEnv.SiteCode == harvestingSite.SiteCode && spEnv.Version == maxVersionSite)
                                //.Select (sp => (SpeciesToHarvest) sp)
                                .ToList<BioRegions>();
                        }
                        else
                        {
                            bioRegionsVersioning = await ctx.Set<BioRegions>().FromSqlRaw($"exec dbo.spGetReferenceBioRegionsBySiteCodeAndVersion  @site, @versionId",
                                        param3, param4).ToListAsync();
                        }

                        List<BioRegions> referencedBioRegions = null;
                        if (bioRegionsRefereceEnvelope != null)
                        {
                            referencedBioRegions = bioRegionsRefereceEnvelope
                                .Where(spEnv => spEnv.SiteCode == harvestingSite.SiteCode && spEnv.Version == storedSite.VersionId)
                                //.Select (sp => (SpeciesToHarvest) sp)
                                .ToList<BioRegions>();
                        }
                        else
                        {
                            referencedBioRegions = await ctx.Set<BioRegions>().FromSqlRaw($"exec dbo.spGetReferenceBioRegionsBySiteCodeAndVersion  @site, @versionId",
                                        param3, param5).ToListAsync();
                        }
                        changes = await siteCode.ChangeDetectionBioRegions(bioRegionsVersioning, referencedBioRegions, changes, envelope, harvestingSite, storedSite, param3, param4, param5, processedEnvelope, ctx);

                        //HabitatChecking
                        List<HabitatToHarvest> habitatVersioning = null;
                        if (habitatsVersioningEnvelope != null)
                        {
                            habitatVersioning = habitatsVersioningEnvelope
                                .Where(spEnv => spEnv.SiteCode == harvestingSite.SiteCode && spEnv.VersionId == maxVersionSite)
                                //.Select (sp => (SpeciesToHarvest) sp)
                                .ToList<HabitatToHarvest>();
                        }
                        else
                        {
                            habitatVersioning = await ctx.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                       param3, param4).ToListAsync();
                        }

                        List<HabitatToHarvest> referencedHabitats = null;
                        if (habitatsReferenceEnvelope != null)
                        {
                            referencedHabitats = habitatsReferenceEnvelope
                                .Where(spEnv => spEnv.SiteCode == harvestingSite.SiteCode && spEnv.VersionId == storedSite.VersionId)
                                //.Select (sp => (SpeciesToHarvest) sp)
                                .ToList<HabitatToHarvest>();
                        }
                        else
                        {
                            referencedHabitats = await ctx.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                        param3, param5).ToListAsync();
                        }
                        HarvestHabitats habitats = new(ctx, _versioningContext);
                        changes = await habitats.ChangeDetectionHabitat(habitatVersioning, referencedHabitats, changes, envelope, harvestingSite, storedSite, param3, param4, param5, habitatCoverHaTolerance, habitatPriority, processedEnvelope, ctx);

                        //SpeciesChecking
                        List<SpeciesToHarvest> speciesVersioning = null;
                        if (speciesVersioningEnvelope != null)
                        {
                            speciesVersioning =
                                speciesVersioningEnvelope
                                .Where(spEnv => spEnv.SiteCode == harvestingSite.SiteCode && spEnv.VersionId == maxVersionSite)
                                //.Select (sp => (SpeciesToHarvest) sp)
                                .ToList<SpeciesToHarvest>();
                        }
                        else
                        {
                            speciesVersioning = await ctx.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                        param3, param4).ToListAsync();
                        }

                        List<SpeciesToHarvest> referencedSpecies = null;
                        if (speciesVersioningEnvelope != null)
                        {
                            referencedSpecies =
                                speciesReferenceEnvelope
                                .Where(spEnv => spEnv.SiteCode == harvestingSite.SiteCode && spEnv.VersionId == storedSite.VersionId)
                                //.Select (sp => (SpeciesToHarvest) sp)
                                .ToList<SpeciesToHarvest>();
                        }
                        else
                        {
                            referencedSpecies = await ctx.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                       param3, param5).ToListAsync();
                        }
                        HarvestSpecies species = new(ctx, _versioningContext);
                        changes = await species.ChangeDetectionSpecies(speciesVersioning, referencedSpecies, changes, envelope, harvestingSite, storedSite, param3, param4, param5, speciesPriority, processedEnvelope, ctx,
                                speciesOtherVersioningEnvelope, speciesOtherReferenceEnvelope);

                        //These booleans declare whether or not each site is a priority
                        Boolean isStoredSitePriority = (bool)await ctx.Set<Sites>().Where(s => s.SiteCode == storedSite.SiteCode && s.Version == storedSite.VersionId).Select(c => c.Priority).FirstOrDefaultAsync();
                        Boolean isHarvestingSitePriority = (bool)await ctx.Set<Sites>().Where(s => s.SiteCode == harvestingSite.SiteCode && s.Version == harvestingSite.VersionId).Select(c => c.Priority).FirstOrDefaultAsync();

                        if (isStoredSitePriority && !isHarvestingSitePriority)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Site General Info",
                                ChangeType = "Site Losing Priority",
                                LineageChangeType = LineageTypes.NoChanges,
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = Convert.ToString(isHarvestingSitePriority),
                                OldValue = Convert.ToString(isStoredSitePriority),
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "Priority",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                        else if (!isStoredSitePriority && isHarvestingSitePriority)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Site General Info",
                                ChangeType = "Site Getting Priority",
                                LineageChangeType = LineageTypes.NoChanges,
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Info,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = Convert.ToString(isHarvestingSitePriority),
                                OldValue = Convert.ToString(isStoredSitePriority),
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "Priority",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }

                        await ctx.Database.ExecuteSqlRawAsync("UPDATE [dbo].[Sites] SET [Priority] = '" + isStoredSitePriority + "' WHERE [SiteCode] = '" + storedSite.SiteCode + "' AND [Version] = '" + storedSite.VersionId + "'");
                        await ctx.Database.ExecuteSqlRawAsync("UPDATE [dbo].[Sites] SET [Priority] = '" + isHarvestingSitePriority + "' WHERE [SiteCode] = '" + harvestingSite.SiteCode + "' AND [Version] = '" + harvestingSite.VersionId + "'");

                        Lineage? lineage = await ctx.Set<Lineage>().FirstOrDefaultAsync(l => l.SiteCode == harvestingSite.SiteCode && l.Version == harvestingSite.VersionId);
                        if (lineage?.Type == LineageTypes.Merge)
                        {
                            string antecessors = string.Join(',',
                                await ctx.Set<LineageAntecessors>().Where(a => a.LineageID == lineage.ID)
                                .Select(a => a.SiteCode).ToArrayAsync());
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Lineage",
                                ChangeType = "Site Merged",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status, ctx),
                                Tags = string.Empty,
                                NewValue = harvestingSite.SiteCode,
                                OldValue = antecessors,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                        else if (lineage?.Type == LineageTypes.Split)
                        {
                            // get sibling sites (sites that resulted from the split)
                            List<long> siblingIDs = await ctx.Set<LineageAntecessors>()
                                .Where(a => a.SiteCode == storedSite.SiteCode && a.Version == storedSite.VersionId)
                                .Select(a => a.LineageID).ToListAsync();
                            string siblingSites = string.Join(',',
                                await ctx.Set<Lineage>()
                                .Where(l => siblingIDs.Contains(l.ID))
                                .Select(l => l.SiteCode).ToArrayAsync());

                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Lineage",
                                ChangeType = "Site Split",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status, ctx),
                                Tags = string.Empty,
                                NewValue = siblingSites,
                                OldValue = storedSite.SiteCode,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }

                        //Add justification files and comments from the current to the new version
                        Sites current = await ctx.Set<Sites>().Where(x => x.SiteCode == harvestingSite.SiteCode && x.Current == true).FirstOrDefaultAsync();
                        if (current != null)
                        {
                            SqlParameter paramSitecode = new("@sitecode", harvestingSite.SiteCode);
                            SqlParameter paramOldVersion = new("@oldVersion", current.Version);
                            SqlParameter paramNewVersion = new("@newVersion", harvestingSite.VersionId);
                            await ctx.Database.ExecuteSqlRawAsync($"exec dbo.spCopyJustificationFilesAndStatusChanges  @sitecode, @oldVersion, @newVersion",
                                    paramSitecode, paramOldVersion, paramNewVersion);
                        }
                    }
                    else
                    {
                        Lineage? lineage = await ctx.Set<Lineage>().FirstOrDefaultAsync(l => l.SiteCode == harvestingSite.SiteCode && l.Version == harvestingSite.VersionId);
                        if (lineage?.Type == LineageTypes.Merge)
                        {
                            string antecessors = string.Join(',',
                                await ctx.Set<LineageAntecessors>().Where(a => a.LineageID == lineage.ID)
                                .Select(a => a.SiteCode).ToArrayAsync());
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Lineage",
                                ChangeType = "Site Merged",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status, ctx),
                                Tags = string.Empty,
                                NewValue = harvestingSite.SiteCode,
                                OldValue = antecessors,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = harvestingSite.VersionId,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                        else if (lineage?.Type == LineageTypes.Split)
                        {
                            // get sibling sites (sites that resulted from the split)
                            LineageAntecessors? antecessor = await ctx.Set<LineageAntecessors>()
                                .FirstOrDefaultAsync(a => a.LineageID == lineage.ID);
                            List<long> siblingIDs = await ctx.Set<LineageAntecessors>()
                                .Where(a => a.SiteCode == antecessor.SiteCode && a.Version == antecessor.Version)
                                .Select(a => a.LineageID).ToListAsync();
                            string siblings = string.Join(',',
                                await ctx.Set<Lineage>()
                                .Where(l => siblingIDs.Contains(l.ID))
                                .Select(l => l.SiteCode).ToArrayAsync());

                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Lineage",
                                ChangeType = "Site Split",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status, ctx),
                                Tags = string.Empty,
                                NewValue = siblings,
                                OldValue = antecessor.SiteCode,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = antecessor.Version,
                                ReferenceSiteCode = antecessor.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                        else
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Lineage",
                                ChangeType = "Site Added",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = harvestingSite.SiteCode,
                                OldValue = null,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = harvestingSite.VersionId,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ParallelSiteChangeDetection - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "", _ctx.Database.GetConnectionString());
            }
            return changes;
        }

        public async Task<List<SiteChangeDb>> SingleSiteChangeDetection(List<SiteChangeDb> changes, SiteToHarvest? storedSite, SiteToHarvest? harvestingSite, EnvelopesToProcess envelope, List<HabitatPriority> habitatPriority, List<SpeciesPriority> speciesPriority, ProcessedEnvelopes? processedEnvelope, N2KBackboneContext ctx)
        {
            //Tolerance values. If the difference between reference and versioning values is bigger than these numbers, then they are notified.
            //If the tolerance is at 0, then it registers ALL changes, no matter how small they are.
            double siteAreaHaTolerance = 0.0;
            double siteLengthKmTolerance = 0.0;
            double habitatCoverHaTolerance = 0.0;

            try
            {
                SqlParameter paramDetection1 = new("@reported_envelop", envelope.VersionId);
                SqlParameter paramDetection2 = new("@country", envelope.CountryCode);
                SqlParameter paramDetection3 = new("@tol", 5);
                List<LineageDetection>? detectedLineageChanges = await ctx.Set<LineageDetection>().FromSqlRaw($"exec dbo.spGetSitesToDetectChangesWithLineage  @reported_envelop, @country, @tol",
                                paramDetection1, paramDetection2, paramDetection3).ToListAsync();

                if (storedSite != null && harvestingSite != null)
                {
                    //SiteAttributesChecking
                    HarvestSiteCode siteCode = new(ctx, _versioningContext);
                    changes = await siteCode.ChangeDetectionSiteAttributes(changes, envelope, harvestingSite, storedSite, siteAreaHaTolerance, siteLengthKmTolerance, processedEnvelope, ctx);

                    SqlParameter param3 = new("@site", harvestingSite.SiteCode);
                    int maxVersionSite = harvestingSite.VersionId;
                    SqlParameter param4 = new("@versionId", maxVersionSite);
                    int previousVersionSite = storedSite.VersionId;
                    SqlParameter param5 = new("@versionId", previousVersionSite);

                    //BioRegionChecking
                    List<BioRegions> bioRegionsVersioning = await ctx.Set<BioRegions>().FromSqlRaw($"exec dbo.spGetReferenceBioRegionsBySiteCodeAndVersion  @site, @versionId",
                                    param3, param4).ToListAsync();
                    List<BioRegions> referencedBioRegions = await ctx.Set<BioRegions>().FromSqlRaw($"exec dbo.spGetReferenceBioRegionsBySiteCodeAndVersion  @site, @versionId",
                                    param3, param5).ToListAsync();
                    changes = await siteCode.ChangeDetectionBioRegions(bioRegionsVersioning, referencedBioRegions, changes, envelope, harvestingSite, storedSite, param3, param4, param5, processedEnvelope, ctx);

                    //HabitatChecking
                    List<HabitatToHarvest> habitatVersioning = await ctx.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                    param3, param4).ToListAsync();
                    List<HabitatToHarvest> referencedHabitats = await ctx.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                    param3, param5).ToListAsync();
                    HarvestHabitats habitats = new(ctx, _versioningContext);
                    changes = await habitats.ChangeDetectionHabitat(habitatVersioning, referencedHabitats, changes, envelope, harvestingSite, storedSite, param3, param4, param5, habitatCoverHaTolerance, habitatPriority, processedEnvelope, ctx);

                    //SpeciesChecking
                    List<SpeciesToHarvest> speciesVersioning = await ctx.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                    param3, param4).ToListAsync();
                    List<SpeciesToHarvest> referencedSpecies = await ctx.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                    param3, param5).ToListAsync();
                    HarvestSpecies species = new(ctx, _versioningContext);
                    changes = await species.ChangeDetectionSpecies(speciesVersioning, referencedSpecies, changes, envelope, harvestingSite, storedSite, param3, param4, param5, speciesPriority, processedEnvelope, ctx);

                    //These booleans declare whether or not each site is a priority
                    Boolean isStoredSitePriority = (bool)await ctx.Set<Sites>().Where(s => s.SiteCode == storedSite.SiteCode && s.Version == storedSite.VersionId).Select(c => c.Priority).FirstOrDefaultAsync();
                    Boolean isHarvestingSitePriority = (bool)await ctx.Set<Sites>().Where(s => s.SiteCode == harvestingSite.SiteCode && s.Version == harvestingSite.VersionId).Select(c => c.Priority).FirstOrDefaultAsync();

                    if (isStoredSitePriority && !isHarvestingSitePriority)
                    {
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Site General Info",
                            ChangeType = "Site Losing Priority",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = (SiteChangeStatus?)processedEnvelope.Status,
                            Tags = string.Empty,
                            NewValue = Convert.ToString(isHarvestingSitePriority),
                            OldValue = Convert.ToString(isStoredSitePriority),
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = storedSite.VersionId,
                            FieldName = "Priority",
                            ReferenceSiteCode = storedSite.SiteCode,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }
                    else if (!isStoredSitePriority && isHarvestingSitePriority)
                    {
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Site General Info",
                            ChangeType = "Site Getting Priority",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Info,
                            Status = (SiteChangeStatus?)processedEnvelope.Status,
                            Tags = string.Empty,
                            NewValue = Convert.ToString(isHarvestingSitePriority),
                            OldValue = Convert.ToString(isStoredSitePriority),
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = storedSite.VersionId,
                            FieldName = "Priority",
                            ReferenceSiteCode = storedSite.SiteCode,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }

                    await ctx.Database.ExecuteSqlRawAsync("UPDATE [dbo].[Sites] SET [Priority] = '" + isStoredSitePriority + "' WHERE [SiteCode] = '" + storedSite.SiteCode + "' AND [Version] = '" + storedSite.VersionId + "'");
                    await ctx.Database.ExecuteSqlRawAsync("UPDATE [dbo].[Sites] SET [Priority] = '" + isHarvestingSitePriority + "' WHERE [SiteCode] = '" + harvestingSite.SiteCode + "' AND [Version] = '" + harvestingSite.VersionId + "'");

                    //Add justification files and comments from the current to the new version
                    Sites current = ctx.Set<Sites>().Where(x => x.SiteCode == harvestingSite.SiteCode && x.Current == true).FirstOrDefault();
                    if (current != null)
                    {
                        SqlParameter paramSitecode = new("@sitecode", harvestingSite.SiteCode);
                        SqlParameter paramOldVersion = new("@oldVersion", current.Version);
                        SqlParameter paramNewVersion = new("@newVersion", harvestingSite.VersionId);
                        await ctx.Database.ExecuteSqlRawAsync($"exec dbo.spCopyJustificationFilesAndStatusChanges  @sitecode, @oldVersion, @newVersion",
                                paramSitecode, paramOldVersion, paramNewVersion);
                    }
                }
                else if (storedSite == null && harvestingSite != null)
                {
                    Lineage? lineage = await ctx.Set<Lineage>().FirstOrDefaultAsync(l => l.SiteCode == harvestingSite.SiteCode && l.Version == harvestingSite.VersionId);
                    if (lineage?.Type == LineageTypes.Merge)
                    {
                        string antecessors = string.Join(',',
                            await ctx.Set<LineageAntecessors>().Where(a => a.LineageID == lineage.ID)
                            .Select(a => a.SiteCode).ToArrayAsync());
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Lineage",
                            ChangeType = "Site Merged",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status, ctx),
                            Tags = string.Empty,
                            NewValue = harvestingSite.SiteCode,
                            OldValue = antecessors,
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = harvestingSite.VersionId,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }
                    else if (lineage?.Type == LineageTypes.Split)
                    {
                        // get sibling sites (sites that resulted from the split)
                        LineageAntecessors? antecessor = await ctx.Set<LineageAntecessors>()
                            .FirstOrDefaultAsync(a => a.LineageID == lineage.ID);
                        List<long> siblingIDs = await ctx.Set<LineageAntecessors>()
                            .Where(a => a.SiteCode == antecessor.SiteCode && a.Version == antecessor.Version)
                            .Select(a => a.LineageID).ToListAsync();
                        string siblings = string.Join(',',
                            await ctx.Set<Lineage>()
                            .Where(l => siblingIDs.Contains(l.ID))
                            .Select(l => l.SiteCode).ToArrayAsync());

                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Lineage",
                            ChangeType = "Site Split",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status, ctx),
                            Tags = string.Empty,
                            NewValue = siblings,
                            OldValue = antecessor.SiteCode,
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = antecessor.Version,
                            ReferenceSiteCode = antecessor.SiteCode,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }
                    else
                    {
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Lineage",
                            ChangeType = "Site Added",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = (SiteChangeStatus?)processedEnvelope.Status,
                            Tags = string.Empty,
                            NewValue = harvestingSite.SiteCode,
                            OldValue = null,
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = harvestingSite.VersionId,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }
                }
                else if (storedSite != null && harvestingSite == null)
                {
                    Lineage? lineage = await ctx.Set<Lineage>().FirstOrDefaultAsync(l => l.SiteCode == storedSite.SiteCode && l.Version == storedSite.VersionId && l.N2KVersioningVersion == envelope.VersionId);
                    if (lineage?.Type == LineageTypes.Deletion) //the site is not been recoded, split or merge
                    {
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = storedSite.SiteCode,
                            Version = storedSite.VersionId,
                            ChangeCategory = "Lineage",
                            ChangeType = "Site Deleted",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status, ctx),
                            Tags = string.Empty,
                            NewValue = null,
                            OldValue = storedSite.SiteCode,
                            Code = storedSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = storedSite.VersionId,
                            ReferenceSiteCode = storedSite.SiteCode,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SingleSiteChangeDetection - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "", ctx.Database.GetConnectionString());
            }
            return changes;
        }

        /// <summary>
        /// This mehtod calls for teh process to harvest the complete data for all sites 
        /// reported in the envelopment reported by the MS
        /// </summary>
        /// <param name="envelopeIDs">A list of Envelops to process</param>
        /// <returns>A list of the envelops with the result of the process</returns>
        public async Task<List<HarvestedEnvelope>> Harvest(EnvelopesToProcess[] envelopeIDs)
        {
            return await _Harvest(envelopeIDs, HarvestingStatus.PreHarvested);
        }

        private async Task<List<HarvestedEnvelope>> _Harvest(EnvelopesToProcess[] envelopeIDs, HarvestingStatus finalStatus)
        {
            try
            {
                List<HarvestedEnvelope> result = new();
                _speciesTypes = await _dataContext.Set<SpeciesTypes>().AsNoTracking().ToListAsync();
                _dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                _ownerShipTypes = await _dataContext.Set<Models.backbone_db.OwnerShipTypes>().ToListAsync();

                //save in memory the fixed codes like priority species and habitat codes
                HarvestSiteCode siteCode = new(_dataContext, _versioningContext)
                {
                    habitatPriority = await _dataContext.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync(),
                    speciesPriority = await _dataContext.Set<SpeciesPriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync()
                };

                //for each envelope to process
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    ClearBulkItems();
                    Console.WriteLine(String.Format("Start envelope harvest {0} - {1}", envelope.CountryCode, envelope.VersionId));
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Start envelope harvest {0} - {1}", envelope.CountryCode, envelope.VersionId), "HarvestedService - _Harvest", "", _dataContext.Database.GetConnectionString());
                    DateTime startEnvelope = DateTime.Now;
                    //Not necessary 
                    //await resetEnvirontment(envelope.CountryCode, envelope.VersionId);
                    DateTime SubmissionDate = envelope.SubmissionDate; //getOptimalDate(envelope);
                                                                       //create a new entry in the processed envelopes table to register that a new one is being harvested
                    ProcessedEnvelopes envelopeToProcess = new()
                    {
                        Country = envelope.CountryCode,
                        Version = envelope.VersionId,
                        ImportDate = envelope.SubmissionDate, //await GetSubmissionDate(envelope.CountryCode, envelope.VersionId)
                        Status = HarvestingStatus.Harvesting,
                        Importer = "AUTOIMPORT",
                        N2K_VersioningDate = SubmissionDate // envelope.SubmissionDate //await GetSubmissionDate(envelope.CountryCode, envelope.VersionId)
                    };

                    try
                    {
                        //add the envelope to the DB
                        _dataContext.Set<ProcessedEnvelopes>().Add(envelopeToProcess);
                        _dataContext.SaveChanges();

                        //Get the sites submitted in the envelope
                        List<NaturaSite> vSites = await _versioningContext.Set<NaturaSite>().Where(v => (v.COUNTRYCODE == envelope.CountryCode) && (v.COUNTRYVERSIONID == envelope.VersionId)).ToListAsync();

                        //save in memory the fixed codes like priority species and habitat codes
                        DateTime start1 = DateTime.Now;
                        List<Sites> bbSites = new();

                        //create a list with the existing version per site in the current country
                        //to avoid querying the db for every single site
                        List<SiteVersion> versionsPerSite = await _dataContext.Set<Sites>().AsNoTracking().Where(v => v.CountryCode == envelope.CountryCode).GroupBy(a => a.SiteCode)
                            .Select(g => new SiteVersion
                            {
                                SiteCode = g.Key,
                                MaxVersion = g.Max(x => x.Version)
                            }).ToListAsync();

                        //save to backbone database the site-versions                          
                        foreach (NaturaSite vSite in vSites)
                        {
                            int versionNext = 0;
                            if (versionsPerSite.Any(s => s.SiteCode == vSite.SITECODE))
                            {
                                SiteVersion? _versionPerSite = versionsPerSite.FirstOrDefault(s => s.SiteCode == vSite.SITECODE);
                                versionNext = _versionPerSite.Value.MaxVersion + 1;
                            }
                            Sites? bbSite = await siteCode.harvestSiteCode(vSite, envelope, versionNext);
                            if (bbSite != null) bbSites.Add(bbSite);
                        }
                        versionsPerSite.Clear();

                        //save all sitecode-version in bulk mode
                        await Sites.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), bbSites);

                        HarvestSpecies species = new(_dataContext, _versioningContext);
                        await species.HarvestByCountry(envelope.CountryCode, envelope.VersionId, _speciesTypes, _versioningContext.Database.GetConnectionString(), _dataContext.Database.GetConnectionString(), bbSites);
                        //Console.WriteLine(String.Format("END species country {0}", (DateTime.Now - start1).TotalSeconds));

                        //Harvest habitats by country
                        HarvestHabitats habitats = new(_dataContext, _versioningContext);
                        await habitats.HarvestByCountry(envelope.CountryCode, envelope.VersionId, _versioningContext.Database.GetConnectionString(), _dataContext.Database.GetConnectionString(), _dataQualityTypes, bbSites);
                        //Console.WriteLine(String.Format("END habitats country {0}", (DateTime.Now - start1).TotalSeconds));

                        HarvestSiteCode sites = new(_dataContext, _versioningContext);
                        await sites.HarvestSite(envelope.CountryCode, envelope.VersionId, _versioningContext.Database.GetConnectionString(), _dataContext.Database.GetConnectionString(), _dataQualityTypes, _ownerShipTypes, bbSites);


                        //set the enevelope as successfully completed
                        envelopeToProcess.Status = HarvestingStatus.DataLoaded;
                        _dataContext.Set<ProcessedEnvelopes>().Update(envelopeToProcess);
                        result.Add(
                            new HarvestedEnvelope
                            {
                                CountryCode = envelope.CountryCode,
                                VersionId = envelope.VersionId,
                                NumChanges = 0,
                                Status = HarvestingStatus.DataLoaded
                            }
                         );
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - _Harvest - Envelope " + envelope.CountryCode + "/" + envelope.VersionId.ToString(), "", _dataContext.Database.GetConnectionString());
                        envelopeToProcess.Status = HarvestingStatus.Error;
                        _dataContext.Set<ProcessedEnvelopes>().Update(envelopeToProcess);
                        result.Add(
                            new HarvestedEnvelope
                            {
                                CountryCode = envelope.CountryCode,
                                VersionId = envelope.VersionId,
                                NumChanges = 0,
                                Status = HarvestingStatus.Error //SiteChangeStatus.Error
                            }
                         );
                    }
                    finally
                    {
                        //save the data of the site in backbone DB
                        _dataContext.SaveChanges();
                    }
                    _countrySpecies.Clear();
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("End envelope harvest {0}", (DateTime.Now - startEnvelope).TotalSeconds), "HarvestedService - _Harvest", "", _dataContext.Database.GetConnectionString());
                    Console.WriteLine(String.Format("End envelope {0}", (DateTime.Now - startEnvelope).TotalSeconds));
                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - _Harvest", "", _dataContext.Database.GetConnectionString());
                return await Task.FromResult(new List<HarvestedEnvelope>());
            }
            finally
            {
                _speciesTypes.Clear();
                _dataQualityTypes.Clear();
                _ownerShipTypes.Clear();
            }
        }

        /// <summary>
        /// In case of development evirontment it return a proper date for the submision date
        /// </summary>
        /// <param name="pEnvelope"></param>
        /// <returns></returns>
        private DateTime getOptimalDate(EnvelopesToProcess pEnvelope)
        {
            DateTime returnDate = pEnvelope.SubmissionDate;
            if (_appSettings.Value.InDevelopment)
            {
                try
                {
                    List<PackageCountrySpatial> packSpatials = _versioningContext.Set<PackageCountrySpatial>().Where(v => v.CountryCode == pEnvelope.CountryCode).OrderByDescending(v => v.CountryVersionID).ToList();
                    foreach (PackageCountrySpatial packSpatial in packSpatials)
                    {
                        if (packSpatial.Importdate != null)
                        {
                            if (packSpatial.Importdate != pEnvelope.SubmissionDate)
                            {
                                returnDate = (DateTime)packSpatial.Importdate;
                            }
                            break;
                        }
                    }
                }
                catch
                {

                }
            }
            return returnDate;
        }

        public async Task HarvestSpatialData(EnvelopesToProcess[] envelopeIDs, IMemoryCache cache)
        {
            try
            {
                //calculate the min versionID of each country    
                var minVersionPerCountry = envelopeIDs.GroupBy(c => c.CountryCode).SelectMany(g => g.Where(p => p.VersionId == g.Min(h => h.VersionId))).ToList();
                //for each envelope to process
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    try
                    {
                        var minCountryVersion = minVersionPerCountry.Find(a => a.CountryCode == envelope.CountryCode).VersionId;
                        await _fmeHarvestJobs.LaunchFMESpatialHarvestBackground(envelope, minCountryVersion);
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestGeodata", "", _dataContext.Database.GetConnectionString());
                    }
                }

                _fmeHarvestJobs.FMEJobCompleted += async (sender, env) =>
                {
                    //handle the event with a semaphore to ensure the same event is handled only one
                    string _connectionString = env.DBConnection;
                    //((BackgroundSpatialHarvestJobs)sender).GetDataContext().Database.GetConnectionString();
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Enter Event handler with fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);

                    SemaphoreAsync _semaphore;
                    string sem_name = string.Format("semaphore_{0}_{1}", env.Envelope.CountryCode, env.Envelope.VersionId);
                    try
                    {
                        //Try to Open the Semaphore if Exists, if not throw an exception
                        _semaphore = SemaphoreAsync.OpenExisting(sem_name);
                        //if it exists it means it is running, So we cancel it
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Cancelled event handler with fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);
                        return;
                    }
                    catch
                    {
                        //If Semaphore not Exists, create a semaphore instance
                        //Here Maximum 2 external threads can access the code at the same time
                        _semaphore = new SemaphoreAsync(1, 1, sem_name);
                    }


                    //make sure the execution completes until it starts a new one
                    await _semaphore.WaitOne();
                    try
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Enter Event handler with semaphore fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);
                        //avoid handling the same event more than once by the means of memory cache
                        //check if the event has been handled previously to avoid duplicated handlers
                        //for that purpose we will use plain-text files
                        var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Resources",
                                    string.Format("FMECompleted-{0}-{1}.txt", env.Envelope.CountryCode, env.Envelope.VersionId));

                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Event handler with fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);
                        //if the file exists means that the event was handled and we ignore it
                        if (!File.Exists(fileName))
                        {

                            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Event handler file {0}", fileName), "EventHandler", "", _connectionString);
                            //if it doesn´t exist create a file
                            //await _semaphoreFME.WaitAsync();
                            StreamWriter sw = new(fileName, true, Encoding.ASCII);
                            await sw.WriteAsync(env.Envelope.JobId.ToString());
                            //close the file
                            sw.Close();
                            //_semaphoreFME.Release();
                            await Task.Run(() => FMEJobCompleted( env, cache));
                        }

                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Event handler END with fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, string.Format("Error Event handler {0}", ex.Message), "EventHandler", "", _connectionString);
                    }
                    finally
                    {
                        //release and reset the sempahore for the next execution
                        _semaphore.Release();
                        _semaphore.Dispose();
                    }

                };
                
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSite", "", _dataContext.Database.GetConnectionString());
            }
            finally
            {
                //TimeLog.setTimeStamp("Harvesting process ", "End");
            }
        }

        /*
        public static async Task ProcessFMEJobCompleted(string connectionstring, FMEJobEventArgs env)
        {
            //handle the event with a semaphore to ensure the same event is handled only one
            string _connectionString = env.DBConnection;
            //((BackgroundSpatialHarvestJobs)sender).GetDataContext().Database.GetConnectionString();
            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Enter Event handler with fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);

            SemaphoreAsync _semaphore;
            string sem_name = string.Format("semaphore_{0}_{1}", env.Envelope.CountryCode, env.Envelope.VersionId);
            try
            {
                //Try to Open the Semaphore if Exists, if not throw an exception
                _semaphore = SemaphoreAsync.OpenExisting(sem_name);
                //if it exists it means it is running, So we cancel it
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Cancelled event handler with fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);

            }
            catch
            {
                //If Semaphore not Exists, create a semaphore instance
                //Here Maximum 2 external threads can access the code at the same time
                _semaphore = new SemaphoreAsync(1, 1, sem_name);
            }


            //make sure the execution completes until it starts a new one
            await _semaphore.WaitOne();
            try
            {
                //avoid handling the same event more than once by the means of memory cache
                //check if the event has been handled previously to avoid duplicated handlers
                //for that purpose we will use plain-text files
                var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Resources",
                            string.Format("FMECompleted-{0}-{1}.txt", env.Envelope.CountryCode, env.Envelope.VersionId));

                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Event handler with fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);
                //if the file exists means that the event was handled and we ignore it
                if (!File.Exists(fileName))
                {

                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Event handler file {0}", fileName), "EventHandler", "", _connectionString);
                    //if it doesn´t exist create a file
                    //await _semaphoreFME.WaitAsync();
                    StreamWriter sw = new(fileName, true, Encoding.ASCII);
                    await sw.WriteAsync(env.Envelope.JobId.ToString());
                    //close the file
                    sw.Close();
                    //_semaphoreFME.Release();

                    //var cacheEntriesFieldCollectionDefinition = typeof(MemoryCache).GetField("_coherentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    //var cacheEntriesPropertyCollectionDefinition = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);


                    await Task.Run(() => FMEJobCompleted(env, _cache));
                }

                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Event handler END with fme job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "EventHandler", "", _connectionString);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, string.Format("Error Event handler {0}", ex.Message), "EventHandler", "", _connectionString);
            }
            finally
            {
                //release and reset the sempahore for the next execution
                _semaphore.Release();
                _semaphore.Dispose();
            }

        }
        */

        private async void FMEJobCompleted( FMEJobEventArgs env, IMemoryCache cache)
        {
            string _connectionString = "";
            
            try
            {
                await Task.Delay(10);
                //create a new DBContext to avoid concurrency errors
                //_dataContext = ((BackgroundSpatialHarvestJobs)sender).GetDataContext();
                _connectionString = env.DBConnection; // ((BackgroundSpatialHarvestJobs)sender).GetDataContext().Database.GetConnectionString();

                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("FMEJobCompleted {0} - {1}", env.Envelope.CountryCode, env.Envelope.VersionId), "FMEJobCompleted", "", _connectionString);

                var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_connectionString,
                    opt => opt.EnableRetryOnFailure()).Options;
                using (N2KBackboneContext ctx = new(options))
                {
                    ProcessedEnvelopes _procEnv = await ctx.Set<ProcessedEnvelopes>().Where(pe => pe.Country == env.Envelope.CountryCode && pe.Version == env.Envelope.VersionId).FirstOrDefaultAsync();
                    if (_procEnv != null)
                    {
                        //avoid processing the event twice
                        if (_procEnv.Status == HarvestingStatus.DataLoaded || _procEnv.Status == HarvestingStatus.SpatialDataLoaded)
                            return;

                        Console.WriteLine(String.Format("Harvest spatial {0}-{1} completed", env.Envelope.CountryCode, env.Envelope.VersionId));
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Harvest spatial {0}-{1} completed", env.Envelope.CountryCode, env.Envelope.VersionId), "FMEJobCompleted", "", _connectionString);

                        if (_procEnv.Status == HarvestingStatus.TabularDataLoaded)
                            _procEnv.Status = HarvestingStatus.DataLoaded;
                        else
                            //Spatial data loaded instead
                            _procEnv.Status = HarvestingStatus.SpatialDataLoaded;

                        // (DateTime) processedEnvelope.N2K_VersioningDate;
                        _procEnv.N2K_VersioningDate = new DateTime(_procEnv.N2K_VersioningDate.Year, _procEnv.N2K_VersioningDate.Month, _procEnv.N2K_VersioningDate.Day);
                        _procEnv.ImportDate = new DateTime(_procEnv.ImportDate.Year, _procEnv.ImportDate.Month, _procEnv.ImportDate.Day);

                        //await _semaphore.WaitAsync();
                        ctx.Set<ProcessedEnvelopes>().Update(_procEnv);
                        try
                        {
                            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("SaveChangesAsync job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "FME Job COmpleted", "", _connectionString);
                            await ctx.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex.Message, "FMEJobCompleted", "Error updating status ", _connectionString);

                        }
                        //_semaphore.Release();

                        //if the tabular data has been already harvested change the status to data loaded
                        //if dataloading is completed launch change detection tool


                        if (_procEnv.Status == HarvestingStatus.DataLoaded)
                        {
                            //When there is no previous envelopes to resolve for this country
                            List<ProcessedEnvelopes> envelopes = await ctx.Set<ProcessedEnvelopes>().AsNoTracking().Where(pe => (pe.Country == env.Envelope.CountryCode) && (pe.Status == HarvestingStatus.Harvested || pe.Status == HarvestingStatus.PreHarvested)).ToListAsync();

                            
                            if (envelopes.Count == 0 && env.FirstInCountry)
                            {
                                //change the status of the whole process to PreHarvested                    
                                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Prev ChangeStatus job {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "FME Job COmpleted", "", _connectionString);
                                await Task.Run(() =>
                                    ChangeStatus(
                                        GetCountryVersionToStatusFromSingleEnvelope(env.Envelope.CountryCode, env.Envelope.VersionId, HarvestingStatus.PreHarvested),
                                        cache, _connectionString)
                                );
                                //await DeleteUnrelatedChanges(ctx);
                            }
                        }

                        //remove the event from the cache as it is already finished and controlled accordingly
                        var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Resources",
                                                          string.Format("FMECompleted-{0}-{1}.txt", env.Envelope.CountryCode, env.Envelope.VersionId));
                        if (File.Exists(fileName)) File.Delete(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex.Message, "FMEJobCompleted ", "", _connectionString);
                Console.WriteLine("FME JOB completed with errors:" + ex.Message);
            }
            finally
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("FMEJobCompleted {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId), "HarvestedService - FME Job Completed", "", _connectionString);
                Console.WriteLine(String.Format("FMEJobCompleted {0}-{1}", env.Envelope.CountryCode, env.Envelope.VersionId));
            }
        }

        /// <summary>
        /// In order to execute the all steps of the process of the harvest from Versioning
        /// </summary>
        /// <returns>A list of the evelopes processed</returns>
        public async Task<List<HarvestedEnvelope>> FullHarvest(IMemoryCache cache)
        {
            try
            {
                //ask if there any harvesting process running (status of the envelope 4)
                //Call the method to know which envelopes are availables
                List<Harvesting> vEnvelopes = await GetPendingEnvelopes();
                //List<EnvelopesToProcess> envelopes = new();
                //List<ProcessedEnvelopes> pEnvelopes = new();

                List<HarvestedEnvelope> bbEnvelopes = new();
                List<EnvelopesToProcess> allEnvelopes = new();
                Dictionary<EnvelopesToProcess, List<Sites>> sitesPerEnvelope = new();
                Dictionary<EnvelopesToProcess, DateTime> startEnvelopes = new();

                if (vEnvelopes.Count > 0)
                {
                    //get the lists from master tables that will be used in all envelopes
                    List<HarvestedEnvelope> result = new();
                    _speciesTypes = await _dataContext.Set<SpeciesTypes>().AsNoTracking().ToListAsync();
                    _dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                    _ownerShipTypes = await _dataContext.Set<Models.backbone_db.OwnerShipTypes>().ToListAsync();

                    //save in memory the fixed codes like priority species and habitat codes
                    HarvestSiteCode siteCode = new(_dataContext, _versioningContext)
                    {
                        habitatPriority = await _dataContext.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync(),
                        speciesPriority = await _dataContext.Set<SpeciesPriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync()
                    };

                    foreach (Harvesting vEnvelope in vEnvelopes)
                    {
                        EnvelopesToProcess envelope = new()
                        {
                            VersionId = Int32.Parse(vEnvelope.Id.ToString()),
                            CountryCode = vEnvelope.Country,
                            SubmissionDate = vEnvelope.SubmissionDate
                        };
                        EnvelopesToProcess[] _tempEnvelope = new EnvelopesToProcess[] { envelope };

                        //Not necessary 
                        //await resetEnvirontment(envelope.CountryCode, envelope.VersionId);
                        DateTime SubmissionDate = envelope.SubmissionDate; //getOptimalDate(envelope);
                                                                           //create a new entry in the processed envelopes table to register that a new one is being harvested

                        ProcessedEnvelopes envelopeToProcess = new()
                        {
                            Country = envelope.CountryCode,
                            Version = envelope.VersionId,
                            ImportDate = envelope.SubmissionDate, //await GetSubmissionDate(envelope.CountryCode, envelope.VersionId)
                            Status = HarvestingStatus.Harvesting,
                            Importer = "AUTOIMPORT",
                            N2K_VersioningDate = SubmissionDate // envelope.SubmissionDate //await GetSubmissionDate(envelope.CountryCode, envelope.VersionId)
                        };

                        //harvest SiteCode-version to fill Sites table.
                        //Get the sites submitted in the envelope
                        List<NaturaSite> vSites = await _versioningContext.Set<NaturaSite>().Where(v => (v.COUNTRYCODE == envelope.CountryCode) && (v.COUNTRYVERSIONID == envelope.VersionId)).ToListAsync();

                        //save in memory the fixed codes like priority species and habitat codes
                        DateTime start1 = DateTime.Now;

                        //create a list with the existing version per site in the current country
                        //to avoid querying the db for every single site
                        List<SiteVersion> versionsPerSite = await _dataContext.Set<Sites>().AsNoTracking().Where(v => v.CountryCode == envelope.CountryCode).GroupBy(a => a.SiteCode)
                            .Select(g => new SiteVersion
                            {
                                SiteCode = g.Key,
                                MaxVersion = g.Max(x => x.Version)
                            }).ToListAsync();

                        //save to backbone database the site-versions
                        List<Sites> _tempSites = new();
                        foreach (NaturaSite vSite in vSites)
                        {
                            int versionNext = 0;
                            if (versionsPerSite.Any(s => s.SiteCode == vSite.SITECODE))
                            {
                                SiteVersion? _versionPerSite = versionsPerSite.FirstOrDefault(s => s.SiteCode == vSite.SITECODE);
                                versionNext = _versionPerSite.Value.MaxVersion + 1;
                            }
                            Sites? bbSite = await siteCode.harvestSiteCode(vSite, envelope, versionNext);
                            if (bbSite != null) _tempSites.Add(bbSite);
                        }
                        if (_tempSites.Count > 0)
                            sitesPerEnvelope.Add(envelope, _tempSites);
                        versionsPerSite.Clear();

                        //save all sitecode-version in bulk mode
                        if (sitesPerEnvelope.ContainsKey(envelope))
                        {
                            //Save thes status of the envelope in the DB
                            _dataContext.Set<ProcessedEnvelopes>().Add(envelopeToProcess);
                            await _dataContext.SaveChangesAsync();

                            Console.WriteLine(String.Format("Start envelope harvest {0} - {1}", envelope.CountryCode, envelope.VersionId));
                            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Start envelope harvest {0} - {1}", envelope.CountryCode, envelope.VersionId), "HarvestedService - _Harvest", "", _dataContext.Database.GetConnectionString());

                            await Sites.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), sitesPerEnvelope[envelope]);
                            allEnvelopes.Add(envelope);
                            startEnvelopes.Add(envelope, DateTime.Now);
                        }
                    }

                    //send FME to harvest all envelopes in sync mode
                    await HarvestSpatialData(allEnvelopes.ToArray(), cache);

                    //while tabular data of the sites is harvested
                    foreach (EnvelopesToProcess envelope in allEnvelopes)
                    {
                        //harvest the extended tabular data
                        HarvestedEnvelope bbEnvelope = await HarvestEnvelopeTabular(envelope, sitesPerEnvelope[envelope], startEnvelopes[envelope]);
                        await PriorityChecker(envelope.CountryCode, envelope.VersionId, false, null);

                        //Harvest proccess did its work successfully
                        if (bbEnvelope.Status == HarvestingStatus.DataLoaded)
                        {
                            //When there is no previous envelopes to resolve for this country
                            List<ProcessedEnvelopes> envelopes = await _dataContext.Set<ProcessedEnvelopes>().AsNoTracking().Where(pe => (pe.Country == envelope.CountryCode) && (pe.Status == HarvestingStatus.Harvested || pe.Status == HarvestingStatus.PreHarvested)).ToListAsync();

                            if (envelopes.Count == 0)
                            {
                                //change the status of the whole process to PreHarvested
                                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("FullHarvest {0}-{1}: Process Change 2", envelope.CountryCode,envelope.VersionId ), "FullHarvest - FME Job Completed", "", _dataContext.Database.GetConnectionString());
                                await ChangeStatus(
                                    GetCountryVersionToStatusFromSingleEnvelope(envelope.CountryCode, envelope.VersionId, HarvestingStatus.PreHarvested)
                                    , cache, this._dataContext.Database.GetConnectionString());
                                bbEnvelope.Status = HarvestingStatus.PreHarvested;
                            }
                            bbEnvelopes.Add(bbEnvelope);
                            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Full Harvest completed {0} - {1}", bbEnvelope.CountryCode, bbEnvelope.VersionId), "HarvestedService - FullHarvest", "", _dataContext.Database.GetConnectionString());
                        }
                        else
                            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Tabular Harvest completed {0} - {1}", bbEnvelope.CountryCode, bbEnvelope.VersionId), "HarvestedService - FullHarvest", "", _dataContext.Database.GetConnectionString());
                    }
                    await DeleteUnrelatedChanges(_dataContext);

                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, "Tabular Harvest completed ", "HarvestedService - FullHarvest", "", _dataContext.Database.GetConnectionString());

                    return bbEnvelopes;
                }
                else
                {
                    return new List<HarvestedEnvelope>();
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - FullHarvest", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            finally
            {

            }
        }

        private async Task DeleteUnrelatedChanges(N2KBackboneContext _dataContext)
        {
            try
            {
                await _dataContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.Changes WHERE ChangeId NOT IN (SELECT MAX(ChangeId) AS MaxRecordID FROM dbo.Changes GROUP BY SiteCode, Version, ChangeType, Code)");
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DeleteUnrelatedChanges", "", _dataContext.Database.GetConnectionString());
            }
        }

        private async Task<HarvestedEnvelope> HarvestEnvelopeTabular(EnvelopesToProcess envelope, List<Sites> bbSites, DateTime startEnvelope)
        {
            HarvestedEnvelope result = new();
            ProcessedEnvelopes processedEnv = null;

            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Start envelope tabular {0} - {1} {2}", envelope.CountryCode, envelope.VersionId, (DateTime.Now - startEnvelope).TotalSeconds), "HarvestedService - _Harvest", "", _dataContext.Database.GetConnectionString());
            Console.WriteLine(String.Format("Start envelope tabular {0} - {1} {2}", envelope.CountryCode, envelope.VersionId, (DateTime.Now - startEnvelope).TotalSeconds));

            var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_dataContext.Database.GetConnectionString(),
                opt => opt.EnableRetryOnFailure()).Options;
            using (N2KBackboneContext ctx = new(options))
            {
                //for each envelope to process
                try
                {
                    //add the envelope to the DB
                    HarvestSpecies species = new(_dataContext, _versioningContext);
                    await species.HarvestByCountry(envelope.CountryCode, envelope.VersionId, _speciesTypes, _versioningContext.Database.GetConnectionString(), _dataContext.Database.GetConnectionString(), bbSites);
                    //Console.WriteLine(String.Format("END species country {0}", (DateTime.Now - start1).TotalSeconds));

                    //Harvest habitats by country
                    HarvestHabitats habitats = new(_dataContext, _versioningContext);
                    await habitats.HarvestByCountry(envelope.CountryCode, envelope.VersionId, _versioningContext.Database.GetConnectionString(), _dataContext.Database.GetConnectionString(), _dataQualityTypes, bbSites);
                    //Console.WriteLine(String.Format("END habitats country {0}", (DateTime.Now - start1).TotalSeconds));

                    HarvestSiteCode sites = new(_dataContext, _versioningContext);
                    await sites.HarvestSite(envelope.CountryCode, envelope.VersionId, _versioningContext.Database.GetConnectionString(), _dataContext.Database.GetConnectionString(), _dataQualityTypes, _ownerShipTypes, bbSites);

                    //set the envelope as successfully completed 
                    //if the spatial harvesting is completed we can assign the envelope to DataLoaded
                    //TabluarDataLoaded instead
                    processedEnv = await ctx.Set<ProcessedEnvelopes>()
                        .Where(_env => _env.Country == envelope.CountryCode && _env.Version == envelope.VersionId)
                        .FirstOrDefaultAsync();

                    //if the tabular data has been already harvested change the status to data loaded
                    if (processedEnv.Status == HarvestingStatus.SpatialDataLoaded)
                    {
                        processedEnv.Status = HarvestingStatus.DataLoaded;
                    }
                    else
                        //Spatial data loaded instead
                        processedEnv.Status = HarvestingStatus.TabularDataLoaded;

                    processedEnv.N2K_VersioningDate = new DateTime(processedEnv.N2K_VersioningDate.Year, processedEnv.N2K_VersioningDate.Month, processedEnv.N2K_VersioningDate.Day);
                    processedEnv.ImportDate = new DateTime(processedEnv.ImportDate.Year, processedEnv.ImportDate.Month, processedEnv.ImportDate.Day);
                    ctx.Set<ProcessedEnvelopes>().Update(processedEnv);

                    result =
                        new HarvestedEnvelope
                        {
                            CountryCode = processedEnv.Country,
                            VersionId = processedEnv.Version,
                            NumChanges = 0,
                            Status = processedEnv.Status
                        };
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSite", "", _dataContext.Database.GetConnectionString());
                    if (processedEnv != null)
                    {
                        processedEnv.Status = HarvestingStatus.Error;
                        ctx.Set<ProcessedEnvelopes>().Update(processedEnv);
                    }
                    result =
                        new HarvestedEnvelope
                        {
                            CountryCode = envelope.CountryCode,
                            VersionId = envelope.VersionId,
                            NumChanges = 0,
                            Status = HarvestingStatus.Error //SiteChangeStatus.Error
                        };
                }
                finally
                {
                    //save the data of the site in backbone DB
                    await ctx.SaveChangesAsync();
                }
                _countrySpecies.Clear();
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("End envelope tabular {0} - {1} {2}", envelope.CountryCode, envelope.VersionId, (DateTime.Now - startEnvelope).TotalSeconds), "HarvestedService - _Harvest", "", _dataContext.Database.GetConnectionString());
                Console.WriteLine(String.Format("End envelope tabular {0} - {1} {2}", envelope.CountryCode, envelope.VersionId, (DateTime.Now - startEnvelope).TotalSeconds));
            }
            return result;
        }

        private HarvestingStatus getStatus(int pStatus)
        {
            return pStatus switch
            {
                0 => HarvestingStatus.Pending,
                1 => HarvestingStatus.Accepted,
                2 => HarvestingStatus.Rejected,
                3 => HarvestingStatus.Harvested,
                4 => HarvestingStatus.Harvesting,
                5 => HarvestingStatus.Queued,
                6 => HarvestingStatus.PreHarvested,
                7 => HarvestingStatus.Discarded,
                8 => HarvestingStatus.Closed,
                11 => HarvestingStatus.TabularDataLoaded,
                12 => HarvestingStatus.SpatialDataLoaded,
                _ => throw new Exception("No statuts definition found"),
            };
        }

        /// <summary>
        /// Set the new status from the current
        /// </summary>
        /// <param name="country"></param>
        /// <param name="version"></param>
        /// <param name="toStatus"></param>
        /// <returns></returns>
        public async Task<List<ProcessedEnvelopes>> ChangeStatus(CountryVersionToStatus changeEnvelopes, IMemoryCache cache, string dbConnString, bool recursive = false)
        {
            string sqlToExecute = "exec dbo.";
            string country = "";
            int version = 0;
            string _DBconnectionString = "";
            HarvestingStatus toStatus = changeEnvelopes.toStatus;
            try
            {
                _DBconnectionString = dbConnString;
                if (string.IsNullOrEmpty(dbConnString))
                    _DBconnectionString = _dataContext.Database.GetConnectionString();


                await SystemLog.WriteAsync(SystemLog.errorLevel.Info,"Change status ", "HarvestedService - _Harvest", "", _DBconnectionString);
                
                List<ProcessedEnvelopes> envelopeList = new();
                ProcessedEnvelopes? envelope = new();
                var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_DBconnectionString,
                    opt => opt.EnableRetryOnFailure()).Options;
                using (N2KBackboneContext ctx = new(options))
                {
                    foreach (CountryVersion data in changeEnvelopes.countryVersion)
                    {
                        country = data.CountryCode;
                        version = data.VersionId;
                        envelope = await ctx.Set<ProcessedEnvelopes>().Where(e => e.Country == country && e.Version == version).FirstOrDefaultAsync();

                        if (envelope != null)
                        {
                            //Get the version for the Sites 
                            //List<Sites> sites = ctx.Set<Sites>().Where(s => s.CountryCode == pCountry && s.N2KVersioningVersion == pVersion).Select(s=> s.Version).First();
                            //Sites site = sites.First();
                            //int _version = await ctx.Set<Sites>().Where(s => s.CountryCode == country && s.N2KVersioningVersion == version).Select(s => s.Version).FirstOrDefaultAsync();
                            if (toStatus != envelope.Status)
                            {
                                DataTable countriesAndVersions = new("sitecodesfilter");
                                countriesAndVersions.Columns.Add("CountryCode", typeof(string));
                                countriesAndVersions.Columns.Add("Version", typeof(int));
                                countriesAndVersions.Rows.Add(new Object[] { country, version });

                                SqlParameter param1 = new("@countryVersion", System.Data.SqlDbType.Structured)
                                {
                                    Value = countriesAndVersions,
                                    TypeName = "[dbo].[CountryVersion]"
                                };

                                await ctx.Database.ExecuteSqlRawAsync("exec dbo.setStatusToEnvelopeProcessing  @countryVersion;", param1);

                                //send message to front-end to make browser aware that envelope is processing
                                await _hubContext.Clients.All.SendAsync("ToProcessing", string.Format("{{\"CountryCode\":\"{0}\",\"VersionId\": {1}}}", data.CountryCode, data.VersionId));

                                if (envelope.Status == HarvestingStatus.DataLoaded)
                                {
                                    Task tabChangeDetectionTask = ChangeDetection(new EnvelopesToProcess[] { new() {
                                        CountryCode = country,
                                        VersionId = version
                                    } }, ctx);

                                    Task spatialChangeDetectionTask = ChangeDetectionSpatialData(new EnvelopesToProcess[] { new() {
                                        CountryCode = country,
                                        VersionId = version
                                    } }, ctx);

                                    //make sure they are all finished
                                    await Task.WhenAll(tabChangeDetectionTask, spatialChangeDetectionTask);
                                }

                                switch (toStatus)
                                {
                                    case HarvestingStatus.Harvested:
                                        sqlToExecute = "exec dbo.setStatusToEnvelopeHarvested  @countryVersion;";
                                        break;
                                    case HarvestingStatus.Discarded:
                                        sqlToExecute = "exec dbo.setStatusToEnvelopeDiscarded  @countryVersion;";
                                        break;
                                    case HarvestingStatus.PreHarvested:
                                        sqlToExecute = "exec dbo.setStatusToEnvelopePreHarvested  @countryVersion;";
                                        break;
                                    case HarvestingStatus.Closed:
                                        sqlToExecute = "exec dbo.setStatusToEnvelopeClosed  @countryVersion;";
                                        break;
                                    case HarvestingStatus.Pending:
                                        sqlToExecute = "exec dbo.setStatusToEnvelopePending  @countryVersion;";
                                        break;
                                    default:
                                        break;
                                }
                                await ctx.Database.ExecuteSqlRawAsync(sqlToExecute, param1);

                                if (toStatus == HarvestingStatus.Closed)
                                {
                                    SqlParameter paramCountry = new("@country", country);
                                    SqlParameter paramVersionId = new("@version", version);

                                    await _dataContext.Database.ExecuteSqlRawAsync(
                                            "exec spAcceptIdenticalSiteCodesBulk @country, @version",
                                            paramCountry,
                                            paramVersionId);
                                }

                                if (toStatus == HarvestingStatus.Discarded || toStatus == HarvestingStatus.Closed)
                                {
                                    ProcessedEnvelopes nextEnvelope = await ctx.Set<ProcessedEnvelopes>().AsNoTracking().Where(pe => (pe.Country == country) && (pe.Status == HarvestingStatus.DataLoaded)).OrderBy(pe => pe.Version).FirstOrDefaultAsync();
                                    if (nextEnvelope != null)
                                    {
                                        EnvelopesToProcess nextEnvelopeToChangeDetection = new()
                                        {
                                            VersionId = Int32.Parse(nextEnvelope.Version.ToString()),
                                            CountryCode = nextEnvelope.Country,
                                            SubmissionDate = DateTime.Now
                                        };
                                        EnvelopesToProcess[] _tempEnvelope = new EnvelopesToProcess[] { nextEnvelopeToChangeDetection };
                                        if (nextEnvelope.Status != HarvestingStatus.DataLoaded)
                                        {
                                            Task tabChangeDetectionTask = ChangeDetection(_tempEnvelope);
                                            Task spatialChangeDetectionTask = ChangeDetectionSpatialData(_tempEnvelope);
                                            //make sure they are all finished
                                            await Task.WhenAll(tabChangeDetectionTask, spatialChangeDetectionTask);
                                        }
                                        //change the status of the whole process to PreHarvested
                                        await ChangeStatus(
                                                GetCountryVersionToStatusFromSingleEnvelope(nextEnvelope.Country, nextEnvelope.Version, HarvestingStatus.PreHarvested),
                                                cache, _DBconnectionString,true);
                                    }
                                }

                                if (toStatus == HarvestingStatus.Harvested || toStatus == HarvestingStatus.Closed)
                                {
                                    //Remove country site changes cache
                                    if (cache != null)
                                    {
                                        MemoryCache? _cache = ((Microsoft.Extensions.Caching.Memory.MemoryCache)cache);
                                        if (_cache != null)
                                        {
                                            if (_cache.Count > 0)
                                            {
                                                foreach (var key in _cache.Keys)
                                                {
                                                    if (key.ToString().IndexOf(country) > -1)
                                                        cache.Remove(key);
                                                }
                                            }
                                        }
                                    }
                                }
                                envelope.Status = toStatus;
                                envelopeList.Add(envelope);
                            }
                            else
                            {
                                throw new Exception("Currently the package (" + country + " - " + version + ") has already the selected status.");
                            }
                        }
                        else
                        {
                            //Manual harvest?

                            PackageCountry package = _versioningContext.Set<PackageCountry>().Where(e => e.CountryCode == country && e.CountryVersionID == version).FirstOrDefault();

                            if (package != null)
                            {
                                EnvelopesToProcess newEnvelope = new()
                                {
                                    CountryCode = country,
                                    VersionId = version,
                                    SubmissionDate = (DateTime)package.Importdate
                                };

                                List<EnvelopesToProcess> envelopes = new()
                                {
                                    newEnvelope
                                };

                                await Harvest(envelopes.ToArray<EnvelopesToProcess>());
                            }
                            else
                            {
                                throw new Exception("The package doesn't exist on source database (" + country + " - " + version + ")");
                            }
                        }
                    }
                    if (!recursive) await DeleteUnrelatedChanges(ctx);
                }
                return envelopeList;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeStatus - Envelope " + country + "/" + version.ToString() + " - Status " + toStatus.ToString(), "", _DBconnectionString);
                return await Task.FromResult(new List<ProcessedEnvelopes>() { new() });
                //throw ex;
            }
            finally
            {
                //remove the file than controls if the FME Completed event has been handled
                var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Resources",
                                                  string.Format("FMECompleted-{0}-{1}.txt", country, version));
                if (File.Exists(fileName)) File.Delete(fileName);
            }
        }

        public const int SqlServerViolationOfUniqueIndex = 2601;
        public const int SqlServerViolationOfUniqueConstraint = 2627;

        /// <summary>
        /// Obtaints the date of the last sumbision for a Country and Version of evelope
        /// </summary>
        /// <param name="country"></param>
        /// <param name="version"></param>
        /// <returns>Date time</returns>
        private async Task<DateTime> GetSubmissionDate(string country, int version)
        {
            try
            {
                SqlParameter param1 = new("@country", country);
                SqlParameter param2 = new("@version", version);

                List<Harvesting> list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.GetSubmissionDateFromCountryAndVersionId  @country, @version",
                                param1, param2).ToListAsync();
                if (list.Count > 0)
                {
                    return list.ElementAt(0).SubmissionDate;
                }
                else
                    return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - GetSubmissionDate", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        private async Task<Sites> harvestSite(NaturaSite pVSite, EnvelopesToProcess pEnvelope)
        {
            //Tomamos el valor más alto que tiene en el campo Version para ese SiteCode. Por defecto es -1 para cuando no existe 
            //por que le vamos a sumar un 1 lo cual dejaría en 0
            Sites bbSite = new();
            int versionNext = 0;

            try
            {
                versionNext = await _dataContext.Set<Sites>().Where(s => s.SiteCode == pVSite.SITECODE).OrderBy(s => s.Version).Select(s => s.Version).FirstOrDefaultAsync();
                bbSite.SiteCode = pVSite.SITECODE;
                bbSite.Version = versionNext + 1;
                bbSite.Current = false;
                bbSite.Name = pVSite.SITENAME;
                if (pVSite.DATE_COMPILATION.HasValue)
                {
                    bbSite.CompilationDate = pVSite.DATE_COMPILATION;
                }
                if (pVSite.DATE_UPDATE.HasValue)
                {
                    bbSite.CompilationDate = pVSite.DATE_COMPILATION;
                }
                bbSite.CurrentStatus = SiteChangeStatus.DataLoaded;
                bbSite.SiteType = pVSite.SITETYPE;
                bbSite.AltitudeMin = pVSite.ALTITUDE_MIN;
                bbSite.AltitudeMax = pVSite.ALTITUDE_MAX;
                bbSite.Area = pVSite.AREAHA;
                bbSite.CountryCode = pEnvelope.CountryCode;
                bbSite.Length = pVSite.LENGTHKM;
                bbSite.N2KVersioningRef = Int32.Parse(pVSite.VERSIONID.ToString());
                bbSite.N2KVersioningVersion = pEnvelope.VersionId;
                return bbSite;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSite", "", _dataContext.Database.GetConnectionString());
                return null;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Remove the version we use in development
        /// </summary>
        /// <remarks> It works just in development environtment. In appsettings change the value of the kay "InDevelopment" to false to deactivate </remarks>
        /// <param name="pCountryCode">Code of two digits for the country</param>
        /// <param name="pCountryVersion">Number of the version</param>
        private async Task<int> resetEnvirontment(string pCountryCode, int pCountryVersion)
        {
            try
            {
                if (_appSettings.Value.InDevelopment)
                {
                    SqlParameter param1 = new("@country", pCountryCode);
                    SqlParameter param2 = new("@version", pCountryVersion);
                    await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spRemoveVersionFromDB  @country, @version", param1, param2);
                }
            }

            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex.Message, "HarvestedService - resetEnvirontment", "", _dataContext.Database.GetConnectionString());
            }
            return 1;
        }

        /// <summary>
        /// Method to delete all the changes create by the envelope
        /// </summary>
        /// <param name="pCountry"></param>
        /// <param name="pVerion"></param>
        private async Task rollback(string pCountry, int pVersion)
        {
            try
            {
                if (_ThereAreChanges)
                {
                    foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in _dataContext.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList())
                    {
                        switch (entry.State)
                        {
                            case EntityState.Modified:
                                entry.CurrentValues.SetValues(entry.OriginalValues);
                                entry.State = EntityState.Unchanged;
                                break;
                            case EntityState.Added:
                                entry.State = EntityState.Detached;
                                break;
                            case EntityState.Deleted:
                                entry.State = EntityState.Unchanged;
                                break;
                            default:
                                break;
                        }
                    }
                }
                List<Sites> toremove = await _dataContext.Set<Sites>().Where(s => s.CountryCode == pCountry && s.N2KVersioningVersion == pVersion).ToListAsync();
                _dataContext.Set<Sites>().RemoveRange(toremove);
                await _dataContext.SaveChangesAsync();
                _ThereAreChanges = false;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - rollback", "", _dataContext.Database.GetConnectionString());
            }
            finally
            {

            }
        }

        public async Task CompleteFMESpatial(string webSocketMsg)
        {
            try
            {

                var response_dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(webSocketMsg);
                EnvelopesToProcess env = new()
                {
                    CountryCode = response_dict["Country"].ToString(),
                    VersionId = System.Convert.ToInt32(response_dict["Version"].ToString()),
                    JobId = System.Convert.ToInt64(response_dict["JobId"].ToString())
                };
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Message received:{0}", webSocketMsg), "Web Socket received", "", _dataContext.Database.GetConnectionString());
                await _fmeHarvestJobs.CompleteTask(env);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "FME Job Completed", "", _dataContext.Database.GetConnectionString());
            }
        }

        /// <summary>
        /// Method to check the priority of the sites
        /// </summary>
        /// <param name="country">Country code</param>
        /// <param name="version">Country versionparam>
        /// <param name="current">Check to only take current sites</param>
        /// <returns>1</returns>
        public async Task<int> PriorityChecker(string country, int version, Boolean current, N2KBackboneContext? ctx = null)
        {
            try
            {
                if (ctx == null) ctx = this._dataContext;

                //Get the lists of priority habitats and species
                List<HabitatPriority> habitatPriority = await ctx.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
                List<SpeciesPriority> speciesPriority = await ctx.Set<SpeciesPriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

                try
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Start PriorityChecker - Country: {0} - Version: {1} - Current: {2}", country, version, current), "PriorityChecker", "", ctx.Database.GetConnectionString());

                    List<Sites> sites = new();

                    if (country.Length == 2 && version != -1 && current == true)
                    {
                        sites = await ctx.Set<Sites>().Where(ss => ss.CountryCode == country && ss.N2KVersioningVersion == version && ss.Current == current).ToListAsync();
                    }
                    else if (country.Length == 2 && version != -1)
                    {
                        sites = await ctx.Set<Sites>().Where(ss => ss.CountryCode == country && ss.N2KVersioningVersion == version).ToListAsync();
                    }
                    else if (country.Length == 2 && current == true)
                    {
                        sites = await ctx.Set<Sites>().Where(ss => ss.CountryCode == country && ss.Current == current).ToListAsync();
                    }
                    else if (country.Length == 2)
                    {
                        sites = await ctx.Set<Sites>().Where(ss => ss.CountryCode == country).ToListAsync();
                    }
                    else if (current == true)
                    {
                        sites = await ctx.Set<Sites>().Where(ss => ss.Current == current).ToListAsync();
                    }
                    else
                    {
                        sites = await ctx.Set<Sites>().ToListAsync();
                    }

                    DataTable sitecodesfilter = new("sitecodesfilter");
                    sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                    sitecodesfilter.Columns.Add("Version", typeof(int));

                    sites.ToList().ForEach(cs =>
                    {
                        sitecodesfilter.Rows.Add(new Object[] { cs.SiteCode, cs.Version });
                    });
                    SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                    {
                        Value = sitecodesfilter,
                        TypeName = "[dbo].[SiteCodeFilter]"
                    };

                    List<HabitatsToHarvestPerEnvelope>? habitatsEnvelope = await ctx.Set<HabitatsToHarvestPerEnvelope>().FromSqlRaw("exec spGetReferenceHabitatsBySiteCodes @siteCodes", paramTable).AsNoTracking().ToListAsync();
                    List<SpeciesToHarvestPerEnvelope>? speciesEnvelope = await ctx.Set<SpeciesToHarvestPerEnvelope>().FromSqlRaw("exec spGetReferenceSpeciesBySiteCodes @siteCodes", paramTable).AsNoTracking().ToListAsync();

                    foreach (Sites temp in sites)
                    {
                        List<HabitatsToHarvestPerEnvelope>? habitatsTemp = habitatsEnvelope.Where(t => t.SiteCode == temp.SiteCode && t.VersionId == temp.Version).ToList();
                        List<SpeciesToHarvestPerEnvelope>? speciesTemp = speciesEnvelope.Where(t => t.SiteCode == temp.SiteCode && t.VersionId == temp.Version).ToList();
                        List<HabitatToHarvest>? habitats = new();
                        List<SpeciesToHarvest>? species = new();

                        foreach (HabitatsToHarvestPerEnvelope hab in habitatsTemp)
                        {
                            HabitatToHarvest habitat = new()
                            {
                                HabitatCode = hab.HabitatCode,
                                VersionId = hab.VersionId,
                                RelSurface = hab.RelSurface,
                                Representativity = hab.Representativity,
                                Cover_ha = hab.Cover_ha,
                                PriorityForm = hab.PriorityForm
                            };
                            habitats.Add(habitat);
                        }

                        foreach (SpeciesToHarvestPerEnvelope spe in speciesTemp)
                        {
                            SpeciesToHarvest specie = new()
                            {
                                SpeciesCode = spe.SpeciesCode,
                                VersionId = spe.VersionId,
                                Population = spe.Population,
                                PopulationType = spe.PopulationType,
                                Motivation = spe.Motivation
                            };
                            species.Add(specie);
                        }

                        await SitePriorityChecker(temp.SiteCode, temp.Version, habitatPriority, speciesPriority, habitats, species);
                    }
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "PriorityChecker - Country: " + country + " - Version: " + version + " - Current: " + current, "", ctx.Database.GetConnectionString());
                    throw ex;

                }
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("End PriorityChecker - Country: {0} - Version: {1} - Current: {2}", country, version, current), "PriorityChecker", "", ctx.Database.GetConnectionString());

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - PriorityChecker - Country: " + country + " - Version: " + version + " - Current: " + current, "", ctx.Database.GetConnectionString());
                throw ex;
            }
        }

        /// <summary>
        /// Method to check the priority of the sites
        /// </summary>
        /// <param name="sitecode">Sitecode of the site to check priority</param>
        /// <param name="version">Version of the site to check priority</param>
        /// <param name="habitatPriority">List of priority Habitats</param>
        /// <param name="speciesPriority">List of priority Species</param>
        /// <returns>1</returns>
        public async Task<Boolean> SitePriorityChecker(string sitecode, int version, List<HabitatPriority>? habitatPriority = null, List<SpeciesPriority>? speciesPriority = null)
        {
            try
            {
                return await SitePriorityChecker(sitecode, version, habitatPriority, speciesPriority, null, null);
                /*
                var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_dataContext.Database.GetConnectionString(),
                    opt => opt.EnableRetryOnFailure()).Options;
                using (N2KBackboneContext ctx = new(options))
                {
                    //Get the lists of priority habitats and species
                    if (habitatPriority == null) habitatPriority = await ctx.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
                    if (speciesPriority == null) speciesPriority = await ctx.Set<SpeciesPriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

                    try
                    {
                        //These booleans declare whether or not each site is a priority
                        Boolean isSitePriority = false;

                        SqlParameter param1 = new SqlParameter("@site", sitecode);
                        SqlParameter param2 = new SqlParameter("@versionId", version);

                        //HabitatChecking
                        List<HabitatToHarvest> habitats = await ctx.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                        param1, param2).ToListAsync();

                        //SpeciesChecking
                        List<SpeciesToHarvest> species = await ctx.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                        param1, param2).ToListAsync();

                        //Priority check is also present in HarvestHabitat/ChangeDetectionHabitat
                        #region HabitatPriority
                        foreach (HabitatToHarvest habitat in habitats)
                        {
                            HabitatPriority priorityCount = habitatPriority.Where(s => s.HabitatCode == habitat.HabitatCode).FirstOrDefault();
                            if (priorityCount != null)
                            {
                                if (priorityCount.Priority == 2)
                                {
                                    if (((habitat.HabitatCode != "21A0" && habitat.PriorityForm == true)
                                        || (habitat.HabitatCode == "21A0" && sitecode.Substring(0, Math.Min(sitecode.Length, 2)) == "IE"))
                                             && (habitat.Representativity.ToUpper() != "D" || habitat.Representativity == null))
                                    {
                                        isSitePriority = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (habitat.Representativity.ToUpper() != "D" || habitat.Representativity == null)
                                    {
                                        isSitePriority = true;
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion

                        //Priority check is also present in HarvestSpecies/ChangeDetectionSpecies
                        #region SpeciesPriority
                        if (!isSitePriority)
                        {
                            foreach (SpeciesToHarvest specie in species)
                            {
                                SpeciesPriority priorityCount = speciesPriority.Where(s => s.SpecieCode == specie.SpeciesCode).FirstOrDefault();
                                if (priorityCount != null)
                                {
                                    if ((specie.Population.ToUpper() != "D" || specie.Population == null) && (specie.Motivation == null || specie.Motivation == ""))
                                    {
                                        isSitePriority = true;
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion

                        await ctx.Database.ExecuteSqlRawAsync("UPDATE [dbo].[Sites] SET [Priority] = '" + isSitePriority + "' WHERE [SiteCode] = '" + sitecode + "' AND [Version] = '" + version + "'");
                        return isSitePriority;
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SitePriorityChecker - Sitecode: " + sitecode + " - Version: " + version, "", ctx.Database.GetConnectionString());
                        throw ex;

                    }
                }
                */
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - SitePriorityChecker - Sitecode: " + sitecode + " - Version: " + version, "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        /// <summary>
        /// Method to check the priority of the sites
        /// </summary>
        /// <param name="sitecode">Sitecode of the site to check priority</param>
        /// <param name="version">Version of the site to check priority</param>
        /// <param name="habitatPriority">List of priority Habitats</param>
        /// <param name="speciesPriority">List of priority Species</param>
        /// <param name="habitats">List of species of the site</param>
        /// <param name="species">List of species of the site</param>
        /// <returns>1</returns>
        private async Task<Boolean> SitePriorityChecker(string sitecode, int version, List<HabitatPriority>? habitatPriority = null, List<SpeciesPriority>? speciesPriority = null, List<HabitatToHarvest>? habitats = null, List<SpeciesToHarvest>? species = null)
        {
            try
            {
                var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_dataContext.Database.GetConnectionString(),
                    opt => opt.EnableRetryOnFailure()).Options;
                using (N2KBackboneContext ctx = new(options))
                {
                    //Get the lists of priority habitats and species
                    if (habitatPriority == null) habitatPriority = await ctx.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
                    if (speciesPriority == null) speciesPriority = await ctx.Set<SpeciesPriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

                    try
                    {
                        //These booleans declare whether or not each site is a priority
                        Boolean isSitePriority = false;

                        SqlParameter param1 = new("@site", sitecode);
                        SqlParameter param2 = new("@versionId", version);

                        //HabitatChecking
                        if (habitats == null)
                        {
                            habitats = await ctx.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                        param1, param2).ToListAsync();
                        }

                        //SpeciesChecking
                        if (species == null)
                        {
                            species = await ctx.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                        param1, param2).ToListAsync();
                        }

                        //Priority check is also present in HarvestHabitat/ChangeDetectionHabitat
                        #region HabitatPriority
                        foreach (HabitatToHarvest habitat in habitats)
                        {
                            HabitatPriority priorityCount = habitatPriority.Where(s => s.HabitatCode == habitat.HabitatCode).FirstOrDefault();
                            if (priorityCount != null)
                            {
                                if (priorityCount.Priority == 2)
                                {
                                    if (((habitat.HabitatCode != "21A0" && habitat.PriorityForm == true)
                                        || (habitat.HabitatCode == "21A0" && sitecode[..Math.Min(sitecode.Length, 2)] == "IE"))
                                             && (habitat.Representativity.ToUpper() != "D" || habitat.Representativity == null || habitat.Representativity == "-"))
                                    {
                                        isSitePriority = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (habitat.Representativity.ToUpper() != "D" || habitat.Representativity == null || habitat.Representativity == "-")
                                    {
                                        isSitePriority = true;
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion

                        //Priority check is also present in HarvestSpecies/ChangeDetectionSpecies
                        #region SpeciesPriority
                        if (!isSitePriority)
                        {
                            foreach (SpeciesToHarvest specie in species)
                            {
                                SpeciesPriority priorityCount = speciesPriority.Where(s => s.SpecieCode == specie.SpeciesCode).FirstOrDefault();
                                if (priorityCount != null)
                                {
                                    if ((specie.Population.ToUpper() != "D" || specie.Population == null || specie.Population == "-") && (specie.Motivation == null || specie.Motivation == ""))
                                    {
                                        isSitePriority = true;
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion

                        await ctx.Database.ExecuteSqlRawAsync("UPDATE [dbo].[Sites] SET [Priority] = '" + isSitePriority + "' WHERE [SiteCode] = '" + sitecode + "' AND [Version] = '" + version + "'");
                        return isSitePriority;
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SitePriorityChecker - Sitecode: " + sitecode + " - Version: " + version, "", ctx.Database.GetConnectionString());
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - SitePriorityChecker - Sitecode: " + sitecode + " - Version: " + version, "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}
