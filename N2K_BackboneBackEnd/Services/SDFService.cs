using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Services
{
    public class SDFService : ISDFService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public SDFService(N2KBackboneContext dataContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _appSettings = app;
        }

        public async Task<SDF> GetData(string SiteCode, int Version = -1)
        {
            try
            {
                string booleanTrue = "yes";
                string booleanFalse = "no";
                string booleanChecked = "x";
                string booleanUnchecked = "";

                Sites site;
                if (Version == -1)
                {
                    // try to get reference, if it doesnt exist return current
                    site = await _dataContext.Set<Sites>().Where(a => a.SiteCode == SiteCode && a.CurrentStatus == Enumerations.SiteChangeStatus.Accepted)
                        .OrderBy(a => a.Version).Reverse().AsNoTracking().FirstOrDefaultAsync();
                    if (site == null)
                    {
                    site = await _dataContext.Set<Sites>().Where(a => a.SiteCode == SiteCode && a.Current == true).AsNoTracking().FirstOrDefaultAsync();
                    }
                }
                else
                {
                    site = await _dataContext.Set<Sites>().Where(a => a.SiteCode == SiteCode && a.Version == Version).AsNoTracking().FirstOrDefaultAsync();
                }

                List<Countries> countries = await _dataContext.Set<Countries>().AsNoTracking().ToListAsync();
                List<Habitats> habitats = await _dataContext.Set<Habitats>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<HabitatTypes> habitatTypes = await _dataContext.Set<HabitatTypes>().AsNoTracking().ToListAsync();
                List<Species> species = await _dataContext.Set<Species>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<SpeciesTypes> speciesTypes = await _dataContext.Set<SpeciesTypes>().AsNoTracking().ToListAsync();
                List<SpeciesOther> speciesOther = await _dataContext.Set<SpeciesOther>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<DataQualityTypes> dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                List<Respondents> respondents = await _dataContext.Set<Respondents>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<NutsBySite> nutsBySite = await _dataContext.Set<NutsBySite>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<Nuts> nuts = await _dataContext.Set<Nuts>().AsNoTracking().ToListAsync();
                List<BioRegions> bioRegions = await _dataContext.Set<BioRegions>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<BioRegionTypes> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().AsNoTracking().ToListAsync();
                List<SiteLargeDescriptions> siteLargeDescriptions = await _dataContext.Set<SiteLargeDescriptions>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<DescribeSites> describeSites = await _dataContext.Set<DescribeSites>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<IsImpactedBy> isImpactedBy = await _dataContext.Set<IsImpactedBy>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<SiteOwnerType> siteOwnerType = await _dataContext.Set<SiteOwnerType>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<OwnerShipTypes> ownerShipTypes = await _dataContext.Set<OwnerShipTypes>().AsNoTracking().ToListAsync();
                List<DocumentationLinks> documentationLinks = await _dataContext.Set<DocumentationLinks>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<HasNationalProtection> hasNationalProtection = await _dataContext.Set<HasNationalProtection>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<DetailedProtectionStatus> detailedProtectionStatus = await _dataContext.Set<DetailedProtectionStatus>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                ReferenceMap referenceMap = await _dataContext.Set<ReferenceMap>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().FirstOrDefaultAsync();

                SDF result = new();
                #region SiteInfo
                if (site != null)
                {
                    result.SiteInfo.SiteName = site.Name;
                    result.SiteInfo.Country = countries.Where(c => c.Code == site.CountryCode.ToLower()).FirstOrDefault().Country;
                    result.SiteInfo.Directive = site.SiteType; //UNSURE
                    result.SiteInfo.SiteCode = SiteCode;
                    result.SiteInfo.Area = site.Area;
                    result.SiteInfo.Est = site.CompilationDate; //UNSURE
                    result.SiteInfo.MarineArea = site.MarineArea;
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
                    result.SiteIdentification.SiteName = site.Name;
                    result.SiteIdentification.FirstCompletionDate = site.CompilationDate;
                    result.SiteIdentification.UpdateDate = site.DateUpdate;
                    SiteDesignation siteDesignation = new()
                    {
                        ClassifiedSPA = site.DateSpa,
                        ReferenceSPA = site.SpaLegalReference,
                        ProposedSCI = site.DatePropSCI,
                        ConfirmedSCI = site.DateConfSCI,
                        DesignatedSAC = site.DateSac,
                        ReferenceSAC = site.SacLegalReference,
                        Explanations = site.Explanations
                    }; //UNSURE HOW COULD THERE BE MORE THAN ONE
                    result.SiteIdentification.SiteDesignation.Add(siteDesignation);
                }
                if (respondents != null && respondents.Count > 0) //UNSURE
                {
                    result.SiteIdentification.Respondent.Name = respondents.FirstOrDefault().ContactName;
                    result.SiteIdentification.Respondent.Address = respondents.FirstOrDefault().addressArea;
                    result.SiteIdentification.Respondent.Email = respondents.FirstOrDefault().Email;
                }
                #endregion

                #region SiteLocation
                if (site != null)
                {
                    result.SiteLocation.Longitude = site.Longitude;
                    result.SiteLocation.Latitude = site.Latitude;
                    result.SiteLocation.Area = site.Area;
                    result.SiteLocation.MarineArea = site.MarineArea;
                    result.SiteLocation.SiteLength = site.Length;
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
                            Name = bioRegionTypes.Where(t => t.Code == br.BGRID).FirstOrDefault().RefBioGeoName,
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
                            Cover = h.CoverHA,
                            Cave = h.Caves,
                            DataQuality = h.DataQty != null ? dataQualityTypes.Where(c => c.Id == h.DataQty).FirstOrDefault().Name : null,
                            Representativity = h.Representativity,
                            RelativeSurface = h.RelativeSurface,
                            Conservation = h.ConsStatus,
                            Global = h.GlobalAssesments
                        };
                        if (h.PriorityForm != null)
                            temp.PF = (h.PriorityForm == true) ? booleanChecked : booleanUnchecked;
                        if (h.NonPresenciInSite != null)
                            temp.NP = (h.NonPresenciInSite == 1) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.HabitatTypes.Add(temp);
                    });
                }
                if (species != null && species.Count > 0)
                {
                    species.ForEach(h =>
                    {
                        SpeciesSDF temp = new()
                        {
                            SpeciesName = h.SpecieCode != null ? speciesTypes.Where(t => t.Code == h.SpecieCode).FirstOrDefault().Name : null,
                            Code = h.SpecieCode,
                            Group = h.SpecieType,
                            Type = h.PopulationType,
                            Min = h.PopulationMin,
                            Max = h.PopulationMax,
                            Unit = h.CountingUnit,
                            Category = h.AbundaceCategory,
                            DataQuality = h.DataQuality,
                            Population = h.Population,
                            Conservation = h.Conservation,
                            Isolation = h.Insolation,
                            Global = h.Global
                        };
                        if (h.SensitiveInfo != null)
                            temp.Sensitive = (h.SensitiveInfo == true) ? booleanTrue : booleanFalse;
                        if (h.NonPersistence != null)
                            temp.NP = (h.NonPersistence == true) ? booleanChecked : booleanUnchecked;
                        result.EcologicalInformation.Species.Add(temp);
                    });
                }
                if (speciesOther != null && speciesOther.Count > 0)
                {
                    speciesOther.ForEach(h =>
                    {
                        SpeciesSDF temp = new()
                        {
                            SpeciesName = h.SpecieCode,
                            Code = h.OtherSpecieCode ?? "-",
                            Group = h.SpecieType,
                            Min = h.PopulationMin,
                            Max = h.PopulationMax,
                            Unit = h.CountingUnit,
                            Category = h.AbundaceCategory
                        };
                        if (h.SensitiveInfo != null)
                            temp.Sensitive = (h.SensitiveInfo == true) ? booleanTrue : booleanFalse;
                        if (h.NonPersistence != null)
                            temp.NP = (h.NonPersistence == true) ? booleanChecked : booleanUnchecked;
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
                            Impacts = h.ActivityCode,
                            Pollution = h.PollutionCode,
                            Origin = h.Ocurrence
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
                            Type = h.Type.ToLower(),
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
                            Organisation = h.ContactName ?? h.OrgName,
                            Address = h.addressArea,
                            Email = h.Email
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SDFService - GetData", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}