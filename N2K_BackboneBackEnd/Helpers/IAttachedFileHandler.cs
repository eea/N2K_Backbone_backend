using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Helpers
{
    public interface IAttachedFileHandler
    {

        Task<string> UploadFileAsync(AttachedFile file);

        Task<int> DeleteFileAsync(string fileName);


    }
}
