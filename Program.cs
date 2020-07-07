using System;
using System.IO;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace PDFSplit
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            var startup = new Startup();

            using(var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(startup.ConfigureHostConfiguration)
                .ConfigureAppConfiguration(startup.ConfigureAppConfiguration)
                .ConfigureLogging(startup.ConfigureLogging)
                .ConfigureServices(startup.ConfigureServices)
                .Build())
                {
                    await hostBuilder.RunAsync();
                };
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            Log.Error("UnhandledException caught : " + ex.Message);
            Log.Error("UnhandledException StackTrace : " + ex.StackTrace);
            Log.Fatal("Runtime terminating: {0}", e.IsTerminating);
        }
    }
}
