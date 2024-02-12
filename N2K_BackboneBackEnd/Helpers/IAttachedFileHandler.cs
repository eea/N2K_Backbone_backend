using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Helpers
{
    public interface IAttachedFileHandler
    {

        Task<List<string>> UploadFileAsync(AttachedFile file);
        Task<List<string>> UploadFileAsync(string file);

        Task<int> DeleteFileAsync(string fileName);
        Task<int> DeleteUnionListsFilesAsync();


    }
}
