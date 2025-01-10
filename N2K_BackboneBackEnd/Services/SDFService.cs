using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.release_db;
using Microsoft.Data.SqlClient;

namespace N2K_BackboneBackEnd.Services
{
    public class SDFService : ISDFService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2KReleasesContext _releaseContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public SDFService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
            _appSettings = app;
        }

        public async Task<SDF> GetExtraData(string SiteCode, int submission)
        {
            try
            {
                List<SiteBasicBulk> queryResults = new();
                SiteBasicBulk mySiteView = new();
                string queryString = String.Empty;

                if (submission == 0)
                {
                    queryString = String.Format(@" 
                        SELECT DISTINCT TOP(1) [SiteCode],
                            [Sites].[Version],
                            [N2KVersioningVersion]
                        FROM [dbo].[Sites]
                        INNER JOIN [dbo].[ProcessedEnvelopes] PE ON [Sites].[CountryCode] = PE.[Country]
                            AND [Sites].[N2KVersioningVersion] = PE.[Version]
                            AND PE.[Status] != 3
                        WHERE [SiteCode] = '{0}' AND [Sites].[CurrentStatus] = 1
                        ORDER BY [SiteCode], [N2KVersioningVersion] DESC, [Sites].[Version] DESC", SiteCode);
                }
                else if (submission == 1)
                {
                    queryString = String.Format(@" 
                        SELECT DISTINCT [SiteCode],
	                        MAX([Sites].[Version]) AS 'Version',
	                        [N2KVersioningVersion]
                        FROM [dbo].[Sites]
                        INNER JOIN [dbo].[ProcessedEnvelopes] PE ON [Sites].[CountryCode] = PE.[Country]
	                        AND [Sites].[N2KVersioningVersion] = PE.[Version]
	                        AND PE.[Status] = 3
                        WHERE [SiteCode] = '{0}'
                        GROUP BY [SiteCode],
	                        [N2KVersioningVersion]
                        ORDER BY [SiteCode]", SiteCode);
                }

                SqlConnection backboneConn = null;
                SqlCommand command = null;
                SqlDataReader reader = null;
                try
                {
                    backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
                    backboneConn.Open();
                    command = new SqlCommand(queryString, backboneConn);
                    reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        mySiteView.SiteCode = reader["SiteCode"].ToString();
                        mySiteView.Version = int.Parse(reader["Version"].ToString());
                        mySiteView.N2KVersioningVersion = int.Parse(reader["N2KVersioningVersion"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SDFService - GetExtraData - GetSiteData", "", _dataContext.Database.GetConnectionString());
                }
                finally
                {
                    if (reader != null) await reader.DisposeAsync();
                    if (command != null) command.Dispose();
                    if (backboneConn != null) backboneConn.Dispose();
                }

                if (mySiteView.SiteCode != "")
                {
                    return await GetData(SiteCode, mySiteView.Version);
                }
                else
                {
                    return await GetData(SiteCode, -2);
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SDFService - GetExtraData", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<SDF> GetData(string SiteCode, int Version = -1)
        {
            try
            {
                string booleanTrue = "Yes";
                string booleanFalse = "No";
                string booleanChecked = "x";
                string booleanUnchecked = "";
                SDF result = new();

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

                if (site == null)
                    return result;

                //Catalogues
                List<BioRegionTypes> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().AsNoTracking().ToListAsync();
                List<Countries> countries = await _dataContext.Set<Countries>().AsNoTracking().ToListAsync();
                List<DataQualityTypes> dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                List<HabitatTypes> habitatTypes = await _dataContext.Set<HabitatTypes>().AsNoTracking().ToListAsync();
                List<Nuts> nuts = await _dataContext.Set<Nuts>().AsNoTracking().ToListAsync();
                List<OwnerShipTypes> ownerShipTypes = await _dataContext.Set<OwnerShipTypes>().AsNoTracking().ToListAsync();
                List<SpeciesTypes> speciesTypes = await _dataContext.Set<SpeciesTypes>().AsNoTracking().ToListAsync();

                //Data
                List<Habitats> habitats = await _dataContext.Set<Habitats>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<Species> species = await _dataContext.Set<Species>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<SpeciesOther> speciesOther = await _dataContext.Set<SpeciesOther>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<Respondents> respondents = await _dataContext.Set<Respondents>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<NutsBySite> nutsBySite = await _dataContext.Set<NutsBySite>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<BioRegions> bioRegions = await _dataContext.Set<BioRegions>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<SiteLargeDescriptions> siteLargeDescriptions = await _dataContext.Set<SiteLargeDescriptions>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<DescribeSites> describeSites = await _dataContext.Set<DescribeSites>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<IsImpactedBy> isImpactedBy = await _dataContext.Set<IsImpactedBy>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<SiteOwnerType> siteOwnerType = await _dataContext.Set<SiteOwnerType>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<DocumentationLinks> documentationLinks = await _dataContext.Set<DocumentationLinks>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<HasNationalProtection> hasNationalProtection = await _dataContext.Set<HasNationalProtection>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                List<DetailedProtectionStatus> detailedProtectionStatus = await _dataContext.Set<DetailedProtectionStatus>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().ToListAsync();
                ReferenceMap referenceMap = await _dataContext.Set<ReferenceMap>().Where(a => a.SiteCode == SiteCode && a.Version == site.Version).AsNoTracking().FirstOrDefaultAsync();

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
                if (respondents != null && respondents.Count > 0)
                {
                    Respondents contact = respondents.Where(r => r.ContactName != null).FirstOrDefault();
                    if (contact != null)
                    {
                        result.SiteIdentification.Respondent.Name = contact.ContactName;
                        result.SiteIdentification.Respondent.Address = contact.addressArea;
                        result.SiteIdentification.Respondent.Email = contact.Email;
                    }
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
                            DataQuality = h.DataQty != null ? dataQualityTypes.Where(c => c.Id == h.DataQty).FirstOrDefault().HabitatCode : null,
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
                            Min = h.PopulationMin.ToString(),
                            Max = h.PopulationMax.ToString(),
                            Unit = h.CountingUnit,
                            Category = h.AbundaceCategory,
                            DataQuality = h.DataQuality,
                            Population = h.Population,
                            Conservation = h.Conservation,
                            Isolation = h.Insolation,
                            Global = h.Global
                        };
                        if (h.SensitiveInfo != null)
                            temp.Sensitive = (h.SensitiveInfo == true) ? booleanTrue : booleanUnchecked;
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
                            Min = h.PopulationMin.ToString(),
                            Max = h.PopulationMax.ToString(),
                            Unit = h.CountingUnit,
                            Category = h.AbundaceCategory
                        };
                        if (h.SensitiveInfo != null)
                            temp.Sensitive = (h.SensitiveInfo == true) ? booleanTrue : booleanUnchecked;
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
                    result.SiteDescription.OtherCharacteristics = siteLargeDescriptions.FirstOrDefault().OtherCharact;
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
                            TypeCode = (h.DesignationCode != null && h.DesignationCode != "") ? h.DesignationCode : h.Convention,
                            SiteName = h.Name,
                            Type = h.OverlapCode,
                            Percent = h.OverlapPercentage
                        };
                        result.SiteProtectionStatus.RelationSites.Add(temp);
                        result.SiteProtectionStatus.RelationSites = result.SiteProtectionStatus.RelationSites.OrderByDescending(o => o.DesignationLevel).ToList();
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
                        if (h.OrgName != null)
                        {
                            BodyResponsible temp = new()
                            {
                                Organisation = h.OrgName,
                                Address = h.addressArea,
                                Email = h.Email
                            };
                            result.SiteManagement.BodyResponsible.Add(temp);
                        }
                    });
                }
                if (siteLargeDescriptions != null && siteLargeDescriptions.Count > 0)
                {
                    siteLargeDescriptions.ForEach(h =>
                    {
                        ManagementPlan temp = new()
                        {
                            Name = h.ManagPlan,
                            Link = h.ManagPlanUrl,
                            Exists = h.ManagStatus
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

        public async Task<ReleaseSDF> GetReleaseData(string SiteCode, int ReleaseId = -1, bool showSensitive = true)
        {
            try
            {
                string booleanTrue = "Yes";
                string booleanFalse = "No";
                string booleanChecked = "x";
                string booleanUnchecked = "";
                ReleaseSDF result = new();

                List<Releases> releases = await _releaseContext.Set<Releases>().AsNoTracking().ToListAsync();
                Releases release;

                if (ReleaseId == -1)
                {
                    release = releases.OrderBy(r => r.CreateDate).Last();
                }
                else
                {
                    release = releases.Where(r => r.ID == ReleaseId).FirstOrDefault();
                }

                if (release == null)
                    return result;

                List<NATURA2000SITES> sites = await _releaseContext.Set<NATURA2000SITES>().Where(a => a.SITECODE == SiteCode).ToListAsync();
                NATURA2000SITES site = sites.Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).FirstOrDefault();

                if (site == null)
                    return result;

                //Catalogues
                List<Countries> countries = await _dataContext.Set<Countries>().AsNoTracking().ToListAsync();
                List<DataQualityTypes> dataQualityTypes = await _dataContext.Set<DataQualityTypes>().AsNoTracking().ToListAsync();
                List<HabitatTypes> habitatTypes = await _dataContext.Set<HabitatTypes>().AsNoTracking().ToListAsync();
                List<Nuts> nuts = await _dataContext.Set<Nuts>().AsNoTracking().ToListAsync();
                List<OwnerShipTypes> ownerShipTypes = await _dataContext.Set<OwnerShipTypes>().AsNoTracking().ToListAsync();
                List<SpeciesTypes> speciesTypes = await _dataContext.Set<SpeciesTypes>().AsNoTracking().ToListAsync();

                //Data
                List<HABITATS> habitats = await _releaseContext.Set<HABITATS>().Where(h => h.SITECODE == SiteCode && h.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<HABITATCLASS> habitatClass = await _releaseContext.Set<HABITATCLASS>().Where(h => h.SITECODE == SiteCode && h.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<SPECIES> species = await _releaseContext.Set<SPECIES>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID && (!(a.SENSITIVE ?? false) || showSensitive)).AsNoTracking().ToListAsync();
                List<OTHERSPECIES> speciesOther = await _releaseContext.Set<OTHERSPECIES>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID && (!(a.SENSITIVE ?? false) || showSensitive)).AsNoTracking().ToListAsync();
                List<CONTACTS> contacts = await _releaseContext.Set<CONTACTS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<MANAGEMENT> management = await _releaseContext.Set<MANAGEMENT>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<NUTSBYSITE> nutsBySite = await _releaseContext.Set<NUTSBYSITE>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<BIOREGION> bioRegions = await _releaseContext.Set<BIOREGION>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<IMPACT> isImpactedBy = await _releaseContext.Set<IMPACT>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<SITEOWNERTYPE> siteOwnerType = await _releaseContext.Set<SITEOWNERTYPE>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<DOCUMENTATIONLINKS> documentationLinks = await _releaseContext.Set<DOCUMENTATIONLINKS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<DESIGNATIONSTATUS> designationStatus = await _releaseContext.Set<DESIGNATIONSTATUS>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                REFERENCEMAP referenceMap = await _releaseContext.Set<REFERENCEMAP>().Where(a => a.SITECODE == SiteCode && a.ReleaseId == release.ID).AsNoTracking().FirstOrDefaultAsync();

                #region SiteInfo
                if (site != null)
                {
                    result.SiteInfo.SiteName = site.SITENAME;
                    result.SiteInfo.Country = countries.Where(c => c.Code == site.COUNTRY_CODE.ToLower()).FirstOrDefault().Country;
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
                sites.ForEach(st =>
                {
                    ReleaseInfo temp = new()
                    {
                        ReleaseId = st.ReleaseId,
                        ReleaseName = releases.Where(w => w.ID == st.ReleaseId).Select(s => s.Title).FirstOrDefault(),
                        ReleaseDate = releases.Where(w => w.ID == st.ReleaseId).Select(s => s.CreateDate).FirstOrDefault()
                    };
                    result.SiteInfo.Releases.Add(temp);
                });
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
                if (contacts != null && contacts.Count > 0)
                {
                    CONTACTS contact = contacts.Where(r => r.NAME != null).FirstOrDefault();
                    if (contact != null)
                    {
                        result.SiteIdentification.Respondent.Name = contact.NAME;
                        result.SiteIdentification.Respondent.Address = contact.ADDRESS;
                        result.SiteIdentification.Respondent.Email = contact.EMAIL;
                    }
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
                            NUTSLevel2Code = nbs.NUTID,
                            RegionName = nuts.Where(t => t.Code == nbs.NUTID).FirstOrDefault().Region
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
                            HabitatName = h.HABITATCODE != null ? habitatTypes.Where(t => t.Code == h.HABITATCODE).FirstOrDefault().Name : null,
                            Code = h.HABITATCODE,
                            Cover = h.COVER_HA,
                            Cave = h.CAVES,
                            DataQuality = h.DATAQUALITY != null ? dataQualityTypes.Where(c => c.HabitatCode == h.DATAQUALITY).FirstOrDefault().HabitatCode : null,
                            Representativity = h.REPRESENTATIVITY,
                            RelativeSurface = h.RELSURFACE,
                            Conservation = h.CONSERVATION,
                            Global = h.GLOBAL_ASSESSMENT
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
                            temp.Sensitive = (h.SENSITIVE == true) ? booleanTrue : booleanUnchecked;
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
                            Min = h.LOWERBOUND.ToString(),
                            Max = h.UPPERBOUND.ToString(),
                            Unit = h.COUNTING_UNIT,
                            Category = h.ABUNDANCE_CATEGORY
                        };
                        if (h.SENSITIVE != null)
                            temp.Sensitive = (h.SENSITIVE == true) ? booleanTrue : booleanUnchecked;
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
                if (habitatClass != null && habitatClass.Count > 0)
                {
                    habitatClass.ForEach(h =>
                    {
                        CodeCover temp = new()
                        {
                            Code = h.HABITATCODE,
                            Cover = h.PERCENTAGECOVER
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
                            Rank = (h.INTENSITY != null && h.INTENSITY.Length > 0) ? h.INTENSITY?.Substring(0, 1).ToUpper() : null,
                            Impacts = h.IMPACTCODE,
                            Pollution = h.POLLUTIONCODE,
                            Origin = (h.OCCURRENCE != null && h.OCCURRENCE.Length > 0) ? h.OCCURRENCE?.Substring(0, 1).ToLower() : null
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
                            Type = h.TYPE == null ? null : h.TYPE.ToLower(),
                            Percent = h.PERCENT
                        };
                        result.SiteDescription.Ownership.Add(temp);
                    });
                }
                result.SiteDescription.Quality = site.QUALITY;
                result.SiteDescription.Documents = site.DOCUMENTATION;
                result.SiteDescription.OtherCharacteristics = site.OTHERCHARACT;
                if (documentationLinks != null && documentationLinks.Count > 0)
                {
                    documentationLinks.ForEach(h =>
                    {
                        result.SiteDescription.Links.Add(h.LINK);
                    });
                }
                #endregion

                #region SiteProtectionStatus
                if (designationStatus != null && designationStatus.Count > 0)
                {
                    designationStatus.ForEach(h =>
                    {
                        if (h.DESIGNATEDSITENAME == null && h.OVERLAPCODE == null)
                        {
                            CodeCover temp = new()
                            {
                                Code = h.DESIGNATIONCODE,
                                Cover = h.OVERLAPPERC
                            };
                            result.SiteProtectionStatus.DesignationTypes.Add(temp);
                        }
                        else
                        {
                            RelationSites temp = new()
                            {
                                DesignationLevel = (h.DESIGNATIONCODE != null && h.DESIGNATIONCODE != "") ? "National or regional" : "International",
                                TypeCode = h.DESIGNATIONCODE,
                                SiteName = h.DESIGNATEDSITENAME,
                                Type = h.OVERLAPCODE,
                                Percent = h.OVERLAPPERC
                            };
                            result.SiteProtectionStatus.RelationSites.Add(temp);
                            result.SiteProtectionStatus.RelationSites = result.SiteProtectionStatus.RelationSites.OrderByDescending(o => o.DesignationLevel).ToList();
                        }
                    });
                }
                result.SiteProtectionStatus.SiteDesignation = site.DESIGNATION;
                #endregion

                #region SiteManagement
                if (management != null && management.Count > 0)
                {
                    management.ForEach(h =>
                    {
                        if (h.ORG_NAME != null)
                        {
                            BodyResponsible temp = new()
                            {
                                Organisation = h.ORG_NAME,
                                Address = h.ORG_ADDRESS,
                                Email = h.ORG_EMAIL
                            };
                            result.SiteManagement.BodyResponsible.Add(temp);
                        }
                    });
                }
                if (management != null && management.Count > 0)
                {
                    management.ForEach(h =>
                    {
                        ManagementPlan temp = new()
                        {
                            Name = h.MANAG_PLAN,
                            Link = h.MANAG_PLAN_URL,
                            Exists = h.MANAG_STATUS
                        };
                        result.SiteManagement.ManagementPlan.Add(temp);
                    });
                    result.SiteManagement.ConservationMeasures = management.FirstOrDefault().MANAG_CONSERV_MEASURES;
                }
                #endregion

                #region MapOfTheSite
                if (referenceMap != null)
                {
                    result.MapOfTheSite.INSPIRE = referenceMap.INSPIRE;
                    result.MapOfTheSite.MapDelivered = (referenceMap.PDFPROVIDED != null && referenceMap.PDFPROVIDED == 1) ? booleanTrue : booleanFalse;
                }
                #endregion

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SDFService - GetReleaseData", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}
