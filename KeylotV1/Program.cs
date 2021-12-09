using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace KeylotV1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.File("log\\log.txt", rollingInterval: RollingInterval.Day)
                  .CreateLogger();
            Log.Information("Application Started.");
            Application.Run(new Form1());
        }
    }
}
