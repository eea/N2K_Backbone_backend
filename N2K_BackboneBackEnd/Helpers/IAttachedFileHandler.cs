using N2K_BackboneBackEnd.Models;
using System.Drawing;

namespace N2K_BackboneBackEnd.Helpers
{
    public interface IAttachedFileHandler
    {

        Task<List<string>> UploadFileAsync(AttachedFile file);
        Task<List<string>> UploadFileAsync(string file);

        Task<int> DeleteFileAsync(string fileName);
        Task<int> DeleteUnionListsFilesAsync();

        Task<byte[]> ReadFile(string fileName);

    }
}
