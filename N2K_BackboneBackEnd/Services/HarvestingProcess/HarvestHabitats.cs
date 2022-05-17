using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;

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

        public async Task<int> HarvestByCountry(string pCountryCode, int pCountryVersion, int pVersion)
        {
            try
            {
                TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Starting");
                Console.WriteLine("=>Start full habitat harvest by country...");

                await HarvestHabitatsByCountry(pCountryCode, pCountryVersion, pVersion);
                await HarvestDescribeSitesByCountry(pCountryCode, pCountryVersion, pVersion);

                Console.WriteLine("=>End full habitat harvest by country...");
                TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("=>End full habitat harvest by country with error...");
                TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Exit");
                return 0;
            }
        }

        public async Task<int> HarvestBySite(string pSiteCode, decimal pSiteVersion, int pVersion)
        {
            try
            {
                TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Starting");
                Console.WriteLine("=>Start full habitat harvest by site...");

                await HarvestHabitatsBySite(pSiteCode, pSiteVersion, pVersion);
                await HarvestDescribeSitesBySite(pSiteCode, pSiteVersion, pVersion);

                Console.WriteLine("=>End full habitat harvest by site...");
                TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("=>End full habitat harvest by site with error...");
                TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Exit");
                return 0;
            }
        }

        public async Task<int> HarvestHabitatsByCountry(string pCountryCode, int pCountryVersion, int pVersion)
        {
            List<ContainsHabitat> elements = null;
            try
            {
                Console.WriteLine("=>Start habitat harvest by country...");

                elements = _versioningContext.Set<ContainsHabitat>().Where(s => s.COUNTRYCODE == pCountryCode && s.COUNTRYVERSIONID == pCountryVersion).ToList();

                foreach (ContainsHabitat element in elements)
                {
                    Habitats item = new Habitats();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.CoverHA = (decimal?)element.COVER_HA;
                    item.PriorityForm = element.PF;
                    item.Representativity = element.REPRESENTATIVITY;
                    item.DataQty = Convert.ToInt32(element.DATAQUALITY);
                    //item.Conservation = element.CONSERVATION; // ??? PENDING
                    item.GlobalAssesments = element.GLOBALASSESMENT;
                    item.RelativeSurface = element.RELSURFACE;
                    item.Percentage = (decimal?)element.PERCENTAGECOVER;
                    item.ConsStatus = element.CONSSTATUS;
                    item.Caves = Convert.ToString(element.CAVES); // ???
                    item.PF = Convert.ToString(element.PF); // ??? PENDING The same as PriorityForm
                    item.NonPresenciInSite = Convert.ToString(element.NONPRESENCEINSITE); // ???

                    _dataContext.Set<Habitats>().Add(item);
                }

                Console.WriteLine("=>End habitat harvest by country...");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("=>End habitat harvest by country with error...");
                return 0;
            }

        }

        public async Task<int> HarvestHabitatsBySite(string pSiteCode, decimal pSiteVersion, int pVersion)
        {
            List<ContainsHabitat> elements = null;
            try
            {
                TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Processing");

                elements = _versioningContext.Set<ContainsHabitat>().Where(s => s.SITECODE == pSiteCode && s.VERSIONID == pSiteVersion).ToList();

                foreach (ContainsHabitat element in elements)
                {
                    Habitats item = new Habitats();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.CoverHA = (decimal?)element.COVER_HA;
                    item.PriorityForm = element.PF;
                    item.Representativity = element.REPRESENTATIVITY;
                    item.DataQty = (element.DATAQUALITY !=null)?_dataContext.Set<DataQualityTypes>().Where(d => d.HabitatCode == element.DATAQUALITY).Select(d => d.Id).FirstOrDefault():null;
                    //item.Conservation = element.CONSERVATION; // ??? PENDING
                    item.GlobalAssesments = element.GLOBALASSESMENT;
                    item.RelativeSurface = element.RELSURFACE;
                    item.Percentage = (decimal?)element.PERCENTAGECOVER;
                    item.ConsStatus = element.CONSSTATUS;
                    item.Caves = Convert.ToString(element.CAVES); // ???
                    item.PF = Convert.ToString(element.PF); // ??? PENDING The same as PriorityForm
                    item.NonPresenciInSite = Convert.ToString(element.NONPRESENCEINSITE); // ???

                    _dataContext.Set<Habitats>().Add(item);
                }

                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestHabitats - HarvestBySite", "");
                return 0;
            }
            finally {
                TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "End");
            }

        }

        public async Task<int> HarvestDescribeSitesByCountry(string pCountryCode, int pCountryVersion, int pVersion)
        {
            List<DescribesSites> elements = null;
            try
            {
                Console.WriteLine("=>Start describeSites harvest by country...");

                elements = _versioningContext.Set<DescribesSites>().Where(s => s.COUNTRYCODE == pCountryCode && s.COUNTRYVERSIONID == pCountryVersion).ToList();

                foreach (DescribesSites element in elements)
                {
                    DescribeSites item = new DescribeSites();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.Percentage = element.PERCENTAGECOVER;

                    _dataContext.Set<DescribeSites>().Add(item);
                }

                Console.WriteLine("=>End describeSites harvest by country...");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("=>End describeSites harvest by country with error...");
                return 0;
            }
        }

        public async Task<int> HarvestDescribeSitesBySite(string pSiteCode, decimal pSiteVersion, int pVersion)
        {
            List<DescribesSites> elements = null;
            try
            {
                Console.WriteLine("=>Start describeSites harvest by site...");

                elements = _versioningContext.Set<DescribesSites>().Where(s => s.SITECODE == pSiteCode && s.VERSIONID == pSiteVersion).ToList();

                foreach (DescribesSites element in elements)
                {
                    DescribeSites item = new DescribeSites();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.Percentage = element.PERCENTAGECOVER;

                    _dataContext.Set<DescribeSites>().Add(item);
                }

                Console.WriteLine("=>End describeSites harvest by site...");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("=>End describeSites harvest by site with error...");
                return 0;
            }
        }

        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start HarvestHabitats validate...");
            await Task.Delay(2000);
            Console.WriteLine("==>End HarvestHabitats validate...");
            return 1;
        }
    }
}
