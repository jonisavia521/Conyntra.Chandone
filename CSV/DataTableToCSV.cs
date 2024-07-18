using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conyntra.Chandone.CSV
{
    public class DataTableToCSV
    {
        public void ConvertDataTableToCsv(DataTable dataTable,string filePath)
        {
            // Verificar si el DataTable contiene datos
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
               throw new Exception("El DataTable está vacío. No se generará un archivo CSV.");                
            }

            try
            {
                // Crear el StreamWriter para escribir en el archivo CSV
                using (StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false)))
                {
                    // Escribir los encabezados de las columnas
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        sw.Write($"{EscapeCsvField(dataTable.Columns[i].ColumnName)}");
                        if (i < dataTable.Columns.Count - 1)
                            sw.Write(";");
                    }
                    sw.WriteLine();


                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            sw.Write($"{EscapeCsvField(row[i].ToString())}");
                            if (i < dataTable.Columns.Count - 1)
                                sw.Write(";");
                        }
                        sw.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al escribir el archivo CSV: {ex.Message}");
            }
        }

        private string EscapeCsvField(string field)
        {
            // Si el campo contiene comillas dobles, las duplicamos para escaparlas
            if (field.Contains("\""))
                field = field.Replace("\"", "\"\"");

            // Si el campo contiene comas o saltos de línea, lo rodeamos con comillas dobles
            if (field.Contains(";") || field.Contains("\n") || field.Contains("\r"))
                field = $"\"{field}\"";

            return field;
        }
    }
}
