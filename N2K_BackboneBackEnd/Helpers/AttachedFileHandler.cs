using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Helpers
{
    public class AttachedFileHandler
    {
        protected readonly AttachedFilesConfig _attachedFilesConfig;
        public AttachedFileHandler(AttachedFilesConfig attachedFilesConfig)
        {
            _attachedFilesConfig = attachedFilesConfig;
        }
        public bool CheckExtensions(string fileName)
        {
            string WhiteList = _attachedFilesConfig.ExtensionWhiteList;
            string[] extensionWhiteList = WhiteList.Split(";");
            string[] fileArray = fileName.Split(".");
            string fileExtension = fileArray[fileArray.Length-1];
            return extensionWhiteList.Any(x => x.ToLower() == fileExtension.ToLower());
        }
    }
}
