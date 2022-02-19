using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
    public partial class Form1 : Form
    {
        private bool checkEnter = true;
        private bool allowshowdisplay = false;
        public static Timer timer1 = new Timer
        {
            Interval = 10000,
            Enabled = false
        };        
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }
        public static string input = ""; 
        public Form1()
        {
            timer1.Tick += Timer1_Tick;
            InitializeComponent();
            SetVisibleCore(allowshowdisplay);

            Subscribe.subscribeValue();
            _globalKeyboardHook = new GlobalKeyboardHook(new Keys[] { Keys.Divide, Keys.Multiply,Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5
                                   ,Keys.NumPad6,Keys.NumPad7,Keys.NumPad8,Keys.NumPad9,Keys.Return,Keys.Subtract,Keys.Decimal});
            Porty.test();
            Porty.sendReady();
            // Hooks into all keys.
            //_globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
            //PostKeylot();
            
           

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (Api.IsJobRunning)
            {
                Porty.sendRunningJob();
            }
            else
            {
                Porty.sendReady();
            }
            
            checkEnter = true;
            input = "";
            timer1.Enabled = false;
        }

        private GlobalKeyboardHook _globalKeyboardHook;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            
            // EDT: No need to filter for VkSnapshot anymore. This now gets handled
            // through the constructor of GlobalKeyboardHook(...).
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                if (!checkEnter)
                {
                    timer1.Enabled = false;
                    countReady();
                }
                // Now you can access both, the key and virtual code
                Keys loggedKey = e.KeyboardData.Key;
                var text = loggedKey.ToString().ToLower();
                
                var key = CheckKey(text);
                if (text == "return")

                {     
                    if (checkEnter)
                    {
                        checkEnter = false;
                        Porty.sendMessage("set_display", "Key Job Name...", "Then Press Enter");
                        countReady();
                    }
                    else
                    {

                        if(input == "9999")
                        { 
                            if (Api.IsJobRunning)
                            {
                                Api.CloseTicket();
                            }
                            else
                            {
                                Porty.sendReady();
                            }
                        }
                        else 
                        {
                            if (!Api.IsJobRunning)
                            {


                                Api.PostKeylot(input); //open ticket   
                            }
                            else
                            {
                                Porty.sendRunningJob();
                            }
                        }
                        
                        checkEnter = true;
                        timer1.Enabled = false;
                    }
                        input = "";
                        
                }else if (text == "subtract")
                {
                    input = "";
                }
                else
                {
                    if (!checkEnter)
                    {
                        input += key;
                    }

                }


                
                
            }
            
        }
        private void countReady()
        {
            timer1.Enabled = true;
        }

        private  string CheckKey(string key)
        {
            if (key.Contains("numpad"))
            {
                return "" + key[6];
            }
            else if (key == "multiply")
            {
                return "*";
            }
            else if (key == "divide")
            { 
                return "/";
            }
            else if(key == "decimal")
            {
                return "."; 
            }
            else
            {
                return "";
            }


        }
        

    }
}


