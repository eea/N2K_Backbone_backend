using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Enumerations;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace N2K_BackboneBackEnd.Models
{
    public class BackgroundSpatialHarvestJobs
    {
        private ConcurrentDictionary<EnvelopesToProcess, long> _fmeJobs = new ConcurrentDictionary<EnvelopesToProcess, long>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        private List<HarvestedEnvelope> result= new List<HarvestedEnvelope> { };
        public void LaunchFMESpatialHarvestBackground(EnvelopesToProcess envelope, IOptions<ConfigSettings> appSettings)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }
            if (_fmeJobs.Count == 0) _signal.Release(); 

            HttpClient client = new HttpClient();
            try
            {
                String serverUrl = String.Format(appSettings.Value.fme_service_spatialload, envelope.VersionId, envelope.CountryCode, appSettings.Value.fme_security_token);

                //prepare the parameters to be sent to FME Server
                Console.WriteLine("launching FME");
                SystemLog.write(SystemLog.errorLevel.Info, string.Format("Harvest spatial {0}-{1} started", envelope.CountryCode, envelope.VersionId), "HarvestSpatialData", "");
                Console.WriteLine(string.Format("Harvest spatial {0}-{1} started", envelope.CountryCode, envelope.VersionId));

                //TimeLog.setTimeStamp("Geodata for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString(), "Starting");
                client.Timeout = TimeSpan.FromHours(5);
                String url = String.Format("{0}/fmerest/v3/transformations/submit/{1}/{2}",
                   "https://fme.discomap.eea.europa.eu",
                   "N2KBackbone",
                   "test_notofocations.fmw");

                String body = string.Format(@"{{""publishedParameters"":[" +
                    @"{{""name"":""CountryVersionId"",""value"":{0}}}," +
                    @"{{""name"":""CountryCode"",""value"": ""{1}""}}]" +
                    @"}}", envelope.VersionId, envelope.CountryCode);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("fmetoken", "token=" + appSettings.Value.fme_security_token);
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(body,
                                                    Encoding.UTF8,
                                                    "application/json");//CONTENT-TYPE header

                //call the FME script in async 
                var res =  client.Send(request);
                //get the JobId 
                var json = res.Content.ReadAsStringAsync().Result;
                JObject jResponse = JObject.Parse(json);
                string jobId = jResponse.GetValue("id").ToString();

                _fmeJobs.TryAdd(envelope,long.Parse(jobId));
                Console.WriteLine(string.Format(@"JobId {0} launched", jobId));

            }
            catch (Exception ex)
            {
                SystemLog.write(SystemLog.errorLevel.Error, ex, "HarvestGeodata", "");

            }
            finally
            {
                client.Dispose();
                client = null;
            }
        }

        public void CheckFMEJobsStatus(IOptions<ConfigSettings> appSettings)
        {
            foreach (var spatialHarvestjob in _fmeJobs)
            {
                long jobId = spatialHarvestjob.Value;

                //send a GET request to FME Server to check the status of the job
                //String serverUrl = String.Format(_appSettings.Value.fme_service_spatialload, envelope.VersionId, envelope.CountryCode, appSettings.Value.fme_security_token);

                //prepare the parameters to be sent to FME Server
                //SystemLog.write(SystemLog.errorLevel.Info, "Start harvest spatial", "HarvestSpatialData", "");
                Console.WriteLine(string.Format("checking fme job {0}", jobId));
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromHours(5);

                String url = String.Format("{0}/fmerest/v3/transformations/jobs/id/{1}",
                   "https://fme.discomap.eea.europa.eu",
                   jobId);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("fmetoken", "token=" + appSettings.Value.fme_security_token);
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                //get the status of the fme job via rest api
                var res = client.Send(request);
                var json = res.Content.ReadAsStringAsync().Result;
                JObject jResponse = JObject.Parse(json);
                if (jResponse.GetValue("status").ToString() == "SUCCESS" || jResponse.GetValue("status").ToString() == "ERROR")
                {
                    result.Add(new HarvestedEnvelope
                            {
                                CountryCode = spatialHarvestjob.Key.CountryCode,
                                VersionId = spatialHarvestjob.Key.VersionId,
                                NumChanges = 0,
                                Status = SiteChangeStatus.Harvested
                            });
                    CompleteTask(spatialHarvestjob.Key);
                    SystemLog.write(SystemLog.errorLevel.Info, string.Format("Harvest spatial {0}-{1} completed", spatialHarvestjob.Key.CountryCode, spatialHarvestjob.Key.VersionId), "HarvestSpatialData", "");
                }
                client.Dispose();
            }
        }

        public void CompleteTask(EnvelopesToProcess envelope)
        {
            _fmeJobs.TryRemove(envelope, out long jobId);
        }

        public async Task<List<HarvestedEnvelope>> WaitUntillAllCompleted()
        {

            while (_fmeJobs.Count >0)
                await Task.Delay(20);

            await _signal.WaitAsync();
            return result;
        }             

    }
}
