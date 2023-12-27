using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.BackboneDB;
using System.Drawing;
using DocumentFormat.OpenXml.Vml;
using System.Collections.Generic;

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

        public async Task<SDF> GetData(string SiteCode)
        {
            try
            {
                string booleanTrue = "yes";
                string booleanFalse = "no";
                string booleanChecked = "x";
                string booleanUnchecked = "";

                Sites site = await _dataContext.Set<Sites>().Where(a => a.SiteCode == SiteCode && a.Current == true).AsNoTracking().FirstOrDefaultAsync();
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

                SDF result = new SDF();
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
                    SiteDesignation siteDesignation = new SiteDesignation(); //UNSURE HOW COULD THERE BE MORE THAN ONE
                    siteDesignation.ClassifiedSPA = site.DateSpa;
                    siteDesignation.ReferenceSPA = site.SpaLegalReference;
                    siteDesignation.ProposedSCI = site.DatePropSCI;
                    siteDesignation.ConfirmedSCI = site.DateConfSCI;
                    siteDesignation.DesignatedSAC = site.DateSac;
                    siteDesignation.ReferenceSAC = site.SacLegalReference;
                    siteDesignation.Explanations = site.Explanations;
                    result.SiteIdentification.SiteDesignation.Add(siteDesignation);
                }
                if (respondents != null && respondents.Count > 0) //UNSURE
                {
                    result.SiteIdentification.Respondent.Name = respondents.FirstOrDefault().name;
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
                        Models.ViewModel.Region temp = new Models.ViewModel.Region();
                        temp.NUTSLevel2Code = nbs.NutId;
                        temp.RegionName = nuts.Where(t => t.Code == nbs.NutId).FirstOrDefault().Region;
                        result.SiteLocation.Region.Add(temp);
                    });
                }
                if (bioRegions != null && bioRegions.Count > 0)
                {
                    bioRegions.ForEach(br =>
                    {
                        BiogeographicalRegions temp = new BiogeographicalRegions();
                        temp.Name = bioRegionTypes.Where(t => t.Code == br.BGRID).FirstOrDefault().RefBioGeoName;
                        temp.Value = br.Percentage;
                        result.SiteLocation.BiogeographicalRegions.Add(temp);
                    });
                }
                #endregion

                #region EcologicalInformation
                if (habitats != null && habitats.Count > 0)
                {
                    habitats.ForEach(h =>
                    {
                        HabitatSDF temp = new HabitatSDF();
                        temp.HabitatName = habitatTypes.Where(t => t.Code == h.HabitatCode).FirstOrDefault().Name;
                        temp.Code = h.HabitatCode;
                        if (h.PriorityForm != null)
                            temp.PF = (h.PriorityForm == true) ? booleanChecked : booleanUnchecked;
                        if (h.NonPresenciInSite != null)
                            temp.NP = (h.NonPresenciInSite == 1) ? booleanChecked : booleanUnchecked;
                        temp.Cover = h.CoverHA;
                        temp.Cave = h.Caves;
                        temp.DataQuality = dataQualityTypes.Where(c => c.Id == h.DataQty).FirstOrDefault().Name;
                        temp.Representativity = h.Representativity;
                        temp.RelativeSurface = h.RelativeSurface;
                        temp.Conservation = h.Conservation;
                        temp.Global = h.GlobalAssesments;
                        result.EcologicalInformation.HabitatTypes.Add(temp);
                    });
                }
                if (species != null && species.Count > 0)
                {
                    species.ForEach(h =>
                    {
                        SpeciesSDF temp = new SpeciesSDF();
                        temp.SpeciesName = speciesTypes.Where(t => t.Code == h.SpecieCode).FirstOrDefault().Name;
                        temp.Code = h.SpecieCode;
                        temp.Group = h.Group;
                        if (h.SensitiveInfo != null)
                            temp.Sensitive = (h.SensitiveInfo == true) ? booleanTrue : booleanFalse;
                        if (h.NonPersistence != null)
                            temp.NP = (h.NonPersistence == true) ? booleanChecked : booleanUnchecked;
                        temp.Type = h.SpecieType;
                        temp.Min = h.PopulationMin;
                        temp.Max = h.PopulationMax;
                        temp.Unit = h.CountingUnit;
                        temp.Category = h.AbundaceCategory;
                        temp.DataQuality = h.DataQuality;
                        temp.Population = h.Population;
                        temp.Conservation = h.Conservation;
                        temp.Isolation = h.Insolation;
                        temp.Global = h.Global;
                        result.EcologicalInformation.Species.Add(temp);
                    });
                }
                if (speciesOther != null && speciesOther.Count > 0)
                {
                    speciesOther.ForEach(h =>
                    {
                        SpeciesSDF temp = new SpeciesSDF();
                        temp.SpeciesName = h.SpecieCode;
                        temp.Code = "-";
                        temp.Group = h.Group;
                        if (h.SensitiveInfo != null)
                            temp.Sensitive = (h.SensitiveInfo == true) ? booleanTrue : booleanFalse;
                        if (h.NonPersistence != null)
                            temp.NP = (h.NonPersistence == true) ? booleanChecked : booleanUnchecked;
                        temp.Type = h.SpecieType;
                        temp.Min = h.PopulationMin;
                        temp.Max = h.PopulationMax;
                        temp.Unit = h.CountingUnit;
                        temp.Category = h.AbundaceCategory;
                        temp.DataQuality = h.DataQuality;
                        temp.Population = h.Population;
                        temp.Conservation = h.Conservation;
                        temp.Isolation = h.Insolation;
                        temp.Global = h.Global;
                        result.EcologicalInformation.OtherSpecies.Add(temp);
                    });
                }
                #endregion

                #region SiteDescription
                if (describeSites != null && describeSites.Count > 0)
                {
                    describeSites.ForEach(h =>
                    {
                        CodeCover temp = new CodeCover();
                        temp.Code = h.HabitatCode;
                        temp.Cover = h.Percentage;
                        result.SiteDescription.GeneralCharacter.Add(temp);
                    });
                }
                if (isImpactedBy != null && isImpactedBy.Count > 0)
                {
                    isImpactedBy.ForEach(h =>
                    {
                        Threats temp = new Threats();
                        temp.Rank = h.Intensity;
                        temp.Impacts = h.ActivityCode;
                        temp.Pollution = h.PollutionCode;
                        temp.Origin = h.InOut;
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
                        Ownership temp = new Ownership();
                        temp.Type = h.Type;
                        temp.Percent = h.Percent;
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
                        CodeCover temp = new CodeCover();
                        temp.Code = h.DesignatedCode;
                        temp.Cover = h.Percentage;
                        result.SiteProtectionStatus.DesignationTypes.Add(temp);
                    });
                }
                if (detailedProtectionStatus != null && detailedProtectionStatus.Count > 0)
                {
                    detailedProtectionStatus.ForEach(h =>
                    {
                        RelationSites temp = new RelationSites();
                        temp.DesignationLevel = (h.DesignationCode != null && h.DesignationCode != "") ? "National or regional" : "International";
                        temp.TypeCode = h.DesignationCode;
                        temp.SiteName = h.Name;
                        temp.Type = h.OverlapCode;
                        temp.Percent = h.OverlapPercentage;
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
                        BodyResponsible temp = new BodyResponsible();
                        temp.Organisation = h.name;
                        temp.Address = h.addressArea;
                        temp.Email = h.Email;
                        result.SiteManagement.BodyResponsible.Add(temp);
                    });
                }
                if (siteLargeDescriptions != null && respondents.Count > 0)
                {
                    siteLargeDescriptions.ForEach(h =>
                    {
                        ManagementPlan temp = new ManagementPlan();
                        temp.Name = h.ManagPlan;
                        temp.Link = h.ManagPlanUrl;
                        result.SiteManagement.ManagementPlan.Add(temp);
                    });
                    result.SiteManagement.ConservationMeasures = siteLargeDescriptions.FirstOrDefault().ManagConservMeasures;
                }
                #endregion

                #region MapOfTheSite
                result.MapOfTheSite.INSPIRE = site.Inspire_ID;
                result.MapOfTheSite.MapDelivered = (site.PDFProvided != null && site.PDFProvided == 1) ? booleanTrue : booleanFalse;
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
