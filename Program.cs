using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAMPChatlogArchiver
{
    static class Program
    {
        public static bool launchOnStartup { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            launchOnStartup = args != null && args.Any(arg => arg.Equals("startup", StringComparison.CurrentCultureIgnoreCase));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ControlWindow());
        }
    }
}
