using DocumentFormat.OpenXml.Bibliography;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using System.ComponentModel.Design;

namespace N2K_BackboneBackEnd.Services
{
    public class SiteDetailsService : ISiteDetailsService
    {

        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        public SiteDetailsService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
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


        public async Task<List<StatusChanges>> AddComment(StatusChanges comment)
        {
            comment.Date = DateTime.Now;
            comment.Owner = GlobalData.Username;
            comment.Edited = 0;
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
            var edited = 1;
            StatusChanges? _comment = await _dataContext.Set<StatusChanges>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == comment.Id);
            if (_comment!= null)
            {
                if (_comment.Edited.HasValue) edited = _comment.Edited.Value + 1;
            }
            comment.EditedDate = DateTime.Now;                        
            comment.Edited =  edited;
            comment.Editedby = GlobalData.Username; 
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
            List<JustificationFiles> result = new List<JustificationFiles>();
            IAttachedFileHandler? fileHandler = null;
            var username = GlobalData.Username;

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
        public async Task<string> SaveEdition(ChangeEditionDb changeEdition)
        {
            var username= GlobalData.Username;
            try
            {
                SqlParameter param_sitecode = new SqlParameter("@sitecode", changeEdition.SiteCode);
                SqlParameter param_version = new SqlParameter("@version", changeEdition.Version);
                SqlParameter param_name = new SqlParameter("@name", changeEdition.SiteName);
                SqlParameter param_sitetype = new SqlParameter("@sitetype", changeEdition.SiteType);
                SqlParameter param_area = new SqlParameter("@area", changeEdition.Area);
                SqlParameter param_length = new SqlParameter("@length", changeEdition.Length);
                SqlParameter param_centrex = new SqlParameter("@centrex", changeEdition.CentreY);
                SqlParameter param_centrey = new SqlParameter("@centrey", changeEdition.CentreY);

                await _dataContext.Database.ExecuteSqlRawAsync("$ exec dbo.spCloneSites " +
                    "@sitecode, @version, @name, @sitetype, @area, @length, @centrex, @centrey"
                    , param_sitecode, param_version, param_name, param_sitetype, param_area, param_length, param_centrex, param_centrey);
                
                Sites site = _dataContext.Set<Sites>().Where(e => e.SiteCode == changeEdition.SiteCode && e.Current == true).FirstOrDefault();
                if (site != null)
                {
                    if (changeEdition.BioRegion != "string")
                    {
                        string[] bioregions = new string[] { };                    
                        if (changeEdition.BioRegion!= null) 
                            bioregions= changeEdition.BioRegion.Split(",");
                        if (bioregions.Length > 0)
                        {
                            foreach (var bioregion in bioregions)
                            {
                                BioRegions bioreg = new BioRegions
                                {
                                    SiteCode = changeEdition.SiteCode,
                                    Version = site.Version,
                                    BGRID = int.Parse(bioregion),
                                    Percentage = 100 / bioregions.Length // NEEDS TO BE CHANGED - PENDING
                                };
                                _dataContext.Set<BioRegions>().Add(bioreg);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "SaveEdition", "");
                throw ex;
            }

            return "OK";
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
