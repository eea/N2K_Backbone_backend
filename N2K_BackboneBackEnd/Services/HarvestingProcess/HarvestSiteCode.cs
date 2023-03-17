using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
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
        public async Task<Sites>? HarvestSite(NaturaSite pVSite, EnvelopesToProcess pEnvelope, Sites? bbSite)
        {
            try
            {
                //Get the data for all related tables
                Respondents.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestRespondents(pVSite, bbSite.Version));
                BioRegions.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestBioregions(pVSite, bbSite.Version));
                NutsBySite.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestNutsBySite(pVSite, bbSite.Version));
                Models.backbone_db.IsImpactedBy.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestIsImpactedBy(pVSite, bbSite.Version));
                Models.backbone_db.HasNationalProtection.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestHasNationalProtection(pVSite, bbSite.Version));
                Models.backbone_db.DetailedProtectionStatus.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestDetailedProtectionStatus(pVSite, bbSite.Version));
                SiteLargeDescriptions.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestSiteLargeDescriptions(pVSite, bbSite.Version));
                SiteOwnerType.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), await harvestSiteOwnerType(pVSite, bbSite.Version));
                //TimeLog.setTimeStamp("Site " + pVSite.SITECODE + " - " + pVSite.VERSIONID.ToString(), "Processed");

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
        private async Task<List<BioRegions>> harvestBioregions(NaturaSite pVSite, int pVersion)
        {
            List<BelongsToBioRegion> elements = null;
            List<BioRegions> items = new List<BioRegions>();
            try
            {
                //elements = await _versioningContext.Set<BelongsToBioRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
                //Chaged to support a multiple primary codes exception (Site, version and Bioregion)
                elements = await _versioningContext.Set<BelongsToBioRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).GroupBy(s => s.BIOREGID).Select(bb => new BelongsToBioRegion
                {
                    COUNTRYCODE = bb.First().COUNTRYCODE,
                    VERSIONID = bb.First().VERSIONID,
                    COUNTRYVERSIONID = bb.First().COUNTRYVERSIONID,
                    SITECODE = bb.First().SITECODE,
                    BIOREGID = bb.First().BIOREGID,
                    PERCENTAGE = bb.Sum(c => c.PERCENTAGE),
                }).ToListAsync();

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

        /// <summary>
        /// Retrives the information of the NUTS for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of NUTS stored</returns>
        private async Task<List<NutsBySite>>? harvestNutsBySite(NaturaSite pVSite, int pVersion)
        {
            List<NutsRegion> elements = null;
            List<NutsBySite> items = new List<NutsBySite>();
            try
            {
                //elements = await _versioningContext.Set<NutsRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
                elements = await _versioningContext.Set<NutsRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).GroupBy(s => new { s.SITECODE, s.VERSIONID, s.NUTSCODE })
                    .Select(gs => new NutsRegion() { SITECODE = gs.Key.SITECODE, VERSIONID = gs.Key.VERSIONID, NUTSCODE = gs.Key.NUTSCODE, COVER = gs.Sum(c => c.COVER) }).ToListAsync();



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

        /// <summary>
        /// Retrives the information of the IsImpactedBy elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of IsImpactedBy stored</returns>
        private async Task<List<Models.backbone_db.IsImpactedBy>>? harvestIsImpactedBy(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.IsImpactedBy> elements = null;
            List<Models.backbone_db.IsImpactedBy> items = new List<Models.backbone_db.IsImpactedBy>();
            try
            {
                elements = await _versioningContext.Set<Models.versioning_db.IsImpactedBy>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
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

        /// <summary>
        /// Retrives the information of the HasNationalProtection elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of HasNationalProtection stored</returns>
        private async Task<List<Models.backbone_db.HasNationalProtection>>? harvestHasNationalProtection(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.HasNationalProtection> elements = null;
            List<Models.backbone_db.HasNationalProtection> items = new List<Models.backbone_db.HasNationalProtection>();
            try
            {
                elements = await _versioningContext.Set<Models.versioning_db.HasNationalProtection>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
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

        /// <summary>
        /// Retrives the information of the DetailedProtectionStatus elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of DetailedProtectionStatus stored</returns>
        private async Task<List<Models.backbone_db.DetailedProtectionStatus>>? harvestDetailedProtectionStatus(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.DetailedProtectionStatus> elements = null;
            List<Models.backbone_db.DetailedProtectionStatus> items = new List<Models.backbone_db.DetailedProtectionStatus>();
            try
            {
                elements = await _versioningContext.Set<Models.versioning_db.DetailedProtectionStatus>().Where(s => s.N2K_SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
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

        /// <summary>
        /// Retrives the information of the SiteLargeDescriptions elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of SiteLargeDescriptions stored</returns>
        private async Task<List<Models.backbone_db.SiteLargeDescriptions>>? harvestSiteLargeDescriptions(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.Description> elements = null;
            List<Models.backbone_db.SiteLargeDescriptions> items = new List<Models.backbone_db.SiteLargeDescriptions>();
            try
            {
                elements = await _versioningContext.Set<Models.versioning_db.Description>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
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

        /// <summary>
        /// Retrives the information of the SiteOwnerType elements for a Site and stores them in BackBone
        /// </summary>
        /// <param name="pVSite">The object Versioning Site</param>
        /// <param name="pVersion">The version in BackBone</param>
        /// <returns>List of SiteOwnerType stored</returns>
        private async Task<List<Models.backbone_db.SiteOwnerType>>? harvestSiteOwnerType(NaturaSite pVSite, int pVersion, IList<Models.backbone_db.OwnerShipTypes> OwnerShipTypes)
        {

            List<Models.versioning_db.OwnerType> elements = null;
            List<Models.backbone_db.SiteOwnerType> items = new List<Models.backbone_db.SiteOwnerType>();
            try
            {
                elements = await _versioningContext.Set<Models.versioning_db.OwnerType>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).GroupBy(s => s.TYPE).Select(cl => new OwnerType
                {
                    COUNTRYCODE = cl.First().COUNTRYCODE,
                    VERSIONID = cl.First().VERSIONID,
                    COUNTRYVERSIONID = cl.First().COUNTRYVERSIONID,
                    SITECODE = cl.First().SITECODE,
                    TYPE = cl.First().TYPE,
                    PERCENT = cl.Sum(c => c.PERCENT),
                }).ToListAsync();
                foreach (OwnerType element in elements)
                {
                    Models.backbone_db.SiteOwnerType item = new Models.backbone_db.SiteOwnerType();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    //6-Unknown for those types not found in the reference OwnerShipTypes
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


        private async Task<List<Respondents>>? harvestRespondents(NaturaSite pVSite, int pVersion)
        {
            try
            {
                List<Contact> vContact = _versioningContext.Set<Contact>().Where(v => (v.SITECODE == pVSite.SITECODE) && (v.COUNTRYVERSIONID == pVSite.COUNTRYVERSIONID)).ToList();
                List<Respondents> items = new List<Respondents>();
                foreach (Contact contact in vContact)
                {
                    Respondents respondent = new Respondents();
                    respondent.SiteCode = contact.SITECODE;
                    respondent.Version = pVersion;
                    respondent.locatorName = contact.LOCATOR_NAME;
                    respondent.addressArea = contact.ADDRESS_AREA;
                    respondent.postName = contact.POST_NAME;
                    respondent.postCode = contact.POSTCODE;
                    respondent.thoroughfare = contact.THOROUGHFARE;
                    respondent.addressUnstructured = contact.UNSTRUCTURED_ADD;
                    respondent.name = contact.CONTACT_NAME;
                    respondent.Email = contact.EMAIL;
                    respondent.AdminUnit = contact.ADMIN_UNIT;
                    respondent.LocatorDesignator = contact.LOCATOR_DESIGNATOR;
                    items.Add(respondent);
                }
                return items;
            }
            catch (Exception ex){
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestedService - HarvestRespondents", "");
                return null;
            }
        }

    }
}
