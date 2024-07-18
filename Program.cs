using Conyntra.Chandone.CSV;
using Conyntra.Chandone.Datos;
using Conyntra.Chandone.FTP;
using Serilog;
using Serilog.Events;

namespace Conyntra.Chandone
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Log", "SeriLog.txt"), LogEventLevel.Warning, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10)                
                .Enrich.FromLogContext()
                .CreateLogger();


            IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<Tango>();
                    services.AddSingleton<DataTableToCSV>();
                    services.AddSingleton<SendToChandone>();
                })
                .Build();

            host.Run();
        }


    }
}