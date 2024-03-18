using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using System.Drawing;

namespace N2K_BackboneBackEnd.Helpers
{
    public interface IAttachedFileHandler
    {

        Task<List<JustificationFiles>> UploadFileAsync(AttachedFile file);
        Task<List<string>> UploadFileAsync(string file);

        Task<int> DeleteFileAsync(string fileName);
        Task<int> DeleteUnionListsFilesAsync();

        Task<byte[]> ReadFile(string fileName);

    }
}
