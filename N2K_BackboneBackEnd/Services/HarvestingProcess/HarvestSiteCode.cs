using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    /// <summary>
    /// Class dedicated to the import of a Site and the entities that dependes directly from the Site Entity.
    /// </summary>
    public class HarvestSiteCode : BaseHarvestingProcess, IHarvestingTables
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="dataContext">Context for the BackBone database</param>
        /// <param name="versioningContext">Context for the Versioning database</param>
        public HarvestSiteCode(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext) : base(dataContext, versioningContext)
        {
        }

        public List<HabitatPriority> habitatPriority = new();
        public List<SpeciesPriority> speciesPriority = new();

        /// <summary>
        /// This mehtod calls for teh process to harvest the complete data for all sites 
        /// reported in the envelopment reported by the MS
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        [Obsolete("Method Harvest is deprecated, and has no code.")]
        public async Task<int> Harvest(string countryCode, int versionId)
        {
            Console.WriteLine("=>Start Site Code harvest...");
            await Task.Delay(5000);
            Console.WriteLine("=>End Site Code harvest...");
            return 1;
        }

        /// <summary>
        /// This mehtod calls for teh process to harvest the complete data for all sites 
        /// reported in the envelopment reported by the MS
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="versionId"></param>
        /// <param name="referenceVersionID"></param>
        /// <returns></returns>
        [Obsolete("Method Harvest is deprecated, and has no code.")]
        public async Task<int> ChangeDetectionChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start Site Code ChangeDetection...");
            await Task.Delay(10000);
            Console.WriteLine("==>ENd Site Code ChangeDetection...");
            return 1;
        }

        /// <summary>
        /// This method retrives the complete information for a Site in Versioning and stores it in BackBone.
        /// (Site and their dependencies but not Species and habitats)
        /// </summary>
        /// <param name="pVSite">The definition ogf the versioning Site</param>
        /// <param name="pEnvelope">The envelope to process</param>
        /// <returns>Returns a BackBone Site object</returns>
        public async Task<int>? HarvestSite(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, IList<DataQualityTypes> dataQualityTypes, IList<OwnerShipTypes> ownerShipTypes, List<Sites> bbSites)
        {
            try
            {
                //Get the data for all related tables
                await harvestRespondents(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestBioregions(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestNutsBySite(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestIsImpactedBy(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestHasNationalProtection(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestDetailedProtectionStatus(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestSiteLargeDescriptions(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestSiteOwnerType(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, ownerShipTypes, bbSites);
                await harvestOwnership(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestDocumentationLinks(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);
                await harvestReferenceMap(countryCode, COUNTRYVERSIONID, versioningDB, backboneDb, bbSites);

                //BioRegions.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await BioRegionsTask);
                //Respondents.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await RespondentsTask);
                /*
                NutsBySite.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await NutsBySitesTask);
                Models.backbone_db.IsImpactedBy.SaveBulkRecord(this._dataContext.Database.GetConnectionString(),await IsImpactedByTask);
                Models.backbone_db.HasNationalProtection.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await HasNationalProtectionTask);
                Models.backbone_db.DetailedProtectionStatus.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await DetailedProtectionStatusTask);
                SiteLargeDescriptions.SaveBulkRecord(this._dataContext.Database.GetConnectionString(),await SiteLargeDescriptionsTask);
                SiteOwnerType.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await SiteOwnerTypeTask);
                */
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - HarvestSite", "", backboneDb);
                return 0;
            }
        }

        /// <summary>
        ///  This method retrives the complete information for a Site in Versioning and stores it in BackBone.
        ///  (Just the Site)
        /// </summary>
        /// <param name="pVSite">The definition ogf the versioning Site</param>
        /// <param name="pEnvelope">The envelope to process</param>
        /// <returns>Returns a BackBone Site object</returns>
        public async Task<Sites> harvestSiteCode(NaturaSite pVSite, EnvelopesToProcess pEnvelope, int versionNext)
        {
            //Tomamos el valor más alto que tiene en el campo Version para ese SiteCode. Por defecto es -1 para cuando no existe 
            //por que le vamos a sumar un 1 lo cual dejaría en 0
            Sites bbSite = new();

            #region SitePriority
            //SqlParameter param3 = new SqlParameter("@site", pVSite.SITECODE);
            //SqlParameter param4 = new SqlParameter("@versionId", pVSite.VERSIONID);

            //List<HabitatToHarvest> habitatVersioning = await _dataContext.Set<HabitatToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceHabitatsBySiteCodeAndVersion  @site, @versionId",
            //                        param3, param4).ToListAsync();
            //List<SpeciesToHarvest> speciesVersioning = await _dataContext.Set<SpeciesToHarvest>().FromSqlRaw($"exec dbo.spGetReferenceSpeciesBySiteCodeAndVersion  @site, @versionId",
            //                param3, param4).ToListAsync();

            ////These booleans declare whether or not each site is a priority
            //Boolean isHarvestingSitePriority = false;

            //foreach (HabitatToHarvest harvestingHabitat in habitatVersioning)
            //{
            //    HabitatPriority priorityCount = habitatPriority.Where(s => s.HabitatCode == harvestingHabitat.HabitatCode).FirstOrDefault();
            //    if (priorityCount != null)
            //    {
            //        if (priorityCount.Priority == 2)
            //        {
            //            if (harvestingHabitat.Representativity.ToUpper() != "D" && harvestingHabitat.PriorityForm == true)
            //            {
            //                isHarvestingSitePriority = true;
            //                break;
            //            }
            //        }
            //        else
            //        {
            //            if (harvestingHabitat.Representativity.ToUpper() != "D")
            //            {
            //                isHarvestingSitePriority = true;
            //                break;
            //            }
            //        }
            //    }
            //}

            //if (!isHarvestingSitePriority)
            //{
            //    foreach (SpeciesToHarvest harvestingSpecies in speciesVersioning)
            //    {
            //        SpeciesPriority priorityCount = speciesPriority.Where(s => s.SpecieCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
            //        if (priorityCount != null)
            //        {
            //            if (harvestingSpecies.Population.ToUpper() != "D")
            //            {
            //                isHarvestingSitePriority = true;
            //                break;
            //            }
            //        }
            //    }
            //}
            #endregion

            try
            {
                bbSite.SiteCode = pVSite.SITECODE;
                bbSite.Version = versionNext;
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
                bbSite.DateConfSCI = pVSite.DATE_CONF_SCI;
                bbSite.Priority = null;
                bbSite.DatePropSCI = pVSite.DATE_PROP_SCI;
                bbSite.DateSpa = pVSite.DATE_SPA;
                bbSite.DateSac = pVSite.DATE_SAC;
                bbSite.Latitude = pVSite.LATITUDE;
                bbSite.Longitude = pVSite.LONGITUDE;
                bbSite.DateUpdate = pVSite.DATE_UPDATE;
                bbSite.SpaLegalReference = pVSite.SPA_LEGAL_REFERENCE;
                bbSite.SacLegalReference = pVSite.SAC_LEGAL_REFERENCE;
                bbSite.Explanations = pVSite.EXPLANATIONS;
                bbSite.MarineArea = pVSite.MARINEAREA;
                return bbSite;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestSiteCode", "", _dataContext.Database.GetConnectionString());
                return null;
            }
        }

        /// <summary>
        /// Retrives the information of the BioRegions for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of bioregions stored</returns>
        //private async Task<List<BioRegions>> harvestBioregions(NaturaSite pVSite, int pVersion, string backboneDb)
        private async Task<int> harvestBioregions(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<BioRegions> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var start = DateTime.Now;

            try
            {
                versioningConn = new SqlConnection(versioningDB);
                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                String queryString = @"select SITECODE as SiteCode,BIOREGID as BGRID, max(PERCENTAGE) as Percentage
                                     from BelongsToBioRegion
                                     where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID
                                     group by SITECODE, BIOREGID";
                command = new SqlCommand(queryString, versioningConn);
                versioningConn.Open();

                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    BioRegions item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.BGRID = TypeConverters.CheckNull<int>(reader["BGRID"]);
                        item.Percentage = null;
                        if (reader["Percentage"] != DBNull.Value)
                            item.Percentage = decimal.ToDouble(TypeConverters.CheckNull<decimal>(reader["Percentage"]));
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - BioRegions", "", backboneDb);
                    }
                }
                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await BioRegions.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - BioRegions.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list bioregions -> {0}", (DateTime.Now - start).TotalSeconds));
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestBioregions", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the NUTS for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of NUTS stored</returns>
        //private async Task<List<NutsBySite>>? harvestNutsBySite(NaturaSite pVSite, int pVersion)
        private async Task<int> harvestNutsBySite(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<NutsBySite> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            var start = DateTime.Now;
            try
            {
                versioningConn = new SqlConnection(versioningDB);

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                String queryString = @"select SITECODE as SiteCode, NUTSCODE as NutId,
                            SUM(COVER) as CoverPercentage 
                            from NutsRegion 
                            where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID
                            group by SITECODE, VERSIONID, NUTSCODE";

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    NutsBySite item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.NutId = TypeConverters.CheckNull<string>(reader["NutId"]);
                        item.CoverPercentage = null;
                        if (reader["CoverPercentage"] != DBNull.Value)
                            item.CoverPercentage = decimal.ToDouble(TypeConverters.CheckNull<decimal>(reader["CoverPercentage"]));

                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - NutsBySite", "", backboneDb);
                    }
                }
                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await NutsBySite.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - NutsBySite.SaveBulkRecord", "", backboneDb);
                }

                //Console.WriteLine(String.Format("End save to list nuts by site -> {0}", (DateTime.Now - start).TotalSeconds));
                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestNutsBySite", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the IsImpactedBy elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of IsImpactedBy stored</returns>
        private async Task<int> harvestIsImpactedBy(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<Models.backbone_db.IsImpactedBy> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var start = DateTime.Now;

            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE as SiteCode,ACTIVITYCODE as ActivityCode,IN_OUT  as InOut,INTENSITY as Intensity,
                        PERCENTAGEAFF as PercentageAff, INFLUENCE as Influence ,STARTDATE as StartDate,ENDDATE as EndDate,POLLUTIONCODE as PollutionCode,OCCURRENCE as Ocurrence,IMPACTTYPE  as ImpactType                         
                            from IsImpactedBy 
                            where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();

                //Console.WriteLine(String.Format("End Query -> {0}", (DateTime.Now - start).TotalSeconds));

                while (reader.Read())
                {
                    Models.backbone_db.IsImpactedBy item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.ActivityCode = TypeConverters.CheckNull<string>(reader["ActivityCode"]);
                        item.InOut = TypeConverters.CheckNull<string>(reader["InOut"]);
                        item.Intensity = TypeConverters.CheckNull<string>(reader["Intensity"]);
                        item.PercentageAff = null;
                        if (reader["PercentageAff"] != DBNull.Value)
                            item.PercentageAff = decimal.ToDouble(TypeConverters.CheckNull<decimal>(reader["PercentageAff"]));
                        item.Influence = TypeConverters.CheckNull<string>(reader["Influence"]);
                        item.StartDate = TypeConverters.CheckNull<DateTime>(reader["StartDate"]);
                        item.EndDate = TypeConverters.CheckNull<DateTime>(reader["EndDate"]);
                        item.PollutionCode = TypeConverters.CheckNull<string>(reader["PollutionCode"]);
                        item.Ocurrence = TypeConverters.CheckNull<string>(reader["Ocurrence"]);
                        item.ImpactType = TypeConverters.CheckNull<string>(reader["ImpactType"]);

                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - IsImpactedBy", "", backboneDb);
                    }
                }

                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await Models.backbone_db.IsImpactedBy.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - IsImpactedBy.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list IsImpactedBy -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestIsImpactedBy", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the HasNationalProtection elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of HasNationalProtection stored</returns>
        private async Task<int> harvestHasNationalProtection(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<Models.backbone_db.HasNationalProtection> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            var start = DateTime.Now;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE as SiteCode, DesignatedCode, Percentage
                            from HasNationalProtection 
                            where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    Models.backbone_db.HasNationalProtection item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.DesignatedCode = TypeConverters.CheckNull<string>(reader["DesignatedCode"]);
                        item.Percentage = TypeConverters.CheckNull<decimal>(reader["Percentage"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - HasNationalProtection", "", backboneDb);
                    }
                }

                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await Models.backbone_db.HasNationalProtection.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - HasNationalProtection.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list HasNationalProtection -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestHasNationalProtection", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the DetailedProtectionStatus elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of DetailedProtectionStatus stored</returns>
        private async Task<int> harvestDetailedProtectionStatus(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<Models.backbone_db.DetailedProtectionStatus> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var start = DateTime.Now;

            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        N2K_SITECODE as SiteCode,DesignationCode,PROTECTEDSITENAME AS Name,OverlapCode,OVERLAPPERC as OverlapPercentage,Convention
                            from DetailedProtectionStatus 
                            where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    Models.backbone_db.DetailedProtectionStatus item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.DesignationCode = TypeConverters.CheckNull<string>(reader["DesignationCode"]);
                        item.Name = TypeConverters.CheckNull<string>(reader["Name"]);
                        item.OverlapCode = TypeConverters.CheckNull<string>(reader["OverlapCode"]);
                        item.OverlapPercentage = TypeConverters.CheckNull<decimal>(reader["OverlapPercentage"]);
                        item.Convention = TypeConverters.CheckNull<string>(reader["Convention"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - DetailedProtectionStatus", "", backboneDb);
                    }
                }

                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await Models.backbone_db.DetailedProtectionStatus.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - DetailedProtectionStatus.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list DetailedProtectionStatus -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestDetailedProtectionStatus", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the SiteLargeDescriptions elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of SiteLargeDescriptions stored</returns>
        private async Task<int>? harvestSiteLargeDescriptions(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<Models.backbone_db.SiteLargeDescriptions> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            var start = DateTime.Now;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE as SiteCode,Quality, Vulnarab,Designation,MANAG_PLAN as ManagPlan,Documentation,
                        OtherCharact,MANAG_CONSERV_MEASURES as  ManagConservMeasures, MANAG_PLAN_URL as ManagPlanUrl,MANAG_STATUS as ManagStatus
                        from Description 
                        where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    SiteLargeDescriptions item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.Quality = TypeConverters.CheckNull<string>(reader["Quality"]);
                        item.Vulnarab = TypeConverters.CheckNull<string>(reader["Vulnarab"]);
                        item.Designation = TypeConverters.CheckNull<string>(reader["Designation"]);
                        item.ManagPlan = TypeConverters.CheckNull<string>(reader["ManagPlan"]);
                        item.Documentation = TypeConverters.CheckNull<string>(reader["Documentation"]);
                        item.OtherCharact = TypeConverters.CheckNull<string>(reader["OtherCharact"]);
                        item.ManagConservMeasures = TypeConverters.CheckNull<string>(reader["ManagConservMeasures"]);
                        item.ManagPlanUrl = TypeConverters.CheckNull<string>(reader["ManagPlanUrl"]);
                        item.ManagStatus = TypeConverters.CheckNull<string>(reader["ManagStatus"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - SiteLargeDescriptions", "", backboneDb);
                    }
                }
                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await Models.backbone_db.SiteLargeDescriptions.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - SiteLargeDescriptions.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list Site Large Description -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestSiteLargeDescriptions", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the SiteOwnerType elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of SiteOwnerType stored</returns>
        private async Task<int> harvestSiteOwnerType(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, IList<Models.backbone_db.OwnerShipTypes> _ownerShipTypes, List<Sites> sites)
        {
            List<Models.backbone_db.SiteOwnerType> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE,TYPE, SUM([PERCENT]) as [percent]
                        from OwnerType 
                        where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID
                        group by SITECODE, VERSIONID, Type";

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    SiteOwnerType item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        //6-Unknown for those types not found in the reference OwnerShipTypes
                        item.Type = TypeConverters.CheckNull<string>(reader["Type"]);
                        item.Percent = TypeConverters.CheckNull<decimal>(reader["Percent"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - SiteOwnerType", "", backboneDb);
                    }
                }
                try
                {
                    await Models.backbone_db.SiteOwnerType.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - SiteOwnerType.SaveBulkRecord", "", backboneDb);
                }
                return 1;

            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestSiteOwnerType", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the Ownership elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of Ownerships stored</returns>
        private async Task<int>? harvestOwnership(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<Models.backbone_db.Ownership> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            var start = DateTime.Now;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE as SiteCode,Type,Content,ObjectID
                        from OWNERSHIP 
                        where COUNTRYCODE=@COUNTRYCODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    Models.backbone_db.Ownership item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.Type = TypeConverters.CheckNull<string>(reader["Type"]);
                        item.Content = TypeConverters.CheckNull<string>(reader["Content"]);
                        item.ContactId = TypeConverters.CheckNull<int>(reader["ObjectID"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - Ownership", "", backboneDb);
                    }
                }
                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await Models.backbone_db.Ownership.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - Ownership.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list Site Large Description -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestOwnership", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the DocumentationLinks elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of DocumentationLinks stored</returns>
        private async Task<int>? harvestDocumentationLinks(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<Models.backbone_db.DocumentationLinks> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            var start = DateTime.Now;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT SITECODE AS SiteCode,
	                                        URL AS Link
                                        FROM DOCUMENTATION_LINK
                                        WHERE COUNTRYCODE = @COUNTRYCODE
	                                        AND COUNTRYVERSIONID = @COUNTRYVERSIONID
	                                        AND URL IS NOT NULL AND URL NOT LIKE ''";

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    Models.backbone_db.DocumentationLinks item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.Link = TypeConverters.CheckNull<string>(reader["Link"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - DocumentationLinks", "", backboneDb);
                    }
                }
                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await Models.backbone_db.DocumentationLinks.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - DocumentationLinks.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list Site Large Description -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestDocumentationLinks", "", backboneDb);
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

        /// <summary>
        /// Retrives the information of the ReferenceMap elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of ReferenceMap stored</returns>
        private async Task<int>? harvestReferenceMap(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<Models.backbone_db.ReferenceMap> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            var start = DateTime.Now;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT SITECODE AS SiteCode,
	                                        NATIONALMAPNUMBER,
	                                        SCALE,
	                                        PROJECTION,
	                                        DETAILS,
	                                        INSPIRE,
	                                        PDFPROVIDED
                                        FROM REFERENCEMAP
                                        WHERE COUNTRYCODE = @COUNTRYCODE
	                                        AND COUNTRYVERSIONID = @COUNTRYVERSIONID";

                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    Models.backbone_db.ReferenceMap item = new()
                    {
                        SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"])
                    };
                    if (sites.Any(s => s.SiteCode == item.SiteCode))
                    {
                        item.Version = sites.FirstOrDefault(s => s.SiteCode == item.SiteCode).Version;
                        item.NationalMapNumber = TypeConverters.CheckNull<string>(reader["NATIONALMAPNUMBER"]);
                        item.Scale = TypeConverters.CheckNull<string>(reader["SCALE"]);
                        item.Projection = TypeConverters.CheckNull<string>(reader["PROJECTION"]);
                        item.Details = TypeConverters.CheckNull<string>(reader["DETAILS"]);
                        item.Inspire = TypeConverters.CheckNull<string>(reader["INSPIRE"]);
                        item.PDFProvided = TypeConverters.CheckNull<Int16>(reader["PDFPROVIDED"]);
                        items.Add(item);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", item.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - ReferenceMap", "", backboneDb);
                    }
                }
                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await Models.backbone_db.ReferenceMap.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - ReferenceMap.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list Site Large Description -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestReferenceMap", "", backboneDb);
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

        public async Task<List<SiteChangeDb>> ChangeDetectionSiteAttributes(List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, double siteAreaHaTolerance, double siteLengthKmTolerance, ProcessedEnvelopes? processedEnvelope, N2KBackboneContext? _ctx = null)
        {
            await Task.Delay(1);
            try
            {
                if (_ctx == null) _ctx = _dataContext;
                var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_dataContext.Database.GetConnectionString(),
                        opt => opt.EnableRetryOnFailure()).Options;
                using (N2KBackboneContext ctx = new(options))
                {
                    Lineage? lineageCheck = await ctx.Set<Lineage>().FirstOrDefaultAsync(l => l.SiteCode == harvestingSite.SiteCode && l.Version == harvestingSite.VersionId);
                    if (lineageCheck?.Type == LineageTypes.Recode)
                    {
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Lineage",
                            ChangeType = "Site Recoded",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = (SiteChangeStatus?)processedEnvelope.Status,
                            Tags = string.Empty,
                            NewValue = harvestingSite.SiteCode,
                            OldValue = storedSite.SiteCode,
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = storedSite.VersionId,
                            FieldName = "SiteCode",
                            ReferenceSiteCode = storedSite.SiteCode,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }

                    /*
                    var param1 = new SqlParameter("@sitecode", storedSite.SiteCode);
                    var param2 = new SqlParameter("@version", storedSite.VersionId);
                    List<SiteSpatialBasic> storedGeometries = await ctx.Set<SiteSpatialBasic>().FromSqlRaw($"exec dbo.spHasSiteGeometry @sitecode, @version", param1, param2).AsNoTracking().ToListAsync();
                    SiteSpatialBasic storedGeometry = storedGeometries.FirstOrDefault();

                    param1 = new SqlParameter("@sitecode", harvestingSite.SiteCode);
                    param2 = new SqlParameter("@version", harvestingSite.VersionId);
                    List<SiteSpatialBasic> harvestingGeometries = await ctx.Set<SiteSpatialBasic>().FromSqlRaw($"exec dbo.spHasSiteGeometry @sitecode, @version", param1, param2).AsNoTracking().ToListAsync();
                    SiteSpatialBasic harvestingGeometry = harvestingGeometries.FirstOrDefault();
                    */

                    //if (storedGeometry == null || harvestingGeometry == null || storedGeometry.data != harvestingGeometry.data)
                    if (!storedSite.HasGeometry || !harvestingSite.HasGeometry || storedSite.HasGeometry != harvestingSite.HasGeometry)
                    {
                        Lineage lineage = await ctx.Set<Lineage>().Where(s => s.SiteCode == harvestingSite.SiteCode && s.N2KVersioningVersion == harvestingSite.N2KVersioningVersion).FirstOrDefaultAsync();
                        //if (storedGeometry != null && storedGeometry.data == true && (harvestingGeometry == null || harvestingGeometry.data == false))
                        if (storedSite.HasGeometry && !harvestingSite.HasGeometry)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Lineage",
                                ChangeType = "No geometry reported",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = "",
                                OldValue = storedSite.SiteCode,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "SiteCode",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                            lineage.Version = harvestingSite.VersionId;
                            lineage.Type = LineageTypes.NoGeometryReported;
                        }
                        //else if ((storedGeometry == null || storedGeometry.data == false) && harvestingGeometry != null && harvestingGeometry.data == true)
                        else if ((!storedSite.HasGeometry) && harvestingSite.HasGeometry)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Lineage",
                                ChangeType = "New geometry reported",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = harvestingSite.SiteCode,
                                OldValue = "",
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "SiteCode",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                            lineage.Type = LineageTypes.NewGeometryReported;
                            LineageAntecessors antecessor = new()
                            {
                                SiteCode = storedSite.SiteCode,
                                Version = storedSite.VersionId,
                                LineageID = lineage.ID,
                                N2KVersioningVersion = storedSite.N2KVersioningVersion
                            };
                            _dataContext.Set<LineageAntecessors>().Add(antecessor);
                        }
                        _dataContext.Set<Lineage>().Update(lineage);
                        _dataContext.SaveChanges();
                    }
                    if ((harvestingSite.SiteName ?? "") != (storedSite.SiteName ?? ""))
                    {
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Site General Info",
                            ChangeType = "SiteName Changed",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Info,
                            Status = (SiteChangeStatus?)processedEnvelope.Status,
                            Tags = string.Empty,
                            NewValue = harvestingSite.SiteName,
                            OldValue = storedSite.SiteName,
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = storedSite.VersionId,
                            FieldName = "SiteName",
                            ReferenceSiteCode = storedSite.SiteCode,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }
                    if ((harvestingSite.SiteType ?? "") != (storedSite.SiteType ?? ""))
                    {
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Site General Info",
                            ChangeType = "SiteType Changed",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = (SiteChangeStatus?)processedEnvelope.Status,
                            Tags = string.Empty,
                            NewValue = harvestingSite.SiteType,
                            OldValue = storedSite.SiteType,
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = storedSite.VersionId,
                            FieldName = "SiteType",
                            ReferenceSiteCode = storedSite.SiteCode,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }
                    if (!Convert.ToString(harvestingSite.DateConfSCI == null ? "" : harvestingSite.DateConfSCI).Equals(Convert.ToString(storedSite.DateConfSCI == null ? "" : storedSite.DateConfSCI)))
                    {
                        if (Convert.ToString(harvestingSite.DateConfSCI == null ? "" : harvestingSite.DateConfSCI).Equals("")
                            && !Convert.ToString(storedSite.DateConfSCI == null ? "" : storedSite.DateConfSCI).Equals(""))
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Site General Info",
                                ChangeType = "Reported DateConfSCI is empty",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Info,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = null,
                                OldValue = storedSite.DateConfSCI.Value.ToShortDateString(),
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "DateConfSCI",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                        else if (!Convert.ToString(harvestingSite.DateConfSCI == null ? "" : harvestingSite.DateConfSCI).Equals("")
                            && Convert.ToString(storedSite.DateConfSCI == null ? "" : storedSite.DateConfSCI).Equals(""))
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Site General Info",
                                ChangeType = "Reference DateConfSCI is empty and reported value is not",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = harvestingSite.DateConfSCI.Value.ToShortDateString(),
                                OldValue = null,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "DateConfSCI",
                                ReferenceSiteCode = storedSite.SiteCode,
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
                                ChangeCategory = "Site General Info",
                                ChangeType = "Reported DateConfSCI is different",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Warning,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = harvestingSite.DateConfSCI.Value.ToShortDateString(),
                                OldValue = storedSite.DateConfSCI.Value.ToShortDateString(),
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "DateConfSCI",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                    }
                    if (harvestingSite.AreaHa != null && storedSite.AreaHa != null && harvestingSite.AreaHa > storedSite.AreaHa)
                    {
                        if (Math.Abs((double)(harvestingSite.AreaHa - storedSite.AreaHa)) > siteAreaHaTolerance)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Change of area",
                                ChangeType = "SDF Area Increase",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Info,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                NewValue = harvestingSite.AreaHa.ToString(),
                                OldValue = storedSite.AreaHa.ToString(),
                                Tags = string.Empty,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "AreaHa",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                    }
                    else if (harvestingSite.AreaHa != null && storedSite.AreaHa != null && harvestingSite.AreaHa < storedSite.AreaHa)
                    {
                        if (Math.Abs((double)(harvestingSite.AreaHa - storedSite.AreaHa)) > siteAreaHaTolerance)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Change of area",
                                ChangeType = "SDF Area Decrease",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Warning,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                NewValue = harvestingSite.AreaHa.ToString(),
                                OldValue = storedSite.AreaHa.ToString(),
                                Tags = string.Empty,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "AreaHa",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                    }
                    else if ((harvestingSite.AreaHa ?? -1) != (storedSite.AreaHa ?? -1))
                    {
                        SiteChangeDb siteChange = new()
                        {
                            SiteCode = harvestingSite.SiteCode,
                            Version = harvestingSite.VersionId,
                            ChangeCategory = "Change of area",
                            ChangeType = "SDF Area Change",
                            Country = envelope.CountryCode,
                            Level = Enumerations.Level.Info,
                            Status = (SiteChangeStatus?)processedEnvelope.Status,
                            NewValue = harvestingSite.AreaHa != null ? harvestingSite.AreaHa.ToString() : null,
                            OldValue = storedSite.AreaHa != null ? storedSite.AreaHa.ToString() : null,
                            Tags = string.Empty,
                            Code = harvestingSite.SiteCode,
                            Section = "Site",
                            VersionReferenceId = storedSite.VersionId,
                            FieldName = "AreaHa",
                            ReferenceSiteCode = storedSite.SiteCode,
                            N2KVersioningVersion = envelope.VersionId
                        };
                        changes.Add(siteChange);
                    }
                    if ((harvestingSite.LengthKm ?? -1) != (storedSite.LengthKm ?? -1))
                    {
                        if ((harvestingSite.LengthKm != null && storedSite.LengthKm != null
                            && Math.Abs((double)(harvestingSite.LengthKm - storedSite.LengthKm)) > siteLengthKmTolerance)
                            || harvestingSite.LengthKm == null || storedSite.LengthKm == null)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Site General Info",
                                ChangeType = "Length Changed",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Info,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                NewValue = harvestingSite.LengthKm != null ? harvestingSite.LengthKm.ToString() : null,
                                OldValue = storedSite.LengthKm != null ? storedSite.LengthKm.ToString() : null,
                                Tags = string.Empty,
                                Code = harvestingSite.SiteCode,
                                Section = "Site",
                                VersionReferenceId = storedSite.VersionId,
                                FieldName = "LengthKm",
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetectionSiteAttributes - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "", _ctx.Database.GetConnectionString());
            }
            return changes;
        }

        public async Task<List<SiteChangeDb>> ChangeDetectionBioRegions(List<BioRegions> bioRegionsVersioning, List<BioRegions> referencedBioRegions, List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5, ProcessedEnvelopes? processedEnvelope, N2KBackboneContext? _ctx = null)
        {
            try
            {
                if (_ctx == null) _ctx = _dataContext;
                var options = new DbContextOptionsBuilder<N2KBackboneContext>().UseSqlServer(_dataContext.Database.GetConnectionString(),
                        opt => opt.EnableRetryOnFailure()).Options;
                using (N2KBackboneContext ctx = new(options))
                {
                    //Get the lists of bioregion types
                    List<BioRegionTypes> bioRegionTypes = await ctx.Set<BioRegionTypes>().AsNoTracking().ToListAsync();

                    //For each BioRegion in Versioning compare it with that BioRegion in backboneDB
                    foreach (BioRegions harvestingBioRegions in bioRegionsVersioning)
                    {
                        BioRegions storedBioRegions = referencedBioRegions.Where(s => s.BGRID == harvestingBioRegions.BGRID).FirstOrDefault();
                        BioRegionTypes bioRegionType = bioRegionTypes.Where(s => s.Code == harvestingBioRegions.BGRID).FirstOrDefault();
                        if (storedBioRegions == null)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = harvestingSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Network general structure",
                                ChangeType = "Sites added due to a change of BGR",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = bioRegionType.RefBioGeoName,
                                OldValue = null,
                                Code = harvestingSite.SiteCode,
                                Section = "BioRegions",
                                VersionReferenceId = storedSite.VersionId,
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                    }

                    //For each BioRegion in backboneDB check if the BioRegion still exists in Versioning
                    foreach (BioRegions storedBioRegions in referencedBioRegions)
                    {
                        BioRegions harvestingBioRegions = bioRegionsVersioning.Where(s => s.BGRID == storedBioRegions.BGRID).FirstOrDefault();
                        BioRegionTypes bioRegionType = bioRegionTypes.Where(s => s.Code == storedBioRegions.BGRID).FirstOrDefault();
                        if (harvestingBioRegions == null)
                        {
                            SiteChangeDb siteChange = new()
                            {
                                SiteCode = storedSite.SiteCode,
                                Version = harvestingSite.VersionId,
                                ChangeCategory = "Network general structure",
                                ChangeType = "Sites deleted due to a change of BGR",
                                Country = envelope.CountryCode,
                                Level = Enumerations.Level.Critical,
                                Status = (SiteChangeStatus?)processedEnvelope.Status,
                                Tags = string.Empty,
                                NewValue = null,
                                OldValue = bioRegionType.RefBioGeoName,
                                Code = harvestingSite.SiteCode,
                                Section = "BioRegions",
                                VersionReferenceId = storedBioRegions.Version,
                                ReferenceSiteCode = storedSite.SiteCode,
                                N2KVersioningVersion = envelope.VersionId
                            };
                            changes.Add(siteChange);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ChangeDetectionBioRegions - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "", _ctx.Database.GetConnectionString());
            }
            return changes;
        }


        private async Task<int> harvestRespondents(string countryCode, decimal COUNTRYVERSIONID, string versioningDB, string backboneDb, List<Sites> sites)
        {
            List<Respondents> items = new();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var start = DateTime.Now;
            try
            {
                items = new List<Respondents>();
                versioningConn = new SqlConnection(versioningDB);
                SqlParameter param1 = new("@COUNTRYCODE", countryCode);
                SqlParameter param2 = new("@COUNTRYVERSIONID", COUNTRYVERSIONID);

                String queryString = @"SELECT SITECODE AS SiteCode,
	                                        LOCATOR_NAME AS locatorName,
	                                        ADDRESS_AREA AS addressArea,
	                                        POST_NAME AS postName,
	                                        POSTCODE AS postCode,
	                                        THOROUGHFARE AS thoroughfare,
	                                        UNSTRUCTURED_ADD AS addressUnstructured,
	                                        CONTACT_NAME AS ContactName,
	                                        ORG_NAME AS OrgName,
	                                        EMAIL AS Email,
	                                        ADMIN_UNIT AS AdminUnit,
	                                        LOCATOR_DESIGNATOR AS LocatorDesignator,
	                                        OBJECTID AS ObjectID
                                        FROM CONTACT
                                        WHERE COUNTRYCODE = @COUNTRYCODE
	                                        AND COUNTRYVERSIONID = @COUNTRYVERSIONID";


                //Console.WriteLine(String.Format("Start respondents  Query -> {0}", (DateTime.Now - start).TotalSeconds));

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                reader = await command.ExecuteReaderAsync();
                //Console.WriteLine(String.Format("End Query -> {0}", (DateTime.Now - start).TotalSeconds));

                while (reader.Read())
                {
                    Respondents respondent = new()
                    {
                        SiteCode = reader["SiteCode"].ToString()
                    };
                    if (sites.Any(s => s.SiteCode == respondent.SiteCode))
                    {
                        respondent.Version = sites.FirstOrDefault(s => s.SiteCode == respondent.SiteCode).Version;
                        respondent.locatorName = TypeConverters.CheckNull<string>(reader["locatorName"]);
                        respondent.addressArea = TypeConverters.CheckNull<string>(reader["addressArea"]);
                        respondent.postName = TypeConverters.CheckNull<string>(reader["postName"]);
                        respondent.postCode = TypeConverters.CheckNull<string>(reader["postCode"]);
                        respondent.thoroughfare = TypeConverters.CheckNull<string>(reader["thoroughfare"]);
                        respondent.addressUnstructured = TypeConverters.CheckNull<string>(reader["addressUnstructured"]);
                        respondent.ContactName = TypeConverters.CheckNull<string>(reader["ContactName"]);
                        respondent.Email = TypeConverters.CheckNull<string>(reader["Email"]);
                        respondent.AdminUnit = TypeConverters.CheckNull<string>(reader["AdminUnit"]);
                        respondent.LocatorDesignator = TypeConverters.CheckNull<string>(reader["LocatorDesignator"]);
                        respondent.ObjectID = TypeConverters.CheckNull<int>(reader["ObjectID"]);
                        respondent.OrgName = TypeConverters.CheckNull<string>(reader["OrgName"]);
                        items.Add(respondent);
                    }
                    else
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, String.Format("The Site {0} from submission {1} was not reported.", respondent.SiteCode, sites.FirstOrDefault().N2KVersioningVersion), "HarvestSiteCode - Respondents", "", backboneDb);
                    }
                }
                //Console.WriteLine(String.Format("End loop -> {0}", (DateTime.Now - start).TotalSeconds));
                try
                {
                    await Respondents.SaveBulkRecord(backboneDb, items);
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - Respondents.SaveBulkRecord", "", backboneDb);
                }
                //Console.WriteLine(String.Format("End save to list describe sites -> {0}", (DateTime.Now - start).TotalSeconds));

                return 1;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestSiteCode - harvestRespondents", "", backboneDb);
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
    }
}