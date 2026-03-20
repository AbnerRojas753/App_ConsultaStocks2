using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using App_ConsultaStocks.Datos;
using Xamarin.Forms;

namespace App_ConsultaStocks.Helpers
{
     public class ScanLookupResult
    {
        /// <summary>Indica si la operación fue exitosa (se encontró exactamente 1 producto).</summary>
        public bool Exito { get; set; }
        /// <summary>Mensaje de estado en español (puede contener texto de error o información para mostrar).</summary>
        public string Mensaje { get; set; }
        /// <summary>Texto crudo escaneado (raw).</summary>
        public string Crudo { get; set; }
        /// <summary>Código EAN extraído (si aplica).</summary>
        public string Ean { get; set; }
        /// <summary>Lote extraído (si aplica).</summary>
        public string Lote { get; set; }
        /// <summary>Cantidad de filas devueltas por el SP (0, 1, >1).</summary>
        public int Filas { get; set; }
        /// <summary>ID del artículo encontrado (cuando Filas == 1).</summary>
        public string IdArticulo { get; set; }
        /// <summary>Nombre/descripción del producto encontrado (cuando Filas == 1).</summary>
        public string NombreArticulo { get; set; }
        /// <summary>DataTable con los datos crudos devueltos por el stored procedure.</summary>
        public System.Data.DataTable RawData { get; set; }
        /// <summary>Alias de Lote con nombre IdLote (compatibilidad con vistas existentes).</summary>
        public string IdLote { get => Lote; set => Lote = value; }
    }

    public static class BarCodeHelper
    {
        /// <summary>
        /// Parsea el código escaneado (soporta formatos: "ean", "ean|lote" y "|ean|lote")
        /// y consulta el stored procedure <c>usp_BuscarProductos_BarCode</c> según el centro (idCd).
        /// Devuelve un <see cref="ScanLookupResult"/> con información para la vista.
        /// </summary>
        public static async Task<ScanLookupResult> ProcesarCodigoProductoRefac(string scannedCode, int idCd)
        {
            return await Task.Run(() =>
            {
                var result = new ScanLookupResult { Crudo = scannedCode };

                try
                {
                    // Parse
                    string ean = scannedCode ?? string.Empty;
                    string lote = string.Empty;
                    if (!string.IsNullOrEmpty(scannedCode) && scannedCode.Contains("|"))
                    {
                        var parts = scannedCode.Split('|');
                        if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[1]) && !string.IsNullOrWhiteSpace(parts[2]))
                        {
                            ean = parts[1].Trim();
                            lote = parts[2].Trim();
                        }
                        else if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            ean = parts[0].Trim();
                            lote = parts[1].Trim();
                        }
                    }

                    result.Ean = ean;
                    result.Lote = lote;

                    // Lookup
                    DataTable dt = new DataTable();
                    if (idCd == 1)
                    {
                        Conexion.Abrir_WMS_LIM();
                        SqlCommand cmd = new SqlCommand("usp_BuscarProductos_BarCode", Conexion.conectar_WMS_LIM);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@CodBar", SqlDbType.VarChar, 31).Value = ean;
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                        Conexion.Cerrar_WMS_LIM();
                    }
                    else
                    {
                        Conexion.Abrir_WMS_ATE();
                        SqlCommand cmd = new SqlCommand("usp_BuscarProductos_BarCode", Conexion.conectar_WMS_ATE);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@CodBar", SqlDbType.VarChar, 31).Value = ean;
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);
                        Conexion.Cerrar_WMS_ATE();
                    }

                    result.RawData = dt;
                    result.Filas = dt.Rows.Count;

                    if (result.Filas == 0)
                    {
                        result.Exito = false;
                        result.Mensaje = "Producto no existe";
                    }
                    else if (result.Filas > 1)
                    {
                        result.Exito = false;
                        result.Mensaje = "Existe más de un producto con el mismo CodBar";
                    }
                    else
                    {
                        result.Exito = true;
                        result.IdArticulo = dt.Rows[0][0].ToString();
                        result.NombreArticulo = dt.Rows[0][1].ToString();
                    }
                }
                catch (Exception ex)
                {
                    result.Exito = false;
                    result.Mensaje = "Error al buscar el producto: " + ex.Message;
                }

                return result;
            });
        }

        /// <summary>
        /// Versión síncrona por compatibilidad: bloquea y devuelve el resultado.
        /// Uso recomendado solo si no puedes marcar tu caller como async. Preferir <see cref="ProcesarCodigoProductoRefac"/>.
        /// </summary>
        public static ScanLookupResult ProcesarCodigoProductoRefacSync(string scannedCode, int idCd)
        {
            return ProcesarCodigoProductoRefac(scannedCode, idCd).GetAwaiter().GetResult();
        }
    }
}
