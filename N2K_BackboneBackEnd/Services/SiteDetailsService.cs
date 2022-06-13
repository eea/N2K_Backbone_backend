using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.Net.Http.Headers;
using N2K_BackboneBackEnd.Helpers;

namespace N2K_BackboneBackEnd.Services
{
    public class SiteDetailsService: ISiteDetailsService
    {

        private readonly N2KBackboneContext _dataContext;

        public SiteDetailsService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
        }


        #region SiteComments

        public async Task<List<StatusChanges>> ListSiteComments(string pSiteCode, int pCountryVersion)
        {
            List<StatusChanges> result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == pSiteCode && ch.Version == pCountryVersion).ToListAsync();
            return result;
        }


        public async Task<List<StatusChanges>> AddComment(StatusChanges comment)
        {

            comment.Date = DateTime.Now;
            await _dataContext.Set<StatusChanges>().AddAsync(comment);
            await _dataContext.SaveChangesAsync();

            List<StatusChanges> result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == comment.SiteCode && ch.Version == comment.Version).ToListAsync();
            return result;

        }

        public async Task<int> DeleteComment(long CommentId)
        {
            int result = 0;
            StatusChanges? comment = await _dataContext.Set<StatusChanges>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == CommentId);
            if (comment != null)
            {
                _dataContext.Set<StatusChanges>().Remove(comment);
                await _dataContext.SaveChangesAsync();
                result = 1;
            }
            return result;
        }

        public async Task<List<StatusChanges>> UpdateComment(StatusChanges comment)
        {
            _dataContext.Set<StatusChanges>().Update(comment);
            await _dataContext.SaveChangesAsync();

            List<StatusChanges> result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == comment.SiteCode && ch.Version == comment.Version).ToListAsync();
            return result;

        }



        #endregion

        #region SiteFiles

        public async Task<List<JustificationFiles>> ListSiteFiles(string pSiteCode, int pCountryVersion)
        {
            List<JustificationFiles> result = await _dataContext.Set<JustificationFiles>().AsNoTracking().Where(f => f.SiteCode == pSiteCode && f.Version == pCountryVersion).ToListAsync();
            return result;
        }

        public async Task<List<JustificationFiles>> UploadFile(AttachedFile attachedFile)
        {
            var folderName = Path.Combine("Resources", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fileName = ContentDispositionHeaderValue.Parse(attachedFile.File.ContentDisposition).FileName.Trim('"');
            var fullPath = Path.Combine(pathToSave, fileName);
            var dbPath = Path.Combine(folderName, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                attachedFile.File.CopyTo(stream);                
            }
            await AzureBlobHandler.UploadFileToBlob(fullPath, fileName);
            File.Delete(fullPath);

            var filesUrl ="https://n2kbackbonesharedfiles.blob.core.windows.net/justificationfiles/";

            JustificationFiles justFile = new JustificationFiles
            {
                 Path= filesUrl + fileName,
                 SiteCode= attachedFile.SiteCode,
                 Version=attachedFile.Version
            };
            await _dataContext.Set<JustificationFiles>().AddAsync(justFile);
            await _dataContext.SaveChangesAsync();
            List<JustificationFiles> result = await _dataContext.Set<JustificationFiles>().AsNoTracking().Where(jf => jf.SiteCode == jf.SiteCode && jf.Version == justFile.Version).ToListAsync();
            return result;
        }


        public async Task<int> DeleteFile(long justificationId)
        {
            int result = 0;
            JustificationFiles? justification = await _dataContext.Set<JustificationFiles>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == justificationId);
            if (justification != null)
            {
                _dataContext.Set<JustificationFiles>().Remove(justification);

                if (!string.IsNullOrEmpty(justification.Path) ) await AzureBlobHandler.DeleteFileFromBlob(justification.Path);
                await _dataContext.SaveChangesAsync();
                result = 1;
            }
            return result;

        }


        #endregion
    }
}
