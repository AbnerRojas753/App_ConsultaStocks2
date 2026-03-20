using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Linq;
using System.Net;
using System.IO;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks.Datos
{
    class dUbicacionesProductos:Conexion
    {
        SqlConnection Conex;

        ////public async Task<List<Model_UbicacionesProductos>> ListarProductosxUbicacion(string Buscar, string idusuario)
        ////{
        ////    try
        ////    {
            ////    Conex = new SqlConnection(Abrir());
            ////    List<Model_UbicacionesProductos> lista = new List<Model_UbicacionesProductos>();
            ////    DataTable dt = new DataTable();
            ////    SqlCommand cmd = new SqlCommand("spConsultaClienteAppWeb", Conex);
            ////    cmd.CommandType = CommandType.StoredProcedure;
            ////    cmd.Parameters.AddWithValue("@BUSCAR", Buscar);
            ////    cmd.Parameters.AddWithValue("@ID_USUARIO", idusuario);
            ////    await Conex.OpenAsync();
            ////    SqlDataAdapter da = new SqlDataAdapter(cmd);
            ////    da.Fill(dt);

            ////    if (dt.Rows.Count > 0)
            ////    {
            ////        Cliente cliente = null;
            ////        foreach (DataRow row in dt.Rows)
            ////        {
            ////            cliente = new Cliente();
            ////            cliente.IdCliente = (string)row["Codigo"];
            ////            cliente.RazonSocial = (string)row["Cliente"];
            ////            cliente.Documento = (string)row["RUC / DNI"];
            ////            lista.Add(cliente);
            ////        }
            ////    }
            ////    return lista;

            ////}
            ////catch (Exception)
            ////{

            ////    throw;
            ////}
            ////finally
            ////{
            ////    Conex.Close();
            ////}

        ////}

    }
}
