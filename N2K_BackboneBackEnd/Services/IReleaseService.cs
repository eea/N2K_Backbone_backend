using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.release_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Services
{
    public interface IReleaseService
    {
        Task<List<BioRegionTypes>> GetUnionBioRegionTypes();
        Task<List<Releases>> GetReleaseHeadersByBioRegion(string? bioRegionShortCode);
        Task<List<ReleaseDetail>> GetCurrentSitesReleaseDetailByBioRegion(string? bioRegionShortCode);
        Task<List<Releases>> GetReleaseHeadersById(long? id);
        Task<UnionListComparerSummaryViewModel> GetCompareSummary(long? idSource, long? idTarget, string? bioRegions, IMemoryCache cache);
        Task<List<UnionListComparerDetailedViewModel>> CompareReleases(long? idSource, long? idTarget, string? bioRegions, string? country, IMemoryCache cache, int page = 1, int pageLimit = 0);
        Task<List<CountriesAttachmentCountViewModel>> GetCountriesAttachmentCount();
        Task<List<JustificationFilesRelease>> GetCountryDocuments(string country);
        Task<List<JustificationFilesRelease>> AddCountryDocument(AttachedFileRelease document);
        Task<List<JustificationFilesRelease>> DeleteCountryDocument(long documentId);
        Task<List<StatusChangesRelease>> GetCountryComments(string country);
        Task<FileContentResult> DownloadFile(int id, ReleaseProductType filetype);
        Task<List<StatusChangesRelease>> AddCountryComment(StatusChangesRelease comment);
        Task<List<StatusChangesRelease>> UpdateCountryComment(StatusChangesRelease comment);
        Task<List<StatusChangesRelease>> DeleteCountryComment(long commentId);
        Task<List<Releases>> CreateRelease(string title, Boolean? Final, string? character);
        Task<List<Releases>> UpdateRelease(long id, string name, Boolean final);
        Task<int> DeleteRelease(long id);
    }
}
