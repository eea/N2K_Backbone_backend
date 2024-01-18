using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using System.Data;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using N2K_BackboneBackEnd.Hubs;

namespace N2K_BackboneBackEnd.Services
{

    public class SiteLineageService : ISiteLineageService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private IBackgroundSpatialHarvestJobs _fmeHarvestJobs;
        private readonly IHubContext<ChatHub> _hubContext;

        public SiteLineageService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext, IHubContext<ChatHub> hubContext, IOptions<ConfigSettings> app, IBackgroundSpatialHarvestJobs harvestJobs)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
            _appSettings = app;
            _fmeHarvestJobs = harvestJobs;
            _hubContext = hubContext;
        }

        public async Task<List<SiteLineage>> GetSiteLineageAsync(string siteCode)
        {
            try
            {
                int limit = 0; //Set a release limit to show
                SqlParameter paramSitecode = new("@sitecode", siteCode);
                List<Lineage> list = await _dataContext.Set<Lineage>().FromSqlRaw($"exec [dbo].[spGetSiteLineageBySitecode]  @sitecode",
                paramSitecode).ToListAsync();

                var changeIDs = list.Select(r => r.ID);
                List<LineageAntecessors> predecessors = await _dataContext.Set<LineageAntecessors>().Where(a => changeIDs.Contains(a.LineageID)).AsNoTracking().ToListAsync();

                DataTable sitecodesfilter = new("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                predecessors.ForEach(d =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { d.SiteCode, d.Version });
                });
                SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                {
                    Value = sitecodesfilter,
                    TypeName = "[dbo].[SiteCodeFilter]"
                };
                list.AddRange(await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetSiteLineageBySitecodeAndVersion  @siteCodes", paramTable).AsNoTracking().ToListAsync());

                List<UnionListHeader> headers = await _dataContext.Set<UnionListHeader>().AsNoTracking().ToListAsync();
                headers = headers.OrderBy(i => i.Date).ToList(); //Order releases by date

                List<SiteLineage> result = new();

                List<long?> releases = new();
                foreach (Lineage lineage in list)
                {
                    releases.Add(lineage.Release);
                }
                releases = releases.Distinct().ToList();

                List<long?> ULHIds = new();
                foreach (UnionListHeader header in headers)
                {
                    ULHIds.Add(header.idULHeader);
                }
                releases = releases.OrderBy(i => ULHIds.IndexOf(i)).ToList();
                if (limit > 0)
                    releases = releases.Skip(Math.Max(0, releases.Count - limit)).ToList();

                list = list.OrderBy(i => ULHIds.IndexOf(i.Release)).ToList();
                if (list.Count > 0)
                {
                    foreach (Lineage lineage in list)
                    {
                        if (releases.Contains(lineage.Release))
                        {
                            try
                            {
                                SiteLineage temp = new()
                                {
                                    SiteCode = lineage.SiteCode,
                                    Release = headers.Where(c => c.idULHeader == lineage.Release).FirstOrDefault().Name
                                };
                                if (predecessors.Where(c => c.LineageID == lineage.ID).ToList().Any())
                                {
                                    temp.Predecessors.SiteCode = string.Join(",", predecessors.Where(c => c.LineageID == lineage.ID).Select(r => r.SiteCode));
                                    temp.Predecessors.Release = headers.Where(c => c.idULHeader == (list.Where(c =>
                                            c.SiteCode == predecessors.Where(c => c.LineageID == lineage.ID).FirstOrDefault().SiteCode &&
                                            c.Version == predecessors.Where(c => c.LineageID == lineage.ID).FirstOrDefault().Version).FirstOrDefault().Release)).FirstOrDefault().Name;
                                }
                                if (predecessors.Where(c => c.SiteCode == lineage.SiteCode && c.Version == lineage.Version).ToList().Any())
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
            List<LineageChanges> result = new();
            try
            {
                LineageStatus statusLineage;
                Enum.TryParse<LineageStatus>(status.ToString(), out statusLineage);
                SqlParameter paramCountry = new("@country", country);
                SqlParameter paramStatus = new("@status", statusLineage);
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
                    Sites? site = await _dataContext.Set<Sites>().AsNoTracking().Where(site => site.SiteCode == change.SiteCode && site.Version == change.Version).FirstOrDefaultAsync();
                    LineageChanges temp = new()
                    {
                        ChangeId = change.ID,
                        SiteCode = change.SiteCode,
                        SiteName = change.Name,
                        SiteType = await _dataContext.Set<SiteTypes>().AsNoTracking().Where(t => t.Code == site.SiteType).Select(t => t.Classification).FirstOrDefaultAsync(),
                        Type = change.Type
                    };
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
                        temp.Submission = "-";
                    }
                    else
                    {
                        temp.Submission = change.SiteCode;
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
            LineageCount result = new();
            try
            {
                SqlParameter paramCountry = new("@country", country);
                SqlParameter paramStatus = new("@status", LineageStatus.Proposed);
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

                result.Proposed = proposed.Count;
                result.Consolidated = consolidated.Count;
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

                //When changing to Creation, check if antecessors have more successors, if not, create Deletion records for the antecessors
                if (consolidateChanges.Type == LineageTypes.Creation)
                {
                    DataTable lineageInsertion = new("LineageInsertion");
                    lineageInsertion.Columns.Add("SiteCode", typeof(string));
                    lineageInsertion.Columns.Add("Version", typeof(int));
                    lineageInsertion.Columns.Add("N2KVersioningVersion", typeof(int));
                    lineageInsertion.Columns.Add("Type", typeof(int));
                    lineageInsertion.Columns.Add("Status", typeof(int));
                    lineageInsertion.Columns.Add("AntecessorSiteCode", typeof(string));
                    lineageInsertion.Columns.Add("AntecessorVersion", typeof(int));

                    List<LineageAntecessors> antecessors = await _dataContext.Set<LineageAntecessors>().Where(c => c.LineageID == consolidateChanges.ChangeId).ToListAsync();

                    antecessors.ForEach(async a =>
                    {
                        LineageAntecessors hasSuccessors = await _dataContext.Set<LineageAntecessors>().Where(c => c.LineageID != consolidateChanges.ChangeId && c.SiteCode == a.SiteCode && c.Version == a.Version && c.N2KVersioningVersion == a.N2KVersioningVersion).FirstOrDefaultAsync();
                        if (hasSuccessors == null)
                        {
                            lineageInsertion.Rows.Add(new Object[] { a.SiteCode, a.Version, lineage.N2KVersioningVersion, LineageTypes.Deletion, LineageStatus.Proposed, a.SiteCode, a.Version });
                        }
                    });

                    SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                    {
                        Value = lineageInsertion,
                        TypeName = "[dbo].[LineageInsertion]"
                    };
                    await _dataContext.Database.ExecuteSqlRawAsync($"exec dbo.spInsertIntoLineageBulk  @siteCodes", paramTable);
                }

                SqlParameter param1 = new("@country", lineage.SiteCode[..2]);
                List<SiteBasic> resultSites = await _dataContext.Set<SiteBasic>().FromSqlRaw($"exec [dbo].[spGetLineageReferenceSites]  @country",
                                    param1).ToListAsync();
                resultSites = resultSites.Where(c => consolidateChanges.Predecessors.Contains(c.SiteCode)).ToList();

                DataTable sitecodesfilter = new("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                resultSites.ToList().ForEach(cs =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { cs.SiteCode, cs.Version });
                });
                SqlParameter paramId = new("@id", consolidateChanges.ChangeId);
                SqlParameter paramSitecodesTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                {
                    Value = sitecodesfilter,
                    TypeName = "[dbo].[SiteCodeFilter]"
                };

                await _dataContext.Database.ExecuteSqlRawAsync("exec dbo.spConsolidatePredecessors @id, @siteCodes", paramId, paramSitecodesTable);

                lineage.Type = consolidateChanges.Type;

                List<LineageAntecessors> antecessorsDeletion = await _dataContext.Set<LineageAntecessors>().Where(c => c.LineageID == consolidateChanges.ChangeId).ToListAsync();
                antecessorsDeletion.ForEach(async a =>
                {
                    Lineage hasSuccessors = await _dataContext.Set<Lineage>().Where(c => c.SiteCode == a.SiteCode && c.Version == a.Version && c.Type == LineageTypes.Deletion).FirstOrDefaultAsync();
                    if (hasSuccessors != null)
                    {
                        _dataContext.Set<Lineage>().Remove(hasSuccessors);
                    }
                });
                await _dataContext.SaveChangesAsync();

                HarvestedService harvest = new(_dataContext, _versioningContext, _hubContext, _appSettings, _fmeHarvestJobs);
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
            List<LineageEditionInfo> result = new();
            try
            {
                List<LineageAntecessors> antecessors = await _dataContext.Set<LineageAntecessors>().AsNoTracking().Where(c => c.LineageID == ChangeId).ToListAsync();

                DataTable sitecodesfilter = new("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                antecessors.ForEach(d =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { d.SiteCode, d.Version });
                });
                SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                {
                    Value = sitecodesfilter,
                    TypeName = "[dbo].[SiteCodeFilter]"
                };
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
                        BioRegion = (bioregions.Any()) && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault().BioRegions != null ? (bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault().BioRegions) : "",
                        AreaSDF = d.AreaHa != null ? Convert.ToDouble(d.AreaHa) : null,
                        AreaGEO = (bioregions.Any()) && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault().area != null ? Convert.ToDouble(bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.VersionId).FirstOrDefault().area) : null,
                        Length = d.LengthKm != null ? Convert.ToDouble(d.LengthKm) : null,
                        Status = LineageStatus.Consolidated.ToString(),
                        ReleaseDate = antecessorsLineage.Count == 0 ? null
                            : antecessorsLineage.Where(a => a.SiteCode == d.SiteCode && a.Version == d.VersionId).FirstOrDefault().Release != null
                                ? unionListHeader.Where(b => b.idULHeader == antecessorsLineage.Where(a => a.SiteCode == d.SiteCode && a.Version == d.VersionId).FirstOrDefault().Release).FirstOrDefault().Date
                                : null
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
                Sites site = await _dataContext.Set<Sites>().AsNoTracking().Where(c => c.SiteCode == change.SiteCode && c.Version == change.Version).FirstOrDefaultAsync();

                DataTable sitecodesfilter = new("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                sitecodesfilter.Rows.Add(new Object[] { change.SiteCode, change.Version });
                SqlParameter paramTable = new("@siteCodes", System.Data.SqlDbType.Structured)
                {
                    Value = sitecodesfilter,
                    TypeName = "[dbo].[SiteCodeFilter]"
                };
                List<SiteBioRegionsAndArea> bioregions = await _dataContext.Set<SiteBioRegionsAndArea>().FromSqlRaw($"exec dbo.spGetBioRegionsAndAreaBySitecodeAndVersion  @siteCodes",
                    paramTable).ToListAsync();

                result = new LineageEditionInfo
                {
                    SiteCode = site.SiteCode,
                    SiteName = site.Name,
                    SiteType = site.SiteType,
                    BioRegion = (bioregions.Any()) && bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault().BioRegions != null ? (bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault().BioRegions) : "",
                    AreaSDF = site.Area != null ? Convert.ToDouble(site.Area) : null,
                    AreaGEO = (bioregions.Any()) && bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault().area != null ? Convert.ToDouble(bioregions.Where(b => b.SiteCode == site.SiteCode && b.Version == site.Version).FirstOrDefault().area) : null,
                    Length = site.Length != null ? Convert.ToDouble(site.Length) : null,
                    Status = change.Status.ToString()
                };
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetLineageChangesInfo", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result;
        }

        public async Task<List<SiteBasic>> GetLineageReferenceSites(string country)
        {
            List<SiteBasic> result = new();
            try
            {
                SqlParameter param1 = new("@country", country);
                result = await _dataContext.Set<SiteBasic>().FromSqlRaw($"exec [dbo].[spGetLineageReferenceSites]  @country",
                                    param1).ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetLineageReferenceSites", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result;
        }

        public DataSet GetDataSet(string storedProcName, DataTable param)
        {
            SqlConnection backboneConn = new(_dataContext.Database.GetConnectionString());
            SqlCommand command = new(storedProcName, backboneConn) { CommandType = CommandType.StoredProcedure };
            SqlParameter paramTable1 = new("@siteCodes", System.Data.SqlDbType.Structured)
            {
                Value = param,
                TypeName = "[dbo].[SiteCodeFilter]"
            };
            command.Parameters.Add(paramTable1);
            DataSet result = new();
            SqlDataAdapter dataAdapter = new(command);
            dataAdapter.Fill(result);

            dataAdapter.Dispose();
            command.Dispose();
            backboneConn.Dispose();
            return result;
        }

        private async Task<List<SiteCodeView>> swapSiteInListCache(IMemoryCache pCache, SiteChangeStatus? pStatus, Level? pLevel, SiteChangeStatus? pListNameFrom, SiteCodeView pSite)
        {
            await Task.Delay(10);
            List<SiteCodeView> cachedlist = new();

            //Site comes from this list
            string listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", pSite.SiteCode[..2], pListNameFrom.ToString(), pLevel.ToString());
            if (pCache.TryGetValue(listName, out cachedlist))
            {
                SiteCodeView element = cachedlist.Where(cl => cl.SiteCode == pSite.SiteCode).FirstOrDefault();
                if (element != null)
                {
                    cachedlist.Remove(element);
                }
            }

            //Site goes to that list
            listName = string.Format("{0}_{1}_{2}_{3}", "listcodes", pSite.SiteCode[..2], pStatus.ToString(), pLevel.ToString());
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