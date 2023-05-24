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

namespace N2K_BackboneBackEnd.Services
{
    public class HarvestedService : IHarvestedService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private bool _ThereAreChanges = false;


        private IList<SpeciesTypes> _speciesTypes = new List<SpeciesTypes>();
        private IList<DataQualityTypes> _dataQualityTypes = new List<DataQualityTypes>();
        private IList<Models.backbone_db.OwnerShipTypes> _ownerShipTypes = new List<Models.backbone_db.OwnerShipTypes>();
        private IList<Models.backbone_db.SpecieBase> _countrySpecies = new List<Models.backbone_db.SpecieBase>();

        private IDictionary<Type, object> _siteItems = new Dictionary<Type, object>();
        private struct SiteVersion
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
        public HarvestedService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
            _appSettings = app;
            InitialiseBulkItems();
        }

        /// <summary>
        /// To define
        /// </summary>
        /// <returns></returns>
        public async Task<List<Harvesting>> GetHarvestedAsync()
        {
            var a = new List<Harvesting>();
            return await Task.FromResult(a);

        }

        /// <summary>
        /// To define
        /// </summary>
        /// <returns></returns>
        public List<Harvesting> GetHarvested()
        {
            var a = new List<Harvesting>();
            return a;

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
#pragma warning disable CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        public async Task<Harvesting> GetHarvestedAsyncById(int id)
#pragma warning restore CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
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
                SqlParameter param1 = new SqlParameter("@status", (int)status);

                List<HarvestingExpanded> result = await _dataContext.Set<HarvestingExpanded>().FromSqlRaw($"exec dbo.spGetEnvelopesByStatus  @status",
                                param1).AsNoTracking().ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - GetEnvelopesByStatus", "", _dataContext.Database.GetConnectionString());
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
                List<Harvesting> result = new List<Harvesting>();
                List<Countries> countries = await _dataContext.Set<Countries>().ToListAsync();
                List<ProcessedEnvelopes> processed = await _dataContext.Set<ProcessedEnvelopes>().FromSqlRaw($"select * from dbo.[vHighVersionProcessedEnvelopes]").AsNoTracking().ToListAsync();
                List<ProcessedEnvelopes> allEnvs = await _dataContext.Set<ProcessedEnvelopes>().AsNoTracking().ToListAsync();
                foreach (var procCountry in processed)
                {
                    SqlParameter param1 = new SqlParameter("@country", procCountry.Country);
                    SqlParameter param2 = new SqlParameter("@version", procCountry.Version);
                    SqlParameter param3 = new SqlParameter("@importdate", procCountry.ImportDate);

                    List<Harvesting> list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.spGetPendingCountryVersion  @country, @version,@importdate",
                                    param1, param2, param3).AsNoTracking().ToListAsync();
                    if (list.Count > 0)
                    {
                        foreach (Harvesting pendEnv in list)
                        {
                            if (!result.Contains(pendEnv))
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
        public async Task<HarvestingStatus> GetSiteChangeStatus(HarvestingStatus envelopeStatus)
        {
            try
            {
                await Task.Delay(1);
                return envelopeStatus == HarvestingStatus.Harvested ? HarvestingStatus.Pending : envelopeStatus;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - GetSiteChangeStatus", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        /// <summary>
        /// Method to validate the quality and the main rules of the data harvested
        /// </summary>
        /// <param name="envelopeIDs">List of the envelops to process</param>
        /// <returns>A list of the envelops with the result of the process</returns>
        public async Task<List<HarvestedEnvelope>> ChangeDetection(EnvelopesToProcess[] envelopeIDs)
        {
            try
            {
                List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
                List<SiteChangeDb> changes = new List<SiteChangeDb>();
                //var latestVersions = await _dataContext.Set<ProcessedEnvelopes>().ToListAsync();
                //await _dataContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.Changes");

                //Get the lists of priority habitats and species
                List<HabitatPriority> habitatPriority = await _dataContext.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
                List<SpeciePriority> speciesPriority = await _dataContext.Set<SpeciePriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

                //from the view vLatest//processedEnvelopes (backbonedb) load the sites with the latest versionid of the countries

                //Load all sites with the CountryVersionID-CountryCode from Versioning
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    try
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Start ChangeDetection {0} - {1}", envelope.CountryCode, envelope.VersionId), "ChangeDetection", "", _dataContext.Database.GetConnectionString());

                        SqlParameter param1 = new SqlParameter("@country", envelope.CountryCode);
                        SqlParameter param2 = new SqlParameter("@version", envelope.VersionId);
                        SqlParameter param3 = new SqlParameter("@last_envelop", envelope.VersionId);

                        //Get the changes status from ProcessedEnvelopes
                        List<ProcessedEnvelopes> processedEnvelopes = await _dataContext.Set<ProcessedEnvelopes>().FromSqlRaw($"exec dbo.spGetProcessedEnvelopesByCountryAndVersion  @country, @version",
                                        param1, param2).ToListAsync();
                        ProcessedEnvelopes? processedEnvelope = processedEnvelopes.FirstOrDefault();

                        List<RelatedSites>? sitesRelation = await _dataContext.Set<RelatedSites>().FromSqlRaw($"exec dbo.spGetSitesToDetectChanges  @last_envelop, @country",
                                        param3, param1).ToListAsync();
                        var previoussitecodesfilter = new DataTable("sitecodesfilter");
                        previoussitecodesfilter.Columns.Add("SiteCode", typeof(string));
                        previoussitecodesfilter.Columns.Add("Version", typeof(int));
                        var newsitecodesfilter = new DataTable("sitecodesfilter");
                        newsitecodesfilter.Columns.Add("SiteCode", typeof(string));
                        newsitecodesfilter.Columns.Add("Version", typeof(int));

                        foreach (var sc in sitesRelation)
                        {
                            if (sc.PreviousSiteCode != null && sc.PreviousVersion != null)
                                previoussitecodesfilter.Rows.Add(new Object[] { sc.PreviousSiteCode, sc.PreviousVersion });
                            if (sc.NewSiteCode != null && sc.NewVersion != null)
                                newsitecodesfilter.Rows.Add(new Object[] { sc.NewSiteCode, sc.NewVersion });
                        }

                        SqlParameter param4 = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                        param4.Value = previoussitecodesfilter;
                        param4.TypeName = "[dbo].[SiteCodeFilter]";
                        List<SiteToHarvest>? previoussites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetSitesBySiteCodeFilter  @siteCodes",
                                        param4).ToListAsync();
                        param4.Value = newsitecodesfilter;
                        List<SiteToHarvest>? newsites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetSitesBySiteCodeFilter  @siteCodes",
                                        param4).ToListAsync();

                        //For each site in Versioning compare it with that site in backboneDB
                        foreach (SiteToHarvest? harvestingSite in newsites)
                        {
                            changes = await SiteChangeDetection(changes, previoussites, harvestingSite, envelope, habitatPriority, speciesPriority, processedEnvelope, sitesRelation);
                        }

                        //For each site in backboneDB check if the site still exists in Versioning
                        foreach (SiteToHarvest? storedSite in previoussites)
                        {
                            RelatedSites? siteRelation = sitesRelation.Where(s => s.PreviousSiteCode == storedSite.SiteCode && s.PreviousVersion == storedSite.VersionId).FirstOrDefault();
                            SiteToHarvest? harvestingSite = null;
                            if (siteRelation != null)
                                harvestingSite = newsites.Where(s => s.SiteCode == siteRelation.NewSiteCode && s.VersionId == siteRelation.NewVersion).FirstOrDefault();
                            if (siteRelation != null && harvestingSite == null)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = storedSite.SiteCode;
                                siteChange.Version = storedSite.VersionId;
                                siteChange.ChangeCategory = "Network general structure";
                                siteChange.ChangeType = "Site Deleted";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Critical;
                                siteChange.Status = (SiteChangeStatus?)await GetSiteChangeStatus(processedEnvelope.Status);
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = null;
                                siteChange.OldValue = storedSite.SiteCode;
                                siteChange.Code = storedSite.SiteCode;
                                siteChange.Section = "Site";
                                siteChange.VersionReferenceId = storedSite.VersionId;
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                        }

                        result.Add(new HarvestedEnvelope
                        {
                            CountryCode = envelope.CountryCode,
                            VersionId = envelope.VersionId,
                            NumChanges = changes.Count,
                            Status = SiteChangeStatus.PreHarvested
                        });

                        try
                        {
                            await SiteChangeDb.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), changes);
                        }
                        catch (Exception ex)
                        {
                            await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetection - SaveBulkRecord", "", _dataContext.Database.GetConnectionString());
                            throw ex;
                        }

                        await _dataContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.Changes WHERE ChangeId NOT IN (SELECT MAX(ChangeId) AS MaxRecordID FROM dbo.Changes GROUP BY SiteCode, Version, ChangeType, Code)");
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetection - Envelope " + envelope.CountryCode + "/" + envelope.VersionId.ToString(), "", _dataContext.Database.GetConnectionString());
                        throw ex;

                    }
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("End ChangeDetection {0} - {1}", envelope.CountryCode, envelope.VersionId), "ChangeDetection", "", _dataContext.Database.GetConnectionString());
                }

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetection", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }


        public async Task<List<HarvestedEnvelope>> ChangeDetectionSpatialData(EnvelopesToProcess[] envelopeIDs)
        {
            try
            {
                List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();

                //for each envelope to process
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    HttpClient client = new HttpClient();
                    String serverUrl = String.Format(_appSettings.Value.fme_service_spatialchanges, envelope.VersionId, envelope.CountryCode, _appSettings.Value.fme_security_token);
                    try
                    {
                        //TimeLog.setTimeStamp("Geospatial changes for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString(), "Starting");
                        client.Timeout = TimeSpan.FromHours(5);
                        Task<HttpResponseMessage> response = client.GetAsync(serverUrl);
                        string content = await response.Result.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "Geospatial changes", "", _dataContext.Database.GetConnectionString());
                    }
                    finally
                    {
                        client.Dispose();
                        //TimeLog.setTimeStamp("Geospatial changes for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString().ToString(), "End");
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetectionSpatialData", "", _dataContext.Database.GetConnectionString());
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

            HttpClient client = new HttpClient();
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


        public async Task<List<HarvestedEnvelope>> ChangeDetectionSingleSite(string siteCode, int versionId)
        {
            try
            {
                SqlParameter param1 = new SqlParameter("@sitecode", siteCode);
                SqlParameter param2 = new SqlParameter("@version", versionId);

                List<SiteToHarvest>? sitesVersioning = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSitesBySitecodeAndVersion  @sitecode, @version",
                                        param1, param2).ToListAsync();

                SiteToHarvest? harvestingSite = sitesVersioning.FirstOrDefault();

                List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
                result = await ChangeDetectionSingleSiteObject(harvestingSite);
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetectionSingleSite", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<HarvestedEnvelope>> ChangeDetectionSingleSiteObject(SiteToHarvest harvestingSite)
        {
            EnvelopesToProcess envelope = new EnvelopesToProcess();
            envelope.CountryCode = harvestingSite.CountryCode;
            envelope.VersionId = (int)harvestingSite.N2KVersioningVersion;

            SqlParameter countryParam1 = new SqlParameter("@country", envelope.CountryCode);
            SqlParameter countryParam2 = new SqlParameter("@version", envelope.VersionId);
            SqlParameter countryParam3 = new SqlParameter("@last_envelop", envelope.VersionId);

            List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
            List<SiteChangeDb> changes = new List<SiteChangeDb>();

            try
            {
                //Get the changes status from ProcessedEnvelopes
                List<ProcessedEnvelopes> processedEnvelopes = await _dataContext.Set<ProcessedEnvelopes>().FromSqlRaw($"exec dbo.spGetProcessedEnvelopesByCountryAndVersion  @country, @version",
                            countryParam1, countryParam2).AsNoTracking().ToListAsync();
                ProcessedEnvelopes? processedEnvelope = processedEnvelopes.FirstOrDefault();

                List<RelatedSites>? sitesRelation = await _dataContext.Set<RelatedSites>().FromSqlRaw($"exec dbo.spGetSitesToDetectChanges  @last_envelop, @country",
                                        countryParam3, countryParam1).ToListAsync();

                //Get the lists of priority habitats and species
                List<HabitatPriority> habitatPriority = await _dataContext.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
                List<SpeciePriority> speciesPriority = await _dataContext.Set<SpeciePriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

                RelatedSites siteRelation = sitesRelation.Where(s => s.NewSiteCode == harvestingSite.SiteCode && s.NewVersion == harvestingSite.VersionId).FirstOrDefault();

                SqlParameter param1 = new SqlParameter("@sitecode", siteRelation.PreviousSiteCode);
                SqlParameter param2 = new SqlParameter("@version", siteRelation.PreviousVersion);

                List<SiteToHarvest>? referencedSites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSitesBySitecodeAndVersion  @sitecode, @version",
                                    param1, param2).ToListAsync();

                changes = await SiteChangeDetection(changes, referencedSites, harvestingSite, envelope, habitatPriority, speciesPriority, processedEnvelope, sitesRelation, true);
                processedEnvelope.Status = HarvestingStatus.Harvested;
                result.Add(new HarvestedEnvelope
                {
                    CountryCode = envelope.CountryCode,
                    VersionId = envelope.VersionId,
                    NumChanges = changes.Count,
                    Status = SiteChangeStatus.Harvested
                });

                //for the time being do not load the changes and keep using test_table 

                try
                {
                    await SiteChangeDb.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), changes);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetectionSingleSiteObject - SaveBulkRecord", "", _dataContext.Database.GetConnectionString());
                }

                await _dataContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.Changes WHERE ChangeId NOT IN (SELECT MAX(ChangeId) AS MaxRecordID FROM dbo.Changes GROUP BY SiteCode, Version, ChangeType, Code)");
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeDetectionSingleSiteObject - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "", _dataContext.Database.GetConnectionString());
            }
            return result;
        }

        public async Task<List<SiteChangeDb>> SiteChangeDetection(List<SiteChangeDb> changes, List<SiteToHarvest> referencedSites, SiteToHarvest harvestingSite, EnvelopesToProcess envelope, List<HabitatPriority> habitatPriority, List<SpeciePriority> speciesPriority, ProcessedEnvelopes? processedEnvelope, List<RelatedSites>? sitesRelation, bool manualEdition = false)
        {
            //Tolerance values. If the difference between reference and versioning values is bigger than these numbers, then they are notified.
            //If the tolerance is at 0, then it registers ALL changes, no matter how small they are.
            double siteAreaHaTolerance = 0.0;
            double siteLengthKmTolerance = 0.0;
            double habitatCoverHaTolerance = 0.0;

            try
            {
                processedEnvelope.Status = await GetSiteChangeStatus(processedEnvelope.Status);
                RelatedSites? siteRelation = sitesRelation.Where(s => s.NewSiteCode == harvestingSite.SiteCode && s.NewVersion == harvestingSite.VersionId).FirstOrDefault();
                SiteToHarvest? storedSite = null;
                if (siteRelation != null)
                    storedSite = referencedSites.Where(s => s.SiteCode == siteRelation.PreviousSiteCode && s.VersionId == siteRelation.PreviousVersion).FirstOrDefault();
                if (siteRelation != null && storedSite != null)
                {
                    //These booleans declare whether or not each site is a priority
                    Boolean isStoredSitePriority = false;
                    Boolean isHarvestingSitePriority = false;

                    //SiteAttributesChecking
                    HarvestSiteCode siteCode = new HarvestSiteCode(_dataContext, _versioningContext);
                    changes = await siteCode.ChangeDetectionSiteAttributes(changes, envelope, harvestingSite, storedSite, siteAreaHaTolerance, siteLengthKmTolerance, processedEnvelope);

                    SqlParameter param3 = new SqlParameter("@site", harvestingSite.SiteCode);
                    int maxVersionSite = harvestingSite.VersionId;
                    SqlParameter param4 = new SqlParameter("@versionId", maxVersionSite);
                    int previousVersionSite = storedSite.VersionId;
                    SqlParameter param5 = new SqlParameter("@versionId", previousVersionSite);

                    //BioRegionChecking
                    List<BioRegions> bioRegionsVersioning = await _dataContext.Set<BioRegions>().FromSqlRaw($"exec dbo.spGetReferenceBioRegionsBySiteCodeAndVersion  @site, @versionId",
                                    param3, param4).ToListAsync();
                    List<BioRegions> referencedBioRegions = await _dataContext.Set<BioRegions>().FromSqlRaw($"exec dbo.spGetReferenceBioRegionsBySiteCodeAndVersion  @site, @versionId",
                                    param3, param5).ToListAsync();
                    changes = await siteCode.ChangeDetectionBioRegions(bioRegionsVersioning, referencedBioRegions, changes, envelope, harvestingSite, storedSite, param3, param4, param5, processedEnvelope);

                    //HabitatChecking
                    List<HabitatToHarvest> habitatVersioning = await _dataContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                    param3, param4).ToListAsync();
                    List<HabitatToHarvest> referencedHabitats = await _dataContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                    param3, param5).ToListAsync();
                    HarvestHabitats habitats = new HarvestHabitats(_dataContext, _versioningContext);
                    changes = await habitats.ChangeDetectionHabitat(habitatVersioning, referencedHabitats, changes, envelope, harvestingSite, storedSite, param3, param4, param5, habitatCoverHaTolerance, habitatPriority, processedEnvelope);

                    //SpeciesChecking
                    List<SpeciesToHarvest> speciesVersioning = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                    param3, param4).ToListAsync();
                    List<SpeciesToHarvest> referencedSpecies = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                    param3, param5).ToListAsync();
                    HarvestSpecies species = new HarvestSpecies(_dataContext, _versioningContext);
                    changes = await species.ChangeDetectionSpecies(speciesVersioning, referencedSpecies, changes, envelope, harvestingSite, storedSite, param3, param4, param5, speciesPriority, processedEnvelope);

                    #region HabitatPriority
                    foreach (HabitatToHarvest harvestingHabitat in habitatVersioning)
                    {
                        HabitatPriority priorityCount = habitatPriority.Where(s => s.HabitatCode == harvestingHabitat.HabitatCode).FirstOrDefault();
                        if (priorityCount != null)
                        {
                            if (priorityCount.Priority == 2)
                            {
                                if (harvestingHabitat.Representativity.ToUpper() != "D" && harvestingHabitat.PriorityForm == true)
                                {
                                    isHarvestingSitePriority = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (harvestingHabitat.Representativity.ToUpper() != "D")
                                {
                                    isHarvestingSitePriority = true;
                                    break;
                                }
                            }
                        }
                    }
                    foreach (HabitatToHarvest storedHabitat in referencedHabitats)
                    {
                        HabitatPriority priorityCount = habitatPriority.Where(s => s.HabitatCode == storedHabitat.HabitatCode).FirstOrDefault();
                        if (priorityCount != null)
                        {
                            if (priorityCount.Priority == 2)
                            {
                                if (storedHabitat.Representativity.ToUpper() != "D" && storedHabitat.PriorityForm == true)
                                {
                                    isStoredSitePriority = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (storedHabitat.Representativity.ToUpper() != "D")
                                {
                                    isStoredSitePriority = true;
                                    break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region SpeciesPriority
                    foreach (SpeciesToHarvest harvestingSpecies in speciesVersioning)
                    {
                        SpeciePriority priorityCount = speciesPriority.Where(s => s.SpecieCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
                        if (priorityCount != null)
                        {
                            if (harvestingSpecies.Population.ToUpper() != "D")
                            {
                                isHarvestingSitePriority = true;
                                break;
                            }
                        }
                    }
                    foreach (SpeciesToHarvest storedSpecies in referencedSpecies)
                    {
                        SpeciePriority priorityCount = speciesPriority.Where(s => s.SpecieCode == storedSpecies.SpeciesCode).FirstOrDefault();
                        if (priorityCount != null)
                        {
                            if (storedSpecies.Population.ToUpper() != "D")
                            {
                                isStoredSitePriority = true;
                                break;
                            }
                        }
                    }
                    #endregion

                    if (isStoredSitePriority && !isHarvestingSitePriority)
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Site General Info";
                        siteChange.ChangeType = "Site Losing Priority";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Critical;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = Convert.ToString(isHarvestingSitePriority);
                        siteChange.OldValue = Convert.ToString(isStoredSitePriority);
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "Priority";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
                        changes.Add(siteChange);
                    }
                    else if (!isStoredSitePriority && isHarvestingSitePriority)
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Site General Info";
                        siteChange.ChangeType = "Site Getting Priority";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Info;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = Convert.ToString(isHarvestingSitePriority);
                        siteChange.OldValue = Convert.ToString(isStoredSitePriority);
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "Priority";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
                        changes.Add(siteChange);
                    }

                    Sites stored = _dataContext.Set<Sites>().Where(ss => ss.SiteCode == storedSite.SiteCode && ss.Version == storedSite.VersionId).FirstOrDefault();
                    Sites harvesting = _dataContext.Set<Sites>().Where(hs => hs.SiteCode == harvestingSite.SiteCode && hs.Version == harvestingSite.VersionId).FirstOrDefault();
                    stored.Priority = isStoredSitePriority;
                    harvesting.Priority = isHarvestingSitePriority;
                    _dataContext.Set<Sites>().Update(stored);
                    _dataContext.Set<Sites>().Update(harvesting);
                    await _dataContext.SaveChangesAsync();

                    //Add justification files and comments from the current to the new version
                    Sites current = _dataContext.Set<Sites>().Single(x => x.SiteCode == harvestingSite.SiteCode && x.Current == true);
                    if (current != null)
                    {
                        SqlParameter paramSitecode = new SqlParameter("@sitecode", harvestingSite.SiteCode);
                        SqlParameter paramOldVersion = new SqlParameter("@oldVersion", current.Version);
                        SqlParameter paramNewVersion = new SqlParameter("@newVersion", harvestingSite.VersionId);
                        await _dataContext.Database.ExecuteSqlRawAsync($"exec dbo.spCopyJustificationFilesAndStatusChanges  @sitecode, @oldVersion, @newVersion",
                                paramSitecode, paramOldVersion, paramNewVersion);
                    }

                }
                else
                {
                    SiteChangeDb siteChange = new SiteChangeDb();
                    siteChange.SiteCode = harvestingSite.SiteCode;
                    siteChange.Version = harvestingSite.VersionId;
                    siteChange.ChangeCategory = "Network general structure";
                    siteChange.ChangeType = "Site Added";
                    siteChange.Country = envelope.CountryCode;
                    siteChange.Level = Enumerations.Level.Info;
                    siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                    siteChange.Tags = string.Empty;
                    siteChange.NewValue = harvestingSite.SiteCode;
                    siteChange.OldValue = null;
                    siteChange.Code = harvestingSite.SiteCode;
                    siteChange.Section = "Site";
                    siteChange.VersionReferenceId = harvestingSite.VersionId;
                    siteChange.ReferenceSiteCode = harvestingSite.SiteCode;
                    siteChange.N2KVersioningVersion = envelope.VersionId;
                    changes.Add(siteChange);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteChangeDetection - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "", _dataContext.Database.GetConnectionString());
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
                List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
                _speciesTypes = await _dataContext.Set<SpeciesTypes>().AsNoTracking().ToListAsync();
                _dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                _ownerShipTypes = await _dataContext.Set<Models.backbone_db.OwnerShipTypes>().ToListAsync();

                //save in memory the fixed codes like priority species and habitat codes
                HarvestSiteCode siteCode = new HarvestSiteCode(_dataContext, _versioningContext);
                siteCode.habitatPriority = await _dataContext.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
                siteCode.speciesPriority = await _dataContext.Set<SpeciePriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

                //for each envelope to process
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    ClearBulkItems();
                    Console.WriteLine(String.Format("Start envelope harvest {0} - {1}", envelope.CountryCode, envelope.VersionId));
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Start envelope harvest {0} - {1}", envelope.CountryCode, envelope.VersionId), "HarvestedService - _Harvest", "", _dataContext.Database.GetConnectionString());
                    var startEnvelope = DateTime.Now;
                    //Not necessary 
                    //await resetEnvirontment(envelope.CountryCode, envelope.VersionId);
                    DateTime SubmissionDate = envelope.SubmissionDate; //getOptimalDate(envelope);
                                                                       //create a new entry in the processed envelopes table to register that a new one is being harvested
                    ProcessedEnvelopes envelopeToProcess = new ProcessedEnvelopes
                    {
                        Country = envelope.CountryCode
                        ,
                        Version = envelope.VersionId
                        ,
                        ImportDate = envelope.SubmissionDate //await GetSubmissionDate(envelope.CountryCode, envelope.VersionId)
                        ,
                        Status = HarvestingStatus.Harvesting
                        ,
                        Importer = "AUTOIMPORT"
                        ,
                        N2K_VersioningDate = SubmissionDate // envelope.SubmissionDate //await GetSubmissionDate(envelope.CountryCode, envelope.VersionId)
                    };
                    try
                    {
                        //add the envelope to the DB
                        _dataContext.Set<ProcessedEnvelopes>().Add(envelopeToProcess);
                        _dataContext.SaveChanges();

                        //Get the sites submitted in the envelope
                        List<NaturaSite> vSites = _versioningContext.Set<NaturaSite>().Where(v => (v.COUNTRYCODE == envelope.CountryCode) && (v.COUNTRYVERSIONID == envelope.VersionId)).ToList();

                        //save in memory the fixed codes like priority species and habitat codes
                        DateTime start1 = DateTime.Now;
                        List<Sites> bbSites = new List<Sites>();

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

                        HarvestSpecies species = new HarvestSpecies(_dataContext, _versioningContext);
                        await species.HarvestByCountry(envelope.CountryCode, envelope.VersionId, _speciesTypes, _versioningContext.Database.GetConnectionString(), _dataContext.Database.GetConnectionString(), bbSites);
                        //Console.WriteLine(String.Format("END species country {0}", (DateTime.Now - start1).TotalSeconds));

                        //Harvest habitats by country
                        HarvestHabitats habitats = new HarvestHabitats(_dataContext, _versioningContext);
                        await habitats.HarvestByCountry(envelope.CountryCode, envelope.VersionId, _versioningContext.Database.GetConnectionString(), _dataContext.Database.GetConnectionString(), _dataQualityTypes, bbSites);
                        //Console.WriteLine(String.Format("END habitats country {0}", (DateTime.Now - start1).TotalSeconds));

                        HarvestSiteCode sites = new HarvestSiteCode(_dataContext, _versioningContext);
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
                                Status = SiteChangeStatus.DataLoaded
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
                                Status = SiteChangeStatus.Error //SiteChangeStatus.Error
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
                catch { }
            }
            return returnDate;
        }


        private async Task<List<HarvestedEnvelope>> HarvestSpatialData(EnvelopesToProcess[] envelopeIDs)
        {
            try
            {
                List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();

                //for each envelope to process
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    HttpClient client = new HttpClient();
                    String serverUrl = String.Format(_appSettings.Value.fme_service_spatialload, envelope.VersionId, envelope.CountryCode, _appSettings.Value.fme_security_token);
                    try
                    {
                        //TimeLog.setTimeStamp("Geodata for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString(), "Starting");
                        client.Timeout = TimeSpan.FromHours(5);
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, "Start harvest spatial", "HarvestSpatialData", "", _dataContext.Database.GetConnectionString());
                        Task<HttpResponseMessage> response = client.GetAsync(serverUrl);
                        var response1 = client.GetAsync(serverUrl);
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, String.Format("Launched {0}", serverUrl), "HarvestSpatialData", "", _dataContext.Database.GetConnectionString());
                        string content = await response.Result.Content.ReadAsStringAsync();
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Info, "Harvest spatial completed", "HarvestSpatialData", "", _dataContext.Database.GetConnectionString());
                        /*
                        result.Add(
                            new HarvestedEnvelope
                            {
                                CountryCode = envelope.CountryCode,
                                VersionId = envelope.VersionId,
                                NumChanges = 0,
                                Status = SiteChangeStatus.Harvested
                            }
                         );
                        */
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestGeodata", "", _dataContext.Database.GetConnectionString());
                        /*
                        result.Add(
                            new HarvestedEnvelope
                            {
                                CountryCode = envelope.CountryCode,
                                VersionId = envelope.VersionId,
                                NumChanges = 0,
                                Status = SiteChangeStatus.Rejected
                            }
                         );
                        */
                    }
                    finally
                    {
                        client.Dispose();
                        client = null;
                        //TimeLog.setTimeStamp("Geodata for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString().ToString(), "End");
                    }

                }
                return result;
            }

            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - HarvestSpatialData", "", _dataContext.Database.GetConnectionString());
                return new List<HarvestedEnvelope>();
            }
            finally
            {
                //TimeLog.setTimeStamp("Harvesting process ", "End");
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
                //List<EnvelopesToProcess> envelopes = new List<EnvelopesToProcess>();
                //List<ProcessedEnvelopes> pEnvelopes = new List<ProcessedEnvelopes>();

                List<HarvestedEnvelope> bbEnvelopes = new List<HarvestedEnvelope>();
                if (vEnvelopes.Count > 0)
                {
                    foreach (Harvesting vEnvelope in vEnvelopes)
                    {
                        EnvelopesToProcess envelope = new EnvelopesToProcess
                        {
                            VersionId = Int32.Parse(vEnvelope.Id.ToString()),
                            CountryCode = vEnvelope.Country,
                            SubmissionDate = vEnvelope.SubmissionDate
                        };
                        EnvelopesToProcess[] _tempEnvelope = new EnvelopesToProcess[] { envelope };
                        List<HarvestedEnvelope> bbEnvelope = await Harvest(_tempEnvelope);
                        List<HarvestedEnvelope> bbGeoData = await HarvestSpatialData(_tempEnvelope);

                        List<ProcessedEnvelopes> envelopes = await _dataContext.Set<ProcessedEnvelopes>().AsNoTracking().Where(pe => (pe.Country == _tempEnvelope[0].CountryCode) && (pe.Status == HarvestingStatus.Harvested || pe.Status == HarvestingStatus.PreHarvested)).ToListAsync();

                        //Harvest proccess did its work successfully
                        if (bbEnvelope.Count > 0)
                        {
                            //When there is no previous envelopes to resolve for this country
                            if (envelopes.Count == 0)
                            {

                                //Task tabChangeDetectionTask = ChangeDetection(_tempEnvelope);
                                //Task spatialChangeDetectionTask = ChangeDetectionSpatialData(_tempEnvelope);
                                //make sure they are all finished

                                //await Task.When
                                //All(tabChangeDetectionTask, spatialChangeDetectionTask);
                                //change the status of the whole process to PreHarvested
                                await ChangeStatus(envelope.CountryCode, envelope.VersionId, HarvestingStatus.PreHarvested, cache);
                                bbEnvelope[0].Status = SiteChangeStatus.PreHarvested;
                            }

                            bbEnvelopes.Add(bbEnvelope[0]);
                        }

                    }

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



        private HarvestingStatus getStatus(int pStatus)
        {
            switch (pStatus)
            {
                case 0:
                    return HarvestingStatus.Pending;
                case 1:
                    return HarvestingStatus.Accepted;
                case 2:
                    return HarvestingStatus.Rejected;
                case 3:
                    return HarvestingStatus.Harvested;
                case 4:
                    return HarvestingStatus.Harvesting;
                case 5:
                    return HarvestingStatus.Queued;
                case 6:
                    return HarvestingStatus.PreHarvested;
                case 7:
                    return HarvestingStatus.Discarded;
                case 8:
                    return HarvestingStatus.Closed;
                default:
                    throw new Exception("No statuts definition found");
            }


        }

        /// <summary>
        /// Set the new status from the current
        /// </summary>
        /// <param name="country"></param>
        /// <param name="version"></param>
        /// <param name="toStatus"></param>
        /// <returns></returns>
        public async Task<ProcessedEnvelopes> ChangeStatus(string country, int version, HarvestingStatus toStatus, IMemoryCache cache)
        {
            string sqlToExecute = "exec dbo.";
            try
            {
                ProcessedEnvelopes envelope = _dataContext.Set<ProcessedEnvelopes>().Where(e => e.Country == country && e.Version == version).FirstOrDefault();
                if (envelope != null)
                {
                    //Get the version for the Sites 
                    //List<Sites> sites = _dataContext.Set<Sites>().Where(s => s.CountryCode == pCountry && s.N2KVersioningVersion == pVersion).Select(s=> s.Version).First();
                    //Sites site = sites.First();
                    //int _version = _dataContext.Set<Sites>().Where(s => s.CountryCode == country && s.N2KVersioningVersion == version).Select(s => s.Version).First();
                    if (toStatus != envelope.Status)
                    {
                        if (envelope.Status == HarvestingStatus.DataLoaded)
                        {
                            Task tabChangeDetectionTask = ChangeDetection(new EnvelopesToProcess[] { new EnvelopesToProcess
                            {
                                CountryCode = country,
                                VersionId = version
                            } });
                            Task spatialChangeDetectionTask = ChangeDetectionSpatialData(new EnvelopesToProcess[] { new EnvelopesToProcess
                            {
                                CountryCode = country,
                                VersionId = version
                            } });
                            //make sure they are all finished
                            await Task.WhenAll(tabChangeDetectionTask, spatialChangeDetectionTask);
                        }

                        SqlParameter param1 = new SqlParameter("@country", country);
                        SqlParameter param2 = new SqlParameter("@version", version);
                        switch (toStatus)
                        {
                            case HarvestingStatus.Harvested:
                                sqlToExecute = "exec dbo.setStatusToEnvelopeHarvested  @country, @version;";
                                break;
                            case HarvestingStatus.Discarded:
                                sqlToExecute = "exec dbo.setStatusToEnvelopeDiscarded  @country, @version;";
                                break;
                            case HarvestingStatus.PreHarvested:
                                sqlToExecute = "exec dbo.setStatusToEnvelopePreHarvested  @country, @version;";
                                break;
                            case HarvestingStatus.Closed:
                                sqlToExecute = "exec dbo.setStatusToEnvelopeClosed  @country, @version;";
                                break;
                            case HarvestingStatus.Pending:
                                sqlToExecute = "exec dbo.setStatusToEnvelopePending  @country, @version;";
                                break;
                            default:
                                break;
                        }
                        await _dataContext.Database.ExecuteSqlRawAsync(sqlToExecute, param1, param2);

                        if (toStatus == HarvestingStatus.Discarded || toStatus == HarvestingStatus.Closed)
                        {
                            ProcessedEnvelopes nextEnvelope = await _dataContext.Set<ProcessedEnvelopes>().AsNoTracking().Where(pe => (pe.Country == country) && (pe.Status == HarvestingStatus.DataLoaded)).OrderBy(pe => pe.Version).FirstOrDefaultAsync();
                            if (nextEnvelope != null)
                            {
                                EnvelopesToProcess nextEnvelopeToChangeDetection = new EnvelopesToProcess
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
                                await ChangeStatus(nextEnvelope.Country, nextEnvelope.Version, HarvestingStatus.PreHarvested, cache);
                            }
                        }

                        if (toStatus == HarvestingStatus.Closed)
                        {
                            HarvestedEnvelope bbEnvelope = new HarvestedEnvelope
                            {
                                VersionId = version,
                                CountryCode = country,
                                NumChanges = 0,
                                Status = SiteChangeStatus.Closed
                            };
                            //accept sites with no changes
                            await AcceptIdenticalSites(bbEnvelope);
                        }

                        if (toStatus == HarvestingStatus.Harvested || toStatus == HarvestingStatus.Closed)
                        {
                            //Remove country site changes cache
                            var field = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            var collection = field.GetValue(cache) as System.Collections.ICollection;
                            if (collection != null)
                            {
                                foreach (var item in collection)
                                {
                                    var methodInfo = item.GetType().GetProperty("Key");
                                    string listName = methodInfo.GetValue(item).ToString();

                                    if (!string.IsNullOrEmpty(listName))
                                    {
                                        if (listName.IndexOf(country) != -1)
                                        {
                                            cache.Remove(listName);
                                        }
                                    }
                                }
                            }
                        }

                        envelope.Status = toStatus;
                        return envelope;
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
                        EnvelopesToProcess newEnvelope = new EnvelopesToProcess();
                        newEnvelope.CountryCode = country;
                        newEnvelope.VersionId = version;
                        newEnvelope.SubmissionDate = (DateTime)package.Importdate;

                        List<EnvelopesToProcess> envelopes = new List<EnvelopesToProcess>();
                        envelopes.Add(newEnvelope);

                        await Harvest(envelopes.ToArray<EnvelopesToProcess>());


                    }
                    else
                    {
                        throw new Exception("The package doesn't exist on source database (" + country + " - " + version + ")");
                    }



                }
                return envelope;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - ChangeStatus - Envelope " + country + "/" + version.ToString() + " - Status " + toStatus.ToString(), "", _dataContext.Database.GetConnectionString());
                //return await Task.FromResult(new ProcessedEnvelopes());
                throw ex;
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
                var param1 = new SqlParameter("@country", country);
                var param2 = new SqlParameter("@version", version);

                var list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.GetSubmissionDateFromCountryAndVersionId  @country, @version",
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
            Sites bbSite = new Sites();
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
                    var param1 = new SqlParameter("@country", pCountryCode);
                    var param2 = new SqlParameter("@version", pCountryVersion);
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

        /// <summary>
        ///  This method retrives the complete information for a Site in Versioning and stores it in BackBone.
        ///  (Just the Site)
        /// </summary>
        /// <param name="pVSite">The definition ogf the versioning Site</param>
        /// <param name="pEnvelope">The envelope to process</param>
        /// <returns>Returns a BackBone Site object</returns>
        private async Task<List<Respondents>>? HarvestRespondents(List<Contact> vContact, EnvelopesToProcess pEnvelope)
        {
            await Task.Delay(1);
            List<Respondents> items = new List<Respondents>();
            foreach (Contact contact in vContact)
            {
                Respondents respondent = new Respondents();
                try
                {
                    respondent.SiteCode = contact.SITECODE;
                    respondent.Version = (int)contact.VERSIONID;
                    respondent.locatorName = contact.LOCATOR_NAME;
                    respondent.addressArea = contact.ADDRESS_AREA;
                    respondent.postName = contact.POST_NAME;
                    respondent.postCode = contact.POSTCODE;
                    respondent.thoroughfare = contact.THOROUGHFARE;
                    respondent.addressUnstructured = contact.UNSTRUCTURED_ADD;
                    respondent.name = contact.CONTACT_NAME;
                    respondent.Email = contact.EMAIL;
                    respondent.AdminUnit = contact.ADMIN_UNIT;
                    respondent.LocatorDesignator = contact.LOCATOR_DESIGNATOR;
                    items.Add(respondent);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - HarvestRespondents", "", _dataContext.Database.GetConnectionString());
                    return null;
                }
            }
            return items;
        }

        /// <summary>
        /// Method to accept sites with no changes
        /// </summary>
        /// <param name="envelope">Envelope to process</param>
        public async Task<HarvestedEnvelope> AcceptIdenticalSites(HarvestedEnvelope envelope)
        {
            try
            {
                SqlParameter paramCountryCode = new SqlParameter("@country", envelope.CountryCode);
                SqlParameter paramVersionId = new SqlParameter("@version", envelope.VersionId);

                await _dataContext.Database.ExecuteSqlRawAsync(
                        "exec spAcceptIdenticalSiteCodesBulk @country, @version",
                        paramCountryCode,
                        paramVersionId);
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "AcceptIdenticalSites - Envelope " + envelope.CountryCode + "/" + envelope.VersionId.ToString(), "", _dataContext.Database.GetConnectionString());
            }
            return envelope;
        }

    }
}

