using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.VersioningDB;
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.ViewModel;

using N2K_BackboneBackEnd.Services.HarvestingProcess;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
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

        public HarvestedService(N2KBackboneContext dataContext,N2K_VersioningContext versioningContext)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
            
        }
        
        public HarvestedService(N2KBackboneContext dataContext,N2K_VersioningContext versioningContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
             _appSettings = app;
        }
        public async Task<List<Harvesting>> GetHarvestedAsync()
        {
            var a = new List<Harvesting>();
            return await Task.FromResult(a);

        }

        public List<Harvesting> GetHarvested()
        {
            var a = new List<Harvesting>();
            return a;

        }



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

        public async Task<List<Harvesting>> GetPendingEnvelopes()
        {
            /*
            var aa = new List<Sites>();
            var processed1 = await _dataContext.Set<Sites>().ToListAsync();
            var bb = new List<BioRegions>();
            var processed2 = await _dataContext.Set<BioRegions>().ToListAsync();
            var cc = new List<NutsBySite>();
            var processed3 = await _dataContext.Set<NutsBySite>().ToListAsync();
            var dd = new List<Models.backbone_db.HasNationalProtection>();
            var processed4 = await _dataContext.Set<Models.backbone_db.HasNationalProtection>().ToListAsync();
            var ee = new List<IsImpactedBy>();
            var processed5 = await _dataContext.Set<IsImpactedBy>().ToListAsync();
            var ff = new List<SitesInXML>();
            var processed6 = await _dataContext.Set<SitesInXML>().ToListAsync();
            var gg = new List<SiteLargeDescriptions>();
            var processed7 = await _dataContext.Set<SiteLargeDescriptions>().ToListAsync();
            var hh = new List<Models.backbone_db.DetailedProtectionStatus>();
            var processed8 = await _dataContext.Set<Models.backbone_db.DetailedProtectionStatus>().ToListAsync();
            var ii = new List<DocumentationLinks>();
            var processed9 = await _dataContext.Set<DocumentationLinks>().ToListAsync();
            var jj = new List<SiteOwnerType>();
            var processed10 = await _dataContext.Set<SiteOwnerType>().ToListAsync();
            
            var kk = new List<Habitats>();
            var processed11 = await _dataContext.Set<Habitats>().ToListAsync();
            var mm = new List<DescribeSites>();
            var processed13 = await _dataContext.Set<DescribeSites>().ToListAsync();

            var nn = new List<Species>();
            var processed14 = await _dataContext.Set<Species>().ToListAsync();
            var oo = new List<SpeciesOther>();
            var processed15 = await _dataContext.Set<SpeciesOther>().ToListAsync();

            */


            var result = new List<Harvesting>();
            var processed = await _dataContext.Set<ProcessedEnvelopes>().FromSqlRaw($"select * from dbo.[vLatestProcessedEnvelopes]").ToListAsync();
            var allEnvs = await _dataContext.Set<ProcessedEnvelopes>().ToListAsync();
            foreach (var procCountry in processed)
            {
                var param1 = new SqlParameter("@country", procCountry.Country);
                var param2 = new SqlParameter("@version", procCountry.Version);
                var param3 = new SqlParameter("@importdate", procCountry.ImportDate);

                var list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.spGetPendingCountryVersion  @country, @version,@importdate",
                                param1, param2, param3).ToListAsync();
                if (list.Count > 0)
                {
                    foreach (var pendEnv in list)
                    {
                        if (!result.Contains(pendEnv))
                        {
                            if (allEnvs.Where(e => e.Version == pendEnv.Id && e.Country == pendEnv.Country && e.Status ==  HarvestingStatus.Harvesting).ToList().Count == 0)
                            {
                                result.Add(
                                    new Harvesting
                                    {
                                        Country = pendEnv.Country,
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

        public async Task<List<HarvestedEnvelope>> Harvest(EnvelopesToProcess[] envelopeIDs)
        {
            var result = new List<HarvestedEnvelope>();
            var changes = new List<SiteChangeDb>();
            var latestVersions = await _dataContext.Set<ProcessedEnvelopes>().ToListAsync();


            //from the view vLatestProcessedEnvelopes (backbonedb) load the sites with the latest versionid of the countries

            //Load all sites with the CountryVersionID-CountryCode from Versioning
            foreach (EnvelopesToProcess envelope in envelopeIDs)
            {

                result.Add(
                    new HarvestedEnvelope
                    {
                        CountryCode = envelope.CountryCode,
                        VersionId = envelope.VersionId,
                        NumChanges = 0,
                        Status = SiteChangeStatus.Harvested
                    }
                 );

                /*
                //remove version from database
                var param1 = new SqlParameter("@country", envelopeIDs[i].CountryCode);
                var param2 = new SqlParameter("@version", envelopeIDs[i].VersionId);
                await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spRemoveVersionFromDB  @country, @version", param1, param2);


                var country = latestVersions.Where(v => v.Country == envelopeIDs[i].CountryCode).FirstOrDefault(); //Coger la ultima version de ese country
                var lastReferenceCountryVersion = 0;
                if (country != null) lastReferenceCountryVersion = country.Version;

                //1. Harvest SiteCodes
                var harvSiteCode = new HarvestSiteCode(_dataContext, _versioningContext);
                await harvSiteCode.Harvest(envelopeIDs[i].CountryCode, envelopeIDs[i].VersionId);

                if (lastReferenceCountryVersion != 0)
                {
                    var tablesToHarvest = new Dictionary<int, IHarvestingTables>();

                    var harvestingTasks = new List<Task<int>>();
                    var validatingTasks = new List<Task<int>>();

                    //2. Once SiteCodes is harvested we can run a number of task in parallel
                    //Run the validation
                    validatingTasks.Add(harvSiteCode.ValidateChanges(envelopeIDs[i].CountryCode, envelopeIDs[i].VersionId, lastReferenceCountryVersion));

                    //harvest 
                    var habitats = new HarvestHabitats(_dataContext, _versioningContext);
                    var habitatsTask = habitats.Harvest(envelopeIDs[i].CountryCode, envelopeIDs[i].VersionId);
                    tablesToHarvest.Add(habitatsTask.Id, habitats);
                    harvestingTasks.Add(habitatsTask);

                    var species = new HarvestSpecies(_dataContext, _versioningContext);
                    var speciesTask = species.Harvest(envelopeIDs[i].CountryCode, envelopeIDs[i].VersionId);
                    tablesToHarvest.Add(speciesTask.Id, species);
                    harvestingTasks.Add(speciesTask);


                    //validate when the harvesting of each one is completed
                    while (harvestingTasks.Count > 0)
                    {
                        var finishedTask = await Task.WhenAny(harvestingTasks);
                        if (finishedTask != null)
                        {
                            if (finishedTask.Id > 0)
                            {
                                IHarvestingTables? harvest = tablesToHarvest[finishedTask.Id]; // .GetValueOrDefault();
                                if (harvest != null)
                                    if (finishedTask.Result == 1)
                                        validatingTasks.Add(harvest.ValidateChanges(envelopeIDs[i].CountryCode, envelopeIDs[i].VersionId, lastReferenceCountryVersion));
                            }
                            harvestingTasks.Remove(finishedTask);
                        }
                    }
                    //...

                    //wait until validation tasks are finished
                    while (validatingTasks.Count > 0)
                    {
                        var finishedTask = await Task.WhenAny(validatingTasks);
                        validatingTasks.Remove(finishedTask);
                    }
                    tablesToHarvest.Clear();

                }

                
                var param1 = new SqlParameter("@country", envelopeIDs[i].CountryCode);
                var param2 = new SqlParameter("@version", envelopeIDs[i].VersionId);

                var sitesVersioning = await _versioningContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetNaturaSiteDataByCountryIdAndCode  @country, @version",
                                param1, param2).ToListAsync();
                var referencedSites = new List<SiteToHarvest>();
                if (lastReferenceCountryVersion != 0)
                {
                    var param3 = new SqlParameter("@version", lastReferenceCountryVersion);
                    referencedSites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.[spGetReferenceSitesByCountryAndVersion]  @country, @version",
                                param1, param3).ToListAsync();
                }


                //For each site in Versioning compare it with that site in backboneDB
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                foreach (var harvestingSite in sitesVersioning)
                {
                    var storedSite = referencedSites.Where(s => s.SiteCode == harvestingSite.SiteCode).FirstOrDefault();
                    if (storedSite != null)
                    {
                        //Tolerance values. If the difference between reference and versioning values is bigger than these numbers, then they are notified.
                        //If the tolerance is at 0, then it registers ALL changes, no matter how small they are.
                        var siteAreaHaTolerance = 0.0;
                        var siteLengthKmTolerance = 0.0;
                        var habitatCoverHaTolerance = 0.0;

                        #region SiteAttributesChecking
                        //Null values are turned into empty strings and -1
                        if (storedSite.SiteName == null) storedSite.SiteName = "";
                        if (harvestingSite.SiteName == null) harvestingSite.SiteName = "";
                        if (storedSite.AreaHa == null) storedSite.AreaHa = -1;
                        if (harvestingSite.AreaHa == null) harvestingSite.AreaHa = -1;
                        if (storedSite.LengthKm == null) storedSite.LengthKm = -1;
                        if (harvestingSite.LengthKm == null) harvestingSite.LengthKm = -1;

                        if (harvestingSite.SiteName != storedSite.SiteName)
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.ChangeCategory = "Site General Info";
                            siteChange.ChangeType = "SiteName Changed";
                            siteChange.Country = envelopeIDs[i].CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            changes.Add(siteChange);
                            processedEnv.NumChanges++;
                        }
                        //if (harvestingSite.SiteType != storedSite.SiteType)
                        //{
                        //    var siteChange = new SiteChangeDb();
                        //    siteChange.SiteCode = harvestingSite.SiteCode;
                        //    siteChange.ChangeCategory = "Site General Info";
                        //    siteChange.ChangeType = "SiteType Changed";
                        //    siteChange.Country = envelopeIDs[i].CountryCode;
                        //    siteChange.Level = Enumerations.Level.Critical;
                        //    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                        //    siteChange.Tags = string.Empty;
                        //    changes.Add(siteChange);
                        //    numChanges++;
                        //}
                        if (harvestingSite.AreaHa > storedSite.AreaHa)
                        {
                            if (Math.Abs((double)(harvestingSite.AreaHa - storedSite.AreaHa)) > siteAreaHaTolerance)
                            {
                                var siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.ChangeCategory = "Change of area";
                                siteChange.ChangeType = "Area Increased";
                                siteChange.Country = envelopeIDs[i].CountryCode;
                                siteChange.Level = Enumerations.Level.Warning;
                                siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                siteChange.Tags = string.Empty;
                                changes.Add(siteChange);
                                processedEnv.NumChanges++;
                            }
                        }
                        else if (harvestingSite.AreaHa < storedSite.AreaHa)
                        {
                            if (Math.Abs((double)(harvestingSite.AreaHa - storedSite.AreaHa)) > siteAreaHaTolerance)
                            {
                                var siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.ChangeCategory = "Change of area";
                                siteChange.ChangeType = "Area Decreased";
                                siteChange.Country = envelopeIDs[i].CountryCode;
                                siteChange.Level = Enumerations.Level.Medium;
                                siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                siteChange.Tags = string.Empty;
                                changes.Add(siteChange);
                                processedEnv.NumChanges++;
                            }
                        }
                        else if (harvestingSite.AreaHa != storedSite.AreaHa)
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.ChangeCategory = "Change of area";
                            siteChange.ChangeType = "Area Change";
                            siteChange.Country = envelopeIDs[i].CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            changes.Add(siteChange);
                            processedEnv.NumChanges++;
                        }
                        if (harvestingSite.LengthKm != storedSite.LengthKm)
                        {
                            if (Math.Abs((double)(harvestingSite.LengthKm - storedSite.LengthKm)) > siteLengthKmTolerance)
                            {
                                var siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.ChangeCategory = "Site General Info";
                                siteChange.ChangeType = "Length Changed";
                                siteChange.Country = envelopeIDs[i].CountryCode;
                                siteChange.Level = Enumerations.Level.Warning;
                                siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                siteChange.Tags = string.Empty;
                                changes.Add(siteChange);
                                processedEnv.NumChanges++;
                            }
                        }
                        #endregion

                        var param3 = new SqlParameter("@site", harvestingSite.SiteCode);

                        #region HabitatChecking
                        var habitatVersioning = await _versioningContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetHabitatDataByCountryIdAndCountryCodeAndSiteCode  @country, @version, @site",
                                        param1, param2, param3).ToListAsync();
                        var referencedHabitats = await _dataContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCode  @site",
                                        param3).ToListAsync();
                        //For each habitat in Versioning compare it with that habitat in backboneDB
                        foreach (var harvestingHabitat in habitatVersioning)
                        {
                            var storedHabitat = referencedHabitats.Where(s => s.HabitatCode == harvestingHabitat.HabitatCode).FirstOrDefault();
                            if (storedHabitat != null)
                            {
                                //Null values are turned into empty strings and -1
                                if (storedHabitat.RelSurface == null) storedHabitat.RelSurface = "";
                                if (harvestingHabitat.RelSurface == null) harvestingHabitat.RelSurface = "";
                                if (storedHabitat.Representativity == null) storedHabitat.Representativity = "";
                                if (harvestingHabitat.Representativity == null) harvestingHabitat.Representativity = "";
                                if (storedHabitat.Cover_ha == null) storedHabitat.Cover_ha = -1;
                                if (harvestingHabitat.Cover_ha == null) harvestingHabitat.Cover_ha = -1;

                                if (((storedHabitat.RelSurface.ToUpper() == "A" || storedHabitat.RelSurface.ToUpper() == "B") && harvestingHabitat.RelSurface.ToUpper() == "C")
                                    || (storedHabitat.RelSurface.ToUpper() == "A" && harvestingHabitat.RelSurface.ToUpper() == "B"))
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Relative surface Decrease";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Medium;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                                else if (((storedHabitat.RelSurface.ToUpper() == "B" || storedHabitat.RelSurface.ToUpper() == "C") && harvestingHabitat.RelSurface.ToUpper() == "A")
                                    || (storedHabitat.RelSurface.ToUpper() == "C" && harvestingHabitat.RelSurface.ToUpper() == "B"))
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Relative surface Increase";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Warning;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                                else if (storedHabitat.RelSurface.ToUpper() != harvestingHabitat.RelSurface.ToUpper())
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Relative surface Change";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Warning;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                                if (storedHabitat.Representativity.ToUpper() != "D" && harvestingHabitat.Representativity.ToUpper() == "D")
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Representativity Decrease";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Medium;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                                else if (storedHabitat.Representativity.ToUpper() == "D" && harvestingHabitat.Representativity.ToUpper() != "D")
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Representativity Increase";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Warning;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                                else if (storedHabitat.Representativity.ToUpper() != harvestingHabitat.Representativity.ToUpper())
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Representativity Change";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Warning;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                                if (storedHabitat.Cover_ha > harvestingHabitat.Cover_ha)
                                {
                                    if (Math.Abs((double)(storedHabitat.Cover_ha - harvestingHabitat.Cover_ha)) > habitatCoverHaTolerance)
                                    {
                                        var siteChange = new SiteChangeDb();
                                        siteChange.SiteCode = harvestingSite.SiteCode;
                                        siteChange.ChangeCategory = "Species and habitats";
                                        siteChange.ChangeType = "Cover_ha Decrease";
                                        siteChange.Country = envelopeIDs[i].CountryCode;
                                        siteChange.Level = Enumerations.Level.Medium;
                                        siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                        siteChange.Tags = string.Empty;
                                        changes.Add(siteChange);
                                        processedEnv.NumChanges++;
                                    }
                                }
                                else if (storedHabitat.Cover_ha < harvestingHabitat.Cover_ha)
                                {
                                    if (Math.Abs((double)(storedHabitat.Cover_ha - harvestingHabitat.Cover_ha)) > habitatCoverHaTolerance)
                                    {
                                        var siteChange = new SiteChangeDb();
                                        siteChange.SiteCode = harvestingSite.SiteCode;
                                        siteChange.ChangeCategory = "Species and habitats";
                                        siteChange.ChangeType = "Cover_ha Increase";
                                        siteChange.Country = envelopeIDs[i].CountryCode;
                                        siteChange.Level = Enumerations.Level.Warning;
                                        siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                        siteChange.Tags = string.Empty;
                                        changes.Add(siteChange);
                                        processedEnv.NumChanges++;
                                    }
                                }
                                else if (storedHabitat.Cover_ha != harvestingHabitat.Cover_ha)
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Cover_ha Change";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Warning;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                            }
                            else
                            {
                                changes.Add(new SiteChangeDb
                                {
                                    SiteCode = harvestingSite.SiteCode,
                                    ChangeCategory = "Habitat Added",
                                    ChangeType = "Habitat Added",
                                    Country = envelopeIDs[i].CountryCode,
                                    Level = Enumerations.Level.Warning,
                                    Status = Enumerations.SiteChangeStatus.Pending,
                                    Tags = string.Empty
                                });
                                processedEnv.NumChanges++;
                            }
                        }

                        //For each habitat in backboneDB check if the habitat still exists in Versioning
                        foreach (var storedHabitat in referencedHabitats)
                        {
                            var harvestingHabitat = habitatVersioning.Where(s => s.HabitatCode == storedHabitat.HabitatCode).FirstOrDefault();
                            if (harvestingHabitat == null)
                            {
                                changes.Add(new SiteChangeDb
                                {
                                    SiteCode = storedSite.SiteCode,
                                    ChangeCategory = "Habitat Deleted",
                                    ChangeType = "Habitat Deleted",
                                    Country = envelopeIDs[i].CountryCode,
                                    Level = Enumerations.Level.Critical,
                                    Status = Enumerations.SiteChangeStatus.Pending,
                                    Tags = string.Empty
                                });
                                processedEnv.NumChanges++;
                            }
                        }
                        #endregion

                        #region SpeciesChecking
                        var speciesVersioning = await _versioningContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetSpeciesDataByCountryIdAndCountryCodeAndSiteCode  @country, @version, @site",
                                        param1, param2, param3).ToListAsync();
                        var referencedSpecies = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCode  @site",
                                        param3).ToListAsync();
                        //For each species in Versioning compare it with that species in backboneDB
                        foreach (var harvestingSpecies in speciesVersioning)
                        {
                            var storedSpecies = referencedSpecies.Where(s => s.SpeciesCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
                            if (storedSpecies != null)
                            {
                                //Null values are turned into empty strings
                                if (storedSpecies.Population == null) storedSpecies.Population = "";
                                if (harvestingSpecies.Population == null) harvestingSpecies.Population = "";

                                if (storedSpecies.Population.ToUpper() != "D" && harvestingSpecies.Population.ToUpper() == "D")
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Population Increase";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Medium;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                                else if (storedSpecies.Population.ToUpper() == "D" && harvestingSpecies.Population.ToUpper() != "D")
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Population Decrease";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Warning;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                                else if (storedSpecies.Population.ToUpper() != harvestingSpecies.Population.ToUpper())
                                {
                                    var siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.ChangeCategory = "Species and habitats";
                                    siteChange.ChangeType = "Population Change";
                                    siteChange.Country = envelopeIDs[i].CountryCode;
                                    siteChange.Level = Enumerations.Level.Warning;
                                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                    siteChange.Tags = string.Empty;
                                    changes.Add(siteChange);
                                    processedEnv.NumChanges++;
                                }
                            }
                            else
                            {
                                changes.Add(new SiteChangeDb
                                {
                                    SiteCode = harvestingSite.SiteCode,
                                    ChangeCategory = "Species Added",
                                    ChangeType = "Species Added",
                                    Country = envelopeIDs[i].CountryCode,
                                    Level = Enumerations.Level.Warning,
                                    Status = Enumerations.SiteChangeStatus.Pending,
                                    Tags = string.Empty
                                });
                                processedEnv.NumChanges++;
                            }
                        }

                        //For each species in backboneDB check if the species still exists in Versioning
                        foreach (var storedSpecies in referencedSpecies)
                        {
                            var harvestingSpecies = speciesVersioning.Where(s => s.SpeciesCode == storedSpecies.SpeciesCode).FirstOrDefault();
                            if (harvestingSpecies == null)
                            {
                                changes.Add(new SiteChangeDb
                                {
                                    SiteCode = storedSite.SiteCode,
                                    ChangeCategory = "Species Deleted",
                                    ChangeType = "Species Deleted",
                                    Country = envelopeIDs[i].CountryCode,
                                    Level = Enumerations.Level.Critical,
                                    Status = Enumerations.SiteChangeStatus.Pending,
                                    Tags = string.Empty
                                });
                                processedEnv.NumChanges++;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        changes.Add(new SiteChangeDb
                        {
                            SiteCode = harvestingSite.SiteCode,
                            ChangeCategory = "Site Added",
                            ChangeType = "Site Added",
                            Country = envelopeIDs[i].CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = Enumerations.SiteChangeStatus.Pending,
                            Tags = string.Empty
                        });
                        processedEnv.NumChanges++;
                    }
                }

                //For each site in backboneDB check if the site still exists in Versioning
                foreach (var storedSite in referencedSites)
                {
                    var harvestingSite = sitesVersioning.Where(s => s.SiteCode == storedSite.SiteCode).FirstOrDefault();
                    if (harvestingSite == null)
                    {
                        changes.Add(new SiteChangeDb
                        {
                            SiteCode = storedSite.SiteCode,
                            ChangeCategory = "Site Deleted",
                            ChangeType = "Site Deleted",
                            Country = envelopeIDs[i].CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = Enumerations.SiteChangeStatus.Pending,
                            Tags = string.Empty
                        });
                        processedEnv.NumChanges++;
                    }
                }
                */


                //for the time being do not load the changes and keep using test_table 
                
                try
                {
                    var a = 1;
                    //_dataContext.SiteChanges.AddRange(changes);
                    //_dataContext.SaveChanges();
                }
                catch
                {
                    throw;
                }
            }

            return result;
        }



        public async Task<List<HarvestedEnvelope>> Start(EnvelopesToProcess[] envelopeIDs)
        {

            List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
            List<NaturaSite> sites = null;
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
                        ,Version = envelope.VersionId
                        ,ImportDate = await GetSubmissionDate(envelope.CountryCode, envelope.VersionId)
                        ,Status = HarvestingStatus.Harvesting
                        ,Importer="TEST"
                    };
                    try
                    {
                        //add the envelope to the DB
                        _dataContext.Set<ProcessedEnvelopes>().Add(envelopeToProcess);
                        _dataContext.SaveChanges();
                        

                        //Get the sites submitted in the envelope
                        List<NaturaSite> vSites = _versioningContext.Set<NaturaSite>().Where(v => (v.COUNTRYCODE == envelope.CountryCode) && (v.COUNTRYVERSIONID == envelope.VersionId)).ToList();
                        List<Sites> bbSites = new List<Sites>();

                        foreach (NaturaSite vSite in vSites)
                        {
                            try
                            {
                                _ThereAreChanges = true;
                                //_timeLog.setTimeStamp(_appSettings.Value.N2K_BackboneBackEndContext, "Site " + vSite.SITECODE + " - " + vSite.VERSIONID.ToString(), "Init");
                                //complete the data of the site and add it to the DB
                                TimeLog.setTimeStamp("Site " + vSite.SITECODE + " - " + vSite.VERSIONID.ToString(), "Init");
                                Sites bbSite = harvestSite(vSite, envelope);
                                _dataContext.Set<Sites>().Add(bbSite);
                                _dataContext.SaveChanges();
                                //Get the data for all related tables                                
                                _dataContext.Set<BioRegions>().AddRange(harvestBioregions(vSite, bbSite.Version));
                                _dataContext.Set<NutsBySite>().AddRange(harvestNutsBySite(vSite, bbSite.Version));
                                _dataContext.Set<Models.backbone_db.IsImpactedBy>().AddRange(harvestIsImpactedBy(vSite, bbSite.Version));
                                _dataContext.Set<Models.backbone_db.HasNationalProtection>().AddRange(harvestHasNationalProtection(vSite, bbSite.Version));
                                _dataContext.Set<Models.backbone_db.DetailedProtectionStatus>().AddRange(harvestDetailedProtectionStatus(vSite, bbSite.Version));
                                _dataContext.Set<SiteLargeDescriptions>().AddRange(harvestSiteLargeDescriptions(vSite, bbSite.Version));
                                _dataContext.Set<SiteOwnerType>().AddRange(harvestSiteOwnerType(vSite, bbSite.Version));
                                TimeLog.setTimeStamp("Site " + vSite.SITECODE + " - " + vSite.VERSIONID.ToString(), "Processed");

                                //TODO: Put species on another threath 
                                HarvestSpecies species = new HarvestSpecies(_dataContext, _versioningContext);
                                await species.HarvestBySite(vSite.SITECODE, vSite.VERSIONID, bbSite.Version);

                                //TODO: Put habitats on another threath 
                                HarvestHabitats habitats = new HarvestHabitats(_dataContext, _versioningContext);
                                await habitats.HarvestBySite(vSite.SITECODE, vSite.VERSIONID, bbSite.Version);


                                _dataContext.SaveChanges();
                                _ThereAreChanges = false;
                            }
                            catch (Exception ex)
                            {
                                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestSites - Start - Site " + vSite.SITECODE + "/" + vSite.VERSIONID.ToString(), "");
                                rollback(envelope.CountryCode, envelope.VersionId);
                                break;
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

        private Sites harvestSite(NaturaSite pVSite, EnvelopesToProcess pEnvelope)
        {
            //Tomamos el valor más alto que tiene en el campo Version para ese SiteCode. Por defecto es -1 para cuando no existe 
            //por que le vamos a sumar un 1 lo cual dejaría en 0
            Sites bbSite = new Sites();
            int versionNext = 0;

            try
            {
                versionNext = _dataContext.Set<Sites>().Where(s => s.SiteCode == pVSite.SITECODE).OrderBy(s => s.Version).Select(s => s.Version).FirstOrDefault();
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

        private List<BioRegions> harvestBioregions(NaturaSite pVSite, int pVersion)
        {
            List<BelongsToBioRegion> elements = null;
            List<BioRegions> items = new List<BioRegions>();
            try
            {
                elements = _versioningContext.Set<BelongsToBioRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (BelongsToBioRegion element in elements)
                {
                    //SystemLog.write(SystemLog.errorLevel.Debug, "Site/Version/BioRegion: " + pVSite.SITECODE + "-" + pVSite.VERSIONID.ToString() + "/" + pVersion.ToString() + "/"+ element.BIOREGID.ToString(), "HarvestedService - harvestBioregions", "");
                    BioRegions item = new BioRegions();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.BGRID = element.BIOREGID;
                    item.Percentage = (double?)element.PERCENTAGE;
                    items.Add(item);
                }
                return items;
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

        private List<NutsBySite> harvestNutsBySite(NaturaSite pVSite, int pVersion)
        {
            List<NutsRegion> elements = null;
            List<NutsBySite> items = new List<NutsBySite>();
            try
            {
                elements = _versioningContext.Set<NutsRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (NutsRegion element in elements)
                {
                    NutsBySite item = new NutsBySite();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.NutId = element.NUTSCODE;
                    item.CoverPercentage = (double?)element.COVER;
                    items.Add(item);
                }
                return items;
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

        private List<Models.backbone_db.IsImpactedBy> harvestIsImpactedBy(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.IsImpactedBy> elements = null;
            List<Models.backbone_db.IsImpactedBy> items = new List<Models.backbone_db.IsImpactedBy>();
            try
            {
                elements = _versioningContext.Set<IsImpactedBy>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.IsImpactedBy element in elements)
                {
                    Models.backbone_db.IsImpactedBy item = new Models.backbone_db.IsImpactedBy();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.ActivityCode = element.ACTIVITYCODE;
                    item.InOut = element.IN_OUT;
                    item.Intensity = element.INTENSITY;
                    item.PercentageAff = element.PERCENTAGEAFF;
                    item.Influence = element.INFLUENCE;
                    if (element.STARTDATE.HasValue)
                    {
                        item.StartDate = element.STARTDATE;
                    }
                    if (element.ENDDATE.HasValue)
                    {
                        item.EndDate = element.ENDDATE;
                    }
                    item.PollutionCode = element.POLLUTIONCODE;
                    item.Ocurrence = element.OCCURRENCE;
                    item.ImpactType = element.IMPACTTYPE;
                    item.InOut = element.IN_OUT;
                    item.InOut = element.IN_OUT;
                    items.Add(item);
                }
                return items;
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

        private List<Models.backbone_db.HasNationalProtection> harvestHasNationalProtection(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.HasNationalProtection> elements = null;
            List<Models.backbone_db.HasNationalProtection> items = new List<Models.backbone_db.HasNationalProtection>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.HasNationalProtection>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.HasNationalProtection element in elements)
                {
                    Models.backbone_db.HasNationalProtection item = new Models.backbone_db.HasNationalProtection();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.DesignatedCode = element.DESIGNATEDCODE;
                    item.Percentage = (decimal?)element.PERCENTAGE;
                    items.Add(item);
                }
                return items;
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
        private List<Models.backbone_db.DetailedProtectionStatus> harvestDetailedProtectionStatus(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.DetailedProtectionStatus> elements = null;
            List<Models.backbone_db.DetailedProtectionStatus> items = new List<Models.backbone_db.DetailedProtectionStatus>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.DetailedProtectionStatus>().Where(s => s.N2K_SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.DetailedProtectionStatus element in elements)
                {
                    Models.backbone_db.DetailedProtectionStatus item = new Models.backbone_db.DetailedProtectionStatus();
                    item.SiteCode = element.N2K_SITECODE;
                    item.Version = pVersion;
                    item.DesignationCode = element.DESIGNATIONCODE;
                    item.OverlapCode = element.OVERLAPCODE;
                    item.OverlapPercentage = (decimal?)element.OVERLAPPERC;
                    item.Convention = element.CONVENTION;
                    items.Add(item);
                }
                return items;
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

        private List<Models.backbone_db.SiteLargeDescriptions> harvestSiteLargeDescriptions(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.Description> elements = null;
            List<Models.backbone_db.SiteLargeDescriptions> items = new List<Models.backbone_db.SiteLargeDescriptions>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.Description>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.Description element in elements)
                {
                    Models.backbone_db.SiteLargeDescriptions item = new Models.backbone_db.SiteLargeDescriptions();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.Quality = element.QUALITY;
                    item.Vulnarab = element.VULNARAB;
                    item.Designation = element.DESIGNATION;
                    item.ManagPlan = element.MANAG_PLAN;
                    item.Documentation = element.DOCUMENTATION;
                    item.OtherCharact = element.OTHERCHARACT;
                    item.ManagConservMeasures = element.MANAG_CONSERV_MEASURES;
                    item.ManagPlanUrl = element.MANAG_PLAN_URL;
                    item.ManagStatus = element.MANAG_STATUS;

                    items.Add(item);
                }
                return items;
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

        private List<Models.backbone_db.SiteOwnerType> harvestSiteOwnerType(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.OwnerType> elements = null;
            List<Models.backbone_db.SiteOwnerType> items = new List<Models.backbone_db.SiteOwnerType>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.OwnerType>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.OwnerType element in elements)
                {
                    Models.backbone_db.SiteOwnerType item = new Models.backbone_db.SiteOwnerType();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.Type = _dataContext.Set<Models.backbone_db.OwnerShipTypes>().Where(s => s.Description == element.TYPE).Select(s => s.Id).FirstOrDefault();
                    item.Percent = (decimal?)element.PERCENT;
                    items.Add(item);
                }
                return items;
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
        /// <param name="pCountryCode">Code of two digits for the country</param>
        /// <param name="pCountryVersion">Number of the version</param>
        private async Task<int> resetEnvirontment(string pCountryCode, int pCountryVersion )
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
            catch (Exception ex) {
                SystemLog.write(SystemLog.errorLevel.Error, ex.Message, "HarvestedService - resetEnvirontment", "");
            }
            return 1;
        }

        /// <summary>
        /// Delete all the changes create by the envelope
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
                SystemLog.write(SystemLog.errorLevel.Error,ex, "HarvestedService - rollback","");

            }
            finally { 
            
            }

        }

    }
}

