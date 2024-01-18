using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using NuGet.Packaging;

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

        public async Task<int> HarvestByCountry(string countryCode, decimal COUNTRYVERSIONID, IEnumerable<SpeciesTypes> _speciesTypes, string versioningDB, string backboneDb, List<Sites> sites)
        {
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            var start = DateTime.Now;
            List<Models.backbone_db.Species> itemsSpecies = new List<Models.backbone_db.Species>();
            List<Models.backbone_db.SpeciesOther> itemsSpeciesOthers = new List<Models.backbone_db.SpeciesOther>();
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                SqlParameter param1 = new SqlParameter("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                String queryString = @"select SITECODE as SiteCode,		
                    SPECIESCODE as SpecieCode,
					CASE WHEN  LOWERBOUND IS NOT NULL then CAST(LOWERBOUND AS int) ELSE NULL END PopulationMin,
					CASE WHEN  UPPERBOUND IS NOT NULL then CAST(UPPERBOUND AS int) ELSE NULL END PopulationMax,
					CASE WHEN  SENSITIVE IS NOT NULL then 
						CASE WHEN SENSITIVE =1 THEN CAST(1 as BIT) ELSE CAST(0 as BIT) END
					ELSE NULL END as SensitiveInfo,
                    RESIDENT as Resident,
                    BREEDING as Breeding,
                    WINTER as Winter ,
                    STAGING as  Staging,
                    --PATH as Path,  -- // ??? PENDING
                    ABUNDANCECATEGORY as AbundaceCategory,
                    Motivation ,
                    POPULATION_TYPE as PopulationType ,
                    CountingUnit,
                    Population,
                    ISOLATIONFACTOR as Insolation,
                    Conservation ,
                    GLOBALIMPORTANCE as Global ,
					--item.NonPersistence = (element.NONPRESENCEINSITE != null) ? ((element.NONPRESENCEINSITE == 1) ? true : false) : null;

					CASE WHEN  NONPRESENCEINSITE IS NOT NULL then 
						CASE WHEN NONPRESENCEINSITE =1 THEN CAST(1 as BIT) ELSE CAST(0 as BIT) END
					ELSE NULL END as NonPersistence,
                    DataQuality ,
                    SPTYPE as SpecieType,
                    SPECIESNAMECLEAN,
                    SPECIESNAME,
                    OTHERSPECIES

                    FROM ContainsSpecies
                    WHERE COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                //Console.WriteLine(String.Format("Start species Query -> {0}", (DateTime.Now - start).TotalSeconds));
                versioningConn.Open();

                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                //Console.WriteLine(String.Format("End Query -> {0}", (DateTime.Now - start).TotalSeconds));
                while (reader.Read())
                {
                    SpecieBase item = new SpecieBase();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.SpecieCode = TypeConverters.CheckNull<string>(reader["SpecieCode"]);
                        item.PopulationMin = TypeConverters.CheckNull<int?>(reader["PopulationMin"]);
                        item.PopulationMax = TypeConverters.CheckNull<int?>(reader["PopulationMax"]);
                        //item.Group = element.GROUP; // PENDING
                        item.SensitiveInfo = TypeConverters.CheckNull<bool?>(reader["SensitiveInfo"]);
                        item.Resident = TypeConverters.CheckNull<string>(reader["Resident"]);
                        item.Breeding = TypeConverters.CheckNull<string>(reader["Breeding"]);
                        item.Winter = TypeConverters.CheckNull<string>(reader["Winter"]);
                        item.Staging = TypeConverters.CheckNull<string>(reader["Staging"]);
                        //item.Path = element.PATH; // ??? PENDING
                        item.AbundaceCategory = TypeConverters.CheckNull<string>(reader["AbundaceCategory"]);
                        item.Motivation = TypeConverters.CheckNull<string>(reader["Motivation"]);
                        item.PopulationType = TypeConverters.CheckNull<string>(reader["PopulationType"]);
                        item.CountingUnit = TypeConverters.CheckNull<string>(reader["CountingUnit"]);
                        item.Population = TypeConverters.CheckNull<string>(reader["Population"]);
                        item.Insolation = TypeConverters.CheckNull<string>(reader["Insolation"]);
                        item.Conservation = TypeConverters.CheckNull<string>(reader["Conservation"]);
                        item.Global = TypeConverters.CheckNull<string>(reader["Global"]);
                        item.NonPersistence = TypeConverters.CheckNull<bool>(reader["NonPersistence"]);
                        item.DataQuality = TypeConverters.CheckNull<string>(reader["DataQuality"]);
                        item.SpecieType = TypeConverters.CheckNull<string>(reader["SpecieType"]);

                        int OtherSpecies = Convert.ToInt32(reader["OTHERSPECIES"].ToString());

                        if (item.SpecieCode is null || item.SpecieCode == "" ||
                            _speciesTypes.Where(a => a.Code == item.SpecieCode && a.Active == true).Count() < 1
                            || OtherSpecies == 1)
                        {
                            //Replace the code (which is Null or empty or no stored in the system)
                            //item.SiteCode = element.SITECODE;
                            item.OtherSpecieCode = item.SpecieCode;
                            item.SpecieCode = (reader["SPECIESNAMECLEAN"] != null) ? reader["SPECIESNAMECLEAN"].ToString() : reader["SPECIESNAME"].ToString();
                            itemsSpeciesOthers.Add(item.getSpeciesOther());
                        }
                        else
                        {
                            itemsSpecies.Add(item.getSpecies());
                        }
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSpecies - Species", "", backboneDb);
                    }
                }

                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));

                try
                {
                    await SpeciesOther.SaveBulkRecord(backboneDb, itemsSpeciesOthers);

                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSpecies - SpeciesOther.SaveBulkRecord", "", backboneDb);
                }

                try
                {
                    await Species.SaveBulkRecord(backboneDb, itemsSpecies);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSpecies - Species.SaveBulkRecord", "", backboneDb);
                }

                //Console.WriteLine(String.Format("End save to list species -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;

            }
            catch (Exception ex)

            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSpecies - HarvestByCountry", "", backboneDb);
                return 0;
            }
            finally
            {
                itemsSpeciesOthers.Clear();
                itemsSpecies.Clear();
                if (versioningConn != null)
                {
                    versioningConn.Close();
                    versioningConn.Dispose();
                    if (command != null) command.Dispose();
                    if (reader != null) await reader.DisposeAsync();
                }

            }
        }




        public async Task<int> ChangeDetectionChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start species ChangeDetection...");
            await Task.Delay(2000);
            Console.WriteLine("==>ENd speciesChangeDetection...");
            return 1;
        }

        public async Task<List<SiteChangeDb>> ChangeDetectionSpecies(List<SpeciesToHarvest> speciesVersioning, List<SpeciesToHarvest> referencedSpecies, List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5, List<SpeciesPriority> speciesPriority, ProcessedEnvelopes? processedEnvelope, N2KBackboneContext _ctx,
            List<SpeciesToHarvestPerEnvelope>? speciesOtherVersioningEnvelope= null, List<SpeciesToHarvestPerEnvelope>? speciesOtherReferenceEnvelope = null
            )
        {
            try
            {
                if (_ctx == null) _ctx = _dataContext;
                var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_dataContext.Database.GetConnectionString(),
                        opt => opt.EnableRetryOnFailure()).Options;
                using (var ctx = new N2KBackboneContext(options))
                {


                    List<SpeciesToHarvest> speciesOtherVersioning = null;
                    if (speciesOtherVersioningEnvelope != null)
                    {
                        speciesOtherVersioning = speciesOtherVersioningEnvelope
                            .Where(spEnv => spEnv.SiteCode == harvestingSite.SiteCode && spEnv.VersionId == storedSite.VersionId)
                            .ToList<SpeciesToHarvest>();
                    }
                    else
                    {
                        speciesOtherVersioning = await ctx.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesOtherBySiteCodeAndVersion  @site, @versionId",
                                                        param3, param4).ToListAsync();
                    }

                    List<SpeciesToHarvest> referencedSpeciesOther = null;
                    if (speciesOtherReferenceEnvelope != null)
                    {
                        referencedSpeciesOther = speciesOtherReferenceEnvelope
                            .Where(spEnv => spEnv.SiteCode == harvestingSite.SiteCode && spEnv.VersionId == (int)param5.Value)
                            .ToList<SpeciesToHarvest>();
                    }
                    else
                    {
                        referencedSpeciesOther = await ctx.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesOtherBySiteCodeAndVersion  @site, @versionId",
                                    param3, param5).ToListAsync();
                    }


                    //For each species in Versioning compare it with that species in backboneDB
                    foreach (SpeciesToHarvest harvestingSpecies in speciesVersioning)
                    {
                        SpeciesToHarvest storedSpecies = referencedSpecies.Where(s => s.SpeciesCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
                        if (storedSpecies != null)
                        {
                            if (storedSpecies.Population.ToUpper() != "D" && harvestingSpecies.Population.ToUpper() == "D")
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Population Decrease";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Warning;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                                siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "Population";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                            else if (storedSpecies.Population.ToUpper() == "D" && harvestingSpecies.Population.ToUpper() != "D")
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Population Increase";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                                siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "Population";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                            else if (storedSpecies.Population.ToUpper() != harvestingSpecies.Population.ToUpper())
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Population Change";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                                siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                                siteChange.Tags = string.Empty;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "Population";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                            if (storedSpecies.PopulationType != harvestingSpecies.PopulationType)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "PopulationType Change";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.PopulationType) ? harvestingSpecies.PopulationType : null;
                                siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.PopulationType) ? storedSpecies.PopulationType : null;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "PopulationType";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }

                            //Priority check is also present in HarvestedService/SitePriorityChecker
                            #region SpeciesPriority
                            SpeciesPriority priorityCount = speciesPriority.Where(s => s.SpecieCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
                            if (priorityCount != null)
                            {
                                //These booleans declare whether or not each species is a priority
                                Boolean isStoredPriority = false;
                                Boolean isHarvestingPriority = false;
                                if ((storedSpecies.Population.ToUpper() != "D" || storedSpecies.Population == null) && (storedSpecies.Motivation == null || storedSpecies.Motivation == ""))
                                    isStoredPriority = true;
                                if ((harvestingSpecies.Population.ToUpper() != "D" || harvestingSpecies.Population == null) && (harvestingSpecies.Motivation == null || harvestingSpecies.Motivation == ""))
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
                                    siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                    siteChange.Tags = string.Empty;
                                    siteChange.NewValue = Convert.ToString(isHarvestingPriority);
                                    siteChange.OldValue = Convert.ToString(isStoredPriority);
                                    siteChange.Code = harvestingSpecies.SpeciesCode;
                                    siteChange.Section = "Species";
                                    siteChange.VersionReferenceId = storedSpecies.VersionId;
                                    siteChange.FieldName = "Priority";
                                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                    siteChange.N2KVersioningVersion = envelope.VersionId;
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
                                    siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                    siteChange.Tags = string.Empty;
                                    siteChange.NewValue = Convert.ToString(isHarvestingPriority);
                                    siteChange.OldValue = Convert.ToString(isStoredPriority);
                                    siteChange.Code = harvestingSpecies.SpeciesCode;
                                    siteChange.Section = "Species";
                                    siteChange.VersionReferenceId = storedSpecies.VersionId;
                                    siteChange.FieldName = "Priority";
                                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                    siteChange.N2KVersioningVersion = envelope.VersionId;
                                    changes.Add(siteChange);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            if (harvestingSpecies.SpeciesCode != null)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Species Added";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = harvestingSpecies.SpeciesCode;
                                siteChange.OldValue = null;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = harvestingSpecies.VersionId;
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                        }
                    }

                    //For each species in backboneDB check if the species still exists in Versioning
                    foreach (SpeciesToHarvest storedSpecies in referencedSpecies)
                    {
                        SpeciesToHarvest harvestingSpecies = speciesVersioning.Where(s => s.SpeciesCode == storedSpecies.SpeciesCode).FirstOrDefault();
                        if (harvestingSpecies == null)
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = storedSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Species";
                            siteChange.ChangeType = "Species Deleted";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Critical;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = null;
                            siteChange.OldValue = storedSpecies.SpeciesCode;
                            siteChange.Code = storedSpecies.SpeciesCode;
                            siteChange.Section = "Species";
                            siteChange.VersionReferenceId = storedSpecies.VersionId;
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                    }

                    //speciesother check
                    foreach (SpeciesToHarvest harvestingSpecies in speciesOtherVersioning)
                    {
                        SpeciesToHarvest storedSpecies = referencedSpeciesOther.Where(s => s.SpeciesCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
                        if (storedSpecies != null)
                        {
                            if (storedSpecies.Population.ToUpper() != "D" && harvestingSpecies.Population.ToUpper() == "D")
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Population Decrease (Other Species)";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                                siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "Population";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                            else if (storedSpecies.Population.ToUpper() == "D" && harvestingSpecies.Population.ToUpper() != "D")
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Population Increase";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                                siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "Population";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                            else if (storedSpecies.Population.ToUpper() != harvestingSpecies.Population.ToUpper())
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Population Change";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.Population) ? harvestingSpecies.Population : null;
                                siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.Population) ? storedSpecies.Population : null;
                                siteChange.Tags = string.Empty;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "Population";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                            if (storedSpecies.PopulationType != harvestingSpecies.PopulationType)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "PopulationType Change";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = !String.IsNullOrEmpty(harvestingSpecies.PopulationType) ? harvestingSpecies.PopulationType : null;
                                siteChange.OldValue = !String.IsNullOrEmpty(storedSpecies.PopulationType) ? storedSpecies.PopulationType : null;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = storedSpecies.VersionId;
                                siteChange.FieldName = "PopulationType";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }

                            #region SpeciesPriority
                            SpeciesPriority priorityCount = speciesPriority.Where(s => s.SpecieCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
                            if (priorityCount != null)
                            {
                                //These booleans declare whether or not each species is a priority
                                Boolean isStoredPriority = false;
                                Boolean isHarvestingPriority = false;
                                if ((storedSpecies.Population.ToUpper() != "D" || storedSpecies.Population == null) && (storedSpecies.Motivation == null || storedSpecies.Motivation == ""))
                                    isStoredPriority = true;
                                if ((harvestingSpecies.Population.ToUpper() != "D" || harvestingSpecies.Population == null) && (harvestingSpecies.Motivation == null || harvestingSpecies.Motivation == ""))
                                    isHarvestingPriority = true;

                                if (isStoredPriority && !isHarvestingPriority)
                                {
                                    SiteChangeDb siteChange = new SiteChangeDb();
                                    siteChange.SiteCode = harvestingSite.SiteCode;
                                    siteChange.Version = harvestingSite.VersionId;
                                    siteChange.ChangeCategory = "Species";
                                    siteChange.ChangeType = "Other Species Losing Priority";
                                    siteChange.Country = envelope.CountryCode;
                                    siteChange.Level = Enumerations.Level.Info;
                                    siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                    siteChange.Tags = string.Empty;
                                    siteChange.NewValue = Convert.ToString(isHarvestingPriority);
                                    siteChange.OldValue = Convert.ToString(isStoredPriority);
                                    siteChange.Code = harvestingSpecies.SpeciesCode;
                                    siteChange.Section = "Species";
                                    siteChange.VersionReferenceId = storedSpecies.VersionId;
                                    siteChange.FieldName = "Priority";
                                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                    siteChange.N2KVersioningVersion = envelope.VersionId;
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
                                    siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                    siteChange.Tags = string.Empty;
                                    siteChange.NewValue = Convert.ToString(isHarvestingPriority);
                                    siteChange.OldValue = Convert.ToString(isStoredPriority);
                                    siteChange.Code = harvestingSpecies.SpeciesCode;
                                    siteChange.Section = "Species";
                                    siteChange.VersionReferenceId = storedSpecies.VersionId;
                                    siteChange.FieldName = "Priority";
                                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                    siteChange.N2KVersioningVersion = envelope.VersionId;
                                    changes.Add(siteChange);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            if (harvestingSpecies.SpeciesCode != null)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Species";
                                siteChange.ChangeType = "Other Species Added";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = harvestingSpecies.SpeciesCode;
                                siteChange.OldValue = null;
                                siteChange.Code = harvestingSpecies.SpeciesCode;
                                siteChange.Section = "Species";
                                siteChange.VersionReferenceId = harvestingSpecies.VersionId;
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                        }
                    }

                    foreach (SpeciesToHarvest storedSpecies in referencedSpeciesOther)
                    {
                        SpeciesToHarvest harvestingSpecies = speciesOtherVersioning.Where(s => s.SpeciesCode == storedSpecies.SpeciesCode && s.PopulationType == storedSpecies.PopulationType).FirstOrDefault();
                        if (harvestingSpecies == null)
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = storedSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Species";
                            siteChange.ChangeType = "Other Species Deleted";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = null;
                            siteChange.OldValue = storedSpecies.SpeciesCode;
                            siteChange.Code = storedSpecies.SpeciesCode;
                            siteChange.Section = "Species";
                            siteChange.VersionReferenceId = storedSpecies.VersionId;
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetectionSpecies - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "", _ctx.Database.GetConnectionString());
            }
            return changes;
        }

    }
}
