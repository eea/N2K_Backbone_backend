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

namespace N2K_BackboneBackEnd.Services
{


    public class SiteLineageService : ISiteLineageService
    {
        private readonly N2KBackboneContext _dataContext;


        public SiteLineageService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
        }


        public async Task<List<SiteLineage>> GetSiteLineageAsync(string siteCode)
        {
            int limit = 0; //Set a release limit to show
            SqlParameter param1 = new SqlParameter("@sitecode", siteCode);
            SqlParameter param2 = new SqlParameter("@limit", DBNull.Value); //limit is not used here since it would limit based on lines, not releases
            List<Lineage> list = await _dataContext.Set<Lineage>().FromSqlRaw($"exec [dbo].[spGetSiteLineageBySitecode]  @sitecode, @limit",
                                param1, param2).ToListAsync();
            List<UnionListHeader> headers = await _dataContext.Set<UnionListHeader>().AsNoTracking().ToListAsync();
            headers = headers.OrderBy(i => i.Date).ToList(); //Order releases by date

            List<SiteLineage> result = new List<SiteLineage>();

            List<long?> releases = new List<long?>();
            foreach (Lineage lineage in list)
            {
                releases.Add(lineage.Version);
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

            list = list.OrderBy(i => ULHIds.IndexOf(i.Version)).ToList();
            if (list.Count > 0)
            {
                //Check if the predecessor of the first one in line exists and if it is in the list, if it's not, add it before the first one
                Lineage originCheck = list.Where(c => c.Version == list.FirstOrDefault().AntecessorsVersion).FirstOrDefault();
                if (list.FirstOrDefault().AntecessorsVersion != null && originCheck == null)
                {
                    List<Lineage> temps = await _dataContext.Set<Lineage>().AsNoTracking().Where(c => c.Version == list.FirstOrDefault().AntecessorsVersion && list.FirstOrDefault().AntecessorsSiteCodes.Contains(c.SiteCode)).ToListAsync();
                    temps.Reverse();
                    foreach (Lineage temp in temps)
                    {
                        list.Insert(0, temp);
                    }
                    releases.Insert(0, temps[0].Version);
                    if (limit > 0)
                        releases = releases.Skip(Math.Max(0, releases.Count() - limit)).ToList();
                }

                foreach (Lineage lineage in list)
                {
                    if (releases.Contains(lineage.Version))
                    {
                        SiteLineage temp = new SiteLineage();
                        temp.SiteCode = lineage.SiteCode;
                        temp.Release = headers.Where(c => c.idULHeader == lineage.Version).FirstOrDefault().Name;
                        if (lineage.AntecessorsSiteCodes == null && lineage.AntecessorsVersion != null)
                        {
                            temp.Predecessors.SiteCode = lineage.SiteCode;
                            temp.Predecessors.Release = headers.Where(c => c.idULHeader == lineage.AntecessorsVersion).FirstOrDefault().Name;
                        }
                        else if (lineage.AntecessorsVersion != null)
                        {
                            temp.Predecessors.SiteCode = lineage.AntecessorsSiteCodes;
                            temp.Predecessors.Release = headers.Where(c => c.idULHeader == lineage.AntecessorsVersion).FirstOrDefault().Name;
                        }
                        if (list.Where(c => c.SiteCode == lineage.SiteCode && c.AntecessorsVersion == lineage.Version).FirstOrDefault() != null)
                        {
                            temp.Successors.SiteCode = lineage.SiteCode;
                            temp.Successors.Release = headers.Where(c => c.idULHeader == (list.Where(c => c.SiteCode == lineage.SiteCode && c.AntecessorsVersion == lineage.Version).FirstOrDefault().Version)).FirstOrDefault().Name;
                        }
                        else if (list.Where(c => c.AntecessorsSiteCodes != null && c.AntecessorsSiteCodes.Contains(lineage.SiteCode) && c.AntecessorsVersion == lineage.Version).FirstOrDefault() != null)
                        {
                            List<Lineage> antecessors = list.Where(c => c.AntecessorsSiteCodes != null && c.AntecessorsSiteCodes.Contains(lineage.SiteCode) && c.AntecessorsVersion == lineage.Version).ToList();
                            string antecessor = "";

                            if (antecessors.Count > 1)
                            {
                                antecessor = string.Join(",", antecessors.Select(r => r.SiteCode));
                            }
                            else
                            {
                                antecessor = antecessors.FirstOrDefault().SiteCode;
                            }

                            temp.Successors.SiteCode = antecessor;
                            temp.Successors.Release = headers.Where(c => c.idULHeader == (list.Where(c => c.AntecessorsSiteCodes != null && c.AntecessorsSiteCodes.Contains(lineage.SiteCode) && c.AntecessorsVersion == lineage.Version).FirstOrDefault().Version)).FirstOrDefault().Name;
                        }
                        result.Add(temp);
                    }
                }
            }
            return result;
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
                List<Lineage> changes = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetLineageData  @country, @status",
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

                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                changes.ToList().ForEach(cs =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { cs.SiteCode, 0 });
                });
                SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                paramTable.Value = sitecodesfilter;
                paramTable.TypeName = "[dbo].[SiteCodeFilter]";
                List<Sites> sites = await _dataContext.Set<Sites>().FromSqlRaw($"exec dbo.spGetLatestSiteVersionsLineage  @siteCodes",
                                paramTable).ToListAsync();

