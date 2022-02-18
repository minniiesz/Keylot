using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using Serilog;
using System.Windows.Forms;

namespace KeylotV1
{
    public class Api
    {

        public static string currentJob = "";
        public static bool IsNewJob = false;
        public static bool IsJobRunning = false;

        public async static void PostKeylot(string input)
        {
            var client = new HttpClient();
            var accessToken = await GetAccessToken(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            
            var jobprofileId = await filterJob(input,accessToken);
                if(jobprofileId == "")
                {
                    Log.Error("Can't found this job.");
                    Porty.sendReady();
                }
                else
                {
                   OpenTicket(client, jobprofileId, input);
                }

        }

        private static async void OpenTicket(HttpClient client, string jobProfileId, string keylot)
        {
            string uri = ConfigurationManager.AppSettings["apiOpenTicket"];
            var message = new ContentOpenTicket();
            message.machineId = Int32.Parse(ConfigurationManager.AppSettings["machineId"]);
            message.progress = 1;
            message.openTicket.categoryId = Int32.Parse(ConfigurationManager.AppSettings["categoryId"]);
            message.openTicket.userId = Int32.Parse(ConfigurationManager.AppSettings["userId"]);
            message.openTicket.JobProfileId = jobProfileId; 

            var json = JsonConvert.SerializeObject(message);
            //MessageBox.Show(json);
            HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(uri, data);
            if (response.IsSuccessStatusCode)
            {
                Log.Information("Running Job : "+keylot);
                currentJob = keylot;
                IsJobRunning = true;
                
            }
            else
            {
                Log.Error("Can't Open Ticket : "+ response.ReasonPhrase);
            }
            if (IsJobRunning)
            {
                Porty.sendRunningJob();
            }
            else
            {
                Porty.sendReady();
            }

        }
        public static async void CloseTicket()
        {
            string uri = ConfigurationManager.AppSettings["apiOpenTicket"]; 
            var client = new HttpClient();
            var accessToken = await GetAccessToken(client);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var message = new ContentCloseTicket();
            message.machineId = Int32.Parse(ConfigurationManager.AppSettings["machineId"]);
            message.progress = 5;
            message.problemReport.categoryId = Int32.Parse(ConfigurationManager.AppSettings["categoryId"]);

            var json = JsonConvert.SerializeObject(message);
            HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(uri, data);
            if (response.IsSuccessStatusCode)
            {
                Log.Information("Closed Job : "+currentJob);
                IsJobRunning = false;
               
            }
            else
            {
                Log.Error("Cant Close Job : "+ response.ReasonPhrase);
            }
            if (IsJobRunning)
            {
                Porty.sendRunningJob();
            }
            else
            {
                Porty.sendReady();
            }

        }

        private static async Task<string> GetAccessToken(HttpClient client)
        {
            string uri = ConfigurationManager.AppSettings["apiGetAccessToken"];

            var message = new ContentGetToken();
            message.email = ConfigurationManager.AppSettings["email"];
            message.password = ConfigurationManager.AppSettings["password"];
            var json = JsonConvert.SerializeObject(message);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(uri, data);
            var jsonString = await response.Content.ReadAsStringAsync();
            ResultToken result = new ResultToken();
            result = JsonConvert.DeserializeObject<ResultToken>(jsonString);
            //Console.WriteLine(result.Result.accessToken);
            return result.Result.accessToken;

        }

        public static async Task<string> getJobId(string accessToken)
        {
            string uri = "https://api.iot.ifra.io/v1/teams/"+ConfigurationManager.AppSettings["teamId"]+"/job-profiles"+"?offset=0&limit=-1";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            byte[] responded;

            HttpResponseMessage response = await client.GetAsync(uri);
        
            if (response.IsSuccessStatusCode)
            {
                response.Content.ReadAsByteArrayAsync().Wait();
                responded = response.Content.ReadAsByteArrayAsync().Result;
                var responseString = Encoding.UTF8.GetString(responded, 0, responded.Length);
                return responseString;
            }
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Cant Get Job Profile : " + response.ReasonPhrase);
            }
            return null;
        }

        public static async Task<string> filterJob(string input, string accessToken)
        {
            string data = await getJobId(accessToken);
            if(data == null)
            {
                return "";
            }
            RootJob objs = JsonConvert.DeserializeObject<RootJob>(data);
            foreach (var obj in objs.data)
            {
                if(obj.name == input)
                {
                    return obj.id; 
                }
            }
            return "";
        }

        public class data
        {
            public string id { get; set; }
            public string name { get; set; }

        }
        public class RootJob
        {
            public List<data> data { get; set; }
        }

        public struct ContentGetToken
        {
            public string email;
            public string password;
        }
        public struct ContentOpenTicket
        {
            public int machineId;
            public int progress;
            public openTicket openTicket;   
        }

        public struct ContentCloseTicket
        {
            public int machineId;
            public int progress;
            public problemReport problemReport;
        }
        public struct openTicket
        {
            public int userId;
            public int categoryId;
            public string JobProfileId;
        }

        public struct problemReport
        {
            public int categoryId;
        }

        public class ResultToken
        {
            public Result Result { get; set; }
        }
        public class Result
        {
            public string accessToken { get; set; }
            public string refreshToken { get; set; }
        }

        public struct Data
        {
            //public string channel;
            public string keylot;
            public string status;
        }
    }
}
