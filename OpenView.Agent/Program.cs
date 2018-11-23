using log4net;
using System;
using System.Threading;
using System.Windows.Forms;

namespace OpenView.Agent
{
    static class Program
    {
        static ILog _logger;

        [STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            _logger = LogManager.GetLogger("Program");

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            _logger.Fatal("Unhandled thread exception", e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal("Unhandled process exception", e.ExceptionObject as Exception);
        }
    }
}
