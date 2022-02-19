using System;
//using Newtonsoft.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;

namespace KeylotV1
{
    class Subscribe
    {
        static readonly CancellationTokenSource Cts = new CancellationTokenSource();
        public static async Task subscribeValue()
        {
            bool isFirst = true;
            DateTime lastTime = new DateTime();
            MqttFactory mqttFactory = new MqttFactory();
            IMqttClient client = mqttFactory.CreateMqttClient();
            var option = new MqttClientOptionsBuilder()
                                .WithClientId(Guid.NewGuid().ToString())
                                .WithTcpServer("127.0.0.1", 1883)
                                .WithCleanSession()
                                .Build();
            client.UseConnectedHandler(async e =>
            {
                var topicFilter = new TopicFilterBuilder()
                                        .WithTopic("test")
                                        .Build();
                await client.SubscribeAsync(topicFilter);
            });

            client.UseDisconnectedHandler(e =>
            {
                MessageBox.Show("disconnected");
            });

            client.UseApplicationMessageReceivedHandler(e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);


                if (payload == "0")
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        lastTime = DateTime.Now;
                    }
                    var minDuration = (DateTime.Now - lastTime).TotalMinutes;

                    if (minDuration > Int32.Parse(ConfigurationManager.AppSettings["minToCloseTicket"])) 
                    {
                        //close job here
                        if (Api.IsJobRunning)
                        {
                            Api.CloseTicket();
                            isFirst = true; 
                        }
                       
                    }
                   
                }

                if (payload == "1")
                {
                    isFirst = true;
                    //check IsJobClosed ?
                    // if running -> end
                    if (Api.IsJobRunning)
                    {
                       
                    }
                    // if closed -> check IsNewJob("มีการ key lot เข้ามาใหม่") ถ้ามีให้open new job ถ้าไม่ให้รัน job เดิม
                    else
                    {
                            if (!Api.IsJobRunning)
                            {
                                Api.PostKeylot(Api.currentJob);
                            }
                    }

                }

                if (payload != "1" && payload != "0")
                {
                    
                }

            });
            await client.ConnectAsync(option);

            await Task.Delay(Timeout.Infinite, Cts.Token).ConfigureAwait(false);

            // await client.DisconnectAsync();
        }

    }
}

