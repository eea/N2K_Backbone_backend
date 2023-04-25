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
            if (originCheck == null)
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


    }
}
