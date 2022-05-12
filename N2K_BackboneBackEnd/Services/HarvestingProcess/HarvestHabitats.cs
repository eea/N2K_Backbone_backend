using N2K_BackboneBackEnd.Data;

namespace N2K_BackboneBackEnd.Services.HarvestingProcess
{
    public class HarvestHabitats : BaseHarvestingProcess, IHarvestingTables
    {
        public HarvestHabitats(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext) : base(dataContext, versioningContext)
        {
        }

        public async Task<int> Harvest(string countryCode, int versionId)
        {

            Console.WriteLine("=>Start HarvestHabitats harvest...");
            await Task.Delay(500);
            Console.WriteLine("=>End HarvestHabitatsharvest...");
            return 1;


        }

        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start HarvestHabitats validate...");
            await Task.Delay(5000);
            Console.WriteLine("==>End HarvestHabitats validate...");
            return 1;
        }

        private List<Models.backbone_db.Habitats> harvestHabitats(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.ContainsHabitat> elements = null;
            List<Models.backbone_db.Habitats> items = new List<Models.backbone_db.Habitats>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.ContainsHabitat>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.ContainsHabitat element in elements)
                {
                    Models.backbone_db.Habitats item = new Models.backbone_db.Habitats();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.PriorityForm = element.PF;
                    items.Add(item);
                }
                return items;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {

            }

        }

        private List<Models.backbone_db.HabitatAreas> harvestHabitatAreas(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.ContainsHabitat> elements = null;
            List<Models.backbone_db.HabitatAreas> items = new List<Models.backbone_db.HabitatAreas>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.ContainsHabitat>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.ContainsHabitat element in elements)
                {
                    Models.backbone_db.HabitatAreas item = new Models.backbone_db.HabitatAreas();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.CoverHA = Convert.ToDecimal(element.COVER_HA);
                    item.PriorityForm = element.PF;
                    item.Representativity = element.REPRESENTATIVITY;
                    item.DataQty = Convert.ToInt32(element.DATAQUALITY); // ???
                    //item.Conservation = element.CONSERVATION; // ??? PENDING
                    item.GlobalAssesments = element.GLOBALASSESMENT;
                    item.RelativeSurface = element.RELSURFACE;
                    item.Percentage = Convert.ToDecimal(element.PERCENTAGECOVER);
                    item.ConsStatus = element.CONSSTATUS;
                    item.Caves = Convert.ToString(element.CAVES); // ???
                    item.PF = Convert.ToString(element.PF); // ??? PENDING The same as PriorityForm
                    item.NonPresenciInSite = Convert.ToString(element.NONPRESENCEINSITE); // ???
                    items.Add(item);
                }
                return items;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {

            }

        }

        private List<Models.backbone_db.DescribeSites> harvestDescribeSites(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.DescribesSites> elements = null;
            List<Models.backbone_db.DescribeSites> items = new List<Models.backbone_db.DescribeSites>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.DescribesSites>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.DescribesSites element in elements)
                {
                    Models.backbone_db.DescribeSites item = new Models.backbone_db.DescribeSites();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.Percentage = element.PERCENTAGECOVER;
                    items.Add(item);
                }
                return items;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {

            }

        }
    }
}
