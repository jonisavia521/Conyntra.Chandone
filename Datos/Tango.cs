using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conyntra.Chandone.Datos
{
    public class Tango
    {
        private IConfiguration _configuration;
        private ILogger<Tango> _logger;
        public Tango(IConfiguration configuration, ILogger<Tango> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        internal DataTable Stock(string codigoDistribuidor)
        {
            DataTable dt = new DataTable();
            using (var cnn = new SqlConnection(_configuration.GetConnectionString("db")))
            {
                cnn.Open();

                var sql = $@"DECLARE @IDFOLDER int = (select IDFOLDER from STA11FLD where DESCRIP like '%CHANDON%');

SELECT  @codigoDistribuidor as COD_DISTRIBUIDOR, 
STA19.COD_ARTICU as MATERIAL,
CAST(SUM(STA19.CANT_STOCK) as int) as CANTIDAD,
FORMAT(getdate(),'yyyyMM') as PERIODO
FROM STA19
INNER JOIN sta11itc on sta11itc.CODE=STA19.COD_ARTICU and sta11itc.IDFOLDER=@IDFOLDER
WHERE STA19.COD_DEPOSI in ('02','08','11') 
GROUP BY STA19.COD_ARTICU
order by 2 ";

                using (var com = cnn.CreateCommand())
                {
                    com.Connection = cnn;
                    com.CommandText = sql;
                    com.CommandTimeout = 0;
                    com.Parameters.Add("@codigoDistribuidor", SqlDbType.VarChar).Value = codigoDistribuidor;
                    using (var rd = com.ExecuteReader())
                    {
                        dt.Load(rd);
                    }
                }


            }
            return dt;
        }

        internal DataTable Ventas(string codigoDistribuidor)
        {
            DataTable dt = new DataTable();
            using (var cnn = new SqlConnection(_configuration.GetConnectionString("db")))
            {
                cnn.Open();
                //GVA53.IMP_NETO_P*100/GVA12.IMPORTE_GR = porcentaje que representa por items segun el IMP_NETO_P: / 100 para tenerlo en decimal
                //GVA12.IMPORTE-GVA12.IMPORTE_GR = importe total de todos los impuestos
                //SUM(GVA53.IMP_NETO_P) = GVA12.IMPORTE_GR
                var sql = $@"DECLARE @IDFOLDER int = (select IDFOLDER from STA11FLD where DESCRIP like '%CHANDON%');

                            Select @codigoDistribuidor as COD_DISTRIBUIDOR, 
                            FORMAT(GVA12.FECHA_EMIS, 'yyyyMMdd') as FECHA,
                            GVA12.COD_CLIENT as CLIENTE,
                            GVA14.RAZON_SOCI as RAZON_SOCIAL,
                            isnull(GVA14.NOM_COM,'') as NOMBRE_FANTASIA,
                            GVA14.DOMICILIO as DIRECCION,
                            GVA14.C_POSTAL as COD_POSTAL,
                            GVA14.LOCALIDAD,
                            GVA18.NOMBRE_PRO as PROVINCIA,
                            gva53.COD_ARTICU as MATERIAL,
                            cast(GVA53.CANTIDAD as int) as CANTIDAD,
                            ROUND( (GVA53.IMP_NETO_P+((GVA53.IMP_NETO_P*100/GVA12.IMPORTE_GR/100) * (GVA12.IMPORTE-GVA12.IMPORTE_GR))) / GVA53.CANTIDAD * iif(gva15.TIPO_COMP='C',-1,1) , 2) as PRECIO,
                            GVA14.COD_RUBRO collate Modern_Spanish_CI_AI + ' | ' + isnull(GVA151.DESC_RUBRO,'') as RUBRO,
                            GVA14.CUIT,
                            GVA12.COD_VENDED as COD_VENDEDOR
                            from GVA12 
                            INNER JOIN GVA14 on GVA12.COD_CLIENT=GVA14.COD_CLIENT
                            INNER JOIN GVA18 ON GVA14.COD_PROVIN=GVA18.COD_PROVIN 
                            INNER JOIN GVA53 ON GVA12.N_COMP=GVA53.N_COMP AND GVA12.T_COMP=GVA53.T_COMP 
                            INNER JOIN GVA151 on GVA14.COD_RUBRO=GVA151.COD_RUBRO
                            INNER JOIN gva15 on gva15.IDENT_COMP=GVA12.T_COMP
                            INNER JOIN sta11itc on sta11itc.CODE=GVA53.COD_ARTICU and sta11itc.IDFOLDER=@IDFOLDER
                            WHERE GVA12.FECHA_EMIS BETWEEN DATEADD(DAY, 1, EOMONTH(GETDATE(), -1)) AND cast(GETDATE() as date) 
                            ORDER BY GVA12.FECHA_EMIS,GVA12.COD_CLIENT,gva53.COD_ARTICU"; 

                using (var com = cnn.CreateCommand())
                {
                    com.Connection = cnn;
                    com.CommandText = sql;
                    com.CommandTimeout = 0;
                    com.Parameters.Add("@codigoDistribuidor", SqlDbType.VarChar).Value = codigoDistribuidor;
                    using (var rd = com.ExecuteReader())
                    {
                        dt.Load(rd);
                    }
                }


            }
            return dt;
        }

        internal DataTable Vendedores(string codigoDistribuidor)
        {
            DataTable dt = new DataTable();
            using (var cnn = new SqlConnection(_configuration.GetConnectionString("db")))
            {
                cnn.Open();

                var sql = $@" select @codigoDistribuidor as COD_DISTRIBUIDOR, 
NOMBRE_VEN as VENDEDOR,
COD_VENDED as COD_VENDEDOR
from GVA23
order by 2";

                using (var com = cnn.CreateCommand())
                {
                    com.Connection = cnn;
                    com.CommandText = sql;
                    com.CommandTimeout = 0;
                    com.Parameters.Add("@codigoDistribuidor", SqlDbType.VarChar).Value = codigoDistribuidor;
                    using (var rd = com.ExecuteReader())
                    {
                        dt.Load(rd);
                    }
                }


            }
            return dt;
        }
    }
}
