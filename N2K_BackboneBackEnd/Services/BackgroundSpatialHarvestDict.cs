using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
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
        private List<HarvestedEnvelope> result = new() { };
        //private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(initialCount:1);

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

        public async Task LaunchFMESpatialHarvestBackground(EnvelopesToProcess envelope, int countryMinVersion)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }
            //if (_fmeJobs.Count == 0) _signal.Release();

            HttpClient client = new();
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

                HttpRequestMessage request = new(HttpMethod.Post, url)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")//CONTENT-TYPE header
                };

                //call the FME script in async 
                var res = await client.SendAsync(request);
                //get the JobId 
                var json = await res.Content.ReadAsStringAsync();
                var response_dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);                
                string jobId = response_dict["id"];

                //create a text file to control the FME Jobs (Countr-Version) launched
                var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Resources",
                            string.Format("FMELaunched-{0}-{1}.txt", envelope.CountryCode, envelope.VersionId));

                //check if there are FME jobs launched for the country
                //if there are no such jobs, this FME job is the first
                bool firstInCountry = envelope.VersionId== countryMinVersion;


                //if the file exists means that the event was handled and we ignore it
                if (!File.Exists(fileName))
                {
                    //if it doesn´t exist create a file saying if it is the first of the country or not
                    StreamWriter sw = new(fileName, true, Encoding.ASCII);
                    await sw.WriteAsync(firstInCountry.ToString());
                    //await sw.WriteAsync(jobId);
                    //close the file
                    sw.Close();
                }
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

        /*
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
        */

        public async Task CompleteTask(EnvelopesToProcess envelope)
        {
            await Task.Delay(1);
            //await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Complete Task {0}-{1}", envelope.CountryCode, envelope.VersionId), "Complete task", "", _dataContext.Database.GetConnectionString());
            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("Complete Task with fme job {0}-{1}", envelope.CountryCode, envelope.VersionId), "Complete task", "", _dataContext.Database.GetConnectionString());
            await OnFMEJobIdCompleted(envelope);
        }

        protected async virtual Task OnFMEJobIdCompleted(EnvelopesToProcess envelope)
        {
            //await _semaphore.WaitAsync();
            await Task.Delay(100);
            bool firstInCountry = false;
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Resources",
                        string.Format("FMELaunched-{0}-{1}.txt", envelope.CountryCode, envelope.VersionId));

            //process the message only if it has been sent from N2kBackbone
            if (!File.Exists(fileName)) return;

            //read the file. The first line shows if the job is the first of the country
            string line1 = File.ReadLines(fileName).First();
            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("OnFMEJobIdCompleted with fme job {0}-{1}: {2}", envelope.CountryCode, envelope.VersionId,line1), "OnFMEJobIdCompleted", "", _dataContext.Database.GetConnectionString());
            //firstInCountry = bool.Parse(line1);
            firstInCountry = line1.Trim().ToLower() == "true";

            /*
            //fetch the FME jobs launched for the present country
            var fmeFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Resources"),
                string.Format("FMELaunched-{0}-*.txt", envelope.CountryCode));

            //await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Join(",", fmeFiles) , "OnFMEJobIdCompleted", "", _dataContext.Database.GetConnectionString());
            //and from these get the one with the minimun version
            foreach (var file in fmeFiles)
            {
                string _aux = file.Split(".txt")[0];
                long _currVersion = 0;
                if (long.TryParse(_aux.Replace(string.Format(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "FMELaunched-{0}-"), envelope.CountryCode), ""), out _currVersion))
                {
                    if (_currVersion < minVersionCountry) minVersionCountry = _currVersion;
                }
            }

            firstInCountry = envelope.VersionId == minVersionCountry;
            */
            //remove the file as the FME job has completed
            //await SystemLog.WriteAsync(SystemLog.errorLevel.Info, fileName, "OnFMEJobIdCompleted", "", _dataContext.Database.GetConnectionString());

            if (File.Exists(fileName))
            {
                //await SystemLog.WriteAsync(SystemLog.errorLevel.Info, fileName + " Deleted ", "OnFMEJobIdCompleted", "", _dataContext.Database.GetConnectionString());
                File.Delete(fileName);
            }
            
            FMEJobEventArgs evt = new()
            {
                Envelope = envelope,
                FirstInCountry = firstInCountry
            };
            await SystemLog.WriteAsync(SystemLog.errorLevel.Info, string.Format("OnFMEJobIdCompleted with fme job {0}-{1}", envelope.CountryCode, envelope.VersionId), "OnFMEJobIdCompleted", "", _dataContext.Database.GetConnectionString());
            FMEJobCompleted?.Invoke(this, evt);
            //_semaphore.Release();
        }
    }
}
