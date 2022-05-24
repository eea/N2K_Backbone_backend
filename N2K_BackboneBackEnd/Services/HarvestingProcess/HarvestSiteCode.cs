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
    public class HarvestSiteCode : BaseHarvestingProcess, IHarvestingTables
    {
        public HarvestSiteCode(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext) : base(dataContext, versioningContext)
        {
        }

        public async Task<int> Harvest(string countryCode, int versionId)
        {
            Console.WriteLine("=>Start Site Code harvest...");



            await Task.Delay(5000);
            Console.WriteLine("=>End Site Code harvest...");
            return 1;

        }


        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start Site Code validate...");
            await Task.Delay(10000);
            Console.WriteLine("==>ENd Site Code validate...");
            return 1;
        }


        public async Task<Sites>? HarvestSite(NaturaSite pVSite, EnvelopesToProcess pEnvelope)
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

        private async Task<Sites>? harvestSiteCode(NaturaSite pVSite, EnvelopesToProcess pEnvelope)
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

        private async Task<List<NutsBySite>>? harvestNutsBySite(NaturaSite pVSite, int pVersion)
        {
            List<NutsRegion> elements = null;
            List<NutsBySite> items = new List<NutsBySite>();
            try
            {
                elements = await _versioningContext.Set<NutsRegion>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToListAsync();
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
    }
}
