using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace N2K_BackboneBackEnd.Services
{
    public class BackgroundSpatialHarvestJobs : IBackgroundSpatialHarvestJobs
    {
        public event EventHandler<FMEJobEventArgs> FMEJobCompleted;

        private readonly N2KBackboneContext _dataContext;
        private readonly ILogger<BackgroundSpatialHarvestJobs> _logger;
        private readonly IOptions<ConfigSettings> _appSettings;
        private ConcurrentDictionary<long,EnvelopesToProcess> _fmeJobs = new ConcurrentDictionary< long, EnvelopesToProcess>();
        private ConcurrentDictionary<string, long> _minCountryJobs = new ConcurrentDictionary<string, long>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        private List<HarvestedEnvelope> result = new List<HarvestedEnvelope> { };
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(initialCount:1);


        public BackgroundSpatialHarvestJobs(N2KBackboneContext dataContext, IOptions<ConfigSettings> appSettings, 
            ILogger<BackgroundSpatialHarvestJobs> logger)
        {
            _dataContext = dataContext;
            _appSettings = appSettings;
            _logger = logger;
        }

        public N2KBackboneContext GetDataContext()
        {
            return _dataContext;
        }

        public async Task LaunchFMESpatialHarvestBackground(EnvelopesToProcess envelope)
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
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Harvest spatial {0}-{1} started", envelope.CountryCode, envelope.VersionId), "HarvestSpatialData", "", _dataContext.Database.GetConnectionString());
                Console.WriteLine(string.Format("Harvest spatial {0}-{1} started", envelope.CountryCode, envelope.VersionId));

                //TimeLog.setTimeStamp("Geodata for site " + envelope.CountryCode + " - " + envelope.VersionId.ToString(), "Starting");
                client.Timeout = TimeSpan.FromHours(5);
                string url = string.Format("{0}/fmerest/v3/transformations/submit/{1}/{2}",
                   _appSettings.Value.fme_service_spatialload.server_url,
                   _appSettings.Value.fme_service_spatialload.repository,
                   _appSettings.Value.fme_service_spatialload.workspace);

                
                string body = string.Format(@"{{""publishedParameters"":[" +
                    @"{{""name"":""CountryVersionId"",""value"":{0}}}," +
                    @"{{""name"":""Environment"",""value"":{1}}}," +
                    @"{{""name"":""CountryCode"",""value"": ""{2}""}}]" +
                    @"}}", envelope.VersionId, _appSettings.Value.Environment, envelope.CountryCode);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("fmetoken", "token=" + _appSettings.Value.fme_security_token);
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(body,
                                                    Encoding.UTF8,
                                                    "application/json");//CONTENT-TYPE header

                //call the FME script in async 
                var res = await client.SendAsync(request);
                //get the JobId 
                var json = await res.Content.ReadAsStringAsync();
                JObject jResponse = JObject.Parse(json);
                string jobId = jResponse.GetValue("id").ToString();

                _fmeJobs.TryAdd(long.Parse(jobId), envelope);
                //add the jobId if it is the first of the country
                if (!_minCountryJobs.ContainsKey(envelope.CountryCode))
                    _minCountryJobs.TryAdd(envelope.CountryCode, envelope.VersionId);
                //Console.WriteLine(string.Format(@"JobId {0} launched", jobId));
                //await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format(@"JobId {0} launched", jobId), "HarvestGeodata", "", _dataContext.Database.GetConnectionString());
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "HarvestGeodata", "", _dataContext.Database.GetConnectionString());

            }
            finally
            {
                client.Dispose();
                client = null;
            }
        }

        public async Task CheckFMEJobsStatus(IOptions<ConfigSettings> appSettings) //, string connString)
        {
            foreach (var spatialHarvestjob in _fmeJobs)
            {
                long jobId = spatialHarvestjob.Key;

                //send a GET request to FME Server to check the status of the job
                //String serverUrl = String.Format(_appSettings.Value.fme_service_spatialload, envelope.VersionId, envelope.CountryCode, appSettings.Value.fme_security_token);

                //prepare the parameters to be sent to FME Server
                //SystemLog.write(SystemLog.errorLevel.Info, "Start harvest spatial", "HarvestSpatialData", "");
                //Console.WriteLine(string.Format("checking fme job {0}", jobId));
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromHours(5);

                string url = string.Format("{0}/fmerest/v3/transformations/jobs/id/{1}",
                   appSettings.Value.fme_service_spatialload.server_url,
                   jobId);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("fmetoken", "token=" + appSettings.Value.fme_security_token);
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                //get the status of the fme job via rest api
                var res = await client.SendAsync(request);
                var json =await res.Content.ReadAsStringAsync();
                JObject jResponse = JObject.Parse(json);
                if (jResponse.GetValue("status").ToString() == "SUCCESS" || jResponse.GetValue("status").ToString() == "ERROR")
                {

                    await CompleteTask(spatialHarvestjob.Value);
                    await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Harvest spatial {0}-{1} completed", spatialHarvestjob.Value.CountryCode, spatialHarvestjob.Value.VersionId), "HarvestSpatialData", "", _dataContext.Database.GetConnectionString());
                }
                client.Dispose();
            }
        }

        public async Task CompleteTask(EnvelopesToProcess envelope)
        {
            
            await Task.Delay(1);
            EnvelopesToProcess _outEnv;
            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Complete Task {0}-{1}", envelope.CountryCode, envelope.VersionId), "Complete task", "", _dataContext.Database.GetConnectionString());
            _fmeJobs.TryRemove(envelope.JobId, out _outEnv);
            if (_outEnv != null)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Complete Task with fme job {0}-{1}", envelope.CountryCode, envelope.VersionId), "Complete task", "", _dataContext.Database.GetConnectionString());
                await OnFMEJobIdCompleted(envelope);
            }
        }


        protected async virtual Task OnFMEJobIdCompleted(EnvelopesToProcess envelope)
        {
            await _semaphore.WaitAsync();

            bool firstInCountry = false;
            long minVersionCountry = 0;
            if (_minCountryJobs.ContainsKey(envelope.CountryCode))
            {
                minVersionCountry = _minCountryJobs[envelope.CountryCode];
                firstInCountry = envelope.VersionId == minVersionCountry;
            }
            FMEJobEventArgs evt = new FMEJobEventArgs
            {
                AllFinished = _fmeJobs.Count == 0,
                Envelope = envelope,
                FirstInCountry = firstInCountry
            };
            //remove country from _minCountryJob dictionary if the job is the latest of the country
            if (_fmeJobs.Where(j => j.Value.CountryCode == envelope.CountryCode).ToList().Count == 0)
            {
                long jobId = 0;
                _minCountryJobs.TryRemove(envelope.CountryCode, out jobId);
            }
            FMEJobCompleted?.Invoke(this, evt);
            _semaphore.Release();

        }

    }
}
