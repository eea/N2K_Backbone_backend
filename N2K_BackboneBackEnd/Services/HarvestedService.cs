using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Models.ViewModel;

using N2K_BackboneBackEnd.Services.HarvestingProcess;
using N2K_BackboneBackEnd.Enumerations;
using IsImpactedBy = N2K_BackboneBackEnd.Models.versioning_db.IsImpactedBy;
using Microsoft.Extensions.Options;

namespace N2K_BackboneBackEnd.Services
{
    public class HarvestedService : IHarvestedService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private bool _ThereAreChanges = false;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="dataContext">>Context for the BackBone database</param>
        /// <param name="versioningContext">Context for the Versioning database</param>
        public HarvestedService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;

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

        /// <summary>
        /// Method that returns all those Envelops not harvested by Backbone 
        /// </summary>
        /// <returns>List of envelops avialable to harvest</returns>
        public async Task<List<Harvesting>> GetPendingEnvelopes()
        {
            var result = new List<Harvesting>();
            var countries = await _dataContext.Set<Countries>().ToListAsync();
            var processed = await _dataContext.Set<ProcessedEnvelopes>().FromSqlRaw($"select * from dbo.[vLatestProcessedEnvelopes]").AsNoTracking().ToListAsync();
            var allEnvs = await _dataContext.Set<ProcessedEnvelopes>().AsNoTracking().ToListAsync();
            foreach (var procCountry in processed)
            {
                var param1 = new SqlParameter("@country", procCountry.Country);
                var param2 = new SqlParameter("@version", procCountry.Version);
                var param3 = new SqlParameter("@importdate", procCountry.ImportDate);

                var list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.spGetPendingCountryVersion  @country, @version,@importdate",
                                param1, param2, param3).AsNoTracking().ToListAsync();
                if (list.Count > 0)
                {
                    foreach (var pendEnv in list)
                    {
                        if (!result.Contains(pendEnv))
                        {
                            if (allEnvs.Where(e => e.Version == pendEnv.Id && e.Country == pendEnv.Country && e.Status == HarvestingStatus.Harvesting).ToList().Count == 0)
                            {
                                result.Add(
                                    new Harvesting
                                    {
                                        Country = countries.Where(ct => ct.Code.ToLower() == pendEnv.Country.ToLower()).FirstOrDefault().Country,
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

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Method to validate the quality and the main rules of the data harvested
        /// </summary>
        /// <param name="envelopeIDs">List of the envelops to process</param>
        /// <returns>A list of the envelops with the result of the process</returns>
        public async Task<List<HarvestedEnvelope>> Validate(EnvelopesToProcess[] envelopeIDs)
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

                    SqlParameter param1 = new SqlParameter("@country", envelope.CountryCode);
                    SqlParameter param2 = new SqlParameter("@version", envelope.VersionId);

                    List<SiteToHarvest>? sitesVersioning = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSitesByCountryAndVersion  @country, @version",
                                    param1, param2).ToListAsync();
                    List<SiteToHarvest>? referencedSites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetCurrentSitesByCountry  @country",
                                    param1).ToListAsync();

                    List<SiteToHarvest>? versionToDelete = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetSitesVersionByCountryAndVersion  @country, @version",
                                    param1, param2).ToListAsync();
                    foreach (SiteToHarvest? deletingSite in versionToDelete)
                        await _dataContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.Changes WHERE Country = '" + envelope.CountryCode + "' AND Version = '" + deletingSite.VersionId + "'");

                    //For each site in Versioning compare it with that site in backboneDB
                    foreach (SiteToHarvest? harvestingSite in sitesVersioning)
                    {
                        changes = await SiteValidation(changes, referencedSites, harvestingSite, envelope, habitatPriority, speciesPriority);
                    }

                    //For each site in backboneDB check if the site still exists in Versioning
                    foreach (SiteToHarvest? storedSite in referencedSites)
                    {
                        SiteToHarvest? harvestingSite = sitesVersioning.Where(s => s.SiteCode == storedSite.SiteCode).FirstOrDefault();
                        if (harvestingSite == null)
                        {
                            changes.Add(new SiteChangeDb
                            {
                                SiteCode = storedSite.SiteCode,
                                Version = storedSite.VersionId,
                                ChangeCategory = "Network general structure",
                                ChangeType = "Site Deleted",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = Enumerations.SiteChangeStatus.Pending,
                                Tags = string.Empty,
                                NewValue = null,
                                OldValue = storedSite.SiteCode,
                                Code = storedSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                ReferenceSiteCode = storedSite.SiteCode
                            });
                        }
                    }

                    result.Add(new HarvestedEnvelope
                    {
                        CountryCode = envelope.CountryCode,
                        VersionId = envelope.VersionId,
                        NumChanges = changes.Count,
                        Status = SiteChangeStatus.Harvested
                    });

                    try
                    {
                        _dataContext.Set<SiteChangeDb>().AddRange(changes);
                        _dataContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        SystemLog.write(SystemLog.errorLevel.Error, ex, "Save Changes", "");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    SystemLog.write(SystemLog.errorLevel.Error, ex, "EnvelopeProcess - Start - Envelope " + envelope.CountryCode + "/" + envelope.VersionId.ToString(), "");
                    break;
                }
            }

            return result;
        }
        public async Task<List<HarvestedEnvelope>> ValidateSingleSite(string siteCode, int versionId)
        {
            SqlParameter param1 = new SqlParameter("@sitecode", siteCode);
            SqlParameter param2 = new SqlParameter("@version", versionId);

            List<SiteToHarvest>? sitesVersioning = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSitesBySitecodeAndVersion  @sitecode, @version",
                                    param1, param2).ToListAsync();

            SiteToHarvest? harvestingSite = sitesVersioning.FirstOrDefault();

            List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
            result = await ValidateSingleSiteObject(harvestingSite);
            return result;
        }
        public async Task<List<HarvestedEnvelope>> ValidateSingleSiteObject(SiteToHarvest harvestingSite)
        {
            EnvelopesToProcess envelope = new EnvelopesToProcess();
            envelope.CountryCode = harvestingSite.CountryCode;
            envelope.VersionId = (int)harvestingSite.N2KVersioningVersion;

            List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
            List<SiteChangeDb> changes = new List<SiteChangeDb>();

            //Get the lists of priority habitats and species
            List<HabitatPriority> habitatPriority = await _dataContext.Set<HabitatPriority>().FromSqlRaw($"exec dbo.spGetPriorityHabitats").ToListAsync();
            List<SpeciePriority> speciesPriority = await _dataContext.Set<SpeciePriority>().FromSqlRaw($"exec dbo.spGetPrioritySpecies").ToListAsync();

            try
            {
                SqlParameter param1 = new SqlParameter("@sitecode", harvestingSite.SiteCode);
                SqlParameter param2 = new SqlParameter("@version", harvestingSite.VersionId);

                List<SiteToHarvest>? referencedSites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetCurrentSiteBySitecode  @sitecode",
                                param1).ToListAsync();

                changes = await SiteValidation(changes, referencedSites, harvestingSite, envelope, habitatPriority, speciesPriority);

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
                    _dataContext.Set<SiteChangeDb>().AddRange(changes);
                    _dataContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    SystemLog.write(SystemLog.errorLevel.Error, ex, "Save Changes", "");
                }
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "EnvelopeProcess - Start - Envelope " + envelope.CountryCode + "/" + envelope.VersionId.ToString(), "");
            }

            return result;
        }

        public async Task<List<SiteChangeDb>> SiteValidation(List<SiteChangeDb> changes, List<SiteToHarvest> referencedSites, SiteToHarvest harvestingSite, EnvelopesToProcess envelope, List<HabitatPriority> habitatPriority, List<SpeciePriority> speciesPriority)
        {
            //Tolerance values. If the difference between reference and versioning values is bigger than these numbers, then they are notified.
            //If the tolerance is at 0, then it registers ALL changes, no matter how small they are.
            double siteAreaHaTolerance = 0.0;
            double siteLengthKmTolerance = 0.0;
            double habitatCoverHaTolerance = 0.0;

            SiteToHarvest? storedSite = referencedSites.Where(s => s.SiteCode == harvestingSite.SiteCode).FirstOrDefault();
            if (storedSite != null)
            {
                //These booleans declare whether or not each habitat is a priority
                Boolean isStoredSitePriority = false;
                Boolean isHarvestingSitePriority = false;

                //SiteAttributesChecking
                HarvestSiteCode siteCode = new HarvestSiteCode(_dataContext, _versioningContext);
                changes = await siteCode.ValidateSiteAttributes(changes, envelope, harvestingSite, storedSite, siteAreaHaTolerance, siteLengthKmTolerance);

                SqlParameter param3 = new SqlParameter("@site", harvestingSite.SiteCode);
                int maxVersionSite = harvestingSite.VersionId;
                SqlParameter param4 = new SqlParameter("@versionId", maxVersionSite);
                int previousVersionSite = storedSite.VersionId;
                SqlParameter param5 = new SqlParameter("@versionId", previousVersionSite);

                //HabitatChecking
                List<HabitatToHarvest> habitatVersioning = await _dataContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                param3, param4).ToListAsync();
                List<HabitatToHarvest> referencedHabitats = await _dataContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                param3, param5).ToListAsync();
                HarvestHabitats habitats = new HarvestHabitats(_dataContext, _versioningContext);
                changes = await habitats.ValidateHabitat(habitatVersioning, referencedHabitats, changes, envelope, harvestingSite, storedSite, param3, param4, param5, habitatCoverHaTolerance, habitatPriority);

                //SpeciesChecking
                List<SpeciesToHarvest> speciesVersioning = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                param3, param4).ToListAsync();
                List<SpeciesToHarvest> referencedSpecies = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                param3, param5).ToListAsync();
                HarvestSpecies species = new HarvestSpecies(_dataContext, _versioningContext);
                changes = await species.ValidateSpecies(speciesVersioning, referencedSpecies, changes, envelope, harvestingSite, storedSite, param3, param4, param5, speciesPriority);

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
                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                    siteChange.Tags = string.Empty;
                    siteChange.NewValue = Convert.ToString(isHarvestingSitePriority);
                    siteChange.OldValue = Convert.ToString(isStoredSitePriority);
                    siteChange.Code = harvestingSite.SiteCode;
                    siteChange.Section = "Site";
                    siteChange.VersionReferenceId = storedSite.VersionId;
                    siteChange.FieldName = "Priority";
                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
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
                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                    siteChange.Tags = string.Empty;
                    siteChange.NewValue = Convert.ToString(isHarvestingSitePriority);
                    siteChange.OldValue = Convert.ToString(isStoredSitePriority);
                    siteChange.Code = harvestingSite.SiteCode;
                    siteChange.Section = "Site";
                    siteChange.VersionReferenceId = storedSite.VersionId;
                    siteChange.FieldName = "Priority";
                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
                    changes.Add(siteChange);
                }
            }
            else
            {
                changes.Add(new SiteChangeDb
                {
                    SiteCode = harvestingSite.SiteCode,
                    Version = harvestingSite.VersionId,
                    ChangeCategory = "Network general structure",
                    ChangeType = "Site Added",
                    Country = envelope.CountryCode,
                    Level = Enumerations.Level.Info,
                    Status = Enumerations.SiteChangeStatus.Pending,
                    NewValue = harvestingSite.SiteCode,
                    OldValue = null,
                    Tags = string.Empty,
                    Code = harvestingSite.SiteCode,
                    Section = "Site",
                    VersionReferenceId = harvestingSite.VersionId,
                    ReferenceSiteCode = harvestingSite.SiteCode
                });
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
            List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
            try
            {
                TimeLog.setTimeStamp("Harvesting process ", "Init");

                //for each envelope to process
                foreach (EnvelopesToProcess envelope in envelopeIDs)
                {
                    //remove version from database
                    await resetEnvirontment(envelope.CountryCode, envelope.VersionId);

                    //create a new entry in the processed envelopes table to register that a new one is being harvested
                    ProcessedEnvelopes envelopeToProcess = new ProcessedEnvelopes
                    {
                        Country = envelope.CountryCode
                        ,
                        Version = envelope.VersionId
                        ,
                        ImportDate = await GetSubmissionDate(envelope.CountryCode, envelope.VersionId)
                        ,
                        Status = HarvestingStatus.Harvesting
                        ,
                        Importer = "TEST"
                    };
                    try
                    {
                        //add the envelope to the DB
                        _dataContext.Set<ProcessedEnvelopes>().Add(envelopeToProcess);
                        _dataContext.SaveChanges();


                        //Get the sites submitted in the envelope
                        List<NaturaSite> vSites = _versioningContext.Set<NaturaSite>().Where(v => (v.COUNTRYCODE == envelope.CountryCode) && (v.COUNTRYVERSIONID == envelope.VersionId)).ToList();
                        //List<NaturaSite> vSites = _versioningContext.Set<NaturaSite>().Where(v => (v.SITECODE == "DE5632303") && (v.VERSIONID == 548)).ToList();

                        List<Sites> bbSites = new List<Sites>();

                        foreach (NaturaSite vSite in vSites)
                        {
                            try
                            {
                                _ThereAreChanges = true;
                                //_timeLog.setTimeStamp(_appSettings.Value.N2K_BackboneBackEndContext, "Site " + vSite.SITECODE + " - " + vSite.VERSIONID.ToString(), "Init");
                                //complete the data of the site and add it to the DB
                                TimeLog.setTimeStamp("Site " + vSite.SITECODE + " - " + vSite.VERSIONID.ToString(), "Init");
                                HarvestSiteCode siteCode = new HarvestSiteCode(_dataContext, _versioningContext);
                                Sites bbSite = await siteCode.HarvestSite(vSite, envelope);
                                if (bbSite != null)
                                {

                                    //TODO: Put species on another threath 
                                    HarvestSpecies species = new HarvestSpecies(_dataContext, _versioningContext);
                                    await species.HarvestBySite(vSite.SITECODE, vSite.VERSIONID, bbSite.Version);

                                    //TODO: Put habitats on another threath 
                                    HarvestHabitats habitats = new HarvestHabitats(_dataContext, _versioningContext);
                                    await habitats.HarvestBySite(vSite.SITECODE, vSite.VERSIONID, bbSite.Version);
                                }
                                _dataContext.SaveChanges();
                                _ThereAreChanges = false;
                            }
                            catch (DbUpdateException ex)
                            {

                                RefusedSites.addAsRefused(vSite, envelope, ex);
                            }
                            catch (Exception ex)
                            {

                                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestSites - Start - Site " + vSite.SITECODE + "/" + vSite.VERSIONID.ToString(), "");
                                //RefusedSites.addAsRefused(vSite);
                                //rollback(envelope.CountryCode, envelope.VersionId);
                                //break;
                            }
                            finally
                            {

                            }

                        }
                        //set the enevelope as successfully completed
                        envelopeToProcess.Status = HarvestingStatus.Harvested;
                        _dataContext.Set<ProcessedEnvelopes>().Update(envelopeToProcess);
                        result.Add(
                            new HarvestedEnvelope
                            {
                                CountryCode = envelope.CountryCode,
                                VersionId = envelope.VersionId,
                                NumChanges = 0,
                                Status = SiteChangeStatus.Harvested
                            }
                         );
                    }
                    catch (Exception ex)
                    {
                        SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSite", "");
                        //if there is an error reject the envelope
                        _dataContext.Set<ProcessedEnvelopes>().Remove(envelopeToProcess);
                        result.Add(
                            new HarvestedEnvelope
                            {
                                CountryCode = envelope.CountryCode,
                                VersionId = envelope.VersionId,
                                NumChanges = 0,
                                Status = SiteChangeStatus.Rejected
                            }
                         );
                    }
                    finally
                    {
                        //save the data of the site in backbone DB
                        _dataContext.SaveChanges();
                    }




                }
                return await Task.FromResult(result);
            }

            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSite", "");
                return await Task.FromResult(new List<HarvestedEnvelope>());
            }
            finally
            {
                TimeLog.setTimeStamp("Harvesting process ", "End");
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
                bbSite.CurrentStatus = (int?)SiteChangeStatus.Pending;
                bbSite.SiteType = pVSite.SITETYPE;
                bbSite.AltitudeMin = pVSite.ALTITUDE_MIN;
                bbSite.AltitudeMax = pVSite.ALTITUDE_MAX;
                bbSite.Area = (double?)pVSite.AREAHA;
                bbSite.CountryCode = pEnvelope.CountryCode;
                bbSite.Length = (double?)pVSite.LENGTHKM;
                bbSite.N2KVersioningRef = Int32.Parse(pVSite.VERSIONID.ToString());
                bbSite.N2KVersioningVersion = pEnvelope.VersionId;
                return bbSite;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSite", "");
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
                SystemLog.write(SystemLog.errorLevel.Error, ex.Message, "HarvestedService - resetEnvirontment", "");
            }
            return 1;
        }

        /// <summary>
        /// Method to delete all the changes create by the envelope
        /// </summary>
        /// <param name="pCountry"></param>
        /// <param name="pVerion"></param>
        private void rollback(string pCountry, int pVersion)
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
                List<Sites> toremove = _dataContext.Set<Sites>().Where(s => s.CountryCode == pCountry && s.N2KVersioningVersion == pVersion).ToList();
                _dataContext.Set<Sites>().RemoveRange(toremove);
                _dataContext.SaveChanges();
                _ThereAreChanges = false;

            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - rollback", "");
            }
            finally
            {

            }

        }

    }
}

