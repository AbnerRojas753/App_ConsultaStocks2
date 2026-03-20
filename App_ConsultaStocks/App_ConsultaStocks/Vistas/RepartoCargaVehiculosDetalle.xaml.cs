using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

using App_ConsultaStocks.Datos;
using App_ConsultaStocks.Modelo;
using ZXing.Net.Mobile.Forms;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RepartoCargaVehiculosDetalle : ContentPage
    {
        private PedidoVehiculo pedido;
        private DateTime fechaReparto;
        private List<BultoItem> bultos = new List<BultoItem>();
        private List<DespachoDetalle> despachos = new List<DespachoDetalle>();
        private int? idSesionActiva = null;
        private string idPicador = "";
        private readonly string baseDatosDestino; // ALFA o GPTOR para las tablas de progreso

        public RepartoCargaVehiculosDetalle(PedidoVehiculo pedidoSeleccionado, DateTime fechaSeleccionada)
        {
            InitializeComponent();
            pedido = pedidoSeleccionado;
            fechaReparto = fechaSeleccionada.Date; // normalizar al día seleccionado
            baseDatosDestino = pedido.FUENTE == "GPTOR" ? "GPTOR" : "ALFA";
            CargarDatos();
        }

        private async void CargarDatos()
        {
            try
            {
                lblCliente.Text = pedido.NOMBRE_CLIENTE;
                lblDireccion.Text = pedido.IDDIRECCION;

                // Verificar si existe sesión activa
                await VerificarSesionActiva();

                // Ejecutar SP para obtener despachos del cliente
                await CargarDespachos();

                // Actualizar resumen
                ActualizarResumen();

                // Generar lista de bultos
                GenerarListaBultos();

                // Mostrar botón según estado
                MostrarControlesSegunEstado();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al cargar datos: " + ex.Message, "Ok");
            }
        }

        private SqlConnection AbrirConexion()
        {
            if (pedido.FUENTE == "GPTOR")
            {
                Conexion.Abrir_WMS_ATE();
                return Conexion.conectar_WMS_ATE;
            }
            else
            {
                Conexion.Abrir();
                return Conexion.conectar;
            }
        }

        private void CerrarConexion()
        {
            if (pedido.FUENTE == "GPTOR")
            {
                Conexion.Cerrar_WMS_ATE();
            }
            else
            {
                Conexion.Cerrar();
            }
        }

        private async Task CargarDespachos()
        {
            try
            {
                Console.WriteLine($"[DEBUG] CargarDespachos - Iniciando carga de despachos");
                Console.WriteLine($"[DEBUG] CargarDespachos - IDCliente: {pedido.IDCLIENTE}");
                Console.WriteLine($"[DEBUG] CargarDespachos - IDDireccion: {pedido.IDDIRECCION}");
                Console.WriteLine($"[DEBUG] CargarDespachos - FechaReparto: {fechaReparto.ToString("yyyy-MM-dd")}");
                Console.WriteLine($"[DEBUG] CargarDespachos - FUENTE: {pedido.FUENTE}");

                // Siempre conectar a ALFA para ejecutar el SP wrapper
                Conexion.Abrir();
                var cmd = new SqlCommand("usp_DespachosPorClienteDireccion", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;
                cmd.Parameters.Add("@IDCliente", SqlDbType.NVarChar, 50).Value = pedido.IDCLIENTE;
                cmd.Parameters.Add("@IDDIRECCION", SqlDbType.NVarChar, 100).Value = pedido.IDDIRECCION;
                cmd.Parameters.Add("@BaseDatos", SqlDbType.NVarChar, 10).Value = pedido.FUENTE == "GPTOR" ? "GPTOR" : "ALFA";
                cmd.Parameters.Add("@COD_VEHICULO", SqlDbType.NVarChar, 50).Value = pedido.COD_VEHICULO;

                Console.WriteLine($"[DEBUG] CargarDespachos - Ejecutando SP: usp_DespachosPorClienteDireccion");

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                Conexion.Cerrar();

                Console.WriteLine($"[DEBUG] CargarDespachos - Filas resumen obtenidas: {dt.Rows.Count}");

                despachos.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    var despacho = new DespachoDetalle
                    {
                        IDCLIENTE = row["IDCLIENTE"].ToString(),
                        ID_DIRECCION = row["ID_DIRECCION"].ToString(),
                        ID_DESPACHO = Convert.ToInt32(row["ID_DESPACHO"]),
                        TOTAL_BULTOS = Convert.ToInt32(row["TOTAL_BULTOS"]),
                        TOTAL_BULTOS_CARGADOS = Convert.ToInt32(row["TOTAL_BULTOS_CARGADOS"]),
                        TOTAL_CAJAS = Convert.ToInt32(row["TOTAL_CAJAS"]),
                        TOTAL_BOLSAS = Convert.ToInt32(row["TOTAL_BOLSAS"])
                    };
                    
                    Console.WriteLine($"[DEBUG] CargarDespachos - Despacho {despacho.ID_DESPACHO}: Total={despacho.TOTAL_BULTOS}, Cargados={despacho.TOTAL_BULTOS_CARGADOS}");
                    despachos.Add(despacho);
                }

                // Consulta adicional: obtener bultos CARGADOS desde tabla de progreso
                await CargarBultosCargados();
            }
            catch (Exception ex)
            {
                Conexion.Cerrar();
                Console.WriteLine($"[ERROR] CargarDespachos: {ex.Message}");
                await DisplayAlert("Error", "Error al cargar despachos: " + ex.Message, "Ok");
            }
        }

        private async Task CargarBultosCargados()
        {
            try
            {
                Console.WriteLine($"[DEBUG] CargarBultosCargados - Usando SP usp_BultosDetallePorClienteDireccion_Ambos");

                Conexion.Abrir();
                using (var cmd = new SqlCommand("usp_BultosDetallePorClienteDireccion_Ambos", Conexion.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;
                    cmd.Parameters.Add("@IDCliente", SqlDbType.NVarChar, 50).Value = pedido.IDCLIENTE;
                    cmd.Parameters.Add("@IDDIRECCION", SqlDbType.NVarChar, 100).Value = pedido.IDDIRECCION;
                    cmd.Parameters.Add("@BaseDatos", SqlDbType.NVarChar, 10).Value = pedido.FUENTE == "GPTOR" ? "GPTOR" : "ALFA";
                    cmd.Parameters.Add("@COD_VEHICULO", SqlDbType.NVarChar, 50).Value = pedido.COD_VEHICULO;

                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        Conexion.Cerrar();

                        Console.WriteLine($"[DEBUG] CargarBultosCargados - Filas obtenidas: {dt.Rows.Count}");

                        bultos.Clear();
                        foreach (DataRow row in dt.Rows)
                        {
                            int idDespacho = Convert.ToInt32(row["ID_DESPACHO"]);
                            int idBulto = Convert.ToInt32(row["ID_BULTO"]);
                            string estado = row["ESTADO"]?.ToString() ?? "PENDIENTE";
                            bool cargado = string.Equals(estado, "CARGADO", StringComparison.OrdinalIgnoreCase);

                            bultos.Add(new BultoItem
                            {
                                ID_DESPACHO = idDespacho,
                                NumeroBulto = idBulto,
                                Descripcion = $"{idDespacho}-BULTO-{idBulto}",
                                Estado = cargado ? "CARGADO" : "PENDIENTE",
                                EstadoColor = cargado ? Color.Green : Color.Red
                            });
                        }

                        Console.WriteLine($"[DEBUG] CargarBultosCargados - Total bultos generados: {bultos.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                Conexion.Cerrar();
                Console.WriteLine($"[ERROR] CargarBultosCargados: {ex.Message}");
            }
        }

        private async Task VerificarSesionActiva()
        {
            try
            {
                Console.WriteLine($"[DEBUG] VerificarSesionActiva - Iniciando verificación");
                Console.WriteLine($"[DEBUG] VerificarSesionActiva - IDCliente: {pedido.IDCLIENTE}");
                Console.WriteLine($"[DEBUG] VerificarSesionActiva - IDDireccion: {pedido.IDDIRECCION}");
                Console.WriteLine($"[DEBUG] VerificarSesionActiva - FechaReparto: {fechaReparto.ToString("yyyy-MM-dd")}");

                Conexion.Abrir();
                using (var cmd = new SqlCommand("usp_SesionCarga_ObtenerActiva2", Conexion.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BaseDatos", SqlDbType.NVarChar, 10).Value = baseDatosDestino;
                    cmd.Parameters.Add("@IDCLIENTE", SqlDbType.NVarChar, 50).Value = pedido.IDCLIENTE;
                    cmd.Parameters.Add("@ID_DIRECCION", SqlDbType.NVarChar, 100).Value = pedido.IDDIRECCION;
                    cmd.Parameters.Add("@FECHA_REPARTO", SqlDbType.Date).Value = fechaReparto;
                    cmd.Parameters.Add("@COD_VEHICULO", SqlDbType.NVarChar, 50).Value = pedido.COD_VEHICULO;

                    Console.WriteLine("[DEBUG] VerificarSesionActiva - Ejecutando SP usp_SesionCarga_ObtenerActiva");
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        Conexion.Cerrar();

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            string status = row["STATUS"]?.ToString() ?? "";
                            string message = row["MESSAGE"]?.ToString() ?? "";

                            Console.WriteLine($"[DEBUG] VerificarSesionActiva - Status: {status}, Message: {message}");

                            if (string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
                            {
                                idSesionActiva = Convert.ToInt32(row["ID_SESION"]);
                                Console.WriteLine($"[DEBUG] VerificarSesionActiva - Sesión activa encontrada: {idSesionActiva}");

                                var usuario = row["USUARIO_INICIO"];
                                if (usuario != null && usuario != DBNull.Value)
                                {
                                    idPicador = usuario.ToString();
                                    Console.WriteLine($"[DEBUG] VerificarSesionActiva - Usuario/Picador establecido: {idPicador}");
                                }
                            }
                            else if (string.Equals(status, "FINALIZADA", StringComparison.OrdinalIgnoreCase))
                            {
                                idSesionActiva = null;
                                Console.WriteLine($"[DEBUG] VerificarSesionActiva - Sesión finalizada: {message}");
                                await DisplayAlert("Sesión Finalizada", message, "Ok");
                            }
                            else if (string.Equals(status, "NO_INICIADA", StringComparison.OrdinalIgnoreCase))
                            {
                                idSesionActiva = null;
                                Console.WriteLine($"[DEBUG] VerificarSesionActiva - No hay sesión iniciada: {message}");
                                // No mostrar alert para NO_INICIADA, ya que es el estado normal
                            }
                        }
                        else
                        {
                            Console.WriteLine("[DEBUG] VerificarSesionActiva - No se recibió respuesta del SP");
                            idSesionActiva = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Conexion.Cerrar();
                Console.WriteLine($"[ERROR] VerificarSesionActiva: {ex.Message}");
                Console.WriteLine($"[ERROR] VerificarSesionActiva - StackTrace: {ex.StackTrace}");
            }
        }

        private void GenerarListaBultos()
        {
            // Actualizar vista
            Console.WriteLine($"[DEBUG] GenerarListaBultos - Total bultos: {bultos.Count}");
            listViewBultos.ItemsSource = null;
            listViewBultos.ItemsSource = bultos;
        }

        private void ActualizarResumen()
        {
            int totalDespachos = despachos.Count;
            int totalBultos = despachos.Sum(d => d.TOTAL_BULTOS);
            int totalCargados = despachos.Sum(d => d.TOTAL_BULTOS_CARGADOS);
            int totalPendientes = totalBultos - totalCargados;

            Console.WriteLine($"[DEBUG] ActualizarResumen - Despachos: {totalDespachos}, Total: {totalBultos}, Cargados: {totalCargados}, Pendientes: {totalPendientes}");

            lblTotalDespachos.Text = totalDespachos.ToString();
            lblTotalBultos.Text = totalBultos.ToString();
            lblBultosCargados.Text = totalCargados.ToString();
            lblBultosPendientes.Text = totalPendientes.ToString();
        }

        private void MostrarControlesSegunEstado()
        {
            int totalCargados = despachos.Sum(d => d.TOTAL_BULTOS_CARGADOS);
            int totalBultos = despachos.Sum(d => d.TOTAL_BULTOS);

            Console.WriteLine($"[DEBUG] MostrarControlesSegunEstado - totalCargados: {totalCargados}, totalBultos: {totalBultos}, idSesionActiva: {idSesionActiva}");

            if (idSesionActiva == null && totalCargados == 0)
            {
                // No ha iniciado - mostrar botón INICIAR
                Console.WriteLine("[DEBUG] MostrarControlesSegunEstado - Mostrando INICIAR");
                btnIniciar.IsVisible = true;
                panelEscaneo.IsVisible = false;
                btnFinalizar.IsVisible = false;
            }
            else if (totalCargados >= totalBultos && totalBultos > 0)
            {
                // Completo - mostrar botón FINALIZAR (revisar PRIMERO esta condición)
                Console.WriteLine("[DEBUG] MostrarControlesSegunEstado - Mostrando FINALIZAR");
                btnIniciar.IsVisible = false;
                panelEscaneo.IsVisible = false;
                btnFinalizar.IsVisible = true;
            }
            else
            {
                // En proceso - mostrar panel de escaneo
                Console.WriteLine("[DEBUG] MostrarControlesSegunEstado - Mostrando ESCANEO");
                btnIniciar.IsVisible = false;
                panelEscaneo.IsVisible = true;
                btnFinalizar.IsVisible = false;
                entryBulto.Focus();
            }
        }

        private async void btnIniciar_Clicked(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Iniciando proceso de carga");
                Console.WriteLine($"[DEBUG] btnIniciar_Clicked - idPicador actual: '{idPicador}'");

                // Validar pedidos antes de solicitar el picador
                bool pedidosValidos = await ValidarPedidosCliente();
                if (!pedidosValidos)
                {
                    Console.WriteLine("[DEBUG] btnIniciar_Clicked - Validación de pedidos falló, no se puede iniciar");
                    return;
                }

                // Solicitar ID del picador solo si la validación pasó
                Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Mostrando prompt para ID picador");
                string inputPicador = "0";
                
                // await DisplayPromptAsync("ID Picador", 
                //     "Ingrese su ID de Picador:", 
                //     placeholder: "ID Picador",
                //     keyboard: Keyboard.Numeric);

                Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Input del usuario: '{inputPicador}'");

                if (string.IsNullOrWhiteSpace(inputPicador))
                {
                    Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Usuario canceló o dejó vacío el prompt");
                    await DisplayAlert("Error", "Debe ingresar un ID de Picador", "Ok");
                    return;
                }

                idPicador = inputPicador.Trim();
                Console.WriteLine($"[DEBUG] btnIniciar_Clicked - idPicador establecido: '{idPicador}'");

                // bool respuesta = await DisplayAlert("Confirmar", 
                //     $"¿Desea iniciar la carga de bultos con el picador {idPicador}?", 
                //     "Sí", "No");
                bool respuesta=true; // Para pruebas automáticas sin prompt

                Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Respuesta de confirmación: {respuesta}");

                if (respuesta)
                {
                    Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Usuario confirmó, creando sesión...");
                    // Crear sesión en BD
                    bool sesionCreada = await CrearSesion();
                    Console.WriteLine($"[DEBUG] btnIniciar_Clicked - CrearSesion retornó: {sesionCreada}");
                    
                    if (sesionCreada)
                    {
                        Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Sesión creada exitosamente, actualizando UI");
                        btnIniciar.IsVisible = false;
                        panelEscaneo.IsVisible = true;
                        entryBulto.Focus();
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Falló la creación de sesión, mostrando error");
                        await DisplayAlert("Error", "No se pudo iniciar la sesión de carga", "Ok");
                    }
                }
                else
                {
                    Console.WriteLine($"[DEBUG] btnIniciar_Clicked - Usuario canceló la confirmación");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] btnIniciar_Clicked: {ex.Message}");
                Console.WriteLine($"[ERROR] btnIniciar_Clicked - StackTrace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Error al iniciar sesión: {ex.Message}", "OK");
            }
        }

        private async Task<bool> ValidarPedidosCliente()
        {
            try
            {
                Console.WriteLine("[DEBUG] ValidarPedidosCliente - Ejecutando SP_ValidarPedidosClienteAmbos");
                Conexion.Abrir();
                using (var cmd = new SqlCommand("SP_ValidarPedidosClienteAmbos", Conexion.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IDCLIENTE", SqlDbType.VarChar, 20).Value = pedido.IDCLIENTE;
                    cmd.Parameters.Add("@FechaReparto", SqlDbType.Date).Value = fechaReparto;

                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        Conexion.Cerrar();

                        var errores = new List<(string Empresa, string Mensaje, string Pedido)>();
                        foreach (DataRow row in dt.Rows)
                        {
                            string estado = row["Estado"]?.ToString() ?? "";
                            if (!string.Equals(estado, "OK", StringComparison.OrdinalIgnoreCase))
                            {
                                string empresa = row["Empresa"]?.ToString() ?? "";
                                string nroPedido = row["NroPedido"]?.ToString() ?? "";
                                string msg = row["Mensaje"]?.ToString() ?? "";
                                errores.Add((empresa, msg, nroPedido));
                            }
                        }

                        if (errores.Any())
                        {
                            // Agrupar por empresa y mensaje, listando pedidos separados por coma
                            var agrupado = new List<string>();
                            foreach (var grupo in errores.GroupBy(e => new { e.Empresa, e.Mensaje }))
                            {
                                string pedidos = string.Join(", ", grupo.Select(x => x.Pedido));
                                agrupado.Add($"{grupo.Key.Empresa}: {grupo.Key.Mensaje} ({pedidos})");
                            }

                            string mensaje = string.Join("\n", agrupado);
                            Console.WriteLine($"[DEBUG] ValidarPedidosCliente - Errores encontrados: {mensaje}");
                            await DisplayAlert("Validación rechazada", mensaje, "Ok");
                            return false;
                        }

                        Console.WriteLine("[DEBUG] ValidarPedidosCliente - Sin errores, se puede iniciar carga");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Conexion.Cerrar();
                Console.WriteLine($"[ERROR] ValidarPedidosCliente: {ex.Message}");
                await DisplayAlert("Error", "Error al validar pedidos: " + ex.Message, "Ok");
                return false;
            }
        }

        private async Task<bool> CrearSesion()
        {
            try
            {
                int totalBultos = despachos.Sum(d => d.TOTAL_BULTOS);
                Console.WriteLine($"[DEBUG] CrearSesion - Total bultos: {totalBultos}");
                Console.WriteLine($"[DEBUG] CrearSesion - IDCliente: {pedido.IDCLIENTE}");
                Console.WriteLine($"[DEBUG] CrearSesion - IDDireccion: {pedido.IDDIRECCION}");
                Console.WriteLine($"[DEBUG] CrearSesion - FechaReparto: {fechaReparto.ToString("yyyy-MM-dd")}");
                Console.WriteLine($"[DEBUG] CrearSesion - IDPicador: {idPicador}");

                Conexion.Abrir();
                using (var cmd = new SqlCommand("usp_SesionCarga_Crear", Conexion.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BaseDatos", SqlDbType.NVarChar, 10).Value = baseDatosDestino;
                    cmd.Parameters.Add("@IDCLIENTE", SqlDbType.NVarChar, 50).Value = pedido.IDCLIENTE;
                    cmd.Parameters.Add("@ID_DIRECCION", SqlDbType.NVarChar, 100).Value = pedido.IDDIRECCION;
                    cmd.Parameters.Add("@FECHA_REPARTO", SqlDbType.Date).Value = fechaReparto;
                    cmd.Parameters.Add("@USUARIO", SqlDbType.NVarChar, 50).Value = idPicador;
                    cmd.Parameters.Add("@TOTAL_BULTOS", SqlDbType.Int).Value = totalBultos;
                    cmd.Parameters.Add("@COD_VEHICULO", SqlDbType.NVarChar, 50).Value = pedido.COD_VEHICULO;

                    Console.WriteLine("[DEBUG] CrearSesion - Ejecutando SP usp_SesionCarga_Crear");
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        Conexion.Cerrar();

                        if (dt.Rows.Count > 0 && string.Equals(dt.Rows[0]["STATUS"]?.ToString(), "OK", StringComparison.OrdinalIgnoreCase))
                        {
                            idSesionActiva = Convert.ToInt32(dt.Rows[0]["ID_SESION"]);
                            Console.WriteLine($"[DEBUG] CrearSesion - Sesión creada exitosamente: {idSesionActiva}");
                            return true;
                        }
                    }

                    Console.WriteLine("[DEBUG] CrearSesion - SP no devolvió STATUS OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Conexion.Cerrar();
                Console.WriteLine($"[ERROR] CrearSesion: {ex.Message}");
                Console.WriteLine($"[ERROR] CrearSesion - StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        private async void entryBulto_Completed(object sender, EventArgs e)
        {
            string codigoBulto = entryBulto.Text?.Trim();
            if (string.IsNullOrEmpty(codigoBulto))
                return;

            await ProcesarBulto(codigoBulto);
        }

        private async void btnScannerBulto_Clicked(object sender, EventArgs e)
        {
            try
            {
                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                scanner.TopText = "Escanear código de bulto...";
                var result = await scanner.Scan();
                if (result != null)
                {
                    string codigoBulto = result.Text?.Trim();
                    if (!string.IsNullOrEmpty(codigoBulto))
                    {
                        await ProcesarBulto(codigoBulto);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async Task ProcesarBulto(string codigoBulto)
        {
            try
            {
                // Resetear colores
                frameBulto.BackgroundColor = Color.White;

                // Validar formato: ID_DESPACHO-NroBulto
                var partes = codigoBulto.Split('-');
                if (partes.Length < 2)
                {
                    frameBulto.BackgroundColor = Color.Red;
                    Vibration.Vibrate(TimeSpan.FromSeconds(0.5));
                    await DisplayAlert("Error", "Formato inválido. Use: ID_DESPACHO-BULTO-NroBulto", "Ok");
                    entryBulto.Text = "";
                    entryBulto.Focus();
                    return;
                }

                int idDespacho;
                int nroBulto;
                
                if (!int.TryParse(partes[0], out idDespacho) || !int.TryParse(partes[partes.Length - 1], out nroBulto))
                {
                    frameBulto.BackgroundColor = Color.Red;
                    Vibration.Vibrate(TimeSpan.FromSeconds(0.5));
                    await DisplayAlert("Error", "Formato inválido. Use: ID_DESPACHO-BULTO-NroBulto", "Ok");
                    entryBulto.Text = "";
                    entryBulto.Focus();
                    return;
                }

                // Buscar en la lista de bultos
                var bulto = bultos.FirstOrDefault(b => b.ID_DESPACHO == idDespacho && b.NumeroBulto == nroBulto);

                if (bulto == null)
                {
                    frameBulto.BackgroundColor = Color.Red;
                    Vibration.Vibrate(TimeSpan.FromSeconds(0.5));
                    await DisplayAlert("Error", "Bulto no encontrado en la lista", "Ok");
                    entryBulto.Text = "";
                    entryBulto.Focus();
                    return;
                }

                if (bulto.Estado == "CARGADO")
                {
                    frameBulto.BackgroundColor = Color.Red;
                    Vibration.Vibrate(TimeSpan.FromSeconds(0.5));
                    await DisplayAlert("Error", "Este bulto ya fue cargado", "Ok");
                    entryBulto.Text = "";
                    entryBulto.Focus();
                    return;
                }

                // Intentar registrar en BD
                bool exito = await RegistrarBultoCargado(idDespacho, nroBulto);

                if (exito)
                {
                    // Éxito - verde
                    frameBulto.BackgroundColor = Color.Green;
                    
                    // REFRESCAR DATOS DESDE BD PARA TENER ESTADO ACTUALIZADO
                    await CargarDespachos();
                    ActualizarResumen();
                    GenerarListaBultos();

                    // Verificar si finalizó todo
                    MostrarControlesSegunEstado();
                }
                else
                {
                    // Error en BD - vibrar y mensaje de ERROR DE RED
                    Vibration.Vibrate(TimeSpan.FromSeconds(0.5));
                    await DisplayAlert("ERROR DE RED", "No se pudo registrar en la base de datos. Verifique la conexión e intente nuevamente.", "Ok");
                }

                entryBulto.Text = "";
                entryBulto.Focus();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
                entryBulto.Text = "";
                entryBulto.Focus();
            }
        }

        private async Task<bool> RegistrarBultoCargado(int idDespacho, int nroBulto)
        {
            try
            {
              
                if (idSesionActiva == null)
                {
                      await DisplayAlert("Debug", $"  RegistrarBultoCargado - No hay sesión activa para registrar bulto {nroBulto} del despacho {idDespacho}", "OK");
                    Console.WriteLine("[ERROR] No hay sesión activa");
                    return false;
                }

                Conexion.Abrir();
                using (var cmd = new SqlCommand("usp_BultoCargado_Registrar", Conexion.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BaseDatos", SqlDbType.NVarChar, 10).Value = baseDatosDestino;
                    cmd.Parameters.Add("@ID_DESPACHO", SqlDbType.Int).Value = idDespacho;
                    cmd.Parameters.Add("@ID_BULTO", SqlDbType.Int).Value = nroBulto;
                    cmd.Parameters.Add("@ID_SESION", SqlDbType.Int).Value = idSesionActiva.Value;
                    cmd.Parameters.Add("@FECHA_REPARTO", SqlDbType.Date).Value = fechaReparto;
                    cmd.Parameters.Add("@USUARIO", SqlDbType.NVarChar, 50).Value = idPicador;
                    cmd.Parameters.Add("@COD_VEHICULO", SqlDbType.NVarChar, 50).Value = pedido.COD_VEHICULO;

                    // await DisplayAlert("Debug", $"RegistrarBultoCargado - Ejecutando SP usp_BultoCargado_Registrar\nParámetros: ID_DESPACHO={idDespacho}, ID_BULTO={nroBulto}, ID_SESION={idSesionActiva}, FECHA_REPARTO={fechaReparto.ToString("yyyy-MM-dd")}, USUARIO={idPicador}, COD_VEHICULO={pedido.COD_VEHICULO}", "OK");
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        Conexion.Cerrar();

                        return dt.Rows.Count > 0 && string.Equals(dt.Rows[0]["STATUS"]?.ToString(), "OK", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception)
            {
                Conexion.Cerrar();
                return false;
            }
        }

        private async void btnFinalizar_Clicked(object sender, EventArgs e)
        {
            // bool respuesta = await DisplayAlert("Confirmar", 
            //     "¿Desea finalizar la carga? Se volverá a la pantalla anterior.", 
            //     "Sí", "No");
            bool respuesta=true; // Para pruebas automáticas sin prompt

            if (respuesta)
            {
                // Finalizar sesión en BD
                bool sesionFinalizada = await FinalizarSesion();
                
                if (sesionFinalizada)
                {
                    // Volver y refrescar
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo finalizar la sesión. ¿Desea salir de todas formas?", "Ok");
                    await Navigation.PopAsync();
                }
            }
        }

        private async Task<bool> FinalizarSesion()
        {
            try
            {
                if (idSesionActiva == null)
                {
                    Console.WriteLine("[WARNING] No hay sesión activa para finalizar");
                    return true; // No es error crítico
                }

                Conexion.Abrir();
                using (var cmd = new SqlCommand("usp_SesionCarga_Finalizar", Conexion.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BaseDatos", SqlDbType.NVarChar, 10).Value = baseDatosDestino;
                    cmd.Parameters.Add("@ID_SESION", SqlDbType.Int).Value = idSesionActiva.Value;
                    cmd.Parameters.Add("@USUARIO", SqlDbType.NVarChar, 50).Value = idPicador;

                    Console.WriteLine("[DEBUG] FinalizarSesion - Ejecutando SP usp_SesionCarga_Finalizar");
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        Conexion.Cerrar();

                        var ok = dt.Rows.Count > 0 && string.Equals(dt.Rows[0]["STATUS"]?.ToString(), "OK", StringComparison.OrdinalIgnoreCase);
                        Console.WriteLine($"[DEBUG] Sesión {idSesionActiva} finalizada: {ok}");
                        return ok;
                    }
                }
            }
            catch (Exception ex)
            {
                Conexion.Cerrar();
                Console.WriteLine($"[ERROR] FinalizarSesion: {ex.Message}");
                return false;
            }
        }
    }

    public class DespachoDetalle
    {
        public string IDCLIENTE { get; set; }
        public string ID_DIRECCION { get; set; }
        public int ID_DESPACHO { get; set; }
        public int TOTAL_BULTOS { get; set; }
        public int TOTAL_BULTOS_CARGADOS { get; set; }
        public int TOTAL_CAJAS { get; set; }
        public int TOTAL_BOLSAS { get; set; }
    }

    public class BultoItem
    {
        public int ID_DESPACHO { get; set; }
        public int NumeroBulto { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public Color EstadoColor { get; set; }
    }
}
