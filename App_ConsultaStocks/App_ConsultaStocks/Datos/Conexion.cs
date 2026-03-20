using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks.Datos
{
    class Conexion
    {
        //public static string connectionString = ("Data Source=192.168.1.2; Initial Catalog=ALFA; Integrated Security=false; uid=sa; pwd=9Chino2016");
        //public static SqlConnection conectar = new SqlConnection(connectionString);

        //public static string connectionString_WMS_LIM = ("Data Source=192.168.1.2; Initial Catalog=WMS_ALFA; Integrated Security=false; uid=sa; pwd=9Chino2016");
        //public static SqlConnection conectar_WMS_LIM = new SqlConnection(connectionString_WMS_LIM);

        //public static string connectionString_WMS_ATE = ("Data Source=192.168.2.20; Initial Catalog=WMS_ALFA; Integrated Security=false; uid=sa; pwd=9Chino2016");
        //public static SqlConnection conectar_WMS_ATE= new SqlConnection(connectionString_WMS_ATE);


        // Conexiones dinámicas según la empresa seleccionada
        public static string connectionString
        {
            get
            {
                string dataSource = AppConfig.GetDataSource();
                string catalogo = AppConfig.GetCatalogoPrincipal();
                string connStr = $"Data Source={dataSource}; Initial Catalog={catalogo}; Integrated Security=false; uid=sa; pwd=9Chino2016";

                // Debug: Mostrar conexión principal
                if (AppConfig.MostrarDebug)
                {
                    Console.WriteLine($"[CONEXION] Servidor: {dataSource}");
                    Console.WriteLine($"[CONEXION] BD Principal: {catalogo}");
                    Console.WriteLine($"[CONEXION] ConnectionString: {connStr}");
                }

                return connStr;
            }
        }

        // Conexión principal (se crea/abre en Abrir() y se cierra en Cerrar())
        private static SqlConnection _conectar;
        public static SqlConnection conectar
        {
            get { return _conectar; }
        }

        // Connection string dinámico para WMS (usa el catálogo según AppConfig)
        public static string connectionString_WMS_LIM
        {
            get
            {
                string dataSource = AppConfig.GetDataSource();
                string catalogoWMS = AppConfig.GetCatalogoWMS();
                string connStr = $"Data Source={dataSource}; Initial Catalog={catalogoWMS}; Integrated Security=false; uid=sa; pwd=9Chino2016";
                if (AppConfig.MostrarDebug)
                {
                    Console.WriteLine($"[CONEXION] Servidor: {dataSource}");
                    Console.WriteLine($"[CONEXION] BD WMS_LIM: {catalogoWMS}");
                    Console.WriteLine($"[CONEXION] ConnectionString WMS_LIM: {connStr}");
                }
                return connStr;
            }
        }

        // Conexión WMS LIM administrada (creada en Abrir_WMS_LIM y cerrada en Cerrar_WMS_LIM)
        private static SqlConnection _conectar_WMS_LIM;
        public static SqlConnection conectar_WMS_LIM
        {
            get { return _conectar_WMS_LIM; }
        }

        public static string connectionString_WMS_ATE
        {
            get
            {
                string dataSource = AppConfig.GetDataSource();
                string catalogoWMS = AppConfig.GetCatalogoWMS();
                string connStr = $"Data Source={dataSource}; Initial Catalog={catalogoWMS}; Integrated Security=false; uid=sa; pwd=9Chino2016";

                // Debug: Mostrar conexión WMS
                if (AppConfig.MostrarDebug)
                {
                    Console.WriteLine($"[CONEXION] Servidor: {dataSource}");
                    Console.WriteLine($"[CONEXION] BD WMS_ATE: {catalogoWMS}");
                    Console.WriteLine($"[CONEXION] ConnectionString WMS: {connStr}");
                }

                return connStr;
            }
        }

        // Conexión WMS ATE administrada (creada en Abrir_WMS_ATE y cerrada en Cerrar_WMS_ATE)
        private static SqlConnection _conectar_WMS_ATE;
        public static SqlConnection conectar_WMS_ATE
        {
            get { return _conectar_WMS_ATE; }
        }
        public static void Abrir()
        {
            if (_conectar == null || _conectar.State == System.Data.ConnectionState.Closed)
            {
                _conectar = new SqlConnection(connectionString);
                _conectar.Open();
            }
        }

        public static void Cerrar()
        {
            if (_conectar != null && _conectar.State == System.Data.ConnectionState.Open)
            {
                _conectar.Close();
                _conectar.Dispose();
                _conectar = null;
            }
        }

        public static void Abrir_WMS_LIM()
        {
            if (_conectar_WMS_LIM == null || _conectar_WMS_LIM.State == System.Data.ConnectionState.Closed)
            {
                _conectar_WMS_LIM = new SqlConnection(connectionString_WMS_LIM);
                _conectar_WMS_LIM.Open();
            }
        }

        public static void Cerrar_WMS_LIM()
        {
            if (_conectar_WMS_LIM != null && _conectar_WMS_LIM.State == System.Data.ConnectionState.Open)
            {
                _conectar_WMS_LIM.Close();
                _conectar_WMS_LIM.Dispose();
                _conectar_WMS_LIM = null;
            }
        }


        public static void Abrir_WMS_ATE()
        {
            if (_conectar_WMS_ATE == null || _conectar_WMS_ATE.State == System.Data.ConnectionState.Closed)
            {
                _conectar_WMS_ATE = new SqlConnection(connectionString_WMS_ATE);
                _conectar_WMS_ATE.Open();
            }
        }

        public static void Cerrar_WMS_ATE()
        {
            if (_conectar_WMS_ATE != null && _conectar_WMS_ATE.State == System.Data.ConnectionState.Open)
            {
                _conectar_WMS_ATE.Close();
                _conectar_WMS_ATE.Dispose();
                _conectar_WMS_ATE = null;
            }
        }


    }
}
