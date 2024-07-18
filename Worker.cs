using Conyntra.Chandone.CSV;
using Conyntra.Chandone.Datos;
using Conyntra.Chandone.FTP;
using System.IO;
using System.Reflection;
using System.Text;

namespace Conyntra.Chandone
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Tango _datos;
        private DataTableToCSV _convert;
        private SendToChandone _ftp;
        private readonly IConfiguration configuration;
        private readonly string _ruta;
        private readonly string _rutaLog;
        private string _codigoDistribuidor;
        private bool ExportarAhoraUnaSolaVez;
        public Worker(ILogger<Worker> logger,Tango datos,DataTableToCSV convert,SendToChandone ftp, IConfiguration configuration)
        {
            _logger = logger;
            _datos = datos;
            _convert = convert;
            _ftp = ftp;
            this.configuration = configuration;
            _ruta = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CSV";
            Directory.CreateDirectory(_ruta);
            _codigoDistribuidor = configuration["Parametros:CodigoDistribuidor"] ?? "";
            ExportarAhoraUnaSolaVez = configuration.GetValue<bool>("Parametros:ExportarAhoraUnaSolaVez");

            _rutaLog = Path.Combine(AppContext.BaseDirectory, "Log");
            Directory.CreateDirectory(_rutaLog);
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!ExportarAhoraUnaSolaVez)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    // Calcula la próxima ejecución a las 18:00 o 23:00, según la hora actual
                    var fecha = DateTime.Now;
                    DateTime nextRunTime = (fecha.Hour < 18) ?
                        new DateTime(fecha.Year, fecha.Month, fecha.Day, 18, 0, 0) :
                        new DateTime(fecha.Year, fecha.Month, fecha.Day, 23, 0, 0);

                    // Calcula el tiempo hasta la próxima ejecución
                    TimeSpan delayUntilNextRun = nextRunTime - fecha;

                    using (TextWriter text = TextWriter.Synchronized(new StreamWriter(Path.Combine(_rutaLog, $"MonitoringLog {DateTime.Now.ToString("yyyyMMdd")}.txt"), true, Encoding.ASCII)))
                    {
                        text.WriteLine($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : Empezo a las: {fecha}\n    La proxima ejecucion es a las: {nextRunTime}\n   Tiempo faltante para su ejecucion:{delayUntilNextRun}");
                    }

                    // Espera hasta la próxima ejecución
                    await Task.Delay(delayUntilNextRun, stoppingToken);
                }
                ExportarAhoraUnaSolaVez = false;
                var ok = false;
                while (!ok) {
                    var rutaDestino = "";
                    try
                    {
                        using (TextWriter text = TextWriter.Synchronized(new StreamWriter(Path.Combine(_rutaLog, $"MonitoringLog {DateTime.Now.ToString("yyyyMMdd")}.txt"), true, Encoding.ASCII)))
                        {
                            text.WriteLine($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : ARRANCO PROCESO A LAS: {DateTime.Now}");
                        }

                        rutaDestino = Path.Combine(_ruta, $"SO_{_codigoDistribuidor}_{DateTime.Now.ToString("yyyyMMdd")}.CSV");
                        _convert.ConvertDataTableToCsv(_datos.Ventas(_codigoDistribuidor), rutaDestino);
                        _ftp.Send(rutaDestino);

                        rutaDestino = Path.Combine(_ruta, $"ST_{_codigoDistribuidor}_{DateTime.Now.ToString("yyyyMMdd")}.CSV");
                        _convert.ConvertDataTableToCsv(_datos.Stock(_codigoDistribuidor), rutaDestino);
                        _ftp.Send(rutaDestino);

                        rutaDestino = Path.Combine(_ruta, $"VE_{_codigoDistribuidor}_{DateTime.Now.ToString("yyyyMMdd")}.CSV");
                        _convert.ConvertDataTableToCsv(_datos.Vendedores(_codigoDistribuidor), rutaDestino);
                        _ftp.Send(rutaDestino);
                        ok = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Ruta: {rutaDestino}");
                        using (TextWriter text = TextWriter.Synchronized(new StreamWriter(Path.Combine(_rutaLog, $"Log {DateTime.Now.ToString("yyyyMMdd")}.txt"), true, Encoding.ASCII)))
                        {
                            text.WriteLine($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : Exception: {ex.Message}");
                        }
                        await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                    }
                }
            }
        }
    }
}
