using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.release_db;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public class InfoService : IInfoService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2KReleasesContext _releaseContext;

        public InfoService(N2KBackboneContext dataContext, N2KReleasesContext releaseContext)
        {
            _dataContext = dataContext;
            _releaseContext = releaseContext;
        }

        public async Task<List<ReleasesCatalog>> GetOfficialReleases()
        {
            try
            {
                return await _releaseContext.Set<Releases>()
                    .Where(w => w.Final == true)
                    .AsNoTracking()
                    .Select(c => new ReleasesCatalog
                    {
                        ReleaseId = c.ID,
                        ReleaseName = c.Title,
                        ReleaseDate = c.CreateDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetOfficialReleases", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SitesParametered>> GetParameteredSites(long? releaseId, string? siteType, string? country, string? bioregion, string? siteCode, string? siteName, string? habitatCode, string? speciesCode)
        {
            try
            {
                List<SitesParameteredExtended> result = await _releaseContext.Set<SitesParameteredExtended>().FromSqlRaw($"exec dbo.GetParameteredSites").AsNoTracking().ToListAsync();

                if (releaseId != null)
                {
                    result = result.Where(w => w.ReleaseId == releaseId).ToList();
                }
                if (siteType != null)
                {
                    result = result.Where(w => w.SiteTypeCode == siteType).ToList();
                }
                if (country != null)
                {
                    result = result.Where(w => country.Contains(w.SiteCode.Substring(0, 2))).ToList();
                }
                if (bioregion != null)
                {
                    List<string> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().Where(w => bioregion.Contains(w.BioRegionShortCode)).Select(s => s.RefBioGeoName).ToListAsync();
                    result = result.Where(w => bioRegionTypes.Contains(w.BioRegion)).ToList();
                }
                if (siteCode != null)
                {
                    result = result.Where(w => w.SiteCode == siteCode).ToList();
                }
                if (siteName != null)
                {
                    result = result.Where(w => w.SiteName == siteName).ToList();
                }
                if (habitatCode != null)
                {
                    result = result.Where(w => w.HABITATCODE.Contains(habitatCode)).ToList();
                }
                if (speciesCode != null)
                {
                    result = result.Where(w => w.SPECIESCODE.Contains(speciesCode)).ToList();
                }

                List<SitesParametered> resultFinal = result.Select(c => new SitesParametered
                {
                    SiteCode = c.SiteCode,
                    SiteName = c.SiteName,
                    SiteTypeCode = c.SiteTypeCode,
                    SiteArea = c.SiteArea,
                    HabitatsNumber = c.HabitatsNumber,
                    SpeciesNumber = c.SpeciesNumber,
                    IsSentitive = c.IsSentitive == null ? false : c.IsSentitive
                }).ToList();

                return resultFinal.DistinctBy(d => d.SiteCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetParameteredSites", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<HabitatsParametered>> GetParameteredHabitats(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitatCode, string? habitatName)
        {
            try
            {
                List<HabitatsParameteredExtended> result = await _releaseContext.Set<HabitatsParameteredExtended>().FromSqlRaw($"exec dbo.GetParameteredHabitats").AsNoTracking().ToListAsync();

                if (releaseId != null)
                {
                    result = result.Where(w => w.ReleaseId == releaseId).ToList();
                }
                //UNUSED: FILTERED BY habitatGroup
                if (country != null)
                {
                    result = result.Where(w => country.Contains(w.Country)).ToList();
                }
                if (bioregion != null)
                {
                    List<string> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().Where(w => bioregion.Contains(w.BioRegionShortCode)).Select(s => s.RefBioGeoName).ToListAsync();
                    result = result.Where(w => bioRegionTypes.Contains(w.BioRegion)).ToList();
                }
                if (habitatCode != null)
                {
                    result = result.Where(w => w.HabitatCode == habitatCode).ToList();
                }
                if (habitatName != null)
                {
                    result = result.Where(w => w.HabitatName == habitatName).ToList();
                }

                List<HabitatsParametered> resultFinal = result.Select(c => new HabitatsParametered
                {
                    HabitatCode = c.HabitatCode,
                    HabitatName = c.HabitatName,
                    HabitatImageUrl = c.HabitatImageUrl,
                    SitesNumber = c.SitesNumber
                }).ToList();

                return resultFinal.DistinctBy(d => d.HabitatCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetParameteredHabitats", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<List<SpeciesParametered>> GetParameteredSpecies(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? speciesCode, string? speciesName)
        {
            try
            {
                List<SpeciesParameteredExtended> result = await _releaseContext.Set<SpeciesParameteredExtended>().FromSqlRaw($"exec dbo.GetParameteredSpecies").AsNoTracking().ToListAsync();

                if (releaseId != null)
                {
                    result = result.Where(w => w.ReleaseId == releaseId).ToList();
                }
                if (speciesGroup != null)
                {
                    result = result.Where(w => w.SpeciesGroupCode == speciesGroup).ToList();
                }
                if (country != null)
                {
                    result = result.Where(w => country.Contains(w.Country)).ToList();
                }
                if (bioregion != null)
                {
                    List<string> bioRegionTypes = await _dataContext.Set<BioRegionTypes>().Where(w => bioregion.Contains(w.BioRegionShortCode)).Select(s => s.RefBioGeoName).ToListAsync();
                    result = result.Where(w => bioRegionTypes.Contains(w.BioRegion)).ToList();
                }
                if (speciesCode != null)
                {
                    result = result.Where(w => w.SpeciesCode == speciesCode).ToList();
                }
                if (speciesName != null)
                {
                    result = result.Where(w => w.SpeciesName == speciesName).ToList();
                }

                List<SpeciesParametered> resultFinal = result.Select(c => new SpeciesParametered
                {
                    SpeciesCode = c.SpeciesCode,
                    SpeciesName = c.SpeciesName,
                    SpeciesScientificName = c.SpeciesScientificName,
                    SpeciesGroupCode = c.SpeciesGroupCode,
                    SpeciesEunisId = c.SpeciesEunisId,
                    SpeciesImageUrl = c.SpeciesImageUrl,
                    SitesNumber = c.SitesNumber
                }).ToList();

                return resultFinal.DistinctBy(d => d.SpeciesCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetParameteredSpecies", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

        public async Task<ReleaseCounters> GetLatestReleaseCounters()
        {
            try
            {
                Releases release = await _releaseContext.Set<Releases>().Where(w => w.Final == true).OrderByDescending(o => o.CreateDate).AsNoTracking().FirstOrDefaultAsync();

                //List<NATURA2000SITES> sites = await _releaseContext.Set<NATURA2000SITES>().Where(w => w.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<string> sites = await _releaseContext.Set<NATURA2000SITES>().Where(w => w.ReleaseId == release.ID).Select(s => s.SITECODE.ToUpper()).AsNoTracking().ToListAsync();

                //List<HABITATS> habitats = await _releaseContext.Set<HABITATS>().Where(w => w.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<string> habitats = await _releaseContext.Set<HABITATS>().Where(w => w.ReleaseId == release.ID).Select(s => s.HABITATCODE.ToUpper()).AsNoTracking().ToListAsync();

                //List<SPECIES> species = await _releaseContext.Set<SPECIES>().Where(w => w.ReleaseId == release.ID).AsNoTracking().ToListAsync();
                List<string> species = await _releaseContext.Set<SPECIES>().Where(w => w.ReleaseId == release.ID).Select(s => s.SPECIESCODE.ToUpper()).AsNoTracking().ToListAsync();

                ReleaseCounters result = new()
                {
                    SitesNumber = sites.Distinct().Count(),
                    HabitatsNumber = habitats.Distinct().Count(),
                    SpeciesNumber = species.Distinct().Count(),
                    ReleaseDate = release.CreateDate
                };

                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "InfoService - GetLatestReleaseCounters", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }
    }
}