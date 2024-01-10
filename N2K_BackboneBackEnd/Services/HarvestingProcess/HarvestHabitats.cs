using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.Collections.Generic;
using System.Xml.Linq;

namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public class HarvestHabitats : BaseHarvestingProcess, IHarvestingTables
    {
        public HarvestHabitats(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext) : base(dataContext, versioningContext)
        {
        }

        public async Task<int> Harvest(string countryCode, int versionId)
        {
            try
            {
                Console.WriteLine("=>Start habitat harvest...");
                await Task.Delay(8000);
                var a = 1;
                var b = 1 / (a - 1);
                Console.WriteLine("=>End habitat harvest...");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("=>End species harvest with error...");
                return 0;
            }
        }

        public async Task<int> HarvestByCountry(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, IList<DataQualityTypes> dataQualityTypes, List<Sites> bbSites)
        {
            try
            {
                //TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Starting");
                //Console.WriteLine("=>Start full habitat harvest by country...");
                //string versioningDB = versioningContext.Database.GetConnectionString();
                await HarvestHabitatsByCountry(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, dataQualityTypes, bbSites);
                await HarvestDescribeSitesByCountry(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);

                //Console.WriteLine("=>End full habitat harvest by country...");
                //TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("=>End full habitat harvest by country with error... {0}", ex.Message));
                //TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Exit");
                return 0;
            }
        }

        public async Task<int> HarvestBySite(NaturaSite pVSite, Sites? bbSite, IList<DataQualityTypes> dataQualityTypes, N2K_VersioningContext versioningContext, IDictionary<Type, object> _siteItems)
        {
            try
            {
                //TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Starting");
                //Console.WriteLine("=>Start full habitat harvest by site...");
                string versioningDB = versioningContext.Database.GetConnectionString();
                await HarvestHabitatsBySite(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, dataQualityTypes, _siteItems);
                await HarvestDescribeSitesBySite(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);

                //Console.WriteLine("=>End full habitat harvest by site...");
                //TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("=>End full habitat harvest by site with error... {0}", ex.Message));
                //TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Exit");
                return 0;
            }
        }

        public async Task<int> HarvestHabitatsByCountry(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, IList<DataQualityTypes> dataQualityTypes, List<Sites> sites)
        {
            List<Habitats> items = new List<Habitats>();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            var start = DateTime.Now;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                SqlParameter param1 = new SqlParameter("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                String queryString = @"select COUNTRYCODE as CountryCode,
                                        VERSIONID as Version,
                                        COUNTRYVERSIONID as CountryVersionId ,
                                        SITECODE as SiteCode,
                                        HABITATCODE as HabitatCode,
                                        PERCENTAGECOVER as PercentageCover,
                                        REPRESENTATIVITY as Representativity,
                                        RELSURFACE as RelSurface,
                                        CONSSTATUS as ConsStatus,
                                        GLOBALASSESMENT as GlobalAssesment,
                                        STARTDATE as StartDate,
                                        ENDDATE as EndDate,
                                        RID as Rid,
                                        NONPRESENCEINSITE as NonPresenceSite,
                                        CAVES as Caves ,
                                        DATAQUALITY as DataQuality,
                                        COVER_HA as Cover_HA,
                                        PF
                                       from CONTAINSHABITAT
                                       where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";


                //Console.WriteLine(String.Format("Start habitats Query -> {0}", (DateTime.Now - start).TotalSeconds));
                versioningConn.Open();

                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                //Console.WriteLine(String.Format("End Query -> {0}", (DateTime.Now - start).TotalSeconds));
                while (reader.Read())
                {
                    Habitats item = new Habitats();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.HabitatCode = TypeConverters.CheckNull<string>(reader["HabitatCode"]);
                        item.CoverHA = null;
                        if (reader["Cover_HA"] != null)
                            if (reader["Cover_HA"].ToString() != "")
                                item.CoverHA = Convert.ToDecimal(TypeConverters.CheckNull<double>(reader["Cover_HA"]));
                        item.PriorityForm = TypeConverters.CheckNull<bool>(reader["PF"]);
                        item.Representativity = TypeConverters.CheckNull<string>(reader["Representativity"]);
                        item.DataQty = null;
                        if (reader["DataQuality"] != null)
                            if (reader["DataQuality"].ToString() != "")
                                item.DataQty = dataQualityTypes.Where(d => d.HabitatCode == reader["DataQuality"].ToString()).Select(d => d.Id).FirstOrDefault();
                        item.GlobalAssesments = TypeConverters.CheckNull<string>(reader["GlobalAssesment"]);
                        item.RelativeSurface = TypeConverters.CheckNull<string>(reader["RelSurface"]);
                        item.Percentage = null;
                        if (reader["PercentageCover"] != null)
                            if (reader["PercentageCover"].ToString() != "")
                                item.Percentage = Convert.ToDecimal(TypeConverters.CheckNull<double>(reader["PercentageCover"]));

                        item.ConsStatus = TypeConverters.CheckNull<string>(reader["ConsStatus"]);

                        if (reader["Caves"] != null)
                            if (reader["Caves"].ToString() != "")
                                item.Caves = TypeConverters.CheckNull<decimal>(reader["Caves"]).ToString(); // ???
                        item.PF = TypeConverters.CheckNull<bool>(reader["PF"]).ToString(); // ??? PENDING The same as PriorityForm

                        if (reader["NonPresenceSite"] != null)
                            if (reader["NonPresenceSite"].ToString() != "")
                                item.NonPresenciInSite = Convert.ToInt32(reader["NonPresenceSite"].ToString()); // ???TypeConverters.CheckNull<string>(reader["PercentageCover"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestHabitats - Habitats", "", backboneDb);
                    }
                }

                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));

                try
                {
                    await Habitats.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestHabitats - Habitats.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list habitats -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;

            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - HarvestHabitatsByCountry", "", backboneDb);
                return 0;
            }
            finally
            {
                items.Clear();
                if (versioningConn != null)
                {
                    versioningConn.Close();
                    versioningConn.Dispose();
                    if (command != null) command.Dispose();
                    if (reader != null) await reader.DisposeAsync();
                }

            }
        }

        public async Task<int> HarvestHabitatsBySite(NaturaSite pVSite, int pVersion, string backboneDb, string versioningDB, IList<DataQualityTypes> dataQualityTypes, IDictionary<Type, object> _siteItems)
        {
            List<Habitats> items = new List<Habitats>();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                String queryString = @"select COUNTRYCODE as CountryCode,VERSIONID as Version,COUNTRYVERSIONID as CountryVersionId ,
                                       SITECODE as SiteCode,HABITATCODE as HabitatCode,PERCENTAGECOVER as PercentageCover,
                                       REPRESENTATIVITY as Representativity,RELSURFACE as RelSurface,CONSSTATUS as ConsStatus,
                                       GLOBALASSESMENT as GlobalAssesment,STARTDATE as StartDate,ENDDATE as EndDate,RID as Rid,NONPRESENCEINSITE as NonPresenceSite,
                                       CAVES as Caves ,DATAQUALITY as DataQuality,COVER_HA as Cover_HA,PF
                                       from CONTAINSHABITAT
                                       where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);
                reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    Habitats item = new Habitats();
                    item.SiteCode = reader["SiteCode"].ToString();
                    item.Version = pVersion;
                    item.HabitatCode = TypeConverters.CheckNull<string>(reader["HabitatCode"]);
                    item.CoverHA = null;
                    if (reader["Cover_HA"] != null)
                        if (reader["Cover_HA"].ToString() != "")
                            item.CoverHA = Convert.ToDecimal(TypeConverters.CheckNull<double>(reader["Cover_HA"]));

                    item.PriorityForm = TypeConverters.CheckNull<bool?>(reader["PF"]);
                    item.Representativity = TypeConverters.CheckNull<string>(reader["Representativity"]);
                    item.DataQty = null;
                    if (reader["DataQuality"] != null)
                        item.DataQty = dataQualityTypes.Where(d => d.HabitatCode == reader["DataQuality"].ToString()).Select(d => d.Id).FirstOrDefault();

                    item.GlobalAssesments = TypeConverters.CheckNull<string>(reader["GlobalAssesment"]);
                    item.RelativeSurface = TypeConverters.CheckNull<string>(reader["RelSurface"]);
                    item.Percentage = null;
                    if (reader["PercentageCover"] != null)
                        if (reader["PercentageCover"].ToString() != "")
                            item.Percentage = Convert.ToDecimal(TypeConverters.CheckNull<double>(reader["PercentageCover"]));

                    item.ConsStatus = TypeConverters.CheckNull<string>(reader["ConsStatus"]);

                    if (reader["Caves"] != null)
                        item.Caves = TypeConverters.CheckNull<decimal>(reader["Caves"]).ToString(); // ???
                    item.PF = TypeConverters.CheckNull<bool>(reader["PF"]).ToString(); // ??? PENDING The same as PriorityForm

                    if (reader["NonPresenceSite"] != null)
                        if (reader["NonPresenceSite"].ToString() != "")
                            item.NonPresenciInSite = Convert.ToInt32(reader["NonPresenceSite"].ToString()); // ???TypeConverters.CheckNull<string>(reader["PercentageCover"]);
                    items.Add(item);
                }

                List<Habitats> _listed = (List<Habitats>)_siteItems[typeof(List<Habitats>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<Habitats>)] = _listed;
                return 1;


            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestHabitats - HarvestHabitatsBySite", "", backboneDb);
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

        public async Task<int> HarvestDescribeSitesByCountry(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<DescribeSites> items = new List<DescribeSites>();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var start = DateTime.Now;

            try
            {
                versioningConn = new SqlConnection(versioningDB);
                SqlParameter param1 = new SqlParameter("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                String queryString = @"select DISTINCT SITECODE as SiteCode,
                                       HABITATCODE as HabitatCode,
                                       max(PERCENTAGECOVER) as PercentageCover
                                       from DESCRIBESSITES 
                                       where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID
									   group by SITECODE,HABITATCODE";

                //Console.WriteLine(String.Format("Start describeSites Query -> {0}", (DateTime.Now - start).TotalSeconds));

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                //Console.WriteLine(String.Format("End Query -> {0}", (DateTime.Now - start).TotalSeconds));

                while (reader.Read())
                {
                    DescribeSites item = new DescribeSites();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]); ;
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.HabitatCode = TypeConverters.CheckNull<string>(reader["HabitatCode"]);
                        item.Percentage = null;
                        if (reader["PercentageCover"] != null)
                            if (reader["PercentageCover"].ToString() != "")
                                item.Percentage = TypeConverters.CheckNull<decimal>(reader["PercentageCover"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestHabitats - DescribeSites", "", backboneDb);
                    }
                }
                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await DescribeSites.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestHabitats - DescribeSites.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list describe sites -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - HarvestDescribeSitesByCountry", "", backboneDb);
                return 0;
            }
            finally
            {
                items.Clear();
                if (versioningConn != null)
                {
                    versioningConn.Close();
                    versioningConn.Dispose();
                    if (command != null) command.Dispose();
                    if (reader != null) await reader.DisposeAsync();
                }
            }
        }

        public async Task<int> HarvestDescribeSitesBySite(NaturaSite pVSite, int pVersion, string backboneDb, string versioningDB, IDictionary<Type, object> _siteItems)
        {
            List<DescribeSites> items = new List<DescribeSites>();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                versioningConn = new SqlConnection(versioningDB);

                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                String queryString = @"select COUNTRYCODE as CountryCode, VERSIONID as  Version, COUNTRYVERSIONID as CountryVersionID, SITECODE as SiteCode, HABITATCODE as HabitatCode, PERCENTAGECOVER as PercentageCover, RID 
                            from DESCRIBESSITES 
                            where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";


                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    DescribeSites item = new DescribeSites();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]); ;
                    item.Version = pVersion;
                    item.HabitatCode = TypeConverters.CheckNull<string>(reader["HabitatCode"]);
                    item.Percentage = null;
                    if (reader["PercentageCover"] != null)
                        if (reader["PercentageCover"].ToString() != "")
                            item.Percentage = TypeConverters.CheckNull<decimal>(reader["PercentageCover"]);
                    items.Add(item);
                }

                List<DescribeSites> _listed = (List<DescribeSites>)_siteItems[typeof(List<DescribeSites>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<DescribeSites>)] = _listed;

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestedService - HarvestDescribeSitesBySite", "", backboneDb);
                return 0;
            }
            finally
            {
                if (versioningConn != null)
                {
                    versioningConn.Close();
                    versioningConn.Dispose();
                    command.Dispose();
                    await reader.DisposeAsync();
                }
            }
        }

        public async Task<int> ChangeDetectionChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start HarvestHabitats ChangeDetection...");
            await Task.Delay(2000);
            Console.WriteLine("==>End HarvestHabitats ChangeDetection...");
            return 1;
        }

        public async Task<List<SiteChangeDb>> ChangeDetectionHabitat(List<HabitatToHarvest> habitatVersioning, List<HabitatToHarvest> referencedHabitats, List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5, double habitatCoverHaTolerance, List<HabitatPriority> habitatPriority, ProcessedEnvelopes? processedEnvelope, N2KBackboneContext? ctx = null)
        {
            try
            {
                if (ctx == null) ctx = _dataContext;
                await Task.Delay(1);
                //For each habitat in Versioning compare it with that habitat in backboneDB
                foreach (HabitatToHarvest harvestingHabitat in habitatVersioning)
                {
                    HabitatToHarvest storedHabitat = referencedHabitats.Where(s => s.HabitatCode == harvestingHabitat.HabitatCode).FirstOrDefault();
                    if (storedHabitat != null)
                    {
                        if (((storedHabitat.RelSurface.ToUpper() == "A" || storedHabitat.RelSurface.ToUpper() == "B") && harvestingHabitat.RelSurface.ToUpper() == "C")
                            || (storedHabitat.RelSurface.ToUpper() == "A" && harvestingHabitat.RelSurface.ToUpper() == "B"))
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Relative surface Decrease";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.RelSurface;
                            siteChange.OldValue = storedHabitat.RelSurface;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            siteChange.VersionReferenceId = storedHabitat.VersionId;
                            siteChange.FieldName = "RelSurface";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                        else if (((storedHabitat.RelSurface.ToUpper() == "B" || storedHabitat.RelSurface.ToUpper() == "C") && harvestingHabitat.RelSurface.ToUpper() == "A")
                            || (storedHabitat.RelSurface.ToUpper() == "C" && harvestingHabitat.RelSurface.ToUpper() == "B"))
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Relative surface Increase";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.RelSurface;
                            siteChange.OldValue = storedHabitat.RelSurface;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            siteChange.VersionReferenceId = storedHabitat.VersionId;
                            siteChange.FieldName = "RelSurface";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                        else if (storedHabitat.RelSurface.ToUpper() != harvestingHabitat.RelSurface.ToUpper())
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Relative surface Change";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.RelSurface;
                            siteChange.OldValue = storedHabitat.RelSurface;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            siteChange.VersionReferenceId = storedHabitat.VersionId;
                            siteChange.FieldName = "RelSurface";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                        if (storedHabitat.Representativity.ToUpper() != "D" && harvestingHabitat.Representativity.ToUpper() == "D")
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Representativity Decrease";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.Representativity;
                            siteChange.OldValue = storedHabitat.Representativity;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            siteChange.VersionReferenceId = storedHabitat.VersionId;
                            siteChange.FieldName = "Representativity";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                        else if (storedHabitat.Representativity.ToUpper() == "D" && harvestingHabitat.Representativity.ToUpper() != "D")
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Representativity Increase";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.Representativity;
                            siteChange.OldValue = storedHabitat.Representativity;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            siteChange.VersionReferenceId = storedHabitat.VersionId;
                            siteChange.FieldName = "Representativity";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                        else if (storedHabitat.Representativity.ToUpper() != harvestingHabitat.Representativity.ToUpper())
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Representativity Change";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.Tags = string.Empty;
                            siteChange.NewValue = harvestingHabitat.Representativity;
                            siteChange.OldValue = storedHabitat.Representativity;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            siteChange.VersionReferenceId = storedHabitat.VersionId;
                            siteChange.FieldName = "Representativity";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                        if (storedHabitat.Cover_ha > harvestingHabitat.Cover_ha)
                        {
                            if (Math.Abs((double)(storedHabitat.Cover_ha - harvestingHabitat.Cover_ha)) > habitatCoverHaTolerance)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Habitats";
                                siteChange.ChangeType = "Cover_ha Decrease";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Warning;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.NewValue = harvestingHabitat.Cover_ha != -1 ? harvestingHabitat.Cover_ha.ToString() : null;
                                siteChange.OldValue = storedHabitat.Cover_ha != -1 ? storedHabitat.Cover_ha.ToString() : null;
                                siteChange.Tags = string.Empty;
                                siteChange.Code = harvestingHabitat.HabitatCode;
                                siteChange.Section = "Habitats";
                                siteChange.VersionReferenceId = storedHabitat.VersionId;
                                siteChange.FieldName = "Cover_ha";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                        }
                        else if (storedHabitat.Cover_ha < harvestingHabitat.Cover_ha)
                        {
                            if (Math.Abs((double)(storedHabitat.Cover_ha - harvestingHabitat.Cover_ha)) > habitatCoverHaTolerance)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Habitats";
                                siteChange.ChangeType = "Cover_ha Increase";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.NewValue = harvestingHabitat.Cover_ha != -1 ? harvestingHabitat.Cover_ha.ToString() : null;
                                siteChange.OldValue = storedHabitat.Cover_ha != -1 ? storedHabitat.Cover_ha.ToString() : null;
                                siteChange.Tags = string.Empty;
                                siteChange.Code = harvestingHabitat.HabitatCode;
                                siteChange.Section = "Habitats";
                                siteChange.VersionReferenceId = storedHabitat.VersionId;
                                siteChange.FieldName = "Cover_ha";
                                siteChange.ReferenceSiteCode = storedSite.SiteCode;
                                siteChange.N2KVersioningVersion = envelope.VersionId;
                                changes.Add(siteChange);
                            }
                        }
                        else if (storedHabitat.Cover_ha != harvestingHabitat.Cover_ha)
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "Cover_ha Change";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Info;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.NewValue = harvestingHabitat.Cover_ha != -1 ? harvestingHabitat.Cover_ha.ToString() : null;
                            siteChange.OldValue = storedHabitat.Cover_ha != -1 ? storedHabitat.Cover_ha.ToString() : null;
                            siteChange.Tags = string.Empty;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            siteChange.VersionReferenceId = storedHabitat.VersionId;
                            siteChange.FieldName = "Cover_ha";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }
                        if (storedHabitat.PriorityForm != harvestingHabitat.PriorityForm)
                        {
                            SiteChangeDb siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.Version = harvestingSite.VersionId;
                            siteChange.ChangeCategory = "Habitats";
                            siteChange.ChangeType = "PriorityForm Change";
                            siteChange.Country = envelope.CountryCode;
                            siteChange.Level = Enumerations.Level.Critical;
                            siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                            siteChange.NewValue = harvestingHabitat.PriorityForm == true ? "Y" : "N";
                            siteChange.OldValue = storedHabitat.PriorityForm == true ? "Y" : "N";
                            siteChange.Tags = string.Empty;
                            siteChange.Code = harvestingHabitat.HabitatCode;
                            siteChange.Section = "Habitats";
                            siteChange.VersionReferenceId = storedHabitat.VersionId;
                            siteChange.FieldName = "PriorityForm";
                            siteChange.ReferenceSiteCode = storedSite.SiteCode;
                            siteChange.N2KVersioningVersion = envelope.VersionId;
                            changes.Add(siteChange);
                        }

                        //Priority check is also present in HarvestedService/SitePriorityChecker
                        #region HabitatPriority
                        HabitatPriority priorityCount = habitatPriority.Where(s => s.HabitatCode == harvestingHabitat.HabitatCode).FirstOrDefault();
                        if (priorityCount != null)
                        {
                            //These booleans declare whether or not each habitat is a priority
                            Boolean isStoredPriority = false;
                            Boolean isHarvestingPriority = false;
                            //if (harvestingHabitat.HabitatCode == "21A0" || harvestingHabitat.HabitatCode == "6210" || harvestingHabitat.HabitatCode == "7130" || harvestingHabitat.HabitatCode == "9430")
                            if (priorityCount.Priority == 2)
                            {
                                //If the Habitat is an exception, three conditions are checked
                                if (((storedHabitat.HabitatCode != "21A0" && storedHabitat.PriorityForm == true)
                                    || (storedHabitat.HabitatCode == "21A0" && storedSite.CountryCode == "IE"))
                                        && (storedHabitat.Representativity.ToUpper() != "D" || storedHabitat.Representativity == null))
                                    isStoredPriority = true;
                                if (((harvestingHabitat.HabitatCode != "21A0" && harvestingHabitat.PriorityForm == true)
                                    || (harvestingHabitat.HabitatCode == "21A0" && harvestingSite.CountryCode == "IE"))
                                        && (harvestingHabitat.Representativity.ToUpper() != "D" || harvestingHabitat.Representativity == null))
                                    isHarvestingPriority = true;
                            }
                            else
                            {
                                //If there is no exception, then two conditions are checked
                                if (storedHabitat.Representativity.ToUpper() != "D" || storedHabitat.Representativity == null)
                                    isStoredPriority = true;
                                if (harvestingHabitat.Representativity.ToUpper() != "D" || harvestingHabitat.Representativity == null)
                                    isHarvestingPriority = true;
                            }

                            if (isStoredPriority && !isHarvestingPriority)
                            {
                                SiteChangeDb siteChange = new SiteChangeDb();
                                siteChange.SiteCode = harvestingSite.SiteCode;
                                siteChange.Version = harvestingSite.VersionId;
                                siteChange.ChangeCategory = "Habitats";
                                siteChange.ChangeType = "Habitat Losing Priority";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Critical;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = Convert.ToString(isHarvestingPriority);
                                siteChange.OldValue = Convert.ToString(isStoredPriority);
                                siteChange.Code = harvestingHabitat.HabitatCode;
                                siteChange.Section = "Habitats";
                                siteChange.VersionReferenceId = storedHabitat.VersionId;
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
                                siteChange.ChangeCategory = "Habitats";
                                siteChange.ChangeType = "Habitat Getting Priority";
                                siteChange.Country = envelope.CountryCode;
                                siteChange.Level = Enumerations.Level.Info;
                                siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                                siteChange.Tags = string.Empty;
                                siteChange.NewValue = Convert.ToString(isHarvestingPriority);
                                siteChange.OldValue = Convert.ToString(isStoredPriority);
                                siteChange.Code = harvestingHabitat.HabitatCode;
                                siteChange.Section = "Habitats";
                                siteChange.VersionReferenceId = storedHabitat.VersionId;
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
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Habitats";
                        siteChange.ChangeType = "Habitat Added";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Info;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = harvestingHabitat.HabitatCode;
                        siteChange.OldValue = null;
                        siteChange.Code = harvestingHabitat.HabitatCode;
                        siteChange.Section = "Habitats";
                        siteChange.VersionReferenceId = harvestingSite.VersionId;
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
                        changes.Add(siteChange);
                    }
                }

                //For each habitat in backboneDB check if the habitat still exists in Versioning
                foreach (HabitatToHarvest storedHabitat in referencedHabitats)
                {
                    HabitatToHarvest harvestingHabitat = habitatVersioning.Where(s => s.HabitatCode == storedHabitat.HabitatCode).FirstOrDefault();
                    if (harvestingHabitat == null)
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = storedSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Habitats";
                        siteChange.ChangeType = "Habitat Deleted";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Critical;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = null;
                        siteChange.OldValue = storedHabitat.HabitatCode;
                        siteChange.Code = storedHabitat.HabitatCode;
                        siteChange.Section = "Habitats";
                        siteChange.VersionReferenceId = storedHabitat.VersionId;
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
                        changes.Add(siteChange);
                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetectionHabitat - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "", ctx.Database.GetConnectionString());
            }
            return changes;
        }

    }
}
