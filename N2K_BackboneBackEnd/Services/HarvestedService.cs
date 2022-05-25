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

        public HarvestedService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;

        }

        public HarvestedService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext, IOptions<ConfigSettings> app)
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
                            if (allEnvs.Where(e => e.Version == pendEnv.Id && e.Country == pendEnv.Country && e.Status == HarvestingStatus.Harvesting).ToList().Count == 0)
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

        public async Task<List<HarvestedEnvelope>> Validate(EnvelopesToProcess[] envelopeIDs)
        {
            List<HarvestedEnvelope> result = new List<HarvestedEnvelope>();
            List<SiteChangeDb> changes = new List<SiteChangeDb>();
            //var latestVersions = await _dataContext.Set<ProcessedEnvelopes>().ToListAsync();
            await _dataContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.Changes");


            //from the view vLatest//processedEnvelopes (backbonedb) load the sites with the latest versionid of the countries

            //Load all sites with the CountryVersionID-CountryCode from Versioning
            foreach (EnvelopesToProcess envelope in envelopeIDs)
            {
                try
                {
                    #region unused code
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
                    /*
                    //Start of validation

                    //remove version from database
                    var param1 = new SqlParameter("@country", envelope.CountryCode);
                    var param2 = new SqlParameter("@version", envelope.VersionId);
                    await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spRemoveVersionFromDB  @country, @version", param1, param2);


                    var country = latestVersions.Where(v => v.Country == envelope.CountryCode).FirstOrDefault(); //Coger la ultima version de ese country
                    var lastReferenceCountryVersion = 0;
                    if (country != null) lastReferenceCountryVersion = country.Version;

                    //1. Harvest SiteCodes
                    var harvSiteCode = new HarvestSiteCode(_dataContext, _versioningContext);
                    await harvSiteCode.Harvest(envelope.CountryCode, envelope.VersionId);

                    if (lastReferenceCountryVersion != 0)
                    {
                        var tablesToHarvest = new Dictionary<int, IHarvestingTables>();

                        var harvestingTasks = new List<Task<int>>();
                        var validatingTasks = new List<Task<int>>();

                        //2. Once SiteCodes is harvested we can run a number of task in parallel
                        //Run the validation
                        validatingTasks.Add(harvSiteCode.ValidateChanges(envelope.CountryCode, envelope.VersionId, lastReferenceCountryVersion));

                        //harvest 
                        var habitats = new HarvestHabitats(_dataContext, _versioningContext);
                        var habitatsTask = habitats.Harvest(envelope.CountryCode, envelope.VersionId);
                        tablesToHarvest.Add(habitatsTask.Id, habitats);
                        harvestingTasks.Add(habitatsTask);

                        var species = new HarvestSpecies(_dataContext, _versioningContext);
                        var speciesTask = species.Harvest(envelope.CountryCode, envelope.VersionId);
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
                                            validatingTasks.Add(harvest.ValidateChanges(envelope.CountryCode, envelope.VersionId, lastReferenceCountryVersion));
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
                    */
                    #endregion

                    SqlParameter param1 = new SqlParameter("@country", envelope.CountryCode);
                    SqlParameter param2 = new SqlParameter("@version", envelope.VersionId);

                    List<SiteToHarvest>? sitesVersioning = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSitesByCountryAndVersion  @country, @version",
                                    param1, param2).ToListAsync();
                    List<SiteToHarvest>? referencedSites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetCurrentSitesByCountry  @country",
                                    param1).ToListAsync();

                    #region old referencedSites
                    /*
                    var referencedSites = new List<SiteToHarvest>();
                    if (lastReferenceCountryVersion != 0)
                    {
                        var param3 = new SqlParameter("@version", lastReferenceCountryVersion);
                        referencedSites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.[spGetReferenceSitesByCountryAndVersion]  @country, @version",
                                    param1, param3).ToListAsync();
                    }
                    */
                    #endregion

                    //For each site in Versioning compare it with that site in backboneDB
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                    foreach (SiteToHarvest? harvestingSite in sitesVersioning)
                    {
                        SiteToHarvest? storedSite = referencedSites.Where(s => s.SiteCode == harvestingSite.SiteCode).FirstOrDefault();
                        if (storedSite != null)
                        {
                            //Tolerance values. If the difference between reference and versioning values is bigger than these numbers, then they are notified.
                            //If the tolerance is at 0, then it registers ALL changes, no matter how small they are.
                            double siteAreaHaTolerance = 0.0;
                            double siteLengthKmTolerance = 0.0;
                            double habitatCoverHaTolerance = 0.0;

                            //SiteAttributesChecking
                            changes = await ValidateSiteAttributes(changes, envelope, harvestingSite, storedSite, siteAreaHaTolerance, siteLengthKmTolerance);

                            SqlParameter param3 = new SqlParameter("@site", harvestingSite.SiteCode);
                            int maxVersionSite = harvestingSite.VersionId;
                            SqlParameter param4 = new SqlParameter("@versionId", maxVersionSite);
                            int previousVersionSite = storedSite.VersionId;
                            SqlParameter param5 = new SqlParameter("@versionId", previousVersionSite);

                            //HabitatChecking
                            changes = await ValidateHabitat(changes, envelope, harvestingSite, storedSite, param3, param4, param5, habitatCoverHaTolerance);

                            //SpeciesChecking
                            changes = await ValidateSpecies(changes, envelope, harvestingSite, storedSite, param3, param4, param5);

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
                                Section = "Site"
                            });
                        }
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
                                Section = "Site"
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

                    //for the time being do not load the changes and keep using test_table 

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

        private async Task<List<SiteChangeDb>> ValidateSiteAttributes(List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, double siteAreaHaTolerance, double siteLengthKmTolerance)
        {
            try
            {
                if (harvestingSite.SiteName != storedSite.SiteName)
                {
                    SiteChangeDb siteChange = new SiteChangeDb();
                    siteChange.SiteCode = harvestingSite.SiteCode;
                    siteChange.Version = harvestingSite.VersionId;
                    siteChange.ChangeCategory = "Site General Info";
                    siteChange.ChangeType = "SiteName Changed";
                    siteChange.Country = envelope.CountryCode;
                    siteChange.Level = Enumerations.Level.Info;
                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                    siteChange.Tags = string.Empty;
                    siteChange.NewValue = harvestingSite.SiteName;
                    siteChange.OldValue = storedSite.SiteName;
                    siteChange.Code = harvestingSite.SiteCode;
                    siteChange.Section = "Site";
                    changes.Add(siteChange);
                }
                #region SiteType comparison (unused)
                //if (harvestingSite.SiteType != storedSite.SiteType)
                //{
                //    var siteChange = new SiteChangeDb();
                //    siteChange.SiteCode = harvestingSite.SiteCode;
                //    siteChange.ChangeCategory = "Site General Info";
                //    siteChange.ChangeType = "SiteType Changed";
                //    siteChange.Country = envelope.CountryCode;
                //    siteChange.Level = Enumerations.Level.Critical;
                //    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                //    siteChange.Tags = string.Empty;
                //    changes.Add(siteChange);
                //    numChanges++;
                //}
                #endregion
                if (harvestingSite.AreaHa > storedSite.AreaHa)
                {
                    if (Math.Abs((double)(harvestingSite.AreaHa - storedSite.AreaHa)) > siteAreaHaTolerance)
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Change of area";
                        siteChange.ChangeType = "Area Increased";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Info;
                        siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                        siteChange.NewValue = harvestingSite.AreaHa != -1 ? harvestingSite.AreaHa.ToString() : null;
                        siteChange.OldValue = storedSite.AreaHa != -1 ? storedSite.AreaHa.ToString() : null;
                        siteChange.Tags = string.Empty;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        changes.Add(siteChange);
                    }
                }
                else if (harvestingSite.AreaHa < storedSite.AreaHa)
                {
                    if (Math.Abs((double)(harvestingSite.AreaHa - storedSite.AreaHa)) > siteAreaHaTolerance)
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Change of area";
                        siteChange.ChangeType = "Area Decreased";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Warning;
                        siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                        siteChange.NewValue = harvestingSite.AreaHa != -1 ? harvestingSite.AreaHa.ToString() : null;
                        siteChange.OldValue = storedSite.AreaHa != -1 ? storedSite.AreaHa.ToString() : null;
                        siteChange.Tags = string.Empty;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        changes.Add(siteChange);
                    }
                }
                else if (harvestingSite.AreaHa != storedSite.AreaHa)
                {
                    SiteChangeDb siteChange = new SiteChangeDb();
                    siteChange.SiteCode = harvestingSite.SiteCode;
                    siteChange.Version = harvestingSite.VersionId;
                    siteChange.ChangeCategory = "Change of area";
                    siteChange.ChangeType = "Area Change";
                    siteChange.Country = envelope.CountryCode;
                    siteChange.Level = Enumerations.Level.Info;
                    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                    siteChange.NewValue = harvestingSite.AreaHa != -1 ? harvestingSite.AreaHa.ToString() : null;
                    siteChange.OldValue = storedSite.AreaHa != -1 ? storedSite.AreaHa.ToString() : null;
                    siteChange.Tags = string.Empty;
                    siteChange.Code = harvestingSite.SiteCode;
                    siteChange.Section = "Site";
                    changes.Add(siteChange);
                }
                if (harvestingSite.LengthKm != storedSite.LengthKm)
                {
                    if (Math.Abs((double)(harvestingSite.LengthKm - storedSite.LengthKm)) > siteLengthKmTolerance)
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Site General Info";
                        siteChange.ChangeType = "Length Changed";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Info;
                        siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                        siteChange.NewValue = harvestingSite.LengthKm != -1 ? harvestingSite.LengthKm.ToString() : null;
                        siteChange.OldValue = storedSite.LengthKm != -1 ? storedSite.LengthKm.ToString() : null;
                        siteChange.Tags = string.Empty;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        changes.Add(siteChange);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "ValidateSites - Start - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "");
            }
            return changes;
        }

        private async Task<List<SiteChangeDb>> ValidateHabitat(List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5, double habitatCoverHaTolerance)
        {
            try
            {
                var habitatVersioning = await _dataContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                param3, param4).ToListAsync();
                var referencedHabitats = await _dataContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
                                param3, param5).ToListAsync();

                //For each habitat in Versioning compare it with that habitat in backboneDB
                foreach (var harvestingHabitat in habitatVersioning)
                {
                    var storedHabitat = referencedHabitats.Where(s => s.HabitatCode == harvestingHabitat.HabitatCode && s.PriorityForm == harvestingHabitat.PriorityForm).FirstOrDefault();
                    if (storedHabitat != null)
                    {
                        if (((storedHabitat.RelSurface.ToUpper() == "A" || storedHabitat.RelSurface.ToUpper() == "B") && harvestingHabitat.RelSurface.ToUpper() == "C")
                            || (storedHabitat.RelSurface.ToUpper() == "A" && harvestingHabitat.RelSurface.ToUpper() == "B"))
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Relative surface Decrease";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.RelSurface;
                            siteChange.OldValue = storedHabitat.RelSurface;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            changes.Add(siteChange);
                        }
                        else if (((storedHabitat.RelSurface.ToUpper() == "B" || storedHabitat.RelSurface.ToUpper() == "C") && harvestingHabitat.RelSurface.ToUpper() == "A")
                            || (storedHabitat.RelSurface.ToUpper() == "C" && harvestingHabitat.RelSurface.ToUpper() == "B"))
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Relative surface Increase";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.RelSurface;
                            siteChange.OldValue = storedHabitat.RelSurface;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            changes.Add(siteChange);
                        }
                        else if (storedHabitat.RelSurface.ToUpper() != harvestingHabitat.RelSurface.ToUpper())
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Relative surface Change";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.RelSurface;
                            siteChange.OldValue = storedHabitat.RelSurface;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            changes.Add(siteChange);
                        }
                        if (storedHabitat.Representativity.ToUpper() != "D" && harvestingHabitat.Representativity.ToUpper() == "D")
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Representativity Decrease";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.Representativity;
                            siteChange.OldValue = storedHabitat.Representativity;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            changes.Add(siteChange);
                        }
                        else if (storedHabitat.Representativity.ToUpper() == "D" && harvestingHabitat.Representativity.ToUpper() != "D")
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Representativity Increase";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.Representativity;
                            siteChange.OldValue = storedHabitat.Representativity;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            changes.Add(siteChange);
                        }
                        else if (storedHabitat.Representativity.ToUpper() != harvestingHabitat.Representativity.ToUpper())
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Representativity Change";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.Representativity;
                            siteChange.OldValue = storedHabitat.Representativity;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            changes.Add(siteChange);
                        }
                        if (storedHabitat.Cover_ha > harvestingHabitat.Cover_ha)
                        {
                            if (Math.Abs((double)(storedHabitat.Cover_ha - harvestingHabitat.Cover_ha)) > habitatCoverHaTolerance)
                            {
                                var siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Habitats";
                                siteChange.ChangeType = "Cover_ha Decrease";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Warning;
                                siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                siteChange.NewValue = harvestingHabitat.Cover_ha != -1 ? harvestingHabitat.Cover_ha.ToString() : null;
                                siteChange.OldValue = storedHabitat.Cover_ha != -1 ? storedHabitat.Cover_ha.ToString() : null;
                                siteChange.Tags = string.Empty;
                                siteChange.Code = harvestingHabitat.HabitatCode;
                                siteChange.Section = "Habitats";
                                changes.Add(siteChange);
                            }
                        }
                        else if (storedHabitat.Cover_ha < harvestingHabitat.Cover_ha)
                        {
                            if (Math.Abs((double)(storedHabitat.Cover_ha - harvestingHabitat.Cover_ha)) > habitatCoverHaTolerance)
                            {
                                var siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Habitats";
                                siteChange.ChangeType = "Cover_ha Increase";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                siteChange.NewValue = harvestingHabitat.Cover_ha != -1 ? harvestingHabitat.Cover_ha.ToString() : null;
                                siteChange.OldValue = storedHabitat.Cover_ha != -1 ? storedHabitat.Cover_ha.ToString() : null;
                                siteChange.Tags = string.Empty;
                                siteChange.Code = harvestingHabitat.HabitatCode;
                                siteChange.Section = "Habitats";
                                changes.Add(siteChange);
                            }
                        }
                        else if (storedHabitat.Cover_ha != harvestingHabitat.Cover_ha)
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Cover_ha Change";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.NewValue = harvestingHabitat.Cover_ha != -1 ? harvestingHabitat.Cover_ha.ToString() : null;
                            siteChange.OldValue = storedHabitat.Cover_ha != -1 ? storedHabitat.Cover_ha.ToString() : null;
                            siteChange.Tags = string.Empty;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            changes.Add(siteChange);
                        }
                    }
                    else
                    {
                        changes.Add(new SiteChangeDb
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Habitat Added",
                            ChangeType = "Habitat Added",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Info,
                            Status = Enumerations.SiteChangeStatus.Pending,
                            NewValue = harvestingHabitat.HabitatCode,
                            OldValue = null,
                            Tags = string.Empty,
                            Code = harvestingHabitat.HabitatCode,
                            Section = "Habitats"
                        });
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
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Habitat Deleted",
                            ChangeType = "Habitat Deleted",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = Enumerations.SiteChangeStatus.Pending,
                            NewValue = null,
                            OldValue = storedHabitat.HabitatCode,
                            Tags = string.Empty,
                            Code = storedHabitat.HabitatCode,
                            Section = "Habitats"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "ValidateHabitats - Start - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "");
            }
            return changes;
        }

        private async Task<List<SiteChangeDb>> ValidateSpecies(List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5)
        {
            try
            {
                var speciesVersioning = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                param3, param4).ToListAsync();
                var referencedSpecies = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                param3, param5).ToListAsync();

                //For each species in Versioning compare it with that species in backboneDB
                foreach (var harvestingSpecies in speciesVersioning)
                {
                    var storedSpecies = referencedSpecies.Where(s => s.SpeciesCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
                    if (storedSpecies != null)
                    {
                        if (storedSpecies.Population.ToUpper() != "D" && harvestingSpecies.Population.ToUpper() == "D")
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Species";
                            siteChange.ChangeType = "Population Priority Decrease";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                            siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                            siteChange.Code = harvestingSpecies.SpeciesCode;
                            siteChange.Section = "Species";
                            changes.Add(siteChange);
                        }
                        else if (storedSpecies.Population.ToUpper() == "D" && harvestingSpecies.Population.ToUpper() != "D")
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Species";
                            siteChange.ChangeType = "Population Priority Increase";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                            siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                            siteChange.Code = harvestingSpecies.SpeciesCode;
                            siteChange.Section = "Species";
                            changes.Add(siteChange);
                        }
                        else if (storedSpecies.Population.ToUpper() != harvestingSpecies.Population.ToUpper())
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Species";
                            siteChange.ChangeType = "Population Priority Change";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                            siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                            siteChange.Tags = string.Empty;
                            siteChange.Code = harvestingSpecies.SpeciesCode;
                            siteChange.Section = "Species";
                            changes.Add(siteChange);
                        }
                    }
                    else
                    {
                        if (harvestingSpecies.SpeciesCode != null)
                        {
                            changes.Add(new SiteChangeDb
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Species Added",
                                ChangeType = "Species Added",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Info,
                                Status = Enumerations.SiteChangeStatus.Pending,
                                Tags = string.Empty,
                                NewValue = harvestingSpecies.SpeciesCode,
                                OldValue = null,
                                Code = harvestingSpecies.SpeciesCode,
                                Section = "Species"
                            });
                        }
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
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Species Deleted",
                            ChangeType = "Species Deleted",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = Enumerations.SiteChangeStatus.Pending,
                            Tags = string.Empty,
                            NewValue = null,
                            OldValue = storedSpecies.SpeciesCode,
                            Code = storedSpecies.SpeciesCode,
                            Section = "Species"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "ValidateSpecies - Start - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "");
            }
            return changes;
        }


        public async Task<List<HarvestedEnvelope>> Harvest(EnvelopesToProcess[] envelopeIDs)
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
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - rollback", "");
            }
            finally
            {

            }

        }

    }
}

