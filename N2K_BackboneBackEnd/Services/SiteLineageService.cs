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

            List<long> releases = new List<long>();
            foreach (Lineage lineage in list)
            {
                releases.Add(lineage.Version);
            }
            releases = releases.Distinct().ToList();

            List<long> ULHIds = new List<long>();
            foreach (UnionListHeader header in headers)
            {
                ULHIds.Add(header.idULHeader);
            }
            releases = releases.OrderBy(i => ULHIds.IndexOf(i)).ToList();
            if (limit > 0)
                releases = releases.Skip(Math.Max(0, releases.Count() - limit)).ToList();

            list = list.OrderBy(i => ULHIds.IndexOf(i.Version)).ToList();

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
            return result;
        }



        public async Task<List<Lineage>> GetChanges(string country, SiteChangeStatus status, IMemoryCache cache, int page = 1, int pageLimit = 0, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true)
        {
            List<Lineage> changes = new List<Lineage>();

            Enum.TryParse<SiteChangeStatus>(status.ToString(), out status);

            SqlParameter paramCountry = new SqlParameter("@country", country);
            SqlParameter paramStatus = new SqlParameter("@status", status);
            string query = "SELECT [SiteCode],[Version],[AntecessorsVersion],[AntecessorsSiteCodes],[Type], [Status] " +
                           "FROM [dbo].[tmpLineage]" +
                           "WHERE @country = LEFT([SiteCode],2) and @status= [Status]";

            SqlConnection backboneConn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;
            try
            {
                backboneConn = new SqlConnection(_dataContext.Database.GetConnectionString());
                backboneConn.Open();
                command = new SqlCommand(query, backboneConn);
                SqlParameter paramTable = new SqlParameter("@siteCodes", System.Data.SqlDbType.Structured);
                paramTable.TypeName = "[dbo].[SiteCodeFilter]";
                command.Parameters.Add(paramTable);
                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {

                    Lineage change = new Lineage
                    {
                        SiteCode = reader["SiteCode"] is null ? null : reader["SiteCode"].ToString(),
                        Version = int.Parse(reader["Version"].ToString()),
                        AntecessorsVersion = long.Parse(reader["AntecessorsVersion"].ToString()),
                        AntecessorsSiteCodes = reader["AntecessorsSiteCodes"] is null ? null : reader["AntecessorsSiteCodes"].ToString(),
                        Type = int.Parse(reader["Type"].ToString())
                    };

                    SiteChangeStatus status1;
                    Enum.TryParse<SiteChangeStatus>(reader["Status"].ToString(), out status1);
                    change.Status = status1;
                    changes.Add(change);
                }
                var startRow = (page - 1) * pageLimit;
                if (pageLimit > 0)
                {
                    changes = changes
                        .Skip(startRow)
                        .Take(pageLimit)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "Load lineage changes", "");
            }
            finally
            {
                if (reader != null) await reader.DisposeAsync();
                if (command != null) command.Dispose();
                if (backboneConn != null) backboneConn.Dispose();
            }
            return changes;

        }

        public async Task<List<ModifiedSiteCode>> AcceptChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache)
        {
            List<ModifiedSiteCode> siteActivities = new List<ModifiedSiteCode>();
            return siteActivities;
        }

        public async Task<List<ModifiedSiteCode>> RejectChanges(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache)
        {
            List<ModifiedSiteCode> siteActivities = new List<ModifiedSiteCode>();
            return siteActivities;
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


        public async Task<List<ModifiedSiteCode>> SetChangesBackToPending(ModifiedSiteCode[] changedSiteStatus, IMemoryCache cache)
        {
            List<ModifiedSiteCode> siteActivities = new List<ModifiedSiteCode>();
            return siteActivities;

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

