using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using App_ConsultaStocks.Datos;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RepartoCargaVehiculos : ContentPage
    {
        private DateTime fechaReparto;
        private PedidoVehiculo selectedPedido = null;
        private List<VehiculoInfo> vehiculos = new List<VehiculoInfo>();
        private List<PedidoVehiculo> allPedidos = new List<PedidoVehiculo>(); // TODOS los pedidos del SP
        private bool vehiculosCargados = false; // Flag para saber si ya cargamos vehículos

        public RepartoCargaVehiculos()
        {
            InitializeComponent();
            fechaReparto = DateTime.Today;
            datePickerFechaReparto.Date = fechaReparto;
            // Removido: CargarVehiculos() y CargarPedidos() del constructor
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Primera vez: cargar vehículos desde el SP
            if (!vehiculosCargados)
            {
                CargarVehiculosInicialmente();
                return; // No cargar pedidos aún, esperar a que seleccione vehículo
            }
            
            // Si ya hay vehículo seleccionado, recargar datos filtrados por ese vehículo
            if (pickerVehiculo.SelectedItem != null)
            {
                var vehiculoSeleccionado = pickerVehiculo.SelectedItem as VehiculoInfo;
                CargarPedidosPorVehiculo(vehiculoSeleccionado?.COD_VEHICULO);
            }
        }

        private void pickerVehiculo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pickerVehiculo.SelectedIndex != -1)
            {
                var vehiculoSeleccionado = pickerVehiculo.SelectedItem as VehiculoInfo;
                if (vehiculoSeleccionado != null)
                {
                    // Cargar pedidos del SP filtrados por este vehículo
                    CargarPedidosPorVehiculo(vehiculoSeleccionado.COD_VEHICULO);
                }
            }
            else
            {
                // Si no hay vehículo seleccionado, limpiar lista
                listViewPedidos.ItemsSource = null;
                selectedPedido = null;
            }
        }

        private void datePickerFechaReparto_DateSelected(object sender, DateChangedEventArgs e)
        {
            fechaReparto = datePickerFechaReparto.Date;
            // Reset state and reload data for new date
            vehiculosCargados = false;
            allPedidos.Clear();
            pickerVehiculo.ItemsSource = null;
            listViewPedidos.ItemsSource = null;
            selectedPedido = null;
            // Reload vehicles and data
            CargarVehiculosInicialmente();
        }

        private async void CargarVehiculosInicialmente()
        {
            try
            {
                Console.WriteLine("[DEBUG] CargarVehiculosInicialmente - Cargando lista de vehículos por primera vez");
                Console.WriteLine($"[DEBUG] FechaReparto: {fechaReparto.ToString("yyyy-MM-dd")}");

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("usp_ResumenBultos_PorClienteDireccion_AmbasBD", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                Conexion.Cerrar();

                Console.WriteLine($"[DEBUG] CargarVehiculosInicialmente - Filas obtenidas: {dt.Rows.Count}");

                // Guardar TODOS los pedidos en memoria
                allPedidos.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    int totalBultos = Convert.ToInt32(row["TOTAL_BULTOS"]);
                    decimal bultosCargados = Convert.ToDecimal(row["TOTAL_BULTOScargados"] ?? 0);
                    int bultosPendientes = totalBultos - (int)bultosCargados;

                    var pedido = new PedidoVehiculo
                    {
                        FUENTE = row["FUENTE"]?.ToString() ?? "",
                        IDCLIENTE = row["IDCLIENTE"]?.ToString() ?? "",
                        NOMBRE_CLIENTE = row["NOMBRE_CLIENTE"]?.ToString() ?? "",
                        IDDIRECCION = row["IDDIRECCION"]?.ToString() ?? "",
                        COD_VEHICULO = row["COD_VEHICULO"]?.ToString() ?? "",
                        MARCA_VEHICULO = row["MARCA_VEHICULO"]?.ToString() ?? "",
                        PLACA_VEHICULO = row["PLACA_VEHICULO"]?.ToString() ?? "",
                        TOTAL_DESPACHOS = Convert.ToInt32(row["TOTAL_DESPACHOS"] ?? 0),
                        TOTAL_BULTOS = totalBultos,
                        TOTAL_BULTOScargados = bultosCargados,
                        BULTOS_PENDIENTES = bultosPendientes
                    };

                    allPedidos.Add(pedido);
                }

                // Extraer vehículos únicos
                var vehiculosUnicos = allPedidos
                    .Where(p => !string.IsNullOrEmpty(p.COD_VEHICULO))
                    .GroupBy(p => p.COD_VEHICULO)
                    .Select(g => g.First())
                    .Select(p => new VehiculoInfo
                    {
                        COD_VEHICULO = p.COD_VEHICULO,
                        MARCA_VEHICULO = p.MARCA_VEHICULO,
                        PLACA_VEHICULO = p.PLACA_VEHICULO
                    })
                    .OrderBy(v => v.COD_VEHICULO)
                    .ToList();

                Console.WriteLine($"[DEBUG] Vehículos únicos encontrados: {vehiculosUnicos.Count}");

                vehiculos.Clear();
                vehiculos.AddRange(vehiculosUnicos);

                pickerVehiculo.ItemsSource = vehiculos;
                pickerVehiculo.ItemDisplayBinding = new Binding("DisplayText");

                vehiculosCargados = true;
                Console.WriteLine("[DEBUG] Lista de vehículos cargada exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CargarVehiculosInicialmente: {ex.Message}");
                await DisplayAlert("Error", "Error al cargar vehículos: " + ex.Message, "Ok");
            }
        }

        private async void CargarPedidosPorVehiculo(string codVehiculo)
        {
            try
            {
                Console.WriteLine($"[DEBUG] CargarPedidosPorVehiculo - Vehículo: {codVehiculo}");
                Console.WriteLine($"[DEBUG] FechaReparto: {fechaReparto.ToString("yyyy-MM-dd")}");

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("usp_ResumenBultos_PorClienteDireccion_AmbasBD", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                Conexion.Cerrar();

                Console.WriteLine($"[DEBUG] CargarPedidosPorVehiculo - Filas obtenidas: {dt.Rows.Count}");

                // Procesar pedidos
                List<PedidoVehiculo> pedidos = new List<PedidoVehiculo>();
                foreach (DataRow row in dt.Rows)
                {
                    int totalBultos = Convert.ToInt32(row["TOTAL_BULTOS"]);
                    decimal bultosCargados = Convert.ToDecimal(row["TOTAL_BULTOScargados"] ?? 0);
                    int bultosPendientes = totalBultos - (int)bultosCargados;

                    var pedido = new PedidoVehiculo
                    {
                        FUENTE = row["FUENTE"]?.ToString() ?? "",
                        IDCLIENTE = row["IDCLIENTE"]?.ToString() ?? "",
                        NOMBRE_CLIENTE = row["NOMBRE_CLIENTE"]?.ToString() ?? "",
                        IDDIRECCION = row["IDDIRECCION"]?.ToString() ?? "",
                        COD_VEHICULO = row["COD_VEHICULO"]?.ToString() ?? "",
                        MARCA_VEHICULO = row["MARCA_VEHICULO"]?.ToString() ?? "",
                        PLACA_VEHICULO = row["PLACA_VEHICULO"]?.ToString() ?? "",
                        TOTAL_DESPACHOS = Convert.ToInt32(row["TOTAL_DESPACHOS"] ?? 0),
                        TOTAL_BULTOS = totalBultos,
                        TOTAL_BULTOScargados = bultosCargados,
                        BULTOS_PENDIENTES = bultosPendientes
                    };

                    pedidos.Add(pedido);
                }

                // Filtrar por vehículo
                var pedidosFiltrados = pedidos
                    .Where(p => p.COD_VEHICULO.Split(',').Any(v => v.Trim() == codVehiculo))
                    .OrderBy(p => p.NOMBRE_CLIENTE)
                    .ToList();

                Console.WriteLine($"[DEBUG] Pedidos filtrados para vehículo {codVehiculo}: {pedidosFiltrados.Count}");

                listViewPedidos.ItemsSource = pedidosFiltrados;

                selectedPedido = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CargarPedidosPorVehiculo: {ex.Message}");
                await DisplayAlert("Error", "Error al cargar pedidos: " + ex.Message, "Ok");
            }
        }

        private async void CargarDatosDesdeSQL(string codVehiculoFiltro = null)
        {
            try
            {
                Console.WriteLine($"[DEBUG] CargarDatosDesdeSQL - Filtro vehículo: {codVehiculoFiltro ?? "NINGUNO"}");
                Console.WriteLine($"[DEBUG] FechaReparto: {fechaReparto.ToString("yyyy-MM-dd")}");

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("usp_ResumenBultos_PorClienteDireccion_AmbasBD", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                Conexion.Cerrar();

                Console.WriteLine($"[DEBUG] CargarDatosDesdeSQL - Filas obtenidas del SP: {dt.Rows.Count}");

                // Guardar TODOS los pedidos
                allPedidos.Clear();
                List<PedidoVehiculo> pedidos = new List<PedidoVehiculo>();
                
                foreach (DataRow row in dt.Rows)
                {
                    int totalBultos = Convert.ToInt32(row["TOTAL_BULTOS"]);
                    decimal bultosCargados = Convert.ToDecimal(row["TOTAL_BULTOScargados"] ?? 0);
                    int bultosPendientes = totalBultos - (int)bultosCargados;

                    var pedido = new PedidoVehiculo
                    {
                        FUENTE = row["FUENTE"]?.ToString() ?? "",
                        IDCLIENTE = row["IDCLIENTE"]?.ToString() ?? "",
                        NOMBRE_CLIENTE = row["NOMBRE_CLIENTE"]?.ToString() ?? "",
                        IDDIRECCION = row["IDDIRECCION"]?.ToString() ?? "",
                        COD_VEHICULO = row["COD_VEHICULO"]?.ToString() ?? "",
                        MARCA_VEHICULO = row["MARCA_VEHICULO"]?.ToString() ?? "",
                        PLACA_VEHICULO = row["PLACA_VEHICULO"]?.ToString() ?? "",
                        TOTAL_DESPACHOS = Convert.ToInt32(row["TOTAL_DESPACHOS"] ?? 0),
                        TOTAL_BULTOS = totalBultos,
                        TOTAL_BULTOScargados = bultosCargados,
                        BULTOS_PENDIENTES = bultosPendientes
                    };

                    allPedidos.Add(pedido);
                }

                Console.WriteLine($"[DEBUG] Total pedidos cargados en memoria: {allPedidos.Count}");

                // Extraer vehículos únicos
                var vehiculosUnicos = allPedidos
                    .Where(p => !string.IsNullOrEmpty(p.COD_VEHICULO))
                    .GroupBy(p => p.COD_VEHICULO)
                    .Select(g => g.First())
                    .Select(p => new VehiculoInfo
                    {
                        COD_VEHICULO = p.COD_VEHICULO,
                        MARCA_VEHICULO = p.MARCA_VEHICULO,
                        PLACA_VEHICULO = p.PLACA_VEHICULO
                    })
                    .OrderBy(v => v.COD_VEHICULO)
                    .ToList();

                Console.WriteLine($"[DEBUG] Vehículos únicos encontrados: {vehiculosUnicos.Count}");

                // Preservar vehículo seleccionado
                var vehiculoAnterior = pickerVehiculo.SelectedItem as VehiculoInfo;
                string codVehiculoAnterior = vehiculoAnterior?.COD_VEHICULO ?? codVehiculoFiltro;

                vehiculos.Clear();
                vehiculos.AddRange(vehiculosUnicos);

                pickerVehiculo.ItemsSource = vehiculos;
                pickerVehiculo.ItemDisplayBinding = new Binding("DisplayText");

                // Restaurar o establecer selección de vehículo
                if (!string.IsNullOrEmpty(codVehiculoAnterior))
                {
                    var vehiculoARestaurar = vehiculos.FirstOrDefault(v => v.COD_VEHICULO == codVehiculoAnterior);
                    if (vehiculoARestaurar != null)
                    {
                        pickerVehiculo.SelectedItem = vehiculoARestaurar;
                        Console.WriteLine($"[DEBUG] Vehículo seleccionado: {codVehiculoAnterior}");
                    }
                }

                // Filtrar pedidos según vehículo seleccionado
                if (!string.IsNullOrEmpty(codVehiculoFiltro))
                {
                    pedidos = allPedidos
                        .Where(p => p.COD_VEHICULO.Split(',').Any(v => v.Trim() == codVehiculoFiltro))
                        .OrderBy(p => p.NOMBRE_CLIENTE)
                        .ToList();
                    
                    Console.WriteLine($"[DEBUG] Pedidos filtrados para vehículo {codVehiculoFiltro}: {pedidos.Count}");
                }
                else
                {
                    pedidos = allPedidos.OrderBy(p => p.NOMBRE_CLIENTE).ToList();
                    Console.WriteLine($"[DEBUG] Mostrando todos los pedidos: {pedidos.Count}");
                }

                listViewPedidos.ItemsSource = pedidos;

                selectedPedido = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CargarDatosDesdeSQL: {ex.Message}");
                await DisplayAlert("Error", "Error al cargar datos: " + ex.Message, "Ok");
            }
        }

        private void CargarVehiculos()
        {
            try
            {
                Console.WriteLine($"[DEBUG] Intentando ejecutar SP en BD: {AppConfig.GetCatalogoPrincipal()}");
                Console.WriteLine($"[DEBUG] FechaReparto: {fechaReparto.ToString("yyyy-MM-dd")}");

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("usp_ResumenBultos_PorClienteDireccion_AmbasBD", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                Conexion.Cerrar();

                // Debug: Mostrar información
                Console.WriteLine($"[DEBUG] CargarVehiculos - Filas obtenidas: {dt.Rows.Count}");
                Console.WriteLine($"[DEBUG] CargarVehiculos - Columnas: {string.Join(", ", dt.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName))}");

                if (dt.Rows.Count == 0)
                {
                    Console.WriteLine("[DEBUG] No se obtuvieron filas. Verificando si el SP existe...");

                    // Verificar si el SP existe
                    Conexion.Abrir();
                    SqlCommand checkCmd = new SqlCommand(@"
                        SELECT COUNT(*) FROM sys.objects 
                        WHERE object_id = OBJECT_ID(N'usp_ResumenBultos_PorClienteDireccion_AmbasBD') 
                        AND type in (N'P', N'PC')", Conexion.conectar);
                    int spCount = (int)checkCmd.ExecuteScalar();
                    Conexion.Cerrar();

                    Console.WriteLine($"[DEBUG] SP existe en BD {AppConfig.GetCatalogoPrincipal()}: {(spCount > 0 ? "SÍ" : "NO")}");

                    if (spCount == 0)
                    {
                        Console.WriteLine("[DEBUG] Intentando ejecutar SP en base de datos WMS...");

                        // Intentar en WMS si existe
                        Conexion.Abrir_WMS_ATE();
                        SqlCommand cmdWMS = new SqlCommand("usp_ResumenBultos_PorClienteDireccion_AmbasBD", Conexion.conectar_WMS_ATE);
                        cmdWMS.CommandType = CommandType.StoredProcedure;
                        cmdWMS.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;

                        SqlDataAdapter adapterWMS = new SqlDataAdapter(cmdWMS);
                        DataTable dtWMS = new DataTable();
                        adapterWMS.Fill(dtWMS);
                        Conexion.Cerrar_WMS_ATE();

                        Console.WriteLine($"[DEBUG] En WMS - Filas obtenidas: {dtWMS.Rows.Count}");

                        if (dtWMS.Rows.Count > 0)
                        {
                            dt = dtWMS; // Usar los datos de WMS
                            Console.WriteLine("[DEBUG] Usando datos de WMS");
                        }
                    }
                }

                // Extraer vehículos únicos del resultado del SP usando un enfoque simple
                var vehiculosUnicos = new Dictionary<string, VehiculoInfo>();
                foreach (DataRow row in dt.Rows)
                {
                    string codVehiculo = row["COD_VEHICULO"]?.ToString();
                    string marcaVehiculo = row["MARCA_VEHICULO"]?.ToString();
                    string placaVehiculo = row["PLACA_VEHICULO"]?.ToString();

                    // Solo agregar si COD_VEHICULO no es NULL o vacío
                    if (!string.IsNullOrEmpty(codVehiculo) && !vehiculosUnicos.ContainsKey(codVehiculo))
                    {
                        vehiculosUnicos[codVehiculo] = new VehiculoInfo
                        {
                            COD_VEHICULO = codVehiculo,
                            MARCA_VEHICULO = marcaVehiculo ?? "",
                            PLACA_VEHICULO = placaVehiculo ?? ""
                        };
                        Console.WriteLine($"[DEBUG] Vehículo agregado: {codVehiculo} - {marcaVehiculo} - {placaVehiculo}");
                    }
                }

                vehiculos.Clear();
                vehiculos.AddRange(vehiculosUnicos.Values.OrderBy(v => v.COD_VEHICULO));

                Console.WriteLine($"[DEBUG] Vehículos únicos encontrados: {vehiculos.Count}");

                // Preservar vehículo seleccionado antes de recargar
                var vehiculoAnterior = pickerVehiculo.SelectedItem as VehiculoInfo;
                string codVehiculoAnterior = vehiculoAnterior?.COD_VEHICULO;

                pickerVehiculo.ItemsSource = vehiculos;
                pickerVehiculo.ItemDisplayBinding = new Binding("DisplayText");

                // Restaurar selección si existía
                if (!string.IsNullOrEmpty(codVehiculoAnterior))
                {
                    var vehiculoARestaurar = vehiculos.FirstOrDefault(v => v.COD_VEHICULO == codVehiculoAnterior);
                    if (vehiculoARestaurar != null)
                    {
                        pickerVehiculo.SelectedItem = vehiculoARestaurar;
                        Console.WriteLine($"[DEBUG] Vehículo restaurado: {codVehiculoAnterior}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CargarVehiculos: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                DisplayAlert("Error", "Error al cargar vehículos: " + ex.Message, "Ok");
            }
        }

        private async void CargarPedidos()
        {
            try
            {
                Console.WriteLine($"[DEBUG] CargarPedidos - Intentando ejecutar SP en BD: {AppConfig.GetCatalogoPrincipal()}");
                Console.WriteLine($"[DEBUG] FechaReparto: {fechaReparto.ToString("yyyy-MM-dd")}");

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("usp_ResumenBultos_PorClienteDireccion_AmbasBD", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                Conexion.Cerrar();

                Console.WriteLine($"[DEBUG] CargarPedidos - Filas obtenidas: {dt.Rows.Count}");

                if (dt.Rows.Count == 0)
                {
                    Console.WriteLine("[DEBUG] No se obtuvieron filas en CargarPedidos. Verificando si el SP existe...");

                    // Verificar si el SP existe
                    Conexion.Abrir();
                    SqlCommand checkCmd = new SqlCommand(@"
                        SELECT COUNT(*) FROM sys.objects 
                        WHERE object_id = OBJECT_ID(N'usp_ResumenBultos_PorClienteDireccion_AmbasBD') 
                        AND type in (N'P', N'PC')", Conexion.conectar);
                    int spCount = (int)checkCmd.ExecuteScalar();
                    Conexion.Cerrar();

                    Console.WriteLine($"[DEBUG] SP existe en BD {AppConfig.GetCatalogoPrincipal()}: {(spCount > 0 ? "SÍ" : "NO")}");

                    if (spCount == 0)
                    {
                        Console.WriteLine("[DEBUG] Intentando ejecutar SP en base de datos WMS...");

                        // Intentar en WMS si existe
                        Conexion.Abrir_WMS_ATE();
                        SqlCommand cmdWMS = new SqlCommand("usp_ResumenBultos_PorClienteDireccion_AmbasBD", Conexion.conectar_WMS_ATE);
                        cmdWMS.CommandType = CommandType.StoredProcedure;
                        cmdWMS.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;

                        SqlDataAdapter adapterWMS = new SqlDataAdapter(cmdWMS);
                        DataTable dtWMS = new DataTable();
                        adapterWMS.Fill(dtWMS);
                        Conexion.Cerrar_WMS_ATE();

                        Console.WriteLine($"[DEBUG] En WMS - Filas obtenidas: {dtWMS.Rows.Count}");

                        if (dtWMS.Rows.Count > 0)
                        {
                            dt = dtWMS; // Usar los datos de WMS
                            Console.WriteLine("[DEBUG] Usando datos de WMS para pedidos");
                        }
                    }
                }

                List<PedidoVehiculo> pedidos = new List<PedidoVehiculo>();
                foreach (DataRow row in dt.Rows)
                {
                    int totalBultos = Convert.ToInt32(row["TOTAL_BULTOS"]);
                    decimal bultosCargados = Convert.ToDecimal(row["TOTAL_BULTOScargados"] ?? 0);
                    int bultosPendientes = totalBultos - (int)bultosCargados;

                    var pedido = new PedidoVehiculo
                    {
                        FUENTE = row["FUENTE"]?.ToString() ?? "",
                        IDCLIENTE = row["IDCLIENTE"]?.ToString() ?? "",
                        NOMBRE_CLIENTE = row["NOMBRE_CLIENTE"]?.ToString() ?? "",
                        IDDIRECCION = row["IDDIRECCION"]?.ToString() ?? "",
                        COD_VEHICULO = row["COD_VEHICULO"]?.ToString() ?? "",
                        MARCA_VEHICULO = row["MARCA_VEHICULO"]?.ToString() ?? "",
                        PLACA_VEHICULO = row["PLACA_VEHICULO"]?.ToString() ?? "",
                        TOTAL_DESPACHOS = Convert.ToInt32(row["TOTAL_DESPACHOS"] ?? 0),
                        TOTAL_BULTOS = totalBultos,
                        TOTAL_BULTOScargados = bultosCargados,
                        BULTOS_PENDIENTES = bultosPendientes
                    };

                    pedidos.Add(pedido);
                    Console.WriteLine($"[DEBUG] Pedido agregado: {pedido.IDCLIENTE} - {pedido.NOMBRE_CLIENTE} - {pedido.BULTOS_PENDIENTES} bultos");
                }

                Console.WriteLine($"[DEBUG] Total pedidos cargados: {pedidos.Count}");

                // Ordenar por nombre de cliente
                pedidos = pedidos.OrderBy(p => p.NOMBRE_CLIENTE).ToList();
                allPedidos = pedidos;
                listViewPedidos.ItemsSource = pedidos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CargarPedidos: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                await DisplayAlert("Error", "Error al cargar pedidos: " + ex.Message, "Ok");
            }
        }

        private void listViewPedidos_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e == null) return;
            selectedPedido = e.Item as PedidoVehiculo;
            listViewPedidos.SelectedItem = selectedPedido;
        }

        private async void btnCargarPedido_Clicked(object sender, EventArgs e)
        {
            if (selectedPedido == null)
            {
            await DisplayAlert("Error", "Seleccione un pedido para cargar", "Ok");
            return;
        }

        // Navegar a la pantalla de detalle de carga
        await Navigation.PushAsync(new RepartoCargaVehiculosDetalle(selectedPedido, datePickerFechaReparto.Date));
    }
}

    public class PedidoVehiculo
    {
        public string FUENTE { get; set; }
        public string IDCLIENTE { get; set; }
        public string NOMBRE_CLIENTE { get; set; }
        public string IDDIRECCION { get; set; }
        public string COD_VEHICULO { get; set; }
        public string MARCA_VEHICULO { get; set; }
        public string PLACA_VEHICULO { get; set; }
        public int TOTAL_DESPACHOS { get; set; }
        public int TOTAL_BULTOS { get; set; }
        public decimal TOTAL_BULTOScargados { get; set; }
        public int BULTOS_PENDIENTES { get; set; }
    }

    public class VehiculoInfo
    {
        public string COD_VEHICULO { get; set; }
        public string MARCA_VEHICULO { get; set; }
        public string PLACA_VEHICULO { get; set; }
        public string DisplayText => $"{MARCA_VEHICULO} - {PLACA_VEHICULO}";
    }
}
