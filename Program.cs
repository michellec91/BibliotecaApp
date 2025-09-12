using System;
using System.Windows.Forms;
using BibliotecaApp.Services;

namespace BibliotecaApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var svc = new LibraryService();

            var dataDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            try
            {
                if (System.IO.Directory.Exists(dataDir))
                    svc.LoadAll(dataDir);
            }
            catch { }

            Application.Run(new MainForm(svc));

            try
            {
                svc.SaveAll(dataDir);
            }
            catch { }
        }
    }
}
