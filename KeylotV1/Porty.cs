using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Configuration;
namespace KeylotV1
{
    class Porty
    {
        public static SerialPort port; 
        public static void test()
        {
            port = new SerialPort(ConfigurationManager.AppSettings["serialPort"], 9600, Parity.None,8, StopBits.One);
            port.Open();
        }
        public static void sendMessage(string cmd, string L1, string L2)
        {
          
            var message = new content();
            message.cmd = cmd;
            message.data_line1 = L1;
            message.data_line2 = L2;
            var json = JsonConvert.SerializeObject(message);
           // MessageBox.Show(json);
            port.Write(json);
            
            
        }

        public static void sendRunningJob()
        {
            var message = new content();
            message.cmd = "set_display";
            message.data_line1 = "Job Running...";
            message.data_line2 = Api.currentJob;
            var json = JsonConvert.SerializeObject(message);
            // MessageBox.Show(json);
            port.Write(json);
        }
        public static void sendReady()
        {
            System.Threading.Thread.Sleep(2000);
            var message = new content();
            message.cmd = "set_display";
            message.data_line1 = "Ready.. Press";
            message.data_line2 = "Enter To Type ";
            var json = JsonConvert.SerializeObject(message);
            
            port.Write(json);
        }

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
        }

        public struct content
        {
            public string cmd;
            public string data_line1;
            public string data_line2;
        }
    }
}
