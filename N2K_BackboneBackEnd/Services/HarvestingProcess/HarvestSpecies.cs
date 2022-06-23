using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;

namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public class HarvestSpecies : BaseHarvestingProcess, IHarvestingTables
    {
        public HarvestSpecies(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext) : base(dataContext, versioningContext)
        {
        }

        public async Task<int> Harvest(string countryCode, int versionId)
        {
            try
            {
                Console.WriteLine("=>Start species harvest...");
                await Task.Delay(8000);
                var a = 1;
                var b = 1 / (a - 1);
                Console.WriteLine("=>End species harvest...");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("=>End species harvest with error...");
                return 0;
            }

        }

        public async Task<int> HarvestByCountry(string pCountryCode, int pCountryVersion, int pVersion)
        {
            List<ContainsSpecies> elements = null;
            try
            {
                TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Starting");

                elements = await _versioningContext.Set<ContainsSpecies>().Where(s => s.COUNTRYCODE == pCountryCode && s.COUNTRYVERSIONID == pCountryVersion).ToListAsync();

                foreach (ContainsSpecies element in elements)
                {

                    SpecieBase item = new SpecieBase();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.SpecieCode = element.SPECIESCODE;
                    item.PopulationMin = (element.LOWERBOUND != null) ? Int32.Parse(element.LOWERBOUND) : null;
                    item.PopulationMax = (element.UPPERBOUND != null) ? Int32.Parse(element.UPPERBOUND) : null;
                    //item.Group = element.GROUP; // PENDING
                    item.SensitiveInfo = (element.LOWERBOUND != null) ? ((element.SENSITIVE == 1) ? true : false) : null;
                    item.Resident = element.RESIDENT;
                    item.Breeding = element.BREEDING;
                    item.Winter = element.WINTER;
                    item.Staging = element.STAGING;
                    //item.Path = element.PATH; // ??? PENDING
                    item.AbundaceCategory = element.ABUNDANCECATEGORY;
                    item.Motivation = element.MOTIVATION;
                    item.PopulationType = element.POPULATION_TYPE;
                    item.CountingUnit = element.COUNTINGUNIT;
                    item.Population = element.POPULATION;
                    item.Insolation = element.ISOLATIONFACTOR;
                    item.Conservation = element.CONSERVATION;
                    item.Global = element.GLOBALIMPORTANCE;
                    item.NonPersistence = (element.NONPRESENCEINSITE != null) ? ((element.NONPRESENCEINSITE == 1) ? true : false) : null;
                    item.DataQuality = element.DATAQUALITY;
                    item.SpecieType = element.SPTYPE;

                    if (element.SPECIESCODE is null || _dataContext.Set<SpeciesTypes>().Where(a => a.Code == element.SPECIESCODE).Count() < 1)
                    {
                        //Use the specie name as a code
                        item.SpecieCode = element.SPECIESNAMECLEAN;
                        _dataContext.Set<SpeciesOther>().Add(item.getSpeciesOther());
                    }
                    else
                    {
                        _dataContext.Set<Species>().Add(item.getSpecies());
                    }


                }

                TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Exit");
                return 0;
            }

        }

        public async Task<int> HarvestBySite(string pSiteCode, decimal pSiteVersion, int pVersion)
        {
            List<ContainsSpecies> elements = null;
            try
            {
                TimeLog.setTimeStamp("Species for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Processing");
                elements = await _versioningContext.Set<ContainsSpecies>().Where(s => s.SITECODE == pSiteCode && s.VERSIONID == pSiteVersion).ToListAsync();
                foreach (ContainsSpecies element in elements)
                {

                    //Check id the specie code is null or not present in the catalog
                    SpecieBase item = new SpecieBase();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.SpecieCode = element.SPECIESCODE;
                    item.PopulationMin = (element.LOWERBOUND != null) ? Int32.Parse(element.LOWERBOUND) : null;
                    item.PopulationMax = (element.UPPERBOUND != null) ? Int32.Parse(element.UPPERBOUND) : null;
                    //item.Group = element.GROUP; // PENDING
                    item.SensitiveInfo = (element.LOWERBOUND != null) ? ((element.SENSITIVE == 1) ? true : false) : null;
                    item.Resident = element.RESIDENT;
                    item.Breeding = element.BREEDING;
                    item.Winter = element.WINTER;
                    item.Staging = element.STAGING;
                    //item.Path = element.PATH; // ??? PENDING
                    item.AbundaceCategory = element.ABUNDANCECATEGORY;
                    item.Motivation = element.MOTIVATION;
                    item.PopulationType = element.POPULATION_TYPE;
                    item.CountingUnit = element.COUNTINGUNIT;
                    item.Population = element.POPULATION;
                    item.Insolation = element.ISOLATIONFACTOR;
                    item.Conservation = element.CONSERVATION;
                    item.Global = element.GLOBALIMPORTANCE;
                    item.NonPersistence = (element.NONPRESENCEINSITE != null) ? ((element.NONPRESENCEINSITE == 1) ? true : false) : null;
                    item.DataQuality = element.DATAQUALITY;
                    item.SpecieType = element.SPTYPE;

                    if (element.SPECIESCODE is null || element.SPECIESCODE == "" || _dataContext.Set<SpeciesTypes>().Where(a => a.Code == element.SPECIESCODE && a.Active == true).Count() < 1)
                    {
                        //Replace the code (which is Null or empty or no stored in the system)
                        //item.SiteCode = element.SITECODE;
                        item.SpecieCode = (element.SPECIESNAMECLEAN != null) ? element.SPECIESNAMECLEAN : element.SPECIESNAME;
                        _dataContext.Set<SpeciesOther>().Add(item.getSpeciesOther());
                    }
                    else
                    {
                        _dataContext.Set<Species>().Add(item.getSpecies());
                    }
                }

                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestSpecies - HarvestBySite", "");

                return 0;
            }
            finally
            {
                TimeLog.setTimeStamp("Species for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Exit");
            }

        }

        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start species validate...");
            await Task.Delay(2000);
            Console.WriteLine("==>ENd speciesvalidate...");
            return 1;
        }

        public async Task<List<SiteChangeDb>> ValidateSpecies(List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5, List<SpeciePriority> speciesPriority)
        {
            try
            {
                List<SpeciesToHarvest> speciesVersioning = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                param3, param4).ToListAsync();
                List<SpeciesToHarvest> referencedSpecies = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
                                param3, param5).ToListAsync();

                //For each species in Versioning compare it with that species in backboneDB
                foreach (SpeciesToHarvest harvestingSpecies in speciesVersioning)
                {
                    SpeciesToHarvest storedSpecies = referencedSpecies.Where(s => s.SpeciesCode == harvestingSpecies.SpeciesCode && s.PopulationType == harvestingSpecies.PopulationType).FirstOrDefault();
                    if (storedSpecies != null)
                    {
                        if (storedSpecies.Population.ToUpper() != "D" && harvestingSpecies.Population.ToUpper() == "D")
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
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
                            siteChange.VersionReferenceId = storedSpecies.VersionId;
                            siteChange.FieldName = "Population";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            changes.Add(siteChange);
                        }
                        else if (storedSpecies.Population.ToUpper() == "D" && harvestingSpecies.Population.ToUpper() != "D")
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
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
                            siteChange.VersionReferenceId = storedSpecies.VersionId;
                            siteChange.FieldName = "Population";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            changes.Add(siteChange);
                        }
                        else if (storedSpecies.Population.ToUpper() != harvestingSpecies.Population.ToUpper())
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
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
                            siteChange.VersionReferenceId = storedSpecies.VersionId;
                            siteChange.FieldName = "Population";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            changes.Add(siteChange);
                        }

                        #region SpeciesPriority
                        SpeciePriority priorityCount = speciesPriority.Where(s => s.SpecieCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
                        if (priorityCount != null)
                        {
                            //These booleans declare whether or not each species is a priority
                            Boolean isStoredPriority = false;
                            Boolean isHarvestingPriority = false;
                            if (storedSpecies.Population.ToUpper() != "D")
                                isStoredPriority = true;
                            if (harvestingSpecies.Population.ToUpper() != "D")
                                isHarvestingPriority = true;

                            if (isStoredPriority && !isHarvestingPriority)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Species Losing Priority";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Critical;
                                siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = Convert.ToString(isHarvestingPriority);
                                siteChange.OldValue = Convert.ToString(isStoredPriority);
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "Priority";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                changes.Add(siteChange);
                            }
                            else if (!isStoredPriority && isHarvestingPriority)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Species Getting Priority";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = Convert.ToString(isHarvestingPriority);
                                siteChange.OldValue = Convert.ToString(isStoredPriority);
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "Priority";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                changes.Add(siteChange);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        if (harvestingSpecies.SpeciesCode != null)
                        {
                            changes.Add(new SiteChangeDb
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Species",
                                ChangeType = "Species Added",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Info,
                                Status = Enumerations.SiteChangeStatus.Pending,
                                Tags = string.Empty,
                                NewValue = harvestingSpecies.SpeciesCode,
                                OldValue = null,
                                Code = harvestingSpecies.SpeciesCode,
                                Section = "Species",
                                VersionReferenceId = harvestingSpecies.VersionId,
                                ReferenceSiteCode = storedSite.SiteCode
                            });
                        }
                    }
                }

                //For each species in backboneDB check if the species still exists in Versioning
                foreach (SpeciesToHarvest storedSpecies in referencedSpecies)
                {
                    SpeciesToHarvest harvestingSpecies = speciesVersioning.Where(s => s.SpeciesCode == storedSpecies.SpeciesCode && s.PopulationType == storedSpecies.PopulationType).FirstOrDefault();
                    if (harvestingSpecies == null)
                    {
                        changes.Add(new SiteChangeDb
                        {
                            SiteCode = storedSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Species",
                            ChangeType = "Species Deleted",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = Enumerations.SiteChangeStatus.Pending,
                            Tags = string.Empty,
                            NewValue = null,
                            OldValue = storedSpecies.SpeciesCode,
                            Code = storedSpecies.SpeciesCode,
                            Section = "Species",
                            VersionReferenceId = storedSpecies.VersionId,
                            ReferenceSiteCode = storedSite.SiteCode
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

    }
}
