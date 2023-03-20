using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using NuGet.Packaging;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Xml.Linq;

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

        public List<HabitatPriority> habitatPriority = new List<HabitatPriority>();
        public List<SpeciePriority> speciesPriority = new List<SpeciePriority>();

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
        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start Site Code validate...");
            await Task.Delay(10000);
            Console.WriteLine("==>ENd Site Code validate...");
            return 1;
        }

        /// <summary>
        /// This method retrives the complete information for a Site in Versioning and stores it in BackBone.
        /// (Site and their dependencies but not Species and habitats)
        /// </summary>
        /// <param name="pVSite">The definition ogf the versioning Site</param>
        /// <param name="pEnvelope">The envelope to process</param>
        /// <returns>Returns a BackBone Site object</returns>
        public async Task<Sites>? HarvestSite(NaturaSite pVSite, EnvelopesToProcess pEnvelope, Sites? bbSite, IList<OwnerShipTypes> ownerShipTypes, N2K_VersioningContext versioningContext, IDictionary<Type, object> _siteItems)
        {
            try
            {
                /*
                //Get the data for all related tables
                Respondents.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestRespondents(pVSite, bbSite.Version));
                BioRegions.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestBioregions(pVSite, bbSite.Version));
                NutsBySite.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestNutsBySite(pVSite, bbSite.Version));
                Models.backbone_db.IsImpactedBy.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestIsImpactedBy(pVSite, bbSite.Version));
                Models.backbone_db.HasNationalProtection.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestHasNationalProtection(pVSite, bbSite.Version));
                Models.backbone_db.DetailedProtectionStatus.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestDetailedProtectionStatus(pVSite, bbSite.Version));
                SiteLargeDescriptions.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestSiteLargeDescriptions(pVSite, bbSite.Version));
                SiteOwnerType.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestSiteOwnerType(pVSite, bbSite.Version, ownerShipTypes));
                //TimeLog.setTimeStamp("Site " + pVSite.SITECODE + " - " + pVSite.VERSIONID.ToString(), "Processed");
                */
                string versioningDB = versioningContext.Database.GetConnectionString();
                await harvestRespondents(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                await harvestBioregions(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB,  _siteItems);
                await harvestNutsBySite(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                await harvestIsImpactedBy(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                await harvestHasNationalProtection(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                await harvestDetailedProtectionStatus(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                await harvestSiteLargeDescriptions(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                await harvestSiteOwnerType(pVSite, bbSite.Version, ownerShipTypes, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);

                /*                              
                Task RespondentsTask= harvestRespondents(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                Task BioRegionsTask= harvestBioregions(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB,  _siteItems);
                Task NutsBySitesTask=  harvestNutsBySite(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                Task IsImpactedByTask = harvestIsImpactedBy(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                Task HasNationalProtectionTask =  harvestHasNationalProtection(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                Task DetailedProtectionStatusTask = harvestDetailedProtectionStatus(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                Task SiteLargeDescriptionsTask = harvestSiteLargeDescriptions(pVSite, bbSite.Version, this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                Task SiteOwnerTypeTask = harvestSiteOwnerType(pVSite, bbSite.Version, ownerShipTypes,  this._dataContext.Database.GetConnectionString(), versioningDB, _siteItems);
                */

                //BioRegions.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestBioregions(pVSite, bbSite.Version));
                //Respondents.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestRespondents(pVSite, bbSite.Version));
                /*
                await Task.WhenAll(BioRegionsTask, RespondentsTask, NutsBySitesTask,
                                  IsImpactedByTask, HasNationalProtectionTask,
                DetailedProtectionStatusTask, SiteLargeDescriptionsTask, SiteOwnerTypeTask);
                */

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
                return bbSite;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSite", "");
                return null;
            }
            finally
            {
                bbSite = null;
            }

        }


        /// <summary>
        ///  This method retrives the complete information for a Site in Versioning and stores it in BackBone.
        ///  (Just the Site)
        /// </summary>
        /// <param name="pVSite">The definition ogf the versioning Site</param>
        /// <param name="pEnvelope">The envelope to process</param>
        /// <returns>Returns a BackBone Site object</returns>
        public Sites harvestSiteCode(NaturaSite pVSite, EnvelopesToProcess pEnvelope, int versionNext)
        {
            //Tomamos el valor más alto que tiene en el campo Version para ese SiteCode. Por defecto es -1 para cuando no existe 
            //por que le vamos a sumar un 1 lo cual dejaría en 0
            Sites bbSite = new Sites();

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
            //        SpeciePriority priorityCount = speciesPriority.Where(s => s.SpecieCode == harvestingSpecies.SpeciesCode).FirstOrDefault();
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
                bbSite.Area = (double?)pVSite.AREAHA;
                bbSite.CountryCode = pEnvelope.CountryCode;
                bbSite.Length = (double?)pVSite.LENGTHKM;
                bbSite.N2KVersioningRef = Int32.Parse(pVSite.VERSIONID.ToString());
                bbSite.N2KVersioningVersion = pEnvelope.VersionId;
                bbSite.DateConfSCI = pVSite.DATE_CONF_SCI;
                bbSite.Priority = null;
                bbSite.DatePropSCI = pVSite.DATE_PROP_SCI;
                bbSite.DateSpa = pVSite.DATE_SPA;
                bbSite.DateSac = pVSite.DATE_SAC;
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
        /// Retrives the information of the BioRegions for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of bioregions stored</returns>
        //private async Task<List<BioRegions>> harvestBioregions(NaturaSite pVSite, int pVersion, string backboneDb)
        private async Task<int> harvestBioregions(NaturaSite pVSite, int pVersion, string backboneDb, string versioningDB, IDictionary<Type, object> _siteItems)
        {
            List<BelongsToBioRegion> elements = null;
            List<BioRegions> items = new List<BioRegions>();
            SqlConnection versioningConn=null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);


                String queryString = @"select SITECODE as SiteCode ,@NEWVERSION as version,BIOREGID as BGRID, PERCENTAGE as Percentage
                                     from BelongsToBioRegion
                                     where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";
                command = new SqlCommand(queryString, versioningConn);
                versioningConn.Open();

                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);
                reader = await command.ExecuteReaderAsync();


                while (reader.Read())
                {
                    BioRegions item = new BioRegions();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    item.Version = pVersion;
                    item.BGRID = TypeConverters.CheckNull<int>(reader["BGRID"]);
                    item.Percentage = null;
                    if (reader["Percentage"] != DBNull.Value) 
                        item.Percentage = decimal.ToDouble(TypeConverters.CheckNull<decimal>(reader["Percentage"]));
                    items.Add(item);
                }
                List<BioRegions> _listed = (List<BioRegions>)_siteItems[typeof(List<BioRegions>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<BioRegions>)] = _listed;

                return 1;

            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestBioregions", "");
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
            return 1;
        }

        /// <summary>
        /// Retrives the information of the NUTS for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of NUTS stored</returns>
        //private async Task<List<NutsBySite>>? harvestNutsBySite(NaturaSite pVSite, int pVersion)
        private async Task<int> harvestNutsBySite(NaturaSite pVSite, int pVersion, string backboneDb, string versioningDB, IDictionary<Type, object> _siteItems)
        {
            List<NutsBySite> items = new List<NutsBySite>();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"select SITECODE as SiteCode, @NEWVERSION as  Version, NUTSCODE as NutId,
                            SUM(COVER) as CoverPercentage 
                            from NutsRegion 
                            where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID
                            group by SITECODE, VERSIONID, NUTSCODE";

                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    NutsBySite item = new NutsBySite();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    item.Version = pVersion;
                    item.NutId = TypeConverters.CheckNull<string>(reader["NutId"]);
                    item.CoverPercentage = null;
                    if (reader["CoverPercentage"] != DBNull.Value)
                        item.CoverPercentage = decimal.ToDouble(TypeConverters.CheckNull<decimal>(reader["CoverPercentage"]));

                    items.Add(item);
                }

                List<NutsBySite> _listed = (List<NutsBySite>) _siteItems[typeof(List<NutsBySite>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<NutsBySite>)] = _listed;

                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestNutsBySite", "");
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

        /// <summary>
        /// Retrives the information of the IsImpactedBy elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of IsImpactedBy stored</returns>
        private async Task<int> harvestIsImpactedBy(NaturaSite pVSite, int pVersion, string backboneDb, string versioningDB, IDictionary<Type, object> _siteItems)
        {
            List<Models.backbone_db.IsImpactedBy> items = new List<Models.backbone_db.IsImpactedBy>();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE as SiteCode,@NEWVERSION as Version,ACTIVITYCODE as ActivityCode,IN_OUT  as InOut,INTENSITY as Intensity,
                        PERCENTAGEAFF as PercentageAff, INFLUENCE as Influence ,STARTDATE as StartDate,ENDDATE as EndDate,POLLUTIONCODE as PollutionCode,OCCURRENCE as Ocurrence,IMPACTTYPE  as ImpactType                         
                            from IsImpactedBy 
                            where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read()) {                                        
                    Models.backbone_db.IsImpactedBy item = new Models.backbone_db.IsImpactedBy();

                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    item.Version = pVersion;
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

                List<Models.backbone_db.IsImpactedBy> _listed = (List<Models.backbone_db.IsImpactedBy>)_siteItems[typeof(List<Models.backbone_db.IsImpactedBy>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<Models.backbone_db.IsImpactedBy>)] = _listed;
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestIsImpactedBy", "");
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

        /// <summary>
        /// Retrives the information of the HasNationalProtection elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of HasNationalProtection stored</returns>
        private async Task<int> harvestHasNationalProtection(NaturaSite pVSite, int pVersion, string backboneDb, string versioningDB, IDictionary<Type, object> _siteItems)
        {
            
            List<Models.backbone_db.HasNationalProtection> items = new List<Models.backbone_db.HasNationalProtection>();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE as SiteCode,@NEWVERSION as Version, DesignatedCode, Percentage
                            from HasNationalProtection 
                            where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {

                    Models.backbone_db.HasNationalProtection item = new Models.backbone_db.HasNationalProtection();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    item.Version = pVersion;
                    item.DesignatedCode = TypeConverters.CheckNull<string>(reader["DesignatedCode"]);
                    item.Percentage = TypeConverters.CheckNull<decimal>(reader["Percentage"]);
                    items.Add(item);
                }

                List<Models.backbone_db.HasNationalProtection> _listed = (List<Models.backbone_db.HasNationalProtection>)_siteItems[typeof(List<Models.backbone_db.HasNationalProtection>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<Models.backbone_db.HasNationalProtection>)] = _listed;
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestHasNationalProtection", "");
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

        /// <summary>
        /// Retrives the information of the DetailedProtectionStatus elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of DetailedProtectionStatus stored</returns>
        private async Task<int> harvestDetailedProtectionStatus(NaturaSite pVSite, int pVersion, string backboneDb, string versioningDB, IDictionary<Type, object> _siteItems)
        {
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                List<Models.backbone_db.DetailedProtectionStatus> items = new List<Models.backbone_db.DetailedProtectionStatus>();
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        N2K_SITECODE as SiteCode,@NEWVERSION as Version,DesignationCode,OverlapCode,OVERLAPPERC as OverlapPercentage,Convention
                            from DetailedProtectionStatus 
                            where N2K_SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {                    
                    Models.backbone_db.DetailedProtectionStatus item = new Models.backbone_db.DetailedProtectionStatus();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    item.Version = pVersion;
                    item.DesignationCode = TypeConverters.CheckNull<string>(reader["DesignationCode"]);
                    item.OverlapCode = TypeConverters.CheckNull<string>(reader["OverlapCode"]);
                    item.OverlapPercentage = TypeConverters.CheckNull<decimal>(reader["OverlapPercentage"]);
                    item.Convention = TypeConverters.CheckNull<string>(reader["Convention"]);
                    items.Add(item);
                }

                List<Models.backbone_db.DetailedProtectionStatus> _listed = (List<Models.backbone_db.DetailedProtectionStatus>)_siteItems[typeof(List<Models.backbone_db.DetailedProtectionStatus>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<Models.backbone_db.DetailedProtectionStatus>)] = _listed;
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestDetailedProtectionStatus", "");
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

        /// <summary>
        /// Retrives the information of the SiteLargeDescriptions elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of SiteLargeDescriptions stored</returns>
        private async Task<int>? harvestSiteLargeDescriptions(NaturaSite pVSite, int pVersion, string backboneDb, string versioningDB, IDictionary<Type, object> _siteItems)
        {
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                List<Models.backbone_db.SiteLargeDescriptions> items = new List<Models.backbone_db.SiteLargeDescriptions>();
                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE as SiteCode,@NEWVERSION as Version,Quality, Vulnarab,Designation,MANAG_PLAN as ManagPlan,Documentation,
                        OtherCharact,MANAG_CONSERV_MEASURES as  ManagConservMeasures, MANAG_PLAN_URL as ManagPlanUrl,MANAG_STATUS as ManagStatus
                        from Description 
                        where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {

                    SiteLargeDescriptions item = new SiteLargeDescriptions();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    item.Version = pVersion;
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
                List<SiteLargeDescriptions> _listed = (List<SiteLargeDescriptions>)_siteItems[typeof(List<SiteLargeDescriptions>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<SiteLargeDescriptions>)] = _listed;
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSiteLargeDescriptions", "");
                return 0;
            }
            finally
            {
                if (versioningConn != null)
                {
                    versioningConn.Close();
                    versioningConn.Dispose();
                    if (command!=null )  command.Dispose();
                    if (reader!=null) await reader.DisposeAsync();
                }
            }

        }

        /// <summary>
        /// Retrives the information of the SiteOwnerType elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of SiteOwnerType stored</returns>
        private async Task<int> harvestSiteOwnerType(NaturaSite pVSite, int pVersion, IList<Models.backbone_db.OwnerShipTypes> _ownerShipTypes,string backboneDb, string versioningDB, IDictionary<Type, object> _siteItems)
        {
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                List<Models.backbone_db.SiteOwnerType> items = new List<Models.backbone_db.SiteOwnerType>();

                versioningConn = new SqlConnection(versioningDB);
                String queryString = @"SELECT 
                        SITECODE,TYPE, SUM([PERCENT]) as [percent]
                        from OwnerType 
                        where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID
                        group by SITECODE, VERSIONID, Type";	

                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read()) { 
                    SiteOwnerType item = new SiteOwnerType();
                    item.SiteCode = TypeConverters.CheckNull<string>(reader["SiteCode"]);
                    item.Version = pVersion;
                    //6-Unknown for those types not found in the reference OwnerShipTypes
                    item.Type = _ownerShipTypes.Where(s => s.Description == reader["Type"].ToString()).Select(s => s.Id).FirstOrDefault();
                    item.Percent = TypeConverters.CheckNull<decimal>(reader["Percent"]);
                    items.Add(item);

                }
                List<SiteOwnerType> _listed = (List<SiteOwnerType>)_siteItems[typeof(List<SiteOwnerType>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<SiteOwnerType>)] = _listed;
                return 1;

            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - harvestSiteOwnerType", "");
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

        public async Task<List<SiteChangeDb>> ValidateSiteAttributes(List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, double siteAreaHaTolerance, double siteLengthKmTolerance, ProcessedEnvelopes? processedEnvelope)
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
                    siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                    siteChange.Tags = string.Empty;
                    siteChange.NewValue = harvestingSite.SiteName;
                    siteChange.OldValue = storedSite.SiteName;
                    siteChange.Code = harvestingSite.SiteCode;
                    siteChange.Section = "Site";
                    siteChange.VersionReferenceId = storedSite.VersionId;
                    siteChange.FieldName = "SiteName";
                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
                    siteChange.N2KVersioningVersion = envelope.VersionId;
                    changes.Add(siteChange);
                }
                if (!Convert.ToString(harvestingSite.DateConfSCI).Equals(Convert.ToString(storedSite.DateConfSCI)))
                {
                    if (Convert.ToString(harvestingSite.DateConfSCI).Equals("01/01/1900 0:00:00") && !Convert.ToString(storedSite.DateConfSCI).Equals("01/01/1900 0:00:00"))
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Site General Info";
                        siteChange.ChangeType = "Reported DateConfSCI is empty";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Info;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = null;
                        siteChange.OldValue = storedSite.DateConfSCI.Value.ToShortDateString();
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "DateConfSCI";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
                        changes.Add(siteChange);
                    }
                    else if (!Convert.ToString(harvestingSite.DateConfSCI).Equals("01/01/1900 0:00:00") && Convert.ToString(storedSite.DateConfSCI).Equals("01/01/1900 0:00:00"))
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Site General Info";
                        siteChange.ChangeType = "Reference DateConfSCI is empty and reported value is not";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Critical;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = harvestingSite.DateConfSCI.Value.ToShortDateString();
                        siteChange.OldValue = null;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "DateConfSCI";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
                        changes.Add(siteChange);
                    }
                    else
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Site General Info";
                        siteChange.ChangeType = "Reported DateConfSCI is different";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Warning;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = harvestingSite.DateConfSCI.Value.ToShortDateString();
                        siteChange.OldValue = storedSite.DateConfSCI.Value.ToShortDateString();
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "DateConfSCI";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
                        changes.Add(siteChange);
                    }
                }
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
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.NewValue = harvestingSite.AreaHa != -1 ? harvestingSite.AreaHa.ToString() : null;
                        siteChange.OldValue = storedSite.AreaHa != -1 ? storedSite.AreaHa.ToString() : null;
                        siteChange.Tags = string.Empty;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "AreaHa";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
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
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.NewValue = harvestingSite.AreaHa != -1 ? harvestingSite.AreaHa.ToString() : null;
                        siteChange.OldValue = storedSite.AreaHa != -1 ? storedSite.AreaHa.ToString() : null;
                        siteChange.Tags = string.Empty;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "AreaHa";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
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
                    siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                    siteChange.NewValue = harvestingSite.AreaHa != -1 ? harvestingSite.AreaHa.ToString() : null;
                    siteChange.OldValue = storedSite.AreaHa != -1 ? storedSite.AreaHa.ToString() : null;
                    siteChange.Tags = string.Empty;
                    siteChange.Code = harvestingSite.SiteCode;
                    siteChange.Section = "Site";
                    siteChange.VersionReferenceId = storedSite.VersionId;
                    siteChange.FieldName = "AreaHa";
                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
                    siteChange.N2KVersioningVersion = envelope.VersionId;
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
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.NewValue = harvestingSite.LengthKm != -1 ? harvestingSite.LengthKm.ToString() : null;
                        siteChange.OldValue = storedSite.LengthKm != -1 ? storedSite.LengthKm.ToString() : null;
                        siteChange.Tags = string.Empty;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "LengthKm";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
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

        public async Task<List<SiteChangeDb>> ValidateBioRegions(List<BioRegions> bioRegionsVersioning, List<BioRegions> referencedBioRegions, List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5, ProcessedEnvelopes? processedEnvelope)
        {
            try
            {
                //Get the lists of bioregion types
                List<BioRegionTypes> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().AsNoTracking().ToListAsync();

                //For each BioRegion in Versioning compare it with that BioRegion in backboneDB
                foreach (BioRegions harvestingBioRegions in bioRegionsVersioning)
                {
                    BioRegions storedBioRegions = referencedBioRegions.Where(s => s.BGRID == harvestingBioRegions.BGRID).FirstOrDefault();
                    BioRegionTypes bioRegionType = bioRegionTypes.Where(s => s.Code == harvestingBioRegions.BGRID).FirstOrDefault();
                    if (storedBioRegions == null)
                    {
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = harvestingSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Network general structure";
                        siteChange.ChangeType = "Sites added due to a change of BGR";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Critical;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = bioRegionType.RefBioGeoName;
                        siteChange.OldValue = null;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "BioRegions";
                        siteChange.VersionReferenceId = harvestingSite.VersionId;
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
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
                        SiteChangeDb siteChange = new SiteChangeDb();
                        siteChange.SiteCode = storedSite.SiteCode;
                        siteChange.Version = harvestingSite.VersionId;
                        siteChange.ChangeCategory = "Network general structure";
                        siteChange.ChangeType = "Sites deleted due to a change of BGR";
                        siteChange.Country = envelope.CountryCode;
                        siteChange.Level = Enumerations.Level.Critical;
                        siteChange.Status = (SiteChangeStatus?)processedEnvelope.Status;
                        siteChange.Tags = string.Empty;
                        siteChange.NewValue = null;
                        siteChange.OldValue = bioRegionType.RefBioGeoName;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "BioRegions";
                        siteChange.VersionReferenceId = storedBioRegions.Version;
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
                        siteChange.N2KVersioningVersion = envelope.VersionId;
                        changes.Add(siteChange);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "ValidateBioRegions - Start - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "");
            }
            return changes;
        }


        private async Task<int> harvestRespondents(NaturaSite pVSite, int pVersion, string backboneDb,string versioningDB, IDictionary<Type, object> _siteItems)
        {
            List<Respondents> items = new List<Respondents>();
            SqlConnection versioningConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                items = new List<Respondents>();
                versioningConn = new SqlConnection(versioningDB);
                SqlParameter param1 = new SqlParameter("@SITECODE", pVSite.SITECODE);
                SqlParameter param2 = new SqlParameter("@COUNTRYVERSIONID", pVSite.COUNTRYVERSIONID);
                SqlParameter param3 = new SqlParameter("@NEWVERSION", pVersion);

                String queryString = @"select SITECODE as SiteCode,@NEWVERSION as version,LOCATOR_NAME as locatorName,ADDRESS_AREA as addressArea,POST_NAME as postName,POSTCODE as postCode,THOROUGHFARE as thoroughfare,UNSTRUCTURED_ADD as addressUnstructured,CONTACT_NAME as name, EMAIL as Email, ADMIN_UNIT as AdminUnit,LOCATOR_DESIGNATOR as LocatorDesignator
                                       from CONTACT
                                       where SITECODE=@SITECODE and COUNTRYVERSIONID=@COUNTRYVERSIONID";

                versioningConn.Open();
                command = new SqlCommand(queryString, versioningConn);
                command.Parameters.Add(param1);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);
                reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    Respondents respondent = new Respondents();
                    respondent.SiteCode = reader["SiteCode"].ToString();
                    respondent.Version = pVersion;
                    respondent.locatorName = TypeConverters.CheckNull<string>(reader["locatorName"]);
                    respondent.addressArea = TypeConverters.CheckNull<string>(reader["addressArea"]);
                    respondent.postName = TypeConverters.CheckNull<string>(reader["postName"]);
                    respondent.postCode = TypeConverters.CheckNull<string>(reader["postCode"]);
                    respondent.thoroughfare = TypeConverters.CheckNull<string>(reader["thoroughfare"]);
                    respondent.addressUnstructured = TypeConverters.CheckNull<string>(reader["addressUnstructured"]);
                    respondent.name = TypeConverters.CheckNull<string>(reader["name"]);
                    respondent.Email = TypeConverters.CheckNull<string>(reader["Email"]);
                    respondent.AdminUnit = TypeConverters.CheckNull<string>(reader["AdminUnit"]);
                    respondent.LocatorDesignator = TypeConverters.CheckNull<string>(reader["LocatorDesignator"]);
                    items.Add(respondent);
                }

                List<Respondents> _listed = (List<Respondents>)_siteItems[typeof(List<Respondents>)];
                _listed.AddRange(items);
                _siteItems[typeof(List<Respondents>)] = _listed;
                return 1;


            }
            catch (Exception ex){
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - HarvestRespondents", "");
                return 0;
            }
            finally
            {
                if (versioningConn !=null)
                {
                    versioningConn.Close();
                    versioningConn.Dispose();
                    if (command!=null) command.Dispose();
                    if (reader!=null) await reader.DisposeAsync();
                }
            }

        }

    }
}
