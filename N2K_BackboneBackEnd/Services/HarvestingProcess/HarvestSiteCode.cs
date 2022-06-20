using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Models.versioning_db;

namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    /// <summary>
    /// Class dedicated to the import of a Site and the entities that dependes directly from the Site Entity.
    /// </summary>
    public  class HarvestSiteCode : BaseHarvestingProcess, IHarvestingTables
    {
        
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="dataContext">Context for the BackBone database</param>
        /// <param name="versioningContext">Context for the Versioning database</param>
        public HarvestSiteCode(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext) : base(dataContext, versioningContext)
        {
        }

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
        public  async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
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
        public async Task<Sites>? HarvestSite (NaturaSite pVSite, EnvelopesToProcess pEnvelope)
        {
            Sites? bbSite = null;
            try
            {
                
                
                bbSite = await harvestSiteCode(pVSite, pEnvelope);
                _dataContext.Set<Sites>().Add(bbSite);
                _dataContext.SaveChanges();

                //Get the data for all related tables                                
                _dataContext.Set<BioRegions>().AddRange(await harvestBioregions(pVSite, bbSite.Version));
                _dataContext.Set<NutsBySite>().AddRange(await harvestNutsBySite(pVSite, bbSite.Version));
                _dataContext.Set<Models.backbone_db.IsImpactedBy>().AddRange(await harvestIsImpactedBy(pVSite, bbSite.Version));
                _dataContext.Set<Models.backbone_db.HasNationalProtection>().AddRange(await harvestHasNationalProtection(pVSite, bbSite.Version));
                _dataContext.Set<Models.backbone_db.DetailedProtectionStatus>().AddRange(await harvestDetailedProtectionStatus(pVSite, bbSite.Version));
                _dataContext.Set<SiteLargeDescriptions>().AddRange(await harvestSiteLargeDescriptions(pVSite, bbSite.Version));
                _dataContext.Set<SiteOwnerType>().AddRange(await harvestSiteOwnerType(pVSite, bbSite.Version));
                TimeLog.setTimeStamp("Site " + pVSite.SITECODE + " - " + pVSite.VERSIONID.ToString(), "Processed");
                
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
        private async Task<Sites>? harvestSiteCode(NaturaSite pVSite, EnvelopesToProcess pEnvelope)
        {
            //Tomamos el valor más alto que tiene en el campo Version para ese SiteCode. Por defecto es -1 para cuando no existe 
            //por que le vamos a sumar un 1 lo cual dejaría en 0
            Sites bbSite = new Sites();
            int versionNext = -1;

            try
            {
                versionNext = await _dataContext.Set<Sites>().Where(s => s.SiteCode == pVSite.SITECODE).OrderBy(s => s.Version).Select(s => s.Version).LastOrDefaultAsync();
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
                elements = await _versioningContext.Set<BelongsToBioRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
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
                elements = await _versioningContext.Set<NutsRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).GroupBy(s=> new { s.SITECODE, s.VERSIONID, s.NUTSCODE })
                    .Select(gs=>new NutsRegion() { SITECODE = gs.Key.SITECODE, VERSIONID = gs.Key.VERSIONID, NUTSCODE = gs.Key.NUTSCODE, COVER = gs.Sum(c => c.COVER) }).ToListAsync();
                
                
                
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
        private async Task< List<Models.backbone_db.SiteLargeDescriptions>>? harvestSiteLargeDescriptions(NaturaSite pVSite, int pVersion)
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
        private async Task<List<Models.backbone_db.SiteOwnerType>>? harvestSiteOwnerType(NaturaSite pVSite, int pVersion)
        {

            List<Models.versioning_db.OwnerType> elements = null;
            List<Models.backbone_db.SiteOwnerType> items = new List<Models.backbone_db.SiteOwnerType>();
            try
            {
                elements = await _versioningContext.Set<Models.versioning_db.OwnerType>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
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

        public async Task<List<SiteChangeDb>> ValidateSiteAttributes(List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, double siteAreaHaTolerance, double siteLengthKmTolerance)
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
                    siteChange.VersionReferenceId = storedSite.VersionId;
                    siteChange.FieldName = "SiteName";
                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
                    changes.Add(siteChange);
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
                        siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                        siteChange.NewValue = harvestingSite.AreaHa != -1 ? harvestingSite.AreaHa.ToString() : null;
                        siteChange.OldValue = storedSite.AreaHa != -1 ? storedSite.AreaHa.ToString() : null;
                        siteChange.Tags = string.Empty;
                        siteChange.Code = harvestingSite.SiteCode;
                        siteChange.Section = "Site";
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "AreaHa";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
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
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "AreaHa";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
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
                    siteChange.VersionReferenceId = storedSite.VersionId;
                    siteChange.FieldName = "AreaHa";
                    siteChange.ReferenceSiteCode = storedSite.SiteCode;
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
                        siteChange.VersionReferenceId = storedSite.VersionId;
                        siteChange.FieldName = "LengthKm";
                        siteChange.ReferenceSiteCode = storedSite.SiteCode;
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

    }
}
