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
                    release = await _releaseContext.Set<Models.releases_db.Releases>().Where(r => r.id == ReleaseId).FirstOrDefaultAsync();
                }

                if (release == null)
                {
                    // TODO error
                }

                Natura2000Sites site = await _releaseContext.Set<Natura2000Sites>().Where(a => a.SiteCode == SiteCode && a.ReleaseId == release.id).FirstOrDefaultAsync();

                List<Models.releases_db.Habitats> habitats = await _releaseContext.Set<Models.releases_db.Habitats>().Where(h => h.SiteCode == SiteCode && h.ReleaseId == release.id).AsNoTracking().ToListAsync();
                List<HabitatClass> habitatClasses = await _releaseContext.Set<HabitatClass>().AsNoTracking().ToListAsync();
                List<HabitatTypes> habitatTypes = await _dataContext.Set<HabitatTypes>().AsNoTracking().ToListAsync();

                List<Models.releases_db.Species> species = await _releaseContext.Set<Models.releases_db.Species>().Where(a => a.SiteCode == SiteCode && a.ReleaseId == release.id).AsNoTracking().ToListAsync();
                List<SpeciesTypes> speciesTypes = await _dataContext.Set<SpeciesTypes>().AsNoTracking().ToListAsync();
                List<OtherSpecies> speciesOther = await _releaseContext.Set<OtherSpecies>().Where(a => a.SiteCode == SiteCode && a.ReleaseId == release.id).AsNoTracking().ToListAsync();

                List<DataQualityTypes> dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();

                List<Management> respondents = await _releaseContext.Set<Management>().Where(a => a.SiteCode == SiteCode && a.ReleaseId == release.id).AsNoTracking().ToListAsync();

                List<NutsBySite> nutsBySite = await _dataContext.Set<NutsBySite>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<Nuts> nuts = await _dataContext.Set<Nuts>().AsNoTracking().ToListAsync();

                List<BioRegion> bioRegions = await _releaseContext.Set<BioRegion>().Where(a => a.SiteCode == SiteCode && a.ReleaseId == release.id).AsNoTracking().ToListAsync();
                List<BioRegionTypes> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().AsNoTracking().ToListAsync();
                
                List<SiteLargeDescriptions> siteLargeDescriptions = await _dataContext.Set<SiteLargeDescriptions>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<DescribeSites> describeSites = await _dataContext.Set<DescribeSites>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();

                List<Impact> isImpactedBy = await _releaseContext.Set<Impact>().Where(a => a.SiteCode == SiteCode && a.ReleaseId == release.id).AsNoTracking().ToListAsync();

                List<SiteOwnerType> siteOwnerType = await _dataContext.Set<SiteOwnerType>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<OwnerShipTypes> ownerShipTypes = await _dataContext.Set<OwnerShipTypes>().AsNoTracking().ToListAsync();
                List<DocumentationLinks> documentationLinks = await _dataContext.Set<DocumentationLinks>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<HasNationalProtection> hasNationalProtection = await _dataContext.Set<HasNationalProtection>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<DetailedProtectionStatus> detailedProtectionStatus = await _dataContext.Set<DetailedProtectionStatus>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                ReferenceMap referenceMap = await _dataContext.Set<ReferenceMap>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().FirstOrDefaultAsync();

                ReleaseSDF result = new();
                #region SiteInfo
                if (site != null)
                {
                    result.SiteInfo.SiteName = site.SiteName;
                    result.SiteInfo.Country = site.CountryCode;
                    result.SiteInfo.Directive = site.SiteType; //UNSURE
                    result.SiteInfo.SiteCode = SiteCode;
                    result.SiteInfo.Area = site.Areaha;
                    result.SiteInfo.Est = site.DateCompilation; //UNSURE
                    result.SiteInfo.MarineArea = site.MarineAreaPercentage;
                }
                if (habitats != null && habitats.Count > 0)
                    result.SiteInfo.Habitats = habitats.Count;
                if (species != null && species.Count > 0)
                    result.SiteInfo.Species = species.Count;
                #endregion

                #region SiteIdentification
                if (site != null)
                {
                    result.SiteIdentification.Type = site.SiteType;
                    result.SiteIdentification.SiteCode = SiteCode;
                    result.SiteIdentification.SiteName = site.SiteName;
                    result.SiteIdentification.FirstCompletionDate = site.DateCompilation;
                    result.SiteIdentification.UpdateDate = site.DateUpdate;
                    SiteDesignation siteDesignation = new()
                    {
                        ClassifiedSPA = site.DateSpa,
                        ReferenceSPA = site.SpaLegalReference,
                        ProposedSCI = site.DatePropSci,
                        ConfirmedSCI = site.DateConfSci,
                        DesignatedSAC = site.DateSac,
                        ReferenceSAC = site.SacLegalReference,
                        Explanations = site.Explanations
                    }; //UNSURE HOW COULD THERE BE MORE THAN ONE
                    result.SiteIdentification.SiteDesignation.Add(siteDesignation);
                }
                if (respondents != null && respondents.Count > 0) //UNSURE
                {
                    result.SiteIdentification.Respondent.Name = respondents.FirstOrDefault().OrgName;
                    result.SiteIdentification.Respondent.Address = respondents.FirstOrDefault().OrgAddress;
                    result.SiteIdentification.Respondent.Email = respondents.FirstOrDefault().OrgEmail;
                }
                #endregion

                #region SiteLocation
                if (site != null)
                {
                    result.SiteLocation.Longitude = site.Longitude;
                    result.SiteLocation.Latitude = site.Latitude;
                    result.SiteLocation.Area = site.Areaha;
                    result.SiteLocation.MarineArea = site.MarineAreaPercentage;
                    result.SiteLocation.SiteLength = site.Lengthkm;
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
                            Name = br.BioGeoGraphicReg,
                            Value = br.Percentage
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
                            Cover = h.CoverHa,
                            Cave = h.Caves,
                            // TODO data quality tables don't match!!
                            //DataQuality = h.DataQuality != null ? dataQualityTypes.Where(c => c.Id == h.DataQuality).FirstOrDefault().Name : null,
                            Representativity = h.Representativity,
                            RelativeSurface = h.RelSurface,
                            Conservation = h.Conservation,
                            Global = h.Global
                        };
                        if (h.PriorityFormHabitatType != null)
                            temp.PF = (h.PriorityFormHabitatType == true) ? booleanChecked : booleanUnchecked;
                        if (h.NonPresenceInSite != null)
                            temp.NP = (h.NonPresenceInSite == 1) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.HabitatTypes.Add(temp);
                    });
                }
                if (species != null && species.Count > 0)
                {
                    species.ForEach(h =>
                    {
                        SpeciesSDF temp = new()
                        {
                            SpeciesName = h.SpeciesCode != null ? speciesTypes.Where(t => t.Code == h.SpeciesCode).FirstOrDefault().Name : null,
                            Code = h.SpeciesCode,
                            Group = h.SPgroup,
                            Type = h.PopulationType,
                            Min = h.Lowerbound,
                            Max = h.Upperbound,
                            Unit = h.CountingUnit,
                            Category = h.AbundanceCategory,
                            DataQuality = h.DataQuality,
                            Population = h.Population,
                            Conservation = h.Conservation,
                            Isolation = h.Isolation,
                            Global = h.Global
                        };
                        if (h.Sensitive != null)
                            temp.Sensitive = (h.Sensitive == true) ? booleanTrue : booleanFalse;
                        if (h.NonPresenceInSite != null)
                            temp.NP = (h.NonPresenceInSite == true) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.Species.Add(temp);
                    });
                }
                if (speciesOther != null && speciesOther.Count > 0)
                {
                    speciesOther.ForEach(h =>
                    {
                        SpeciesSDF temp = new()
                        {
                            SpeciesName = h.SpeciesName,
                            Code = h.SpeciesCode ?? "-",
                            Group = h.SpeciesGroup,
                            Min = h.Lowerbound,
                            Max = h.Upperbound,
                            Unit = h.CountingUnit,
                            Category = h.AbundanceCategory
                        };
                        if (h.Sensitive != null)
                            temp.Sensitive = (h.Sensitive == true) ? booleanTrue : booleanFalse;
                        if (h.NonPresenceInSite != null)
                            temp.NP = (h.NonPresenceInSite == true) ? booleanChecked : booleanUnchecked;
                        if (h.Motivation != null)
                        {
                            temp.AnnexIV = h.Motivation.Contains("IV") ? booleanChecked : booleanUnchecked;
                            string annex = h.Motivation.Replace("IV", "");
                            temp.AnnexV = annex.Contains("V") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesA = h.Motivation.Contains("A") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesB = h.Motivation.Contains("B") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesC = h.Motivation.Contains("C") ? booleanChecked : booleanUnchecked;
                            temp.OtherCategoriesD = h.Motivation.Contains("D") ? booleanChecked : booleanUnchecked;
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
                            Rank = h.Intensity,
                            Impacts = h.ImpactCode,
                            Pollution = h.PollutionCode,
                            Origin = h.Occurrence
                        };
                        if (h.ImpactType == "N")
                        {
                            result.SiteDescription.NegativeThreats.Add(temp);
                        }
                        else if (h.ImpactType == "P")
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
                            Organisation = h.OrgName,
                            Address = h.OrgAddress,
                            Email = h.OrgEmail
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
