using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;

namespace N2K_BackboneBackEnd.Services
{
    public class HarvestedService : IHarvestedService
    {
        private readonly N2KBackboneContext _dataContext;
        private readonly N2K_VersioningContext _versioningContext;

        public HarvestedService(N2KBackboneContext dataContext, N2K_VersioningContext versioningContext)
        {
            _dataContext = dataContext;
            _versioningContext = versioningContext;
        }
        public async Task<List<Harvesting>> GetHarvestedAsync()
        {
            var a = new List<Harvesting>();
            return await Task.FromResult(a);

        }

        public List<Harvesting> GetHarvested()
        {
            var a = new List<Harvesting>();
            return a;

        }



#pragma warning disable CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        public async Task<Harvesting> GetHarvestedAsyncById(int id)
#pragma warning restore CS8613 // La nulabilidad de los tipos de referencia en el tipo de valor devuelto no coincide con el miembro implementado de forma implícita
        {
            return await Task.FromResult(new Harvesting
            {
                Id = id,
                Country = "ES",
                Status = Enumerations.HarvestingStatus.Pending,
                SubmissionDate = DateTime.Today
            });

        }

        public async Task<List<Harvesting>> GetPendingEnvelopes()
        {
            var result = new List<Harvesting>();
            var processed = await _dataContext.ProcessedEnvelopes.ToListAsync();
            foreach (var procCountry in processed)
            {
                var param1 = new SqlParameter("@country", procCountry.Country);
                var param2 = new SqlParameter("@version", procCountry.Version);
                var param3 = new SqlParameter("@importdate", procCountry.ImportDate);

                var list = await _versioningContext.Set<Harvesting>().FromSqlRaw($"exec dbo.spGetPendingCountryVersion  @country, @version,@importdate",
                                param1, param2, param3).ToListAsync();
                if (list.Count > 0) result.AddRange(list);
            }
            return await Task.FromResult(result);
        }

        public async Task<int?> Harvest(EnvelopesToProcess[] envelopeIDs)
        {
            var changes = new List<SiteChangeDb>();
            var latestVersions = await _dataContext.ProcessedEnvelopes.ToListAsync();
            var numChanges = 0;
            await _dataContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.test_table");

            //from the view vLatestProcessedEnvelopes (backbonedb) load the sites with the latest versionid of the countries

            //Load all sites with the CountryVersionID-CountryCode from Versioning
            for (var i = 0; i < envelopeIDs.Length; i++)
            {
                var country = latestVersions.Where(v => v.Country == envelopeIDs[i].CountryCode).FirstOrDefault(); //Coger la ultima version de ese country
                var lastReferenceCountryVersion = 0;
                if (country != null)
                {
                    lastReferenceCountryVersion = country.Version;
                }
                var param1 = new SqlParameter("@country", envelopeIDs[i].CountryCode);
                var param2 = new SqlParameter("@version", envelopeIDs[i].VersionId);

                var sitesVersioning = await _versioningContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.spGetNaturaSiteDataByCountryIdAndCode  @country, @version",
                                param1, param2).ToListAsync();
                var referencedSites = new List<SiteToHarvest>();
                if (lastReferenceCountryVersion != 0)
                {
                    var param3 = new SqlParameter("@version", lastReferenceCountryVersion);
                    referencedSites = await _dataContext.Set<SiteToHarvest>().FromSqlRaw($"exec dbo.[spGetReferenceSitesByCountryAndVersion]  @country, @version",
                                param1, param3).ToListAsync();
                }


                //For each site in Versioning compare it with the that site in backboneDB
                foreach (var harvestingSite in sitesVersioning)
                {
                    var storedSite = referencedSites.Where(s => s.SiteCode == harvestingSite.SiteCode).FirstOrDefault();
                    if (storedSite != null)
                    {
                        if (harvestingSite.SiteName != storedSite.SiteName)
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.ChangeCategory = "Site General Info";
                            siteChange.ChangeType = "SiteName Changed";
                            siteChange.Country = envelopeIDs[i].CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            changes.Add(siteChange);
                            numChanges++;
                        }
                        //if (harvestingSite.SiteType != storedSite.SiteType)
                        //{
                        //    var siteChange = new SiteChangeDb();
                        //    siteChange.SiteCode = harvestingSite.SiteCode;
                        //    siteChange.ChangeCategory = "SiteType Changed";
                        //    siteChange.ChangeType = "SiteType Changed";
                        //    siteChange.Country = envelopeIDs[i].CountryCode;
                        //    siteChange.Level = Enumerations.Level.Critical;
                        //    siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                        //    siteChange.Tags = string.Empty;
                        //    changes.Add(siteChange);
                        //    numChanges++;
                        //}
                        if (harvestingSite.AreaHa > storedSite.AreaHa)
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.ChangeCategory = "Area Increased";
                            siteChange.ChangeType = "Area Increased";
                            siteChange.Country = envelopeIDs[i].CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            changes.Add(siteChange);
                            numChanges++;
                        }
                        if (harvestingSite.AreaHa < storedSite.AreaHa)
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.ChangeCategory = "Area Changed";
                            siteChange.ChangeType = "Area Changed";
                            siteChange.Country = envelopeIDs[i].CountryCode;
                            siteChange.Level = Enumerations.Level.Medium;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            changes.Add(siteChange);
                            numChanges++;
                        }
                        if (harvestingSite.LengthKm != storedSite.LengthKm)
                        {
                            var siteChange = new SiteChangeDb();
                            siteChange.SiteCode = harvestingSite.SiteCode;
                            siteChange.ChangeCategory = "Site General Info";
                            siteChange.ChangeType = "Length Changed";
                            siteChange.Country = envelopeIDs[i].CountryCode;
                            siteChange.Level = Enumerations.Level.Warning;
                            siteChange.Status = Enumerations.SiteChangeStatus.Pending;
                            siteChange.Tags = string.Empty;
                            changes.Add(siteChange);
                            numChanges++;
                        }
                    }
                    else
                    {
                        changes.Add(new SiteChangeDb
                        {
                            SiteCode = harvestingSite.SiteCode,
                            ChangeCategory = "Site Added",
                            ChangeType = "Site Added",
                            Country = envelopeIDs[i].CountryCode,
                            Level = Enumerations.Level.Critical,
                            Status = Enumerations.SiteChangeStatus.Pending,
                            Tags = string.Empty
                        });
                        numChanges++;
                    }
                }
                try
                {
                    _dataContext.SiteChanges.AddRange(changes);
                    _dataContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    var a = ex.Message;
                    throw;
                }
                //if there is a change load it to changes list
            }

            return numChanges;
        }

    }
}
