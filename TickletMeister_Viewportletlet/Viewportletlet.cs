using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TickletMeister_Viewportletlet
{
    static class Viewportletlet
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Viewportletlet_Window());
            }
            catch (System.AccessViolationException e)
            {
                //do nothing/close
            }
        }
    }
}