                foreach (Lineage change in changes)
                {
                    LineageChanges temp = new LineageChanges();
                    temp.ChangeId = change.ID;
                    temp.SiteCode = change.SiteCode;
                    temp.SiteName = sites.Where(c => c.SiteCode == change.SiteCode).FirstOrDefault().Name;
                    temp.Type = change.Type;
                    if (change.AntecessorsSiteCodes != null)
                    {
                        temp.Reference = change.AntecessorsSiteCodes;
                    }
                    else if (change.AntecessorsVersion != null)
                    {
                        temp.Reference = change.SiteCode;
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
                List<Lineage> proposed = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetLineageData  @country, @status",
                                paramCountry, paramStatus).ToListAsync();
                paramStatus = new SqlParameter("@status", LineageStatus.Consolidated);
                List<Lineage> consolidated = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spGetLineageData  @country, @status",
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


        public async Task<List<long>> ConsolidateChanges(LineageConsolidation[] consolidateChanges)
        {
            List<long> result = new List<long>();
            try
            {
                var lineageConsolidate = new DataTable("lineageConsolidate");
                lineageConsolidate.Columns.Add("id", typeof(int));
                lineageConsolidate.Columns.Add("Type", typeof(int));
                lineageConsolidate.Columns.Add("predecessors", typeof(string));

                consolidateChanges.ToList().ForEach(cs =>
                {
                    if (cs.Predecessors == "" || cs.Predecessors == "string")
                        cs.Predecessors = null;
                    lineageConsolidate.Rows.Add(new Object[] { cs.ChangeId, cs.Type, (cs.Predecessors == null) ? DBNull.Value : cs.Predecessors });
                    result.Add(cs.ChangeId);
                });

                SqlParameter paramTable = new SqlParameter("@lineageConsolidate", System.Data.SqlDbType.Structured);
                paramTable.Value = lineageConsolidate;
                paramTable.TypeName = "[dbo].[lineageConsolidate]";

                List<Lineage> data = await _dataContext.Set<Lineage>().FromSqlRaw($"exec dbo.spConsolidateChanges  @lineageConsolidate",
                                paramTable).ToListAsync();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - ConsolidateChanges", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result;
        }


        public async Task<List<long>> SetChangesBackToProposed(long[] ChangeId)
        {
            List<long> result = new List<long>();
            try
            {
                List<Lineage> lineageBackProposed = await _dataContext.Set<Lineage>().Where(c => c.Status == LineageStatus.Consolidated && ChangeId.Contains(c.ID)).ToListAsync();
                lineageBackProposed.ForEach(y =>
                {
                    y.Status = LineageStatus.Proposed;
                    result.Add(y.ID);
                });
                _dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - SetChangesBackToProposed", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result;
        }

        public async Task<List<LineageEditionInfo>> GetPredecessorsInfo(long ChangeId)
        {
            List<LineageEditionInfo> result = new List<LineageEditionInfo>();
            try
            {
                Lineage change = await _dataContext.Set<Lineage>().AsNoTracking().Where(c => c.ID == ChangeId).FirstOrDefaultAsync();
                List<UnionListDetail> details = new List<UnionListDetail>();
                if (change.AntecessorsSiteCodes != null)
                {
                    details = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(c => c.idUnionListHeader == change.AntecessorsVersion && change.AntecessorsSiteCodes.Contains(c.SCI_code)).ToListAsync();
                }
                else
                {
                    details = await _dataContext.Set<UnionListDetail>().AsNoTracking().Where(c => c.idUnionListHeader == change.AntecessorsVersion && c.SCI_code == change.SiteCode).ToListAsync();
                }

                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                details.ForEach(d =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { d.SCI_code, d.version });
                });
                SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                paramTable.Value = sitecodesfilter;
                paramTable.TypeName = "[dbo].[SiteCodeFilter]";
                List<SiteBioRegions> bioregions = await _dataContext.Set<SiteBioRegions>().FromSqlRaw($"exec dbo.spGetBioRegionsAndAreaBySitecodeAndVersion  @siteCodes",
                                paramTable).ToListAsync();

                details.ForEach(d =>
                {
                    result.Add(new LineageEditionInfo
                    {
                        SiteCode = d.SCI_code,
                        SiteName = d.SCI_Name,
                        SiteType = _dataContext.Set<Sites>().AsNoTracking().Where(c => c.SiteCode == d.SCI_code && c.Version == d.version).Select(c => c.SiteType).First(),
                        BioRegion = (bioregions.Count() > 0) && bioregions.Where(b => b.SiteCode == d.SCI_code && b.Version == d.version).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == d.SCI_code && b.Version == d.version).FirstOrDefault().BioRegions != null ? (bioregions.Where(b => b.SiteCode == d.SCI_code && b.Version == d.version).FirstOrDefault().BioRegions) : "",
                        AreaSDF = d.Area != null ? Convert.ToDouble(d.Area) : null,
                        AreaGEO = (bioregions.Count() > 0) && bioregions.Where(b => b.SiteCode == d.SCI_code && b.Version == d.version).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == d.SCI_code && b.Version == d.version).FirstOrDefault().area != null ? Convert.ToDouble(bioregions.Where(b => b.SiteCode == d.SCI_code && b.Version == d.version).FirstOrDefault().area) : null,
                        Length = d.Length != null ? Convert.ToDouble(d.Length) : null,
                        Status = LineageStatus.Consolidated.ToString()
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
            List<LineageEditionInfo> result = new List<LineageEditionInfo>();
            try
            {
                Lineage change = await _dataContext.Set<Lineage>().AsNoTracking().Where(c => c.ID == ChangeId).FirstOrDefaultAsync();

                var sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));

                sitecodesfilter.Rows.Add(new Object[] { change.SiteCode, 0 });

                SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                paramTable.Value = sitecodesfilter;
                paramTable.TypeName = "[dbo].[SiteCodeFilter]";
                List<Sites> sites = await _dataContext.Set<Sites>().FromSqlRaw($"exec dbo.spGetLatestSiteVersionsLineage  @siteCodes",
                                paramTable).ToListAsync();

                sitecodesfilter = new DataTable("sitecodesfilter");
                sitecodesfilter.Columns.Add("SiteCode", typeof(string));
                sitecodesfilter.Columns.Add("Version", typeof(int));
                sites.ForEach(d =>
                {
                    sitecodesfilter.Rows.Add(new Object[] { d.SiteCode, d.Version });
                });
                paramTable.Value = sitecodesfilter;
                List<SiteBioRegions> bioregions = await _dataContext.Set<SiteBioRegions>().FromSqlRaw($"exec dbo.spGetBioRegionsAndAreaBySitecodeAndVersion  @siteCodes",
                                paramTable).ToListAsync();

                sites.ForEach(d =>
                {
                    result.Add(new LineageEditionInfo
                    {
                        SiteCode = d.SiteCode,
                        SiteName = d.Name,
                        SiteType = d.SiteType,
                        BioRegion = (bioregions.Count() > 0) && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.Version).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.Version).FirstOrDefault().BioRegions != null ? (bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.Version).FirstOrDefault().BioRegions) : "",
                        AreaSDF = d.Area != null ? Convert.ToDouble(d.Area) : null,
                        AreaGEO = (bioregions.Count() > 0) && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.Version).FirstOrDefault() != null && bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.Version).FirstOrDefault().area != null ? Convert.ToDouble(bioregions.Where(b => b.SiteCode == d.SiteCode && b.Version == d.Version).FirstOrDefault().area) : null,
                        Length = d.Length != null ? Convert.ToDouble(d.Length) : null,
                        Status = change.Status.ToString()
                    });
                });

            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "SiteLineageService - GetLineageChangesInfo", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result.First();
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