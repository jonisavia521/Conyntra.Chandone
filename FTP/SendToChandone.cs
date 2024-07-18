using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conyntra.Chandone.FTP
{
    public class SendToChandone
    {
        private readonly IConfiguration configuration;
        public SendToChandone(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void Send(string rutaArchivoOrigen)
        {
            try
            {
                string host = configuration["SFTP:Host"];
                string username = configuration["SFTP:Usuario"];
                string password = configuration["SFTP:Contraseña"];
                int port = int.Parse(configuration["SFTP:Puerto"]);

                string remoteDirectory = configuration["SFTP:Carpeta"];

                using (var client = new SftpClient(host, port, username, password))
                {
                    client.Connect();
                    if (client.IsConnected)
                    {
                        // Subir el archivo al servidor SFTP
                        using (var fileStream = new FileStream(rutaArchivoOrigen, FileMode.Open))
                        {
                            client.UploadFile(fileStream, Path.Combine(remoteDirectory, Path.GetFileName(rutaArchivoOrigen)));
                        }
                        client.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
