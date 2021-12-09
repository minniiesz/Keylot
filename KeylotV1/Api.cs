using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Configuration;
using Serilog; 

namespace KeylotV1
{
    public class Api
    {


        public async static void PostKeylot(string input)
        {
            var client = new HttpClient();
            var accessToken = await GetAccessToken(client);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            (string keylot, string status) = GetInput(input, out string err);
            if (err != null)
            {
                Log.Error(err); 

            }
            else
            {
                SendData(client, keylot, status);
                // MessageBox.Show(res.StatusCode.ToString());
            }
        }
        private static (string, string) GetInput(string input, out string err)
        {

            var data = new Data();
            try
            {
                var message = input.Split('/');
                if (!IsTrueFormatLot(message[0]))
                {
                    err = "Wrong Format Keylot.";
                    return (null, null);
                }
                if(message[1] != "1" && message[1] != "0")
                {
                    err = "Wrong Format Status.";
                    return (null, null);
                }
                if (message.Count() == 2)
                {
                    data.keylot = message[0];
                    data.status = message[1];
                    //Console.WriteLine("channel : " + data.channel + "  value : " + data.value + " status : " + data.status);
                    err = null;
                    return (data.keylot, data.status);
                }
                else
                {
                    err = "Wrong Input.";
                    return (null, null);
                }

            }
            catch
            {
                err = "Cannot split. ";


            }

            return (null, null);
        }
        private static bool IsTrueFormatLot(string keylot)
        {
            var split = keylot.Split('*');
            if(split.Count() != 3)
            {
                return false;
            }else if(split[0] == "" || split[1] == "" || split[2] == "")
            {
                return false;
            }

            return true; 
        }

        private static async void SendData(HttpClient client, string keylot, string status)
        {
            string uri = "https://api.iot.ifra.io/v1/teams/" + ConfigurationManager.AppSettings["organization"] + "/jobs";
            var message = new ContentSendData();
            message.lot = keylot;
            message.name = keylot;

            try
            {
                message.progress = Int32.Parse(status);
                message.openTicketCategoryId = Int32.Parse(ConfigurationManager.AppSettings["openTicketCategoryId"]);
            }
            catch (Exception err)
            {
                Log.Error(err.ToString());

            }

            message.publisher = ConfigurationManager.AppSettings["publisherId"];
            var json = JsonConvert.SerializeObject(message);
            HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(uri, data);
            if (response.IsSuccessStatusCode)
            {
                Log.Information("Status Code : "+response.StatusCode);
            }
            else
            {
                Log.Error("Status Code : " + response.StatusCode);
            }

        }
        private static async Task<string> GetAccessToken(HttpClient client)
        {
            string uri = "https://api.iot.ifra.io/v1/auth/login";

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


        public struct ContentGetToken
        {
            public string email;
            public string password;
        }
        public struct ContentSendData
        {
            public string lot;
            public string name;
            public string publisher;
            public int openTicketCategoryId;
            public int progress;
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
