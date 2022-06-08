using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.versioning_db;
using N2K_BackboneBackEnd.Enumerations;



namespace N2K_BackboneBackEnd.Services
{
    public interface ISiteDetailsService
    {
        Task<List<StatusChanges>> ListSiteComments(string siteCode, int version);

        Task<List<StatusChanges>> AddComment(StatusChanges comment);

        Task<int> DeleteComment(int CommentId);

        Task<List<StatusChanges>> UpdateComment(StatusChanges comment);


        Task<List<JustificationFiles>> ListSiteFiles(string siteCode, int version);
    }


}

