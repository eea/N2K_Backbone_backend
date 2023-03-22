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
            v1.SiteCode = siteCode;
            v1.Version = "V1";
            v1.Successors.SiteCode = country + (siteNumber + 1).ToString("D7") + ", " + country + (siteNumber + 2).ToString("D7");
            v1.Successors.Version = "V2";
            result.Add(v1);

            SiteLineage v2_1 = new SiteLineage();
            v2_1.SiteCode = country + (siteNumber + 1).ToString("D7");
            v2_1.Version = "V2";
            v2_1.Predecessors.SiteCode = siteCode;
            v2_1.Predecessors.Version = "V1";
            v2_1.Successors.SiteCode = country + (siteNumber + 3).ToString("D7");
            v2_1.Successors.Version = "V3";
            result.Add(v2_1);

            SiteLineage v2_2 = new SiteLineage();
            v2_2.SiteCode = country + (siteNumber + 2).ToString("D7");
            v2_2.Version = "V2";
            v2_2.Predecessors.SiteCode = siteCode;
            v2_2.Predecessors.Version = "V1";
            v2_2.Successors.SiteCode = country + (siteNumber + 4).ToString("D7");
            v2_2.Successors.Version = "V3";
            result.Add(v2_2);

            SiteLineage v3_1 = new SiteLineage();
            v3_1.SiteCode = country + (siteNumber + 3).ToString("D7");
            v3_1.Version = "V3";
            v3_1.Predecessors.SiteCode = country + (siteNumber + 1).ToString("D7");
            v3_1.Predecessors.Version = "V2";
            result.Add(v3_1);

            SiteLineage v3_2 = new SiteLineage();
            v3_2.SiteCode = country + (siteNumber + 4).ToString("D7");
            v3_2.Version = "V3";
            v3_2.Predecessors.SiteCode = country + (siteNumber + 2).ToString("D7") + ", " + country + (siteNumber - 1).ToString("D7");
            v3_2.Predecessors.Version = "V2";
            result.Add(v3_2);

            return result;
        }

        

    }
}
