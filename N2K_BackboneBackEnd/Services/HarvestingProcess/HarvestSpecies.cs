using N2K_BackboneBackEnd.Data;

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

        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start species validate...");
            await Task.Delay(2000);
            Console.WriteLine("==>ENd speciesvalidate...");
            return 1;
        }

        private List<Models.backbone_db.Species> harvestSpecies(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.ContainsSpecies> elements = null;
            List<Models.backbone_db.Species> items = new List<Models.backbone_db.Species>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.ContainsSpecies>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.ContainsSpecies element in elements)
                {
                    Models.backbone_db.Species item = new Models.backbone_db.Species();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.SpecieCode = element.SPECIESCODE;
                    item.PopulationMin = element.POPULATION; // ??? PENDING
                    item.PopulationMax = element.POPULATION; // ??? PENDING
                    item.Group = element.GROUP; // PENDING
                    item.SensitiveInfo = element.SENSITIVE; // ??? PENDING
                    item.Resident = element.RESIDENT;
                    item.Breeding = element.BREEDING;
                    item.Winter = element.WINTER;
                    item.Staging = element.STAGING;
                    item.Path = element.PATH; // ??? PENDING
                    item.AbundaceCategory = element.ABUNDANCECATEGORY;
                    item.Motivation = element.MOTIVATION;
                    item.PopulationType = element.POPULATION_TYPE;
                    item.CountingUnit = element.COUNTINGUNIT;
                    item.Population = element.POPULATION;
                    item.Insolation = element.ISOLATIONFACTOR; // ??? PENDING
                    item.Conservation = element.CONSERVATION;
                    item.Global = element.GLOBALIMPORTANCE; // ??? PENDING
                    item.NonPersistence = element.NONPRESENCEINSITE; // ??? PENDING
                    item.DataQuality = element.DATAQUALITY;
                    item.SpecieType = element.SPTYPE;
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

        private List<Models.backbone_db.SpeciesOther> harvestSpeciesOther(NaturaSite pVSite, int pVersion)
        {
            List<Models.versioning_db.ContainsSpecies> elements = null;
            List<Models.backbone_db.SpeciesOther> items = new List<Models.backbone_db.SpeciesOther>();
            try
            {
                elements = _versioningContext.Set<Models.versioning_db.ContainsSpecies>().Where(s => s.SITECODE == pVSite.SITECODE && s.VERSIONID == pVSite.VERSIONID).ToList();
                foreach (Models.versioning_db.ContainsSpecies element in elements)
                {
                    Models.backbone_db.SpeciesOther item = new Models.backbone_db.SpeciesOther();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.SpecieCode = element.SPECIESCODE;
                    item.PopulationMin = element.POPULATION; // ??? PENDING
                    item.PopulationMax = element.POPULATION; // ??? PENDING
                    item.Group = element.GROUP; // PENDING
                    item.SensitiveInfo = element.SENSITIVE; // ??? PENDING
                    item.Resident = element.RESIDENT;
                    item.Breeding = element.BREEDING;
                    item.Winter = element.WINTER;
                    item.Staging = element.STAGING;
                    item.Path = element.PATH; // ??? PENDING
                    item.AbundaceCategory = element.ABUNDANCECATEGORY;
                    item.Motivation = element.MOTIVATION;
                    item.PopulationType = element.POPULATION_TYPE;
                    item.CountingUnit = element.COUNTINGUNIT;
                    item.Population = element.POPULATION;
                    item.Insolation = element.ISOLATIONFACTOR; // ??? PENDING
                    item.Conservation = element.CONSERVATION;
                    item.Global = element.GLOBALIMPORTANCE; // ??? PENDING
                    item.NonPersistence = element.NONPRESENCEINSITE; // ??? PENDING
                    item.DataQuality = element.DATAQUALITY;
                    item.SpecieType = element.SPTYPE;
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
