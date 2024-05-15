using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.releases_db;

namespace N2K_BackboneBackEnd.Services
{
    public class ReleaseSDFService : IReleaseSDFService
    {
        private readonly N2KReleasesContext _releaseContext;
        private readonly N2KBackboneContext _dataContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public ReleaseSDFService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
            _appSettings = app;
        }

        public async Task<List<Models.releases_db.Releases>> GetReleases()
        {
            return await _releaseContext.Set<Models.releases_db.Releases>().ToListAsync();
        }

        public async Task<ReleaseSDF> GetData(string SiteCode, int ReleaseId = -1)
        {
            try
            {
                string booleanTrue = "yes";
                string booleanFalse = "no";
                string booleanChecked = "x";
                string booleanUnchecked = "";

                Models.releases_db.Releases release;

                if (ReleaseId == -1)
                {
                    release = await _releaseContext.Set<Models.releases_db.Releases>().OrderBy(r => r.CreateDate).LastAsync();
                }
                else
                {
                    release = await _releaseContext.Set<Models.releases_db.Releases>().Where(r => r.ID == ReleaseId).FirstOrDefaultAsync();
                }

                if (release == null)
                {
                    // TODO error
                }

                Natura2000Sites site = await _releaseContext.Set<Natura2000Sites>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).FirstOrDefaultAsync();

                List<Models.releases_db.Habitats> habitats = await _releaseContext.Set<Models.releases_db.Habitats>().Where(h => h.SITECODE == SiteCode && h.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<HabitatClass> habitatClasses = await _releaseContext.Set<HabitatClass>().AsNoTracking().ToListAsync();
                List<HabitatTypes> habitatTypes = await _dataContext.Set<HabitatTypes>().AsNoTracking().ToListAsync();

                List<Models.releases_db.Species> species = await _releaseContext.Set<Models.releases_db.Species>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<SpeciesTypes> speciesTypes = await _dataContext.Set<SpeciesTypes>().AsNoTracking().ToListAsync();
                List<OtherSpecies> speciesOther = await _releaseContext.Set<OtherSpecies>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();

                List<DataQualityTypes> dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();

                List<Management> respondents = await _releaseContext.Set<Management>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();

                List<NutsBySite> nutsBySite = await _dataContext.Set<NutsBySite>().Where(a => a.SiteCode == SiteCode && a.Version == site.VERSION).AsNoTracking().ToListAsync();
                List<Nuts> nuts = await _dataContext.Set<Nuts>().AsNoTracking().ToListAsync();

                List<BioRegion> bioRegions = await _releaseContext.Set<BioRegion>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<BioRegionTypes> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().AsNoTracking().ToListAsync();

                List<SiteLargeDescriptions> siteLargeDescriptions = await _dataContext.Set<SiteLargeDescriptions>().Where(a => a.SiteCode == SiteCode && a.Version == site.VERSION).AsNoTracking().ToListAsync();
                List<DescribeSites> describeSites = await _dataContext.Set<DescribeSites>().Where(a => a.SiteCode == SiteCode && a.Version == site.VERSION).AsNoTracking().ToListAsync();

                List<Impact> isImpactedBy = await _releaseContext.Set<Impact>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();

                List<SiteOwnerType> siteOwnerType = await _dataContext.Set<SiteOwnerType>().Where(a => a.SiteCode == SiteCode && a.Version == site.VERSION).AsNoTracking().ToListAsync();
                List<OwnerShipTypes> ownerShipTypes = await _dataContext.Set<OwnerShipTypes>().AsNoTracking().ToListAsync();
                List<DocumentationLinks> documentationLinks = await _dataContext.Set<DocumentationLinks>().Where(a => a.SiteCode == SiteCode && a.Version == site.VERSION).AsNoTracking().ToListAsync();
                List<HasNationalProtection> hasNationalProtection = await _dataContext.Set<HasNationalProtection>().Where(a => a.SiteCode == SiteCode && a.Version == site.VERSION).AsNoTracking().ToListAsync();
                List<DetailedProtectionStatus> detailedProtectionStatus = await _dataContext.Set<DetailedProtectionStatus>().Where(a => a.SiteCode == SiteCode && a.Version == site.VERSION).AsNoTracking().ToListAsync();
                ReferenceMap referenceMap = await _dataContext.Set<ReferenceMap>().Where(a => a.SiteCode == SiteCode && a.Version == site.VERSION).AsNoTracking().FirstOrDefaultAsync();

                ReleaseSDF result = new();
                #region SiteInfo
                if (site != null)
                {
                    result.SiteInfo.SiteName = site.SITENAME;
                    result.SiteInfo.Country = site.COUNTRY_CODE;
                    result.SiteInfo.Directive = site.SITETYPE; //UNSURE
                    result.SiteInfo.SiteCode = SiteCode;
                    result.SiteInfo.Area = site.AREAHA;
                    result.SiteInfo.Est = site.DATE_COMPILATION; //UNSURE
                    result.SiteInfo.MarineArea = site.MARINE_AREA_PERCENTAGE;
                }
                if (habitats != null && habitats.Count > 0)
                    result.SiteInfo.Habitats = habitats.Count;
                if (species != null && species.Count > 0)
                    result.SiteInfo.Species = species.Count;
                #endregion

                #region SiteIdentification
                if (site != null)
                {
                    result.SiteIdentification.Type = site.SITETYPE;
                    result.SiteIdentification.SiteCode = SiteCode;
                    result.SiteIdentification.SiteName = site.SITENAME;
                    result.SiteIdentification.FirstCompletionDate = site.DATE_COMPILATION;
                    result.SiteIdentification.UpdateDate = site.DATE_UPDATE;
                    SiteDesignation siteDesignation = new()
                    {
                        ClassifiedSPA = site.DATE_SPA,
                        ReferenceSPA = site.SPA_LEGAL_REFERENCE,
                        ProposedSCI = site.DATE_PROP_SCI,
                        ConfirmedSCI = site.DATE_CONF_SCI,
                        DesignatedSAC = site.DATE_SAC,
                        ReferenceSAC = site.SAC_LEGAL_REFERENCE,
                        Explanations = site.EXPLANATIONS
                    }; //UNSURE HOW COULD THERE BE MORE THAN ONE
                    result.SiteIdentification.SiteDesignation.Add(siteDesignation);
                }
                if (respondents != null && respondents.Count > 0) //UNSURE
                {
                    result.SiteIdentification.Respondent.Name = respondents.FirstOrDefault().ORG_NAME;
                    result.SiteIdentification.Respondent.Address = respondents.FirstOrDefault().ORG_ADDRESS;
                    result.SiteIdentification.Respondent.Email = respondents.FirstOrDefault().ORG_EMAIL;
                }
                #endregion

                #region SiteLocation
                if (site != null)
                {
                    result.SiteLocation.Longitude = site.LONGITUDE;
                    result.SiteLocation.Latitude = site.LATITUDE;
                    result.SiteLocation.Area = site.AREAHA;
                    result.SiteLocation.MarineArea = site.MARINE_AREA_PERCENTAGE;
                    result.SiteLocation.SiteLength = site.LENGTHKM;
                }
                if (nutsBySite != null && nutsBySite.Count > 0)
                {
                    nutsBySite.ForEach(nbs =>
                    {
                        Models.ViewModel.Region temp = new()
                        {
                            NUTSLevel2Code = nbs.NutId,
                            RegionName = nuts.Where(t => t.Code == nbs.NutId).FirstOrDefault().Region
                        };
                        result.SiteLocation.Region.Add(temp);
                    });
                }
                if (bioRegions != null && bioRegions.Count > 0)
                {
                    bioRegions.ForEach(br =>
                    {
                        BiogeographicalRegions temp = new()
                        {
                            Name = br.BIOGEOGRAPHICREG,
                            Value = br.PERCENTAGE
                        };
                        result.SiteLocation.BiogeographicalRegions.Add(temp);
                    });
                }
                #endregion

                #region EcologicalInformation
                if (habitats != null && habitats.Count > 0)
                {
                    habitats.ForEach(h =>
                    {
                        HabitatSDF temp = new()
                        {
                            HabitatName = h.HabitatCode != null ? habitatTypes.Where(t => t.Code == h.HabitatCode).FirstOrDefault().Name : null,
                            Code = h.HabitatCode,
                            Cover = h.COVER_HA,
                            Cave = h.CAVES,
                            // TODO data quality tables don't match!!
                            //DataQuality = h.DataQuality != null ? dataQualityTypes.Where(c => c.Id == h.DataQuality).FirstOrDefault().Name : null,
                            Representativity = h.REPRESENTATIVITY,
                            RelativeSurface = h.RELSURFACE,
                            Conservation = h.CONSERVATION,
                            Global = h.GLOBAL
                        };
                        if (h.PRIORITY_FORM_HABITAT_TYPE != null)
                            temp.PF = (h.PRIORITY_FORM_HABITAT_TYPE == true) ? booleanChecked : booleanUnchecked;
                        if (h.NON_PRESENCE_IN_SITE != null)
                            temp.NP = (h.NON_PRESENCE_IN_SITE == 1) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.HabitatTypes.Add(temp);
                    });
                }
                if (species != null && species.Count > 0)
                {
                    species.ForEach(h =>
                    {
                        SpeciesSDF temp = new()
                        {
                            SpeciesName = h.SPECIESCODE != null ? speciesTypes.Where(t => t.Code == h.SPECIESCODE).FirstOrDefault().Name : null,
                            Code = h.SPECIESCODE,
                            Group = h.SPGROUP,
                            Type = h.POPULATION_TYPE,
                            Min = h.LOWERBOUND,
                            Max = h.UPPERBOUND,
                            Unit = h.COUNTING_UNIT,
                            Category = h.ABUNDANCE_CATEGORY,
                            DataQuality = h.DATAQUALITY,
                            Population = h.POPULATION,
                            Conservation = h.CONSERVATION,
                            Isolation = h.ISOLATION,
                            Global = h.GLOBAL
                        };
                        if (h.SENSITIVE != null)
                            temp.Sensitive = (h.SENSITIVE == true) ? booleanTrue : booleanFalse;
                        if (h.NONPRESENCEINSITE != null)
                            temp.NP = (h.NONPRESENCEINSITE == true) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.Species.Add(temp);
                    });
                }
                if (speciesOther != null && speciesOther.Count > 0)
                {
                    speciesOther.ForEach(h =>
                    {
                        SpeciesSDF temp = new()
                        {
                            SpeciesName = h.SPECIESNAME,
                            Code = h.SPECIESCODE ?? "-",
                            Group = h.SPECIESGROUP,
                            Min = h.LOWERBOUND,
                            Max = h.UPPERBOUND,
                            Unit = h.COUNTING_UNIT,
                            Category = h.ABUNDANCE_CATEGORY
                        };
                        if (h.SENSITIVE != null)
                            temp.Sensitive = (h.SENSITIVE == true) ? booleanTrue : booleanFalse;
                        if (h.NONPRESENCEINSITE != null)
                            temp.NP = (h.NONPRESENCEINSITE == true) ? booleanChecked : booleanUnchecked;
                        if (h.MOTIVATION != null)
                        {
                            temp.AnnexIV = h.MOTIVATION.Contains("IV") ? booleanChecked : booleanUnchecked;
                            string annex = h.MOTIVATION.Replace("IV", "");
                            temp.AnnexV = annex.Contains("V") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesA = h.MOTIVATION.Contains("A") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesB = h.MOTIVATION.Contains("B") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesC = h.MOTIVATION.Contains("C") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesD = h.MOTIVATION.Contains("D") ? booleanChecked : booleanUnchecked;
                        }
                        result.EcologicalInformation.OtherSpecies.Add(temp);
                    });
                }
                #endregion

                #region SiteDescription
                if (describeSites != null && describeSites.Count > 0)
                {
                    describeSites.ForEach(h =>
                    {
                        CodeCover temp = new()
                        {
                            Code = h.HabitatCode,
                            Cover = h.Percentage
                        };
                        result.SiteDescription.GeneralCharacter.Add(temp);
                    });
                }
                if (isImpactedBy != null && isImpactedBy.Count > 0)
                {
                    isImpactedBy.ForEach(h =>
                    {
                        Threats temp = new()
                        {
                            Rank = h.INTENSITY,
                            Impacts = h.IMPACTCODE,
                            Pollution = h.POLLUTIONCODE,
                            Origin = h.OCCURRENCE
                        };
                        if (h.IMPACT_TYPE == "N")
                        {
                            result.SiteDescription.NegativeThreats.Add(temp);
                        }
                        else if (h.IMPACT_TYPE == "P")
                        {
                            result.SiteDescription.PositiveThreats.Add(temp);
                        }
                    });
                }
                if (siteOwnerType != null && siteOwnerType.Count > 0)
                {
                    siteOwnerType.ForEach(h =>
                    {
                        N2K_BackboneBackEnd.Models.ViewModel.Ownership temp = new()
                        {
                            Type = h.Type,
                            Percent = h.Percent
                        };
                        result.SiteDescription.Ownership.Add(temp);
                    });
                }
                if (siteLargeDescriptions != null && siteLargeDescriptions.Count > 0)
                {
                    result.SiteDescription.Quality = siteLargeDescriptions.FirstOrDefault().Quality;
                    result.SiteDescription.Documents = siteLargeDescriptions.FirstOrDefault().Documentation;
                }
                if (documentationLinks != null && documentationLinks.Count > 0)
                {
                    documentationLinks.ForEach(h =>
                    {
                        result.SiteDescription.Links.Add(h.Link);
                    });
                }
                #endregion

                #region SiteProtectionStatus
                if (hasNationalProtection != null && hasNationalProtection.Count > 0)
                {
                    hasNationalProtection.ForEach(h =>
                    {
                        CodeCover temp = new()
                        {
                            Code = h.DesignatedCode,
                            Cover = h.Percentage
                        };
                        result.SiteProtectionStatus.DesignationTypes.Add(temp);
                    });
                }
                if (detailedProtectionStatus != null && detailedProtectionStatus.Count > 0)
                {
                    detailedProtectionStatus.ForEach(h =>
                    {
                        RelationSites temp = new()
                        {
                            DesignationLevel = (h.DesignationCode != null && h.DesignationCode != "") ? "National or regional" : "International",
                            TypeCode = h.DesignationCode,
                            SiteName = h.Name,
                            Type = h.OverlapCode,
                            Percent = h.OverlapPercentage
                        };
                        result.SiteProtectionStatus.RelationSites.Add(temp);
                    });
                }
                if (siteLargeDescriptions != null && siteLargeDescriptions.Count > 0)
                {
                    result.SiteProtectionStatus.SiteDesignation = siteLargeDescriptions.FirstOrDefault().Designation;
                }
                #endregion

                #region SiteManagement
                if (respondents != null && respondents.Count > 0)
                {
                    respondents.ForEach(h =>
                    {
                        BodyResponsible temp = new()
                        {
                            Organisation = h.ORG_NAME,
                            Address = h.ORG_ADDRESS,
                            Email = h.ORG_EMAIL
                        };
                        result.SiteManagement.BodyResponsible.Add(temp);
                    });
                }
                if (siteLargeDescriptions != null && siteLargeDescriptions.Count > 0)
                {
                    siteLargeDescriptions.ForEach(h =>
                    {
                        ManagementPlan temp = new()
                        {
                            Name = h.ManagPlan,
                            Link = h.ManagPlanUrl
                        };
                        result.SiteManagement.ManagementPlan.Add(temp);
                    });
                    result.SiteManagement.ConservationMeasures = siteLargeDescriptions.FirstOrDefault().ManagConservMeasures;
                }
                #endregion

                #region MapOfTheSite
                if (referenceMap != null)
                {
                    result.MapOfTheSite.INSPIRE = referenceMap.Inspire;
                    result.MapOfTheSite.MapDelivered = (referenceMap.PDFProvided != null && referenceMap.PDFProvided == 1) ? booleanTrue : booleanFalse;
                }
                #endregion

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "ReleaseSDFService - GetData", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}
