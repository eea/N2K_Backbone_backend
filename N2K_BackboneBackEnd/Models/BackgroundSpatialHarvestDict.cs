using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace N2K_BackboneBackEnd.Models
{
    public interface IBackgroundSpatialHarvestJobs
    {
        event EventHandler<FMEJobEventArgs> FMEJobCompleted;
        void CheckFMEJobsStatus(IOptions<ConfigSettings> appSettings);
        void LaunchFMESpatialHarvestBackground(EnvelopesToProcess envelope);

        N2KBackboneContext GetDataContext();
    }

    public class BackgroundSpatialHarvestJobs: IBackgroundSpatialHarvestJobs
    {
        public event EventHandler<FMEJobEventArgs> FMEJobCompleted;

        private readonly N2KBackboneContext _dataContext;
        private readonly IOptions<ConfigSettings> _appSettings;
        private ConcurrentDictionary<EnvelopesToProcess, long> _fmeJobs = new ConcurrentDictionary<EnvelopesToProcess, long>();
        private ConcurrentDictionary<string, long> _minCountryJobs = new ConcurrentDictionary<string, long>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        private List<HarvestedEnvelope> result = new List<HarvestedEnvelope> { };


        public BackgroundSpatialHarvestJobs(N2KBackboneContext dataContext,IOptions<ConfigSettings> appSettings)
        {
            _dataContext = dataContext;
            _appSettings = appSettings;
        }

        public N2KBackboneContext GetDataContext()
        {
            return _dataContext;
        }

        public void LaunchFMESpatialHarvestBackground(EnvelopesToProcess envelope)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }
            if (_fmeJobs.Count == 0) _signal.Release();
            
            HttpClient client = new HttpClient();
            try
            {
                //String serverUrl = String.Format(appSettings.Value.fme_service_spatialload, envelope.VersionId, envelope.CountryCode, appSettings.Value.fme_security_token);

                //prepare the parameters to be sent to FME Server
                Console.WriteLine("launching FME");
                SystemLog.write(SystemLog.errorLevel.Info, string.Format("Harvest spatial {0}-{1} started", envelope.CountryCode, envelope.VersionId), "HarvestSpatialData", "");
                Console.WriteLine(string.Format("Harvest spatial {0}-{1} started", envelope.CountryCode, envelope.VersionId));

                //TimeLog.setTimeStamp("Geodata for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString(), "Starting");
                client.Timeout = TimeSpan.FromHours(5);
                String url = String.Format("{0}/fmerest/v3/transformations/submit/{1}/{2}",
                   _appSettings.Value.fme_service_spatialload.server_url,
                   _appSettings.Value.fme_service_spatialload.repository,
                   _appSettings.Value.fme_service_spatialload.workspace);

                String body = string.Format(@"{{""publishedParameters"":[" +
                    @"{{""name"":""CountryVersionId"",""value"":{0}}}," +
                    @"{{""name"":""CountryCode"",""value"": ""{1}""}}]" +
                    @"}}", envelope.VersionId, envelope.CountryCode);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("fmetoken", "token=" + _appSettings.Value.fme_security_token);
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(body,
                                                    Encoding.UTF8,
                                                    "application/json");//CONTENT-TYPE header

                //call the FME script in async 
                var res = client.Send(request);
                //get the JobId 
                var json = res.Content.ReadAsStringAsync().Result;
                JObject jResponse = JObject.Parse(json);
                string jobId = jResponse.GetValue("id").ToString();

                _fmeJobs.TryAdd(envelope, long.Parse(jobId));
                //add the jobId if it is the first of the country
                if (!_minCountryJobs.ContainsKey(envelope.CountryCode)) 
                    _minCountryJobs.TryAdd(envelope.CountryCode, envelope.VersionId);
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

        public void CheckFMEJobsStatus(IOptions<ConfigSettings> appSettings) //, string connString)
        {
            foreach (var spatialHarvestjob in _fmeJobs)
            {
                long jobId = spatialHarvestjob.Value;

                //send a GET request to FME Server to check the status of the job
                //String serverUrl = String.Format(_appSettings.Value.fme_service_spatialload, envelope.VersionId, envelope.CountryCode, appSettings.Value.fme_security_token);

                //prepare the parameters to be sent to FME Server
                //SystemLog.write(SystemLog.errorLevel.Info, "Start harvest spatial", "HarvestSpatialData", "");
                //Console.WriteLine(string.Format("checking fme job {0}", jobId));
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromHours(5);

                String url = String.Format("{0}/fmerest/v3/transformations/jobs/id/{1}",
                   appSettings.Value.fme_service_spatialload.server_url,
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

                    CompleteTask(spatialHarvestjob.Key);
                    SystemLog.write(SystemLog.errorLevel.Info, string.Format("Harvest spatial {0}-{1} completed", spatialHarvestjob.Key.CountryCode, spatialHarvestjob.Key.VersionId), "HarvestSpatialData", "");
                }
                client.Dispose();
            }
        }

        public void CompleteTask(EnvelopesToProcess envelope)
        {
            _fmeJobs.TryRemove(envelope, out long jobId);
            OnFMEJobIdCompleted(envelope);
        }


        protected virtual void OnFMEJobIdCompleted(EnvelopesToProcess envelope)
        {
            bool firstInCountry = false;
            long minVersionCountry = 0;
            if (_minCountryJobs.ContainsKey(envelope.CountryCode))
            {
                minVersionCountry = _minCountryJobs[envelope.CountryCode];
                firstInCountry = envelope.VersionId == minVersionCountry;
            }

            FMEJobEventArgs evt = new FMEJobEventArgs {
                AllFinished = _fmeJobs.Count == 0,
                Envelope = envelope,
                FirstInCountry = firstInCountry
            };
            //remove country from _minCountryJob dictionary if the job is the latest of the country
            if (_fmeJobs.Where(j=> j.Key.CountryCode== envelope.CountryCode).ToList().Count == 0)
            {
                long jobId = 0; 
                _minCountryJobs.TryRemove(envelope.CountryCode,out jobId);
            }
            FMEJobCompleted?.Invoke(this, evt);
        }
    }
}
