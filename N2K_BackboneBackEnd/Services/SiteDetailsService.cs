using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2019.Word.Cid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
using N2K_BackboneBackEnd.Models.BackboneDB;
using N2K_BackboneBackEnd.Models.ViewModel;
using NuGet.Packaging;
using System.ComponentModel.Design;
using System.Security.Policy;

namespace N2K_BackboneBackEnd.Services
{
    public class CachedListItem<T> where T : DocumentationChanges
    {
        private readonly IMemoryCache _cache;
        private readonly string _listName;

        private MemoryCacheEntryOptions CreateCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(2500))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(40000);
        }

        public CachedListItem(string listName, IMemoryCache cache)
        {
            _cache = cache;
            _listName = listName;
            List<T> cachedList = new List<T>();
            if (!cache.TryGetValue(listName, out cachedList))
            {
                cache.Set(listName, new List<T>(), CreateCacheEntryOptions());
            }
        }

        public List<T> GetCachedList()
        {
            List<T> cachedList = new List<T>();
            if (!_cache.TryGetValue(_listName, out cachedList))
            {
                if (cachedList == null)
                    return new List<T>();
            }
            return cachedList;
        }

        public List<T> GetFinalList(List<T> dbList)
        {
            List<T> cachedList = GetCachedList();
            //build the list to return
            List<T> finalResult = new List<T>();
            foreach (T item in dbList)
            {
                //check if the item is in the cached list
                var cachedItem = cachedList.FindLast(it => it.SiteCode == item.SiteCode && it.Version == item.Version && it.Id == item.Id);
                if (cachedItem != null)
                {
                    //if the item in the cached list 
                    //if the new item is an updated one
                    if (item.Tags == "Updated")
                    {
                        finalResult.Add(cachedItem);
                    }
                }
                else
                {
                    //add the item from te DB
                    finalResult.Add(item);
                }
            }

            //add the new items to the list
            foreach (T item in cachedList.Where(it => it.Temporal == true || it.Tags != "Deleted").ToList())
            {
                finalResult.Add(item);
            }
            return finalResult;
        }
    }


    public class SiteDetailsService : ISiteDetailsService
    {

        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;
        private readonly IOptions<ConfigSettings> _appSettings;

        private string comlistName = string.Format("{0}_{1}", GlobalData.Username, "temp_comments");
        private string justiflistName = string.Format("{0}_{1}", GlobalData.Username, "temp_files");

        public SiteDetailsService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext, IOptions<ConfigSettings> app)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
            _appSettings = app;
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

        public async Task<List<StatusChanges>> ListSiteComments(string pSiteCode, int pCountryVersion, IMemoryCache cache, bool temporal = false)
        {
            List<StatusChanges> result = new List<StatusChanges>();
            result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == pSiteCode && ch.Version == pCountryVersion).ToListAsync();

            if (temporal)
            {
                CachedListItem<StatusChanges> ComItem = new CachedListItem<StatusChanges>(comlistName, cache);
                return ComItem.GetFinalList(result);
            }
            return result;
        }


        public async Task<List<StatusChanges>> AddComment(StatusChanges comment, IMemoryCache cache, bool temporal = false)
        {
            List<StatusChanges> result = new List<StatusChanges>();
            comment.Date = DateTime.Now;
            comment.Owner = GlobalData.Username;
            comment.Temporal = true;
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
                CachedListItem<StatusChanges> ComItem = new CachedListItem<StatusChanges>(comlistName, cache);
                List<StatusChanges> cachedList = ComItem.GetCachedList();
                comment.Id = GetRandomId();
                cachedList.Add(comment);

                cache.Set(comlistName, cachedList);
                return await ListSiteComments(comment.SiteCode, comment.Version, cache, temporal);
            }
            return result;
        }

        public async Task<int> DeleteComment(long CommentId, IMemoryCache cache, bool temporal = false)
        {
            int result = 0;
            StatusChanges? comment = await _dataContext.Set<StatusChanges>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == CommentId);
            if (temporal)
            {
                CachedListItem<StatusChanges> ComItem = new CachedListItem<StatusChanges>(comlistName, cache);
                List<StatusChanges> cachedList = ComItem.GetCachedList();
                //if it is a comment existing in the database add it to the cache tagged as deleted
                if (comment != null)
                {
                    comment.EditedDate = DateTime.Now;
                    comment.Tags = "Deleted";
                    comment.Editedby = GlobalData.Username;
                    cachedList.Add(comment);

                    cache.Set(comlistName, cachedList);
                    return 1;
                }
                else
                    //it is a temp comment, delete it from cache
                    if (cachedList.FirstOrDefault(a => a.Id == CommentId) != null)
                {
                    cachedList.Remove(cachedList.FirstOrDefault(a => a.Id == CommentId));
                    cache.Set(comlistName, cachedList);
                    return 1;
                }
                return 0;
            }
            else
            {
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
            StatusChanges? _comment = await _dataContext.Set<StatusChanges>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == comment.Id);
            if (temporal)
            {
                CachedListItem<StatusChanges> ComItem = new CachedListItem<StatusChanges>(comlistName, cache);
                cachedList = ComItem.GetCachedList();
                //if it is a comment existing in the database add it to the cache tagged as updated
                if (_comment != null)
                {
                    if (_comment.Edited.HasValue) edited = _comment.Edited.Value + 1;
                    var item = cachedList.FirstOrDefault(a => a.Id == comment.Id);
                    if (item != null)
                    {
                        item.EditedDate = DateTime.Now;
                        item.Edited = edited;
                        item.Editedby = GlobalData.Username;
                        item.Tags = "Updated";
                        item.Comments = comment.Comments;
                        item.Date = _comment.Date;
                        item.Owner = _comment.Owner;
                    }
                    else
                    {
                        comment.Date = _comment.Date;
                        comment.Owner = _comment.Owner;
                        comment.EditedDate = DateTime.Now;
                        comment.Edited = edited;
                        comment.Temporal = true;
                        comment.Editedby = GlobalData.Username;
                        comment.Tags = "Updated";
                        cachedList.Add(comment);
                    }
                    cache.Set(comlistName, cachedList);
                }
                else
                {
                    //cyeck if it exists in the cache and update the item
                    if (cachedList.FirstOrDefault(a => a.Id == comment.Id) != null)
                    {
                        var item = cachedList.FirstOrDefault(a => a.Id == comment.Id);
                        item.Comments = comment.Comments;
                        item.Justification = comment.Justification;
                    }
                }
            }

            else
            {
                if (_comment != null)
                {
                    if (_comment.Edited.HasValue) edited = _comment.Edited.Value + 1;

                    comment.EditedDate = DateTime.Now;
                    comment.Edited = edited;
                    comment.Editedby = GlobalData.Username;
                    _dataContext.Set<StatusChanges>().Update(comment);
                    await _dataContext.SaveChangesAsync();
                }
            }

            return await ListSiteComments(comment.SiteCode, comment.Version, cache, temporal);
        }


        #endregion

        #region SiteFiles

        public async Task<List<JustificationFiles>> ListSiteFiles(string pSiteCode, int pCountryVersion, IMemoryCache cache, bool temporal = false)
        {

            List<JustificationFiles> result = new List<JustificationFiles>();
            result = await _dataContext.Set<JustificationFiles>().AsNoTracking().Where(f => f.SiteCode == pSiteCode && f.Version == pCountryVersion).ToListAsync();

            if (temporal)
            {
                CachedListItem<JustificationFiles> JustifItem = new CachedListItem<JustificationFiles>(justiflistName, cache);
                return JustifItem.GetFinalList(result);
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
                    Username = username
                };
                if (temporal)
                {

                    CachedListItem<JustificationFiles> JustifItem = new CachedListItem<JustificationFiles>(justiflistName, cache);
                    cachedList = JustifItem.GetCachedList();
                    justFile.Id = GetRandomId();
                    cachedList.Add(justFile);

                    cache.Set(justiflistName, cachedList);
                }
                else
                {
                    await _dataContext.Set<JustificationFiles>().AddAsync(justFile);
                    await _dataContext.SaveChangesAsync();
                }


            }
            return await ListSiteFiles(attachedFile.SiteCode, attachedFile.Version, cache, temporal);
        }


        public async Task<int> DeleteFile(long justificationId, IMemoryCache cache, bool temporal = false)
        {
            int result = 0;
            JustificationFiles? justification = await _dataContext.Set<JustificationFiles>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == justificationId);
            if (temporal)
            {
                CachedListItem<JustificationFiles> JustifItem = new CachedListItem<JustificationFiles>(justiflistName, cache);
                List<JustificationFiles> cachedList = JustifItem.GetCachedList();
                //if it is a justification existing in the database, add it to the cache tagged as deleted
                if (justification != null)
                {
                    justification.Tags = "Deleted";
                    cachedList.Add(justification);
                    cache.Set(justiflistName, cachedList);
                    return 1;
                }
                else
                {
                    //it is a temp justification, delete it from cache
                    if (cachedList.FirstOrDefault(a => a.Id == justificationId) != null)
                    {
                        //check if the file is linked to another id
                        //if not delete from repository
                        //delete file from repository
                        JustificationFiles cachedItem = cachedList.FirstOrDefault(a => a.Id == justificationId);

                        if (!_dataContext.Set<JustificationFiles>().Any(it => it.Path == cachedItem.Path))
                        {
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
                        }

                        //remove item from cache
                        cachedList.Remove(cachedList.FirstOrDefault(a => a.Id == justificationId));
                        cache.Set(justiflistName, cachedList);
                        return 1;
                    }
                }
                return 0;
            }
            else
            {
                if (justification != null)
                {
                    //delete record from DB
                    _dataContext.Set<JustificationFiles>().Remove(justification);
                    await _dataContext.SaveChangesAsync();
                }
            }

            if (justification != null)
            {
                if (!temporal)
                {
                    //delete record from DB
                    _dataContext.Set<JustificationFiles>().Remove(justification);
                    await _dataContext.SaveChangesAsync();


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
            }
            return result;

        }
        #endregion


        #region SiteEdition
        public async Task<string> SaveEdition(ChangeEditionDb changeEdition, IMemoryCache cache)
        {
            var username = GlobalData.Username;
            Sites site = null;
            SiteChangeDb change = null, reject = null;
            SiteActivities activityCheck = null;
            SiteActivities activity = null;
            SiteChangeStatus status = SiteChangeStatus.Accepted; //Added generic value since null is not an option
            Level level = Level.Critical; //Added generic value since null is not an option
            List<SiteCodeView> cachedlist = null;

            SqlParameter param_sitecode = null;
            SqlParameter param_version = null;
            SqlParameter param_name = null;
            SqlParameter param_sitetype = null;
            SqlParameter param_area = null;
            SqlParameter param_length = null;
            SqlParameter param_centrex = null;
            SqlParameter param_centrey = null;
            SqlParameter param_justif_required = null;
            SqlParameter param_justif_provided = null;

            try
            {
                //Verify the site & current version exists


                site = _dataContext.Set<Sites>().Single(x => x.SiteCode == changeEdition.SiteCode && x.Current == true);


                if (site != null && (site.CurrentStatus == SiteChangeStatus.Accepted || site.CurrentStatus == SiteChangeStatus.Rejected))
                {


                    activity = new SiteActivities
                    {
                        SiteCode = changeEdition.SiteCode,
                        Version = changeEdition.Version,
                        Author = GlobalData.Username,
                        Date = DateTime.Now,
                        Action = "User edition",
                        Deleted = false
                    };

                    activity.Version = site.Version;

                    //Loading the neccesary list for the changes of sent version
                    List<SiteChangeDb> deletionChanges = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == changeEdition.SiteCode && e.Version == changeEdition.Version).ToListAsync();
                    if (deletionChanges.Count > 0)
                    {
                        status = (SiteChangeStatus)deletionChanges.FirstOrDefault().Status;
                        level = (Level)deletionChanges.Max(a => a.Level);
                    }

                    List<SiteChangeDb> changes = deletionChanges;
                    if (site.Version != changeEdition.Version)
                    {
                        changes = await _dataContext.Set<SiteChangeDb>().Where(e => e.SiteCode == changeEdition.SiteCode && e.Version == site.Version).ToListAsync();
                    }
                    //Was it edited after rejection?
                    activityCheck = await _dataContext.Set<SiteActivities>().Where(e => e.SiteCode == changeEdition.SiteCode && e.Action == "User edition after rejection of version " + changeEdition.Version && e.Deleted == false).FirstOrDefaultAsync();

                    //Is the sender site Rejected?
                    reject = deletionChanges.Where(e => e.Status == SiteChangeStatus.Rejected).FirstOrDefault();
                    if (reject != null)
                    {
                        activity.Action = "User edition after rejection of version " + changeEdition.Version;
                    }

                    //Load the params for the stored procedures
                    param_sitecode = new SqlParameter("@sitecode", changeEdition.SiteCode);
                    param_version = new SqlParameter("@version", site.Version);
                    param_name = new SqlParameter("@name", changeEdition.SiteName is null ? DBNull.Value : changeEdition.SiteName);
                    param_sitetype = new SqlParameter("@sitetype", changeEdition.SiteType is null ? DBNull.Value : changeEdition.SiteType);
                    param_area = new SqlParameter("@area", changeEdition.Area);
                    param_length = new SqlParameter("@length", changeEdition.Length);
                    param_centrex = new SqlParameter("@centrex", changeEdition.CentreX);
                    param_centrey = new SqlParameter("@centrey", changeEdition.CentreY);
                    param_justif_required = new SqlParameter("@justif_required", changeEdition.JustificationRequired == null ? false : changeEdition.JustificationRequired);
                    param_justif_provided = new SqlParameter("@justif_provided", changeEdition.JustificationProvided == null ? false : changeEdition.JustificationProvided);

                    //Was the site previously edited?
                    change = changes.Where(e => e.ChangeType == "User edition").FirstOrDefault();

                    if (change == null || (reject != null && activityCheck == null))
                    {
                        await _dataContext.Database.ExecuteSqlRawAsync($"exec dbo.spCloneSites " +
                            "@sitecode, @version, @name, @sitetype, @area, @length, @centrex, @centrey, @justif_required , @justif_provided "
                            , param_sitecode, param_version, param_name, param_sitetype, param_area, param_length, param_centrex, param_centrey, param_justif_required, param_justif_provided);

                        site = await _dataContext.Set<Sites>().Where(e => e.SiteCode == changeEdition.SiteCode && e.Current == true).FirstOrDefaultAsync();
                    }
                    else
                    {
                        await _dataContext.Database.ExecuteSqlRawAsync($"exec dbo.spUpdateSites " +
                            "@sitecode, @version, @name, @sitetype, @area, @length, @centrex, @centrey, @justif_required , @justif_provided "
                            , param_sitecode, param_version, param_name, param_sitetype, param_area, param_length, param_centrex, param_centrey, param_justif_required, param_justif_provided);
                    }

                    //To prevent the faillure of the spCloneSites procedure
                    //site = await _dataContext.Set<Sites>().Where(e => e.SiteCode == changeEdition.SiteCode && e.Current == true).FirstOrDefaultAsync();
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
                            await _dataContext.SaveChangesAsync();
                        }

                        _dataContext.Set<SiteActivities>().Add(activity);
                        await _dataContext.SaveChangesAsync();

                        //For a edition the site only has two status: Accepted and Rejected
                        //Just in case of the site was previously in Accepted status, update the  element
                        //In case of Rejected status, the site must remain in the list of rejecteds
                        if (status == SiteChangeStatus.Accepted)
                        {
                            String listName = string.Format("{0}_{1}_{2}_{3}_{4}", GlobalData.Username, "list_codes", site.CountryCode, SiteChangeStatus.Accepted.ToString(), level.ToString());
                            cachedlist = new List<SiteCodeView>();
                            if (cache.TryGetValue(listName, out cachedlist))
                            {
                                SiteCodeView element = cachedlist.Where(x => x.SiteCode == site.SiteCode).FirstOrDefault();
                                if (element != null)
                                {
                                    //Exists, so update it
                                    element.Name = site.Name;
                                    element.Version = site.Version;
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("The status for this Site (" + changeEdition.SiteCode + " - " + changeEdition.Version.ToString() + ") is wrong");
                }
            }
            catch (System.InvalidOperationException iex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, "The version for this Site doesn't exist (" + changeEdition.SiteCode + " - " + changeEdition.Version.ToString() + ")", "SaveEdition", "");
                throw new Exception("The version for this Site doesn't exist (" + changeEdition.SiteCode + " - " + changeEdition.Version.ToString() + ")");
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
