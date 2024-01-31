using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using Microsoft.Extensions.Caching.Memory;

namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteDetailsService
    {
        Task<SiteGeometryDetailed> GetSiteGeometry(string siteCode, int version);
        Task<List<StatusChanges>> ListSiteComments(string siteCode, int version,  IMemoryCache cache, bool temporal = false);
        Task<List<StatusChanges>> AddComment(StatusChanges comment, IMemoryCache cache, bool temporal=false);
        Task<int> DeleteComment(long CommentId, IMemoryCache cache, bool temporal = false);
        Task<List<StatusChanges>> UpdateComment(StatusChanges comment, IMemoryCache cache, bool temporal = false);
        Task<List<JustificationFiles>> ListSiteFiles(string siteCode, int version, IMemoryCache cache, bool temporal = false);
        Task<List<JustificationFiles>> UploadFile(AttachedFile attachedFile, IMemoryCache cache, bool temporal = false);
        Task<int> DeleteFile(long justificationId, IMemoryCache cache, bool temporal = false);
        Task<string> SaveEdition(ChangeEditionDb changeEdition, IMemoryCache cache);
        Task<ChangeEditionViewModelOriginalExtended?> GetReferenceEditInfo(string siteCode);
    }
}