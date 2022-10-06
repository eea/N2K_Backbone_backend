using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Enumerations;



namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteDetailsService
    {
        Task<SiteGeometryDetailed> GetSiteGeometry(string siteCode, int version);

        Task<List<StatusChanges>> ListSiteComments(string siteCode, int version);

        Task<List<StatusChanges>> AddComment(StatusChanges comment, string username="");

        Task<int> DeleteComment(long CommentId);

        Task<List<StatusChanges>> UpdateComment(StatusChanges comment,string username="");


        Task<List<JustificationFiles>> ListSiteFiles(string siteCode, int version);

        Task<List<JustificationFiles>> UploadFile(AttachedFile attachedFile,string username="");


        Task<int> DeleteFile(long justificationId);
        Task<string> SaveEdition(ChangeEditionDb changeEdition,string username="");
        Task<ChangeEditionViewModel?> GetReferenceEditInfo(string siteCode);
    }


}

