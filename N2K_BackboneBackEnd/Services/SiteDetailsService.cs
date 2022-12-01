using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2019.Word.Cid;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Helpers;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.ViewModel;
using NuGet.Packaging;
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

        private MemoryCacheEntryOptions CreateCacheEntryOptions() {
            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(2500))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(40000);
        }
        private void CreateEmptyCommentCache(string listName, IMemoryCache cache)
        {
            cache.Set(listName, new List<StatusChanges>() , CreateCacheEntryOptions());            
        }

        public long GetRandomId()
        {
            Random random = new Random();  
            return random.NextInt64(1, 8696761735052207);
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

        public async Task<List<StatusChanges>> ListSiteComments(string pSiteCode, int pCountryVersion, IMemoryCache cache,  bool temporal = false)
        {
            List<StatusChanges> result = new List<StatusChanges>();
            result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == pSiteCode && ch.Version == pCountryVersion).ToListAsync();

            if (temporal)
            {
                string listName = string.Format("{0}_{1}", 
                        GlobalData.Username, 
                        "temp_comments"
                       );
                if (cache.TryGetValue(listName, out List<StatusChanges> cachedList))
                {
                    result.AddRange(cachedList.Where(a => a.SiteCode == pSiteCode && a.Version == pCountryVersion));
                }
            }
            return result;
        }


        public async Task<List<StatusChanges>> AddComment(StatusChanges comment, IMemoryCache cache, bool temporal = false)
        {
            List<StatusChanges> result = new List<StatusChanges>();
            comment.Date = DateTime.Now;
            comment.Owner = GlobalData.Username;
            comment.Edited = 0;            

            if (!temporal)
            {
                comment.Temporal = false;
                await _dataContext.Set<StatusChanges>().AddAsync(comment);
                await _dataContext.SaveChangesAsync();
                result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == comment.SiteCode && ch.Version == comment.Version).ToListAsync();

            }
            else
            {
                comment.Temporal = true;
                string listName = string.Format("{0}_{1}",
                        GlobalData.Username,
                        "temp_comments"
                       );

                List<StatusChanges> cachedList = new List<StatusChanges>();
                if (!cache.TryGetValue(listName, out cachedList))
                {
                    CreateEmptyCommentCache(listName, cache);
                    cachedList = new List<StatusChanges>();
                }
                comment.Id = GetRandomId();
                cachedList.Add(comment);

                cache.Set(listName, cachedList);
                result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == comment.SiteCode && ch.Version == comment.Version).ToListAsync();
                result.AddRange(cachedList.Where(a => a.SiteCode == comment.SiteCode && a.Version == comment.Version));
            }
            return result;
        }

        public async Task<int> DeleteComment(long CommentId, IMemoryCache cache, bool temporal = false)
        {
            int result = 0;
            if (temporal)
            {
                string listName = string.Format("{0}_{1}",
                        GlobalData.Username,
                        "temp_comments"
                       );
                List<StatusChanges> cachedList = new List<StatusChanges>();
                if (!cache.TryGetValue(listName, out cachedList))
                {
                    return 0;
                }

                if (cachedList.FirstOrDefault(a => a.Id == CommentId) != null)
                {
                    cachedList.Remove(cachedList.FirstOrDefault(a => a.Id == CommentId));
                    cache.Set(listName, cachedList);
                    return 1;
                }
                return 0;
            }
            else
            {

                StatusChanges? comment = await _dataContext.Set<StatusChanges>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == CommentId);
                if (comment != null)
                {
                    _dataContext.Set<StatusChanges>().Remove(comment);
                    await _dataContext.SaveChangesAsync();
                    result = 1;
                }
            }
            return result;
        }

        public async Task<List<StatusChanges>> UpdateComment(StatusChanges comment, IMemoryCache cache, bool temporal = false)
        {
            List<StatusChanges> result = new List<StatusChanges>();
            var edited = 1;
            List<StatusChanges> cachedList = new List<StatusChanges>();
            if (temporal)
            {
                string listName = string.Format("{0}_{1}",
                        GlobalData.Username,
                        "temp_comments"
                       );                
                if (!cache.TryGetValue(listName, out cachedList)) {
                    CreateEmptyCommentCache(listName, cache);
                    cachedList = new List<StatusChanges>();    
                }

                if (cachedList.FirstOrDefault(a => a.Id == comment.Id ) != null)
                {
                    var item = cachedList.FirstOrDefault(a => a.Id == comment.Id);
                    item.Comments = comment.Comments;
                    item.Justification = comment.Justification;
                }
                else
                {
                    StatusChanges? _comment = await _dataContext.Set<StatusChanges>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == comment.Id);
                    if (_comment != null)
                    {
                        if (_comment.Edited.HasValue) edited = _comment.Edited.Value + 1;
                    }
                    comment.EditedDate = DateTime.Now;
                    comment.Edited = edited;
                    comment.Editedby = GlobalData.Username;
                    comment.Temporal = true;
                    
                    cachedList.Add(comment);

                    cache.Set(listName, cachedList);
                    //result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == comment.SiteCode && ch.Version == comment.Version).ToListAsync();
                    //result.AddRange(cachedList);
                }
            }

            else  { 
                StatusChanges? _comment = await _dataContext.Set<StatusChanges>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == comment.Id);
                if (_comment != null)
                {
                    if (_comment.Edited.HasValue) edited = _comment.Edited.Value + 1;
                }
                comment.EditedDate = DateTime.Now;
                comment.Edited = edited;
                comment.Editedby = GlobalData.Username;
                _dataContext.Set<StatusChanges>().Update(comment);
                await _dataContext.SaveChangesAsync();
            }

            result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == comment.SiteCode && ch.Version == comment.Version).ToListAsync();
            if (temporal)
            {
                result.AddRange(cachedList.Where(a=> a.SiteCode== comment.SiteCode && a.Version== comment.Version)); 
            }
            return result;
        }


        #endregion

        #region SiteFiles

        public async Task<List<JustificationFiles>> ListSiteFiles(string pSiteCode, int pCountryVersion, IMemoryCache cache, bool temporal = false)
        {
            List<JustificationFiles> result = new List<JustificationFiles>();
            result = await _dataContext.Set<JustificationFiles>().AsNoTracking().Where(f => f.SiteCode == pSiteCode && f.Version == pCountryVersion).ToListAsync();

            if (temporal)
            {
                string listName = string.Format("{0}_{1}",
                        GlobalData.Username,
                        "temp_files"
                       );
                if (cache.TryGetValue(listName, out List<JustificationFiles> cachedList))
                {
                    result.AddRange(cachedList.Where(a => a.SiteCode == pSiteCode && a.Version == pCountryVersion));
                }
            }
            return result;
        }

        public async Task<List<JustificationFiles>> UploadFile(AttachedFile attachedFile, IMemoryCache cache, bool temporal = false)
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
            List<JustificationFiles> cachedList = new List<JustificationFiles>();
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
                if (temporal)
                {
                    string listName = string.Format("{0}_{1}",
                            GlobalData.Username,
                            "temp_files"
                           );
                    if (!cache.TryGetValue(listName, out cachedList))
                    {
                        CreateEmptyCommentCache(listName, cache);
                        cachedList = new List<JustificationFiles>();
                    }
                    justFile.Id = GetRandomId();
                    cachedList.Add(justFile);

                    cache.Set(listName, cachedList);
                }
                else
                {
                    await _dataContext.Set<JustificationFiles>().AddAsync(justFile);
                    await _dataContext.SaveChangesAsync();
                }
                
                result = await _dataContext.Set<JustificationFiles>().AsNoTracking().Where(jf => jf.SiteCode == attachedFile.SiteCode && jf.Version == attachedFile.Version).ToListAsync();
                if (temporal)
                    result.AddRange(cachedList.Where(a => a.SiteCode == attachedFile.SiteCode && a.Version == attachedFile.Version));
            }
            return result;
        }


        public async Task<int> DeleteFile(long justificationId, IMemoryCache cache, bool temporal = false)
        {
            int result = 0;
            JustificationFiles? justification = null;
            if (temporal)
            {
                string listName = string.Format("{0}_{1}",
                        GlobalData.Username,
                        "temp_files"
                       );
                List<JustificationFiles> cachedList = new List<JustificationFiles>();
                if (!cache.TryGetValue(listName, out cachedList))
                {
                    return 0;
                }
                if (cachedList.FirstOrDefault(a => a.Id == justificationId) == null) return 0;

                cachedList.Remove(cachedList.FirstOrDefault(a => a.Id == justificationId));
                cache.Set(listName, cachedList);
                
            }                
            else
            {
                justification = await _dataContext.Set<JustificationFiles>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == justificationId);
            }

            if (justification != null)
            {
                if (temporal)
                {

                }
                else
                {
                    //delete record from DB
                    _dataContext.Set<JustificationFiles>().Remove(justification);
                    await _dataContext.SaveChangesAsync();
                }

                //delete file from repository
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
                result = 1;
            }
            return result;

        }
        #endregion


        #region SiteEdition
        public async Task<string> SaveEdition(ChangeEditionDb changeEdition, IMemoryCache cache)
        {
            var username= GlobalData.Username;
            try
            {
                SqlParameter param_sitecode = new SqlParameter("@sitecode", changeEdition.SiteCode);
                SqlParameter param_version = new SqlParameter("@version", changeEdition.Version);
                SqlParameter param_name = new SqlParameter("@name", changeEdition.SiteName is null ? DBNull.Value : changeEdition.SiteName);
                SqlParameter param_sitetype = new SqlParameter("@sitetype", changeEdition.SiteType is null ? DBNull.Value : changeEdition.SiteType);
                SqlParameter param_area = new SqlParameter("@area", changeEdition.Area == 0 ? DBNull.Value : changeEdition.Area);
                SqlParameter param_length = new SqlParameter("@length", changeEdition.Length == 0 ? DBNull.Value : changeEdition.Length);
                SqlParameter param_centrex = new SqlParameter("@centrex", changeEdition.CentreX == 0 ? DBNull.Value : changeEdition.CentreX);
                SqlParameter param_centrey = new SqlParameter("@centrey", changeEdition.CentreY == 0 ? DBNull.Value : changeEdition.CentreY);
                SqlParameter param_justif_required = new SqlParameter("@justif_required", changeEdition.JustificationRequired == null? false : changeEdition.JustificationRequired);
                SqlParameter param_justif_provided = new SqlParameter("@justif_provided", changeEdition.JustificationProvided == null ? false : changeEdition.JustificationProvided ) ;

                await _dataContext.Database.ExecuteSqlRawAsync($"exec dbo.spCloneSites " +
                    "@sitecode, @version, @name, @sitetype, @area, @length, @centrex, @centrey, @justif_required , @justif_provided " 
                    , param_sitecode, param_version, param_name, param_sitetype, param_area, param_length, param_centrex, param_centrey , param_justif_required , param_justif_provided);
                
                Sites site = _dataContext.Set<Sites>().Where(e => e.SiteCode == changeEdition.SiteCode && e.Current == true).FirstOrDefault();
                if (site != null)
                {
                    if (changeEdition.BioRegion != null && changeEdition.BioRegion != "string" && changeEdition.BioRegion != "")
                    {
                        string[] bioregions = changeEdition.BioRegion.Split(",");
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
                        _dataContext.SaveChanges();
                    }

                    //add temporal comments
                    string listName = string.Format("{0}_{1}",GlobalData.Username,"temp_comments");
                    List<StatusChanges> comCachedList = new List<StatusChanges>();
                    if (cache.TryGetValue(listName, out comCachedList))
                    {
                        foreach (var comm in comCachedList)
                        {
                            if (site.SiteCode == comm.SiteCode)
                            {
                                comm.Version = site.Version;
                                comm.Date = DateTime.Now;
                                comm.Owner = GlobalData.Username;
                                comm.Edited = 0;
                                await _dataContext.Set<StatusChanges>().AddAsync(comm);
                                comCachedList.Remove(comm);
                            }
                        }
                        cache.Set(listName, comCachedList);
                    }
                    if (comCachedList!=null)  comCachedList.Clear();
                    cache.Remove(listName);


                    //add temporal files
                    listName = string.Format("{0}_{1}",GlobalData.Username,"temp_files");
                    List<JustificationFiles> justifCachedList = new List<JustificationFiles>();
                    if (cache.TryGetValue(listName, out justifCachedList))
                    {
                        foreach (var justif in justifCachedList)
                        {
                            if (site.SiteCode == justif.SiteCode)
                            {
                                justif.Version = site.Version;
                                justif.ImportDate = DateTime.Now;
                                justif.Username  = GlobalData.Username;
                                await _dataContext.Set<JustificationFiles>().AddAsync(justif);
                                justifCachedList.Remove(justif);
                            }
                        }
                        
                    }
                    if (justifCachedList != null) justifCachedList.Clear();
                    cache.Remove(listName);
                    await _dataContext.SaveChangesAsync();
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
            List<ChangeEditionDb> list = await _dataContext.Set<ChangeEditionDb>().FromSqlRaw($"exec dbo.[spGetReferenceEditInfo]  @sitecode",
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
                    Version = changeEdition.Version,
                    JustificationRequired = changeEdition.JustificationRequired,
                    JustificationProvided = changeEdition.JustificationProvided
                };
            }
        }

        #endregion
    }
}
