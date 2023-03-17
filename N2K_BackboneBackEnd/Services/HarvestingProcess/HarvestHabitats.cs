using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
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
                //TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Starting");
                Console.WriteLine("=>Start full habitat harvest by country...");

                await HarvestHabitatsByCountry(pCountryCode, pCountryVersion, pVersion);
                await HarvestDescribeSitesByCountry(pCountryCode, pCountryVersion, pVersion);

                Console.WriteLine("=>End full habitat harvest by country...");
                //TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("=>End full habitat harvest by country with error...");
                //TimeLog.setTimeStamp("Habitats for country " + pCountryCode + " - " + pCountryVersion.ToString(), "Exit");
                return 0;
            }
        }

        public async Task<int> HarvestBySite(string pSiteCode, decimal pSiteVersion, int pVersion,IList<DataQualityTypes> dataQualityTypes)
        {
            try
            {
                //TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Starting");
                //Console.WriteLine("=>Start full habitat harvest by site...");

                await HarvestHabitatsBySite(pSiteCode, pSiteVersion, pVersion, dataQualityTypes);
                await HarvestDescribeSitesBySite(pSiteCode, pSiteVersion, pVersion);

                //Console.WriteLine("=>End full habitat harvest by site...");
                //TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "End");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("=>End full habitat harvest by site with error...");
                //TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Exit");
                return 0;
            }
        }

        public async Task<int> HarvestHabitatsByCountry(string pCountryCode, int pCountryVersion, int pVersion)
        {
            List<ContainsHabitat> elements = null;
            try
            {
                Console.WriteLine("=>Start habitat harvest by country...");
                elements = await _versioningContext.Set<ContainsHabitat>().Where(s => s.COUNTRYCODE == pCountryCode && s.COUNTRYVERSIONID == pCountryVersion).ToListAsync();
                List<Models.backbone_db.Habitats> items = new List<Models.backbone_db.Habitats>();
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
                    item.NonPresenciInSite = Convert.ToInt32(element.NONPRESENCEINSITE); // ???
                    items.Add(item);

                }
                Habitats.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), items);
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

        public async Task<int> HarvestHabitatsBySite(string pSiteCode, decimal pSiteVersion, int pVersion, IList<DataQualityTypes> dataQualityTypes )
        {
            List<ContainsHabitat> elements = null;
            try
            {
                //TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "Processing");

                elements = await _versioningContext.Set<ContainsHabitat>().Where(s => s.SITECODE == pSiteCode && s.VERSIONID == pSiteVersion).ToListAsync();
                List<Models.backbone_db.Habitats> items = new List<Models.backbone_db.Habitats>();
                foreach (ContainsHabitat element in elements)
                {
                    Habitats item = new Habitats();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.CoverHA = (decimal?)element.COVER_HA;
                    item.PriorityForm = element.PF;
                    item.Representativity = element.REPRESENTATIVITY;                    
                    item.DataQty = (element.DATAQUALITY != null) ? dataQualityTypes.Where(d => d.HabitatCode == element.DATAQUALITY).Select(d => d.Id).FirstOrDefault() : null;
                    //item.Conservation = element.CONSERVATION; // ??? PENDING
                    item.GlobalAssesments = element.GLOBALASSESMENT;
                    item.RelativeSurface = element.RELSURFACE;
                    item.Percentage = (decimal?)element.PERCENTAGECOVER;
                    item.ConsStatus = element.CONSSTATUS;
                    item.Caves = Convert.ToString(element.CAVES); // ???
                    item.PF = Convert.ToString(element.PF); // ??? PENDING The same as PriorityForm
                    item.NonPresenciInSite = Convert.ToInt32(element.NONPRESENCEINSITE); // ???
                    items.Add(item);
                }
                Habitats.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), items);
                return 1;
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestHabitats - HarvestBySite", "");
                throw ex;
            }
            finally
            {
                //TimeLog.setTimeStamp("Habitats for site " + pSiteCode + " - " + pSiteVersion.ToString(), "End");
            }

        }

        public async Task<int> HarvestDescribeSitesByCountry(string pCountryCode, int pCountryVersion, int pVersion)
        {
            List<DescribesSites> elements = null;
            try
            {

                Console.WriteLine("=>Start describeSites harvest by country...");

                elements = await _versioningContext.Set<DescribesSites>().Where(s => s.COUNTRYCODE == pCountryCode && s.COUNTRYVERSIONID == pCountryVersion).ToListAsync();
                List<Models.backbone_db.DescribeSites> items = new List<Models.backbone_db.DescribeSites>();
                foreach (DescribesSites element in elements)
                {
                    DescribeSites item = new DescribeSites();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.Percentage = element.PERCENTAGECOVER;
                    items.Add(item);
                    //item.SaveRecord(this._dataContext.Database.GetConnectionString());
                }
                DescribeSites.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), items);

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
                //Console.WriteLine("=>Start describeSites harvest by site...");

                elements = await _versioningContext.Set<DescribesSites>().Where(s => s.SITECODE == pSiteCode && s.VERSIONID == pSiteVersion).ToListAsync();
                List<Models.backbone_db.DescribeSites> items = new List<Models.backbone_db.DescribeSites>();
                foreach (DescribesSites element in elements)
                {
                    DescribeSites item = new DescribeSites();
                    item.SiteCode = element.SITECODE;
                    item.Version = pVersion;
                    item.HabitatCode = element.HABITATCODE;
                    item.Percentage = element.PERCENTAGECOVER;
                    items.Add(item);
                }
                DescribeSites.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), items);
                //Console.WriteLine("=>End describeSites harvest by site...");
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

        public async Task<List<SiteChangeDb>> ValidateHabitat(List<HabitatToHarvest> habitatVersioning, List<HabitatToHarvest> referencedHabitats, List<SiteChangeDb> changes, EnvelopesToProcess envelope, SiteToHarvest harvestingSite, SiteToHarvest storedSite, SqlParameter param3, SqlParameter param4, SqlParameter param5, double habitatCoverHaTolerance, List<HabitatPriority> habitatPriority, ProcessedEnvelopes? processedEnvelope)
        {
            try
            {
                //For each habitat in Versioning compare it with that habitat in backboneDB
                foreach (HabitatToHarvest harvestingHabitat in habitatVersioning)
                {
                    HabitatToHarvest storedHabitat = referencedHabitats.Where(s => s.HabitatCode == harvestingHabitat.HabitatCode && s.PriorityForm == harvestingHabitat.PriorityForm).FirstOrDefault();
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
                                if (storedHabitat.Representativity.ToUpper() != "D" && storedHabitat.PriorityForm == true)
                                    isStoredPriority = true;
                                if (harvestingHabitat.Representativity.ToUpper() != "D" && harvestingHabitat.PriorityForm == true)
                                    isHarvestingPriority = true;
                            }
                            else
                            {
                                //If there is no exception, then two conditions are checked
                                if (storedHabitat.Representativity.ToUpper() != "D")
                                    isStoredPriority = true;
                                if (harvestingHabitat.Representativity.ToUpper() != "D")
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
                    HabitatToHarvest harvestingHabitat = habitatVersioning.Where(s => s.HabitatCode == storedHabitat.HabitatCode && s.PriorityForm == storedHabitat.PriorityForm).FirstOrDefault();
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
                SystemLog.write(SystemLog.errorLevel.Error, ex, "ValidateHabitats - Start - Site " + harvestingSite.SiteCode + "/" + harvestingSite.VersionId.ToString(), "");
            }
            SiteChangeDb.SaveBulkRecord(this._dataContext.Database.GetConnectionString(), changes);
            return changes;
        }

    }
}
