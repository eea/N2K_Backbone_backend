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
using Microsoft.AspNetCore.Http;

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
            await Task.Delay(1);
            List<SiteLineage> result = new List<SiteLineage>();

            int siteNumber = Int32.Parse(siteCode.Substring(siteCode.Length - 7));
            string country = siteCode.Substring(0, 2);

            SiteLineage v1 = new SiteLineage();
            v1.SiteCode = "AT1101112";
            v1.Release = "2019-2020";
            v1.Successors.SiteCode = "AT1101112";
            v1.Successors.Release = "2020-2021";
            result.Add(v1);

            SiteLineage v2 = new SiteLineage();
            v2.SiteCode = "AT1101112";
            v2.Release = "2020-2021";
            v2.Predecessors.SiteCode = "AT1101112";
            v2.Predecessors.Release = "2019-2020";
            v2.Successors.SiteCode = "AT2208000,AT2209000";
            v2.Successors.Release = "2021-2022";
            result.Add(v2);

            SiteLineage v3_1 = new SiteLineage();
            v3_1.SiteCode = "AT2208000";
            v3_1.Release = "2021-2022";
            v3_1.Predecessors.SiteCode = "AT1101112";
            v3_1.Predecessors.Release = "2020-2021";
            result.Add(v3_1);

            SiteLineage v3_2 = new SiteLineage();
            v3_2.SiteCode = "AT2209000";
            v3_2.Release = "2021-2022";
            v3_2.Predecessors.SiteCode = "AT1101112";
            v3_2.Predecessors.Release = "2020-2021";
            result.Add(v3_2);

            return result;
        }

        public async Task<List<Lineage>> GetChanges(string country, SiteChangeStatus status, IMemoryCache cache, int page = 1, int pageLimit = 0, bool creation = true, bool deletion = true, bool split = true, bool merge = true, bool recode = true)
        {
            List<Lineage> changes = new List<Lineage>();

            Enum.TryParse<SiteChangeStatus>(status.ToString(), out status);

            SqlParameter paramCountry = new SqlParameter("@country", country);
            SqlParameter paramStatus = new SqlParameter("@status", status);
            string query = "SELECT [SiteCode],[Version],[AntecessorsVersion],[AntecessorsSiteCodes],[Operation], [Status] " +
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
                        Operation = int.Parse(reader["Operation"].ToString())
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
