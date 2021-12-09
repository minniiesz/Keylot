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
        private bool allowshowdisplay = false;
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }
        public static string input = ""; 
        public Form1()
        {
            InitializeComponent();
            SetVisibleCore(allowshowdisplay);
           
            _globalKeyboardHook = new GlobalKeyboardHook(new Keys[] { Keys.Divide, Keys.Multiply,Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5
                                   ,Keys.NumPad6,Keys.NumPad7,Keys.NumPad9,Keys.Return,Keys.Subtract,Keys.Decimal});

            // Hooks into all keys.
            //_globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
            //PostKeylot();
            

        }

        private GlobalKeyboardHook _globalKeyboardHook;

        /*        private void buttonHook_Click(object sender, EventArgs e)
                {
                    // Hooks only into specified Keys (here "A" and "B").
                    _globalKeyboardHook = new GlobalKeyboardHook(new Keys[] { Keys.A, Keys.B });

                    // Hooks into all keys.
                    _globalKeyboardHook = new GlobalKeyboardHook();
                    _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
                }*/
        //bool isOpen = false;
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private  void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            // EDT: No need to filter for VkSnapshot anymore. This now gets handled
            // through the constructor of GlobalKeyboardHook(...).
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                // Now you can access both, the key and virtual code
                Keys loggedKey = e.KeyboardData.Key;

                var text = loggedKey.ToString().ToLower();
                var key = CheckKey(text);
                if (text == "return")
                {
                        Api.PostKeylot(input);
                        input = "";
                        
                }else if (text == "subtract")
                {
                    input = "";
                }
                else
                {

                        input += key;
                }

                
                
            }
            
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
