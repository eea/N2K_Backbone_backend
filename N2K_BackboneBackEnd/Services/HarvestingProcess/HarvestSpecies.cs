using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;

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
                TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Starting");

                elements =await  _versioningContext.Set<ContainsSpecies>().Where(s=> s.COUNTRYCODE == pCountryCode && s.COUNTRYVERSIONID == pCountryVersion).ToListAsync();

                foreach (ContainsSpecies element in elements) {
                    
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
                        _dataContext.Set<SpeciesOther>().Add(item.getSpeciesOther());
                    }
                    else
                    {
                        _dataContext.Set<Species>().Add(item.getSpecies());
                    }


                }

                TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                TimeLog.setTimeStamp("Species for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Exit");
                return 0;
            }

        }

        public async Task<int> HarvestBySite(string pSiteCode, decimal pSiteVersion, int pVersion)
        {
            List<ContainsSpecies> elements = null;
            try
            {
                TimeLog.setTimeStamp("Species for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Processing");
                elements =await  _versioningContext.Set<ContainsSpecies>(). Where(s => s.SITECODE == pSiteCode && s.VERSIONID == pSiteVersion).ToListAsync();
                foreach (ContainsSpecies element in elements)
                {

                    //Check id the specie code is null or not present in the catalog
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
                        _dataContext.Set<SpeciesOther>().Add(item.getSpeciesOther());
                    }
                    else
                    {
                        _dataContext.Set<Species>().Add(item.getSpecies());
                    }
                }
                
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestSpecies - HarvestBySite", "");
              
                return 0;
            }
            finally {
                TimeLog.setTimeStamp("Species for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Exit");
            }

        }

        public async Task<int> ValidateChanges(string countryCode, int versionId, int referenceVersionID)
        {
            Console.WriteLine("==>Start species validate...");
            await Task.Delay(2000);
            Console.WriteLine("==>ENd speciesvalidate...");
            return 1;
        }

        

    }
}
