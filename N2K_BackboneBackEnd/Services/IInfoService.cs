using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public interface IInfoService
    {
        Task<List<ReleasesCatalog>> GetOfficialReleases();
        Task<List<SitesParametered>> GetParameteredSites(long? releaseId, string? siteType, string? country, string? bioregion, string? siteCode, string? siteName, string? habitatCode, string? speciesCode);
        Task<List<HabitatsParametered>> GetParameteredHabitats(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitatCode, string? habitatName);
        Task<List<SpeciesParametered>> GetParameteredSpecies(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? speciesCode, string? speciesName);
        Task<ReleaseCounters> GetLatestReleaseCounters();
    }
}