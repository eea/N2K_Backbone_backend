using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace N2K_BackboneBackEnd.Services
{
    public class FMELongRunningService : BackgroundService
    {
        private readonly BackgroundSpatialHarvestJobs fme_jobs;
        private readonly IOptions<ConfigSettings> _appSettings;

        public FMELongRunningService(BackgroundSpatialHarvestJobs jobs, IOptions<ConfigSettings> appSettings)
        {
            this.fme_jobs = jobs;
            _appSettings = appSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //check every 20seconds if fme jobs have been completed
                await Task.Delay(20000);
                foreach (var spatialHarvestjob in fme_jobs)
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

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("fmetoken", "token=" + _appSettings.Value.fme_security_token);
                    client.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    //get the status of the fme job via rest api
                    var res = client.Send(request);
                    var json = res.Content.ReadAsStringAsync().Result;
                    JObject jResponse = JObject.Parse(json);
                    if (jResponse.GetValue("status").ToString()=="SUCCESS" || jResponse.GetValue("status").ToString()=="ERROR")
                    {
                        fme_jobs.CompleteTask(spatialHarvestjob.Key);
                        SystemLog.write(SystemLog.errorLevel.Info,string.Format("Harvest spatial {0}-{1} completed", spatialHarvestjob.Key.CountryCode, spatialHarvestjob.Key.VersionId) , "HarvestSpatialData", "");
                    }
                    client.Dispose();
                }
            }
        }
    }
}