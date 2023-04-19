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
using DocumentFormat.OpenXml.Wordprocessing;

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

        

    }
}
