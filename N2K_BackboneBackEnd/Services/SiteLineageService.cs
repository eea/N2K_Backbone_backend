using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;
using System.Data;
using NuGet.Protocol;
using N2K_BackboneBackEnd.Helpers;
using System.Security.Policy;
using System.Collections.Generic;
using N2K_BackboneBackEnd.Models.BackboneDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace N2K_BackboneBackEnd.Services
{


    public class SiteLineageService : ISiteLineageService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private IBackgroundSpatialHarvestJobs _fmeHarvestJobs;


        public SiteLineageService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext, IOptions<ConfigSettings> app, IBackgroundSpatialHarvestJobs harvestJobs)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
            _appSettings = app;
            _fmeHarvestJobs = harvestJobs;
        }


        public async Task<List<SiteLineage>> GetSiteLineageAsync(string siteCode)
        {
            try
            {
                int limit = 0; //Set a release limit to show
                SqlParameter paramSitecode = new SqlParameter("@sitecode", siteCode);
                List<Lineage> list = await _dataContext.Set<Lineage>().FromSqlRaw($"exec [dbo].[spGetSiteLineageBySitecode]  @sitecode",
                paramSitecode).ToListAsync();

                var changeIDs = list.Select(r => r.ID);
                List<LineageAntecessors> predecessors = await _dataContext.Set<LineageAntecessors>().Where(a => changeIDs.Contains(a.LineageID)).AsNoTracking().ToListAsync();

                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                predecessors.ForEach(d =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { d.SiteCode, d.Version });
                });
                SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                paramTable.Value = sitecodesfilter;
                paramTable.TypeName = "[dbo].[SiteCodeFilter]";
                list.AddRange(await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetSiteLineageBySitecodeAndVersion  @siteCodes", paramTable).AsNoTracking().ToListAsync());

                List<UnionListHeader> headers = await _dataContext.Set<UnionListHeader>().AsNoTracking().ToListAsync();
                headers = headers.OrderBy(i => i.Date).ToList(); //Order releases by date

                List<SiteLineage> result = new List<SiteLineage>();

                List<long?> releases = new List<long?>();
                foreach (Lineage lineage in list)
                {
                    releases.Add(lineage.Release);
                }
                releases = releases.Distinct().ToList();

                List<long?> ULHIds = new List<long?>();
                foreach (UnionListHeader header in headers)
                {
                    ULHIds.Add(header.idULHeader);
                }
                releases = releases.OrderBy(i => ULHIds.IndexOf(i)).ToList();
                if (limit > 0)
                    releases = releases.Skip(Math.Max(0, releases.Count() - limit)).ToList();

                list = list.OrderBy(i => ULHIds.IndexOf(i.Release)).ToList();
                if (list.Count > 0)
                {
                    foreach (Lineage lineage in list)
                    {
                        if (releases.Contains(lineage.Release))
                        {
                            try
                            {
                                SiteLineage temp = new SiteLineage();
                                temp.SiteCode = lineage.SiteCode;
                                temp.Release = headers.Where(c => c.idULHeader == lineage.Release).FirstOrDefault().Name;
                                if (predecessors.Where(c => c.LineageID == lineage.ID).ToList().Count() > 0)
                                {
                                    temp.Predecessors.SiteCode = string.Join(",", predecessors.Where(c => c.LineageID == lineage.ID).Select(r => r.SiteCode));
                                    temp.Predecessors.Release = headers.Where(c => c.idULHeader == (list.Where(c =>
                                            c.SiteCode == predecessors.Where(c => c.LineageID == lineage.ID).FirstOrDefault().SiteCode &&
                                            c.Version == predecessors.Where(c => c.LineageID == lineage.ID).FirstOrDefault().Version).FirstOrDefault().Release)).FirstOrDefault().Name;
                                }
                                if (predecessors.Where(c => c.SiteCode == lineage.SiteCode && c.Version == lineage.Version).ToList().Count() > 0)
                                {
                                    temp.Successors.SiteCode = string.Join(",", list.Where(c => predecessors.Where(c =>
                                            c.SiteCode == lineage.SiteCode && c.Version == lineage.Version).Select(r => r.LineageID).Contains(c.ID)).Select(b => b.SiteCode));
                                    temp.Successors.Release = headers.Where(c => c.idULHeader == (list.Where(c =>
                                            c.ID == predecessors.Where(c => c.SiteCode == lineage.SiteCode && c.Version == lineage.Version).FirstOrDefault().LineageID)
                                                .FirstOrDefault().Release)).FirstOrDefault().Name;
                                }
                                result.Add(temp);
                            }
                            catch (Exception ex)
                            {
                                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetSiteLineageAsync - " + lineage.SiteCode + "/" + lineage.Version, "", _dataContext.Database.GetConnectionString());
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetSiteLineageAsync", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }


        public async Task<List<LineageCountry>> GetOverview()
        {
            try
            {
                return await _dataContext.Set<LineageCountry>().FromSqlRaw($"exec dbo.spGetLineageOverview").ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetOverview", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }


        public async Task<List<LineageChanges>> GetChanges(string country, LineageStatus status, IMemoryCache cache, int page = 1, int pageLimit = 0, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true)
        {
            List<LineageChanges> result = new List<LineageChanges>();
            try
            {
                LineageStatus statusLineage;
                Enum.TryParse<LineageStatus>(status.ToString(), out statusLineage);
                SqlParameter paramCountry = new SqlParameter("@country", country);
                SqlParameter paramStatus = new SqlParameter("@status", statusLineage);
                List<LineageExtended> changes = await _dataContext.Set<LineageExtended>().FromSqlRaw($"exec dbo.spGetLineageData  @country, @status",
                                paramCountry, paramStatus).ToListAsync();

                string filter = "";
                if (creation)
                    filter = String.Concat(filter, "Creation,");
                if (deletion)
                    filter = String.Concat(filter, "Deletion,");
                if (split)
                    filter = String.Concat(filter, "Split,");
                if (merge)
                    filter = String.Concat(filter, "Merge,");
                if (recode)
                    filter = String.Concat(filter, "Recode,");
                changes = changes.Where(c => filter.Contains(c.Type.ToString())).ToList();

                foreach (LineageExtended change in changes)
                {
                    LineageChanges temp = new LineageChanges();
                    temp.ChangeId = change.ID;
                    temp.SiteCode = change.SiteCode;
                    temp.SiteName = change.Name;
                    temp.Type = change.Type;
                    if (change.AntecessorsSiteCodes != null)
                    {
                        temp.Reference = change.AntecessorsSiteCodes;
                    }
                    else
                    {
                        temp.Reference = "-";
                    }
                    if (change.Type == LineageTypes.Deletion)
                    {
                        temp.Reported = "-";
                    }
                    else
                    {
                        temp.Reported = change.SiteCode;
                    }
                    result.Add(temp);
                }

                var startRow = (page - 1) * pageLimit;
                if (pageLimit > 0)
                {
                    result = result
                        .Skip(startRow)
                        .Take(pageLimit)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetChanges", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result;
        }


        public async Task<LineageCount> GetCodesCount(string country, IMemoryCache cache, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true)
        {
            LineageCount result = new LineageCount();
            try
            {
                SqlParameter paramCountry = new SqlParameter("@country", country);
                SqlParameter paramStatus = new SqlParameter("@status", LineageStatus.Proposed);
                List<LineageExtended> proposed = await _dataContext.Set<LineageExtended>().FromSqlRaw($"exec dbo.spGetLineageData  @country, @status",
                                paramCountry, paramStatus).ToListAsync();
                paramStatus = new SqlParameter("@status", LineageStatus.Consolidated);
                List<LineageExtended> consolidated = await _dataContext.Set<LineageExtended>().FromSqlRaw($"exec dbo.spGetLineageData  @country, @status",
                                paramCountry, paramStatus).ToListAsync();

                string filter = "";
                if (creation)
                    filter = String.Concat(filter, "Creation,");
                if (deletion)
                    filter = String.Concat(filter, "Deletion,");
                if (split)
                    filter = String.Concat(filter, "Split,");
                if (merge)
                    filter = String.Concat(filter, "Merge,");
                if (recode)
                    filter = String.Concat(filter, "Recode,");
                proposed = proposed.Where(c => filter.Contains(c.Type.ToString())).ToList();
                consolidated = consolidated.Where(c => filter.Contains(c.Type.ToString())).ToList();

                result.Proposed = proposed.Count();
                result.Consolidated = consolidated.Count();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetCodesCount", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result;
        }


        public async Task<long> SaveEdition(LineageConsolidation consolidateChanges)
        {
            try
            {
                Lineage lineage = await _dataContext.Set<Lineage>().Where(c => c.ID == consolidateChanges.ChangeId).FirstOrDefaultAsync();

                SqlParameter param1 = new SqlParameter("@country", lineage.SiteCode.Substring(0, 2));
                List<SiteBasic> resultSites = await _dataContext.Set<SiteBasic>().FromSqlRaw($"exec [dbo].[spGetLineageReferenceSites]  @country",
                                    param1).ToListAsync();
                resultSites = resultSites.Where(c => consolidateChanges.Predecessors.Contains(c.SiteCode)).ToList();

                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                resultSites.ToList().ForEach(cs =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { cs.SiteCode, cs.Version });
                });
                SqlParameter paramId = new SqlParameter("@id", consolidateChanges.ChangeId);
                SqlParameter paramSitecodesTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                paramSitecodesTable.Value = sitecodesfilter;
                paramSitecodesTable.TypeName = "[dbo].[SiteCodeFilter]";

                await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spConsolidatePredecessors @id, @siteCodes", paramId, paramSitecodesTable);

                lineage.Type = consolidateChanges.Type;
                await _dataContext.SaveChangesAsync();

                HarvestedService harvest = new HarvestedService(_dataContext, _versioningContext, _appSettings, _fmeHarvestJobs);
                await harvest.ChangeDetectionSingleSite(lineage.SiteCode, lineage.Version, _dataContext.Database.GetConnectionString());
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - SaveEdition", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return consolidateChanges.ChangeId;
        }


        public async Task<List<LineageEditionInfo>> GetPredecessorsInfo(long ChangeId)
        {
            List<LineageEditionInfo> result = new List<LineageEditionInfo>();
            try
            {
                List<LineageAntecessors> antecessors = await _dataContext.Set<LineageAntecessors>().AsNoTracking().Where(c => c.LineageID == ChangeId).ToListAsync();

                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                antecessors.ForEach(d =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { d.SiteCode, d.Version });
                });
                SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                paramTable.Value = sitecodesfilter;
                paramTable.TypeName = "[dbo].[SiteCodeFilter]";
                List<SiteToHarvest> antecessorSites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetSitesBySiteCodeFilter  @siteCodes",
                                paramTable).ToListAsync();
                List<SiteBioRegionsAndArea> bioregions = await _dataContext.Set<SiteBioRegionsAndArea>().FromSqlRaw($"exec dbo.spGetBioRegionsAndAreaBySitecodeAndVersion  @siteCodes",
                                paramTable).ToListAsync();
                List<Lineage> antecessorsLineage = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetSiteLineageBySitecodeAndVersion  @siteCodes",
                                paramTable).ToListAsync();
                List<UnionListHeader> unionListHeader = await _dataContext.Set<UnionListHeader>().AsNoTracking().ToListAsync();

                antecessorSites.ForEach(d =>
                {
                    result.Add(new LineageEditionInfo
                    {
                        SiteCode = d.SiteCode,
                        SiteName = d.SiteName,
                        SiteType = d.SiteType,
                        BioRegion = (bioregions.Count() > 0) && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault().BioRegions != null ? (bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault().BioRegions) : "",
                        AreaSDF = d.AreaHa != null ? Convert.ToDouble(d.AreaHa) : null,
                        AreaGEO = (bioregions.Count() > 0) && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault().area != null ? Convert.ToDouble(bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault().area) : null,
                        Length = d.LengthKm != null ? Convert.ToDouble(d.LengthKm) : null,
                        Status = LineageStatus.Consolidated.ToString(),
                        ReleaseDate = antecessorsLineage.Where(a => a.SiteCode == d.SiteCode && a.Version == d.VersionId).FirstOrDefault().Release != null ? unionListHeader.Where(b => b.idULHeader == antecessorsLineage.Where(a => a.SiteCode == d.SiteCode && a.Version == d.VersionId).FirstOrDefault().Release).FirstOrDefault().Date.Value.ToShortDateString().ToString() : null
                    });
                });
                result = result.DistinctBy(c => c.SiteCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetPredecessorsInfo", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result.Distinct().ToList();
        }


        public async Task<LineageEditionInfo> GetLineageChangesInfo(long ChangeId)
        {
            LineageEditionInfo result = null;
            try
            {
                Lineage change = await _dataContext.Set<Lineage>().AsNoTracking().Where(c => c.ID == ChangeId).FirstOrDefaultAsync();
                if (change.Type != LineageTypes.Deletion)
                {
                    Sites site = await _dataContext.Set<Sites>().AsNoTracking().Where(c => c.SiteCode == change.SiteCode && c.Version == change.Version).FirstOrDefaultAsync();

                    var sitecodesfilter = new DataTable("sitecodesfilter");
                    sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                    sitecodesfilter.Columns.Add("Version", typeof(int));
                    sitecodesfilter.Rows.Add(new Object[] { change.SiteCode, change.Version });
                    SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                    paramTable.Value = sitecodesfilter;
                    paramTable.TypeName = "[dbo].[SiteCodeFilter]";
                    List<SiteBioRegionsAndArea> bioregions = await _dataContext.Set<SiteBioRegionsAndArea>().FromSqlRaw($"exec dbo.spGetBioRegionsAndAreaBySitecodeAndVersion  @siteCodes",
                                    paramTable).ToListAsync();

                    result = new LineageEditionInfo
                    {
                        SiteCode = site.SiteCode,
                        SiteName = site.Name,
                        SiteType = site.SiteType,
                        BioRegion = (bioregions.Count() > 0) && bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault().BioRegions != null ? (bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault().BioRegions) : "",
                        AreaSDF = site.Area != null ? Convert.ToDouble(site.Area) : null,
                        AreaGEO = (bioregions.Count() > 0) && bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault().area != null ? Convert.ToDouble(bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault().area) : null,
                        Length = site.Length != null ? Convert.ToDouble(site.Length) : null,
                        Status = change.Status.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetLineageChangesInfo", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result;
        }


        public async Task<List<string>> GetLineageReferenceSites(string country)
        {
            List<string> result = new List<string>();
            try
            {
                SqlParameter param1 = new SqlParameter("@country", country);
                List<SiteBasic> resultSites = await _dataContext.Set<SiteBasic>().FromSqlRaw($"exec [dbo].[spGetLineageReferenceSites]  @country",
                                    param1).ToListAsync();
                result = resultSites.Select(s => s.SiteCode).ToList();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetLineageReferenceSites", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result.Distinct().ToList();
        }


        public DataSet GetDataSet(string storedProcName, DataTable param)
        {
            SqlConnection backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
            var command = new SqlCommand(storedProcName, backboneConn) { CommandType = CommandType.StoredProcedure };
            SqlParameter paramTable1 = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
            paramTable1.Value = param;
            paramTable1.TypeName = "[dbo].[SiteCodeFilter]";
            command.Parameters.Add(paramTable1);
            var result = new DataSet();
            var dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(result);

            dataAdapter.Dispose();
            command.Dispose();
            backboneConn.Dispose();
            return result;
        }


        private async Task<List<SiteCodeView>> swapSiteInListCache(IMemoryCache pCache, SiteChangeStatus? pStatus, Level? pLevel, SiteChangeStatus? pListNameFrom, SiteCodeView pSite)
        {

            await Task.Delay(10);
            List<SiteCodeView> cachedlist = new List<SiteCodeView>();


            //Site comes from this list
            string listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", pSite.SiteCode.Substring(0, 2), pListNameFrom.ToString(), pLevel.ToString());
            if (pCache.TryGetValue(listName, out cachedlist))
            {
                SiteCodeView element = cachedlist.Where(cl => cl.SiteCode == pSite.SiteCode).FirstOrDefault();
                if (element != null)
                {
                    cachedlist.Remove(element);
                }
            }


            //Site goes to that list
            listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", pSite.SiteCode.Substring(0, 2), pStatus.ToString(), pLevel.ToString());
            if (pCache.TryGetValue(listName, out cachedlist))
            {
                SiteCodeView element = cachedlist.Where(cl => cl.SiteCode == pSite.SiteCode).FirstOrDefault();
                if (element != null)
                {
                    element.Version = pSite.Version;
                    element.Name = pSite.Name;
                }
                else
                {
                    cachedlist.Add(pSite);
                }

            }
            return null;
        }


    }
}