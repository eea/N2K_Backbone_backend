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

        public async Task<int> HarvestByCountry(string pCountryCode, int pCountryVersion, int pVersion)
        {
            List<ContainsSpecies> elements = null;
            try
            {
                //TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Starting");

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
                        item.getSpeciesOther().SaveRecord(this._dataContext.Database.GetConnectionString());
                    }
                    else
                    {
                        item.getSpecies().SaveRecord(this._dataContext.Database.GetConnectionString());
                    }


                }

                //TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                //TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Exit");
                return 0;
            }

        }                

        public async Task<int> HarvestByCountry(string countryCode, decimal COUNTRYVERSIONID,  IEnumerable<SpeciesTypes> _speciesTypes, string versioningDB, string backboneDb, List<Sites> sites, IDictionary<Type, object> _siteItems)
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
                    SPECIESNAME

                    FROM ContainsSpecies
                    WHERE COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                Console.WriteLine(String.Format("Start species Query -> {0}", (DateTime.Now - start).TotalSeconds));
                versioningConn.Open();

                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                Console.WriteLine(String.Format("End Query -> {0}", (DateTime.Now - start).TotalSeconds));
                while (reader.Read())
                {
                    SpecieBase item = new SpecieBase();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    item.Version = 0;
                    if (sites.Any(s=> s.SiteCode== item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s=>s.SiteCode== item.SiteCode).Version;
                    }
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

                    if (reader["SiteCode"] is null || reader["SpecieCode"].ToString() == "" ||
                        _speciesTypes.Where(a => a.Code == item.SiteCode && a.Active == true).Count() < 1)
                    {
                        //Replace the code (which is Null or empty or no stored in the system)
                        //item.SiteCode = element.SITECODE;
                        item.SpecieCode = (reader["SPECIESNAMECLEAN"] != null) ? reader["SPECIESNAMECLEAN"].ToString() : reader["SPECIESNAME"].ToString();
                        itemsSpeciesOthers.Add(item.getSpeciesOther());
                    }
                    else
                    {
                        itemsSpecies.Add(item.getSpecies());
                    }
                }

                Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));

                try
                {
                    await SpeciesOther.SaveBulkRecord(backboneDb, itemsSpeciesOthers);

                }
                catch (Exception ex)
                {
                    SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - SpeciesOther.SaveBulkRecord", "");
                }

                try
                {
                    await Species.SaveBulkRecord( backboneDb, itemsSpecies);
                }
                catch (Exception ex)
                {
                    SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - Species.SaveBulkRecord", "");
                }

                Console.WriteLine(String.Format("End save to list species -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;

            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestSpecies - HarvestBySite", "");
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

        public async Task<int> HarvestBySite(string pSiteCode,int pVersion, IList<Models.backbone_db.SpecieBase> countrySpecies, IDictionary<Type, object> _siteItems)
        {
            try
            {
                await Task.Delay(1);
                var itemsSpecies = countrySpecies.Where(cp => cp.SiteCode == pSiteCode && cp.Other == false)
                    .Select(c => { c.Version = pVersion; return c; })
                    .Select(c => c.getSpecies()).ToList();

                if (itemsSpecies.Count > 0)
                {
                    var tt = 1;
                }

                List<SpeciesOther> itemsSpeciesOthers = countrySpecies.Where(cp => cp.SiteCode == pSiteCode && cp.Other == true)
                    .Select(c => { c.Version = pVersion; return c; })
                    .Select(c => c.getSpeciesOther()).ToList();

                List<Species>  _listed1 = (List<Species>)_siteItems[typeof(List<Species>)];
                _listed1.AddRange(_listed1);
                _siteItems[typeof(List<Species>)] = _listed1;

                List<SpeciesOther> _listed2 = (List<SpeciesOther>)_siteItems[typeof(List<SpeciesOther>)];
                _listed2.AddRange(itemsSpeciesOthers);
                _siteItems[typeof(List<SpeciesOther>)] = _listed2;

                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestSpecies - HarvestBySite", "");

                return 0;
            }



}


/*
public async Task<int> HarvestBySite(string pSiteCode, decimal pSiteVersion, int pVersion, IEnumerable<SpeciesTypes> _speciesTypes, string versioningDB,IDictionary<Type, object> _siteItems)
{
    SqlConnection versioningConn = null;
    SqlCommand command = null;
    SqlDataReader reader = null;
    var start = DateTime.Now;
    try
    {
        List<Models.backbone_db.Species> itemsSpecies = new List<Models.backbone_db.Species>();
        List<Models.backbone_db.SpeciesOther> itemsSpeciesOthers = new List<Models.backbone_db.SpeciesOther>();

        versioningConn = new SqlConnection(versioningDB);
        SqlParameter param1 = new SqlParameter("@SITECODE", pSiteCode);
        SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pSiteVersion);
        SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

        String queryString = @"select SITECODE as SiteCode, @NEWVERSION as Version,				
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
            SPECIESNAME

            FROM ContainsSpecies
            WHERE SITECODE=@SITECODE and VERSIONID=@COUNTRYVERSIONID";

        Console.WriteLine(String.Format("Start species Query -> {0}", (DateTime.Now - start).TotalSeconds));
        versioningConn.Open();

        command = new SqlCommand(queryString, versioningConn);
        command.Parameters.Add(param1);
        command.Parameters.Add(param2);
        command.Parameters.Add(param3);
        reader = await command.ExecuteReaderAsync();
        Console.WriteLine(String.Format("End Query -> {0}", (DateTime.Now - start).TotalSeconds));
        while (reader.Read())
        {
            SpecieBase item = new SpecieBase();
            item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
            item.Version = pVersion;
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

            if (reader["SiteCode"] is null || reader["SpecieCode"].ToString() == "" ||
                _speciesTypes.Where(a => a.Code == item.SiteCode && a.Active == true).Count() < 1)
            {
                //Replace the code (which is Null or empty or no stored in the system)
                //item.SiteCode = element.SITECODE;
                item.SpecieCode = (reader["SPECIESNAMECLEAN"] != null) ? reader["SPECIESNAMECLEAN"].ToString() : reader["SPECIESNAME"].ToString();
                itemsSpeciesOthers.Add(item.getSpeciesOther());
            }
            else
            {
                itemsSpecies.Add(item.getSpecies());
            }
        }
        Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));

        List<Species> _listed1 = (List<Species>)_siteItems[typeof(List<Species>)];
        _listed1.AddRange(itemsSpecies);
        _siteItems[typeof(List<Species>)] = _listed1;

        List<SpeciesOther> _listed2 = (List<SpeciesOther>)_siteItems[typeof(List<SpeciesOther>)];
        _listed2.AddRange(itemsSpeciesOthers);
        _siteItems[typeof(List<SpeciesOther>)] = _listed2;
        Console.WriteLine(String.Format("End save to list species -> {0}", (DateTime.Now - start).TotalSeconds));
        return 1;
    }
    catch (Exception ex)
    {
        SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestSpecies - HarvestBySite", "");

        return 0;
    }
    finally
    {
        if (versioningConn != null)
        {
            versioningConn.Close();
            versioningConn.Dispose();
            if (command != null) command.Dispose();                    
            if (reader != null) await reader.DisposeAsync();
        }
    }
}
*/

public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start species validate...");
            await Task.Delay(2000);
            Console.WriteLine("==>ENd speciesvalidate...");
            return 1;
        }

        public async Task<List<SiteChangeDb>> ValidateSpecies(List<SpeciesToHarvest> speciesVersioning, List<SpeciesToHarvest> referencedSpecies, List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5, List<SpeciePriority> speciesPriority, ProcessedEnvelopes? processedEnvelope)
        {
            try
            {
                List<SpeciesToHarvest> speciesOtherVersioning = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesOtherBySiteCodeAndVersion  @site, @versionId",
                                param3, param4).ToListAsync();
                List<SpeciesToHarvest> referencedSpeciesOther = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesOtherBySiteCodeAndVersion  @site, @versionId",
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
                            siteChange.ChangeType = "Population Priority Increase";
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
                            siteChange.ChangeType = "Population Priority Change";
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
                    SpeciesToHarvest harvestingSpecies = speciesVersioning.Where(s => s.SpeciesCode == storedSpecies.SpeciesCode && s.PopulationType == storedSpecies.PopulationType).FirstOrDefault();
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
                    SpeciesToHarvest storedSpecies = referencedSpeciesOther.Where(s => s.SpeciesCode == harvestingSpecies.SpeciesCode && s.PopulationType == harvestingSpecies.PopulationType).FirstOrDefault();
                    if (storedSpecies != null)
                    {
                        if (storedSpecies.Population.ToUpper() != "D" && harvestingSpecies.Population.ToUpper() == "D")
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Species";
                            siteChange.ChangeType = "Population Priority Decrease (Other Species)";
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
                            siteChange.ChangeType = "Population Priority Increase";
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
                            siteChange.ChangeType = "Population Priority Change";
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
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "ValidateSpecies - Start - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "");
            }
            return changes;
        }

    }
}
