using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public class HarvestedService : IHarvestedService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;

        public HarvestedService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
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
            var processed = await _dataContext.ProcessedEnvelopes.ToListAsync();
            foreach (var procCountry in processed)
            {
                var param1 = new SqlParameter("@country", procCountry.Country);
                var param2 = new SqlParameter("@version", procCountry.Version);
                var param3 = new SqlParameter("@importdate", procCountry.ImportDate);

                var list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.spGetPendingCountryVersion  @country, @version,@importdate",
                                param1, param2, param3).ToListAsync();
                if (list.Count > 0) result.AddRange(list);
            }
            return await Task.FromResult(result);
        }

        public async Task<List<HarvestedEnvelope>> Harvest(EnvelopesToProcess[] envelopeIDs)
        {
            var result = new List<HarvestedEnvelope>();
            var changes = new List<SiteChangeDb>();
            var latestVersions = await _dataContext.ProcessedEnvelopes.ToListAsync();
            await _dataContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.test_table");

            //from the view vLatestProcessedEnvelopes (backbonedb) load the sites with the latest versionid of the countries

            //Load all sites with the CountryVersionID-CountryCode from Versioning
            for (var i = 0; i < envelopeIDs.Length; i++)
            {

                var processedEnv = new HarvestedEnvelope
                {
                    CountryCode = envelopeIDs[i].CountryCode,
                    VersionId = envelopeIDs[i].VersionId,
                    NumChanges = 0
                };

                var country = latestVersions.Where(v => v.Country == envelopeIDs[i].CountryCode).FirstOrDefault(); //Coger la ultima version de ese country
                var lastReferenceCountryVersion = 0;
                if (country != null)
                {
                    lastReferenceCountryVersion = country.Version;
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

                result.Add(processedEnv);
                try
                {
                    _dataContext.SiteChanges.AddRange(changes);
                    _dataContext.SaveChanges();
                }
                catch
                {
                    throw;
                }
            }

            return result;
        }

    }
}
