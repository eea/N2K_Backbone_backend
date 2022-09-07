using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;

namespace N2K_BackboneBackEnd.Services
{
    public class SiteDetailsService : ISiteDetailsService
    {

        private readonly N2KBackboneContext _dataContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public SiteDetailsService(N2KBackboneContext dataContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _appSettings = app;
        }

        #region SiteGeometry
        public async Task<SiteGeometryDetailed> GetSiteGeometry(string siteCode, int version)
        {
            SiteGeometryDetailed result = new SiteGeometryDetailed();
            SqlParameter param1 = new SqlParameter("@SiteCode", siteCode);
            SqlParameter param2 = new SqlParameter("@Version", version);

            var geometries = await _dataContext.Set<SiteGeometryDetailed>().FromSqlRaw($"exec dbo.spGetSiteVersionGeometryDetailed  @SiteCode, @Version",
                            param1, param2).ToArrayAsync();


#pragma warning disable CS8603 // Posible tipo de valor devuelto de referencia nulo
            if (geometries.Length > 0 && !string.IsNullOrEmpty(geometries[0].SiteCode))
                return geometries[0];

#pragma warning restore CS8603 // Posible tipo de valor devuelto de referencia nulo
            return result;
        }
        #endregion 



        #region SiteComments

        public async Task<List<StatusChanges>> ListSiteComments(string pSiteCode, int pCountryVersion)
        {
            List<StatusChanges> result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == pSiteCode && ch.Version == pCountryVersion).ToListAsync();
            return result;
        }


        public async Task<List<StatusChanges>> AddComment(StatusChanges comment,string  username ="")
        {

            comment.Date = DateTime.Now;
            comment.Owner = username;
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

        public async Task<List<StatusChanges>> UpdateComment(StatusChanges comment, string username = "")
        {
            comment.Date= DateTime.Now;
            comment.Owner = username;
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

        public async Task<List<JustificationFiles>> UploadFile(AttachedFile attachedFile, string username = "")
        {
            List<JustificationFiles> result = new List<JustificationFiles>();
            IAttachedFileHandler? fileHandler = null;

            if (_appSettings.Value.AttachedFiles == null) return result;

            if (_appSettings.Value.AttachedFiles.AzureBlob)
            {
                fileHandler = new AzureBlobHandler(_appSettings.Value.AttachedFiles);
            }
            else
            {
                fileHandler = new FileSystemHandler(_appSettings.Value.AttachedFiles);
            }
            var fileUrl = await fileHandler.UploadFileAsync(attachedFile);
            foreach (var fUrl in fileUrl)
            {
                JustificationFiles justFile = new JustificationFiles
                {
                    Path = fUrl,
                    SiteCode = attachedFile.SiteCode,
                    Version = attachedFile.Version,
                    ImportDate = DateTime.Now,
                    Username= username
                };
                await _dataContext.Set<JustificationFiles>().AddAsync(justFile);
                await _dataContext.SaveChangesAsync();

                result = await _dataContext.Set<JustificationFiles>().AsNoTracking().Where(jf => jf.SiteCode == attachedFile.SiteCode && jf.Version == attachedFile.Version).ToListAsync();
            }
            return result;
        }


        public async Task<int> DeleteFile(long justificationId)
        {
            int result = 0;
            JustificationFiles? justification = await _dataContext.Set<JustificationFiles>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == justificationId);
            if (justification != null)
            {
                _dataContext.Set<JustificationFiles>().Remove(justification);

                IAttachedFileHandler? fileHandler = null;
                if (_appSettings.Value.AttachedFiles == null) return 0;
                if (_appSettings.Value.AttachedFiles.AzureBlob)
                {
                    fileHandler = new AzureBlobHandler(_appSettings.Value.AttachedFiles);
                }
                else
                {
                    fileHandler = new FileSystemHandler(_appSettings.Value.AttachedFiles);
                }

                if (!string.IsNullOrEmpty(justification.Path)) await fileHandler.DeleteFileAsync(justification.Path);
                await _dataContext.SaveChangesAsync();
                result = 1;
            }
            return result;

        }
        #endregion

        #region SiteEdition
        public async Task<string> SaveEdition(ChangeEditionDb changeEdition, string username = "")
        {
            return await Task.FromResult("OK");
        }

        public async Task<ChangeEditionViewModel?> GetReferenceEditInfo(string siteCode)
        {
            SqlParameter param1 = new SqlParameter("@sitecode", siteCode);
            List<ChangeEditionDb> list = await _dataContext.Set<ChangeEditionDb>().FromSqlRaw($"exec dbo.spGetReferenceEditInfo  @sitecode",
                                param1).ToListAsync();
            ChangeEditionDb changeEdition = list.FirstOrDefault();
            if (changeEdition == null)
            {
                return null;
            }
            else
            {
                return new ChangeEditionViewModel
                {
                    Area = changeEdition.Area,
                    BioRegion = !string.IsNullOrEmpty(changeEdition.BioRegion) ? changeEdition.BioRegion.Split(',').Select(it => int.Parse(it)).ToList() : new List<int>(),
                    CentreX = changeEdition.CentreX,
                    CentreY = changeEdition.CentreY,
                    Length = changeEdition.Length,
                    SiteCode = changeEdition.SiteCode,
                    SiteName = changeEdition.SiteName,
                    SiteType = changeEdition.SiteType,
                    Version = changeEdition.Version
                };
            }
        }

        #endregion
    }
}
