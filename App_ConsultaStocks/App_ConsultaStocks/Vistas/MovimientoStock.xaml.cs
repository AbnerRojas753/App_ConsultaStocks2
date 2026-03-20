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
using App_ConsultaStocks.Helpers;
using ZXing.Net.Mobile.Forms;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovimientoStock : ContentPage
    {
        private const string SinUbicacionTexto = "(Sin ubicación)";
        private readonly int v_Id_CD;
        private readonly string _idAlmacen;
        // seleccionado en la lista
        private StockItem selectedStockItem = null;

        public MovimientoStock(int idCD, string idAlmacen)
        {
            InitializeComponent();
            v_Id_CD = idCD;
            _idAlmacen = idAlmacen;
            ActualizarEstadoUbicacion(null);

            // Suscribirse a notificación para recargar cuando se mueva stock
            MessagingCenter.Subscribe<MovimientoStockProcesar, string>(this, "StockMovido", (sender, ubicacion) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (!string.IsNullOrEmpty(ubicacion))
                        await CargarStockUbicacion(ubicacion);
                });
            });
        }

        private async void entryUbicacion_Completed(object sender, EventArgs e)
        {
            string ubicacion = entryUbicacion.Text?.Trim();
            if (string.IsNullOrEmpty(ubicacion))
                return;

            // Validar que la ubicación existe
            bool existe = await ValidarUbicacionExiste(ubicacion);
            if (existe)
            {
                ActualizarEstadoUbicacion(ubicacion);

                // Limpiar el entry para permitir escanear otra ubicación
                entryUbicacion.Text = "";

                // Cargar el stock completo de la ubicación
                await CargarStockUbicacion(ubicacion);

                // Mover focus al campo de artículo
                entryArticulo.Focus();
            }
            else
            {
                entryUbicacion.Text = "";
                await DisplayAlert("Error", "La ubicación no existe en el maestro de ubicaciones", "Ok");
                entryUbicacion.Focus();
            }
        }

        private async void btnScannerUbicacion_Clicked(object sender, EventArgs e)
        {
            try
            {
                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                scanner.TopText = "Leer código de barras...";
                var result = await scanner.Scan();
                if (result != null)
                {
                    string ubicacion = result.Text?.Trim();
                    if (string.IsNullOrEmpty(ubicacion))
                        return;

                    // Validar que la ubicación existe
                    bool existe = await ValidarUbicacionExiste(ubicacion);
                    if (existe)
                    {
                        ActualizarEstadoUbicacion(ubicacion);

                        // Limpiar el entry para permitir escanear otra ubicación
                        entryUbicacion.Text = "";

                        // Cargar el stock completo de la ubicación
                        await CargarStockUbicacion(ubicacion);

                        // Mover focus al campo de artículo
                        entryArticulo.Focus();
                    }
                    else
                    {
                        entryUbicacion.Text = "";
                        await DisplayAlert("Error", "La ubicación no existe en el maestro de ubicaciones", "Ok");
                        entryUbicacion.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async Task CargarStockUbicacion(string ubicacion)
        {
            try
            {
                DataTable dt = new DataTable();

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_CargarStockUbicacion", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = _idAlmacen;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 31).Value = ubicacion;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_CargarStockUbicacion", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = _idAlmacen;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 31).Value = ubicacion;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                List<StockItem> resultados = new List<StockItem>();
                foreach (DataRow row in dt.Rows)
                {
                    resultados.Add(new StockItem
                    {
                        ID_UBICACION = row.Table.Columns.Contains("ID_UBICACION") ? row["ID_UBICACION"].ToString() : ubicacion,
                        ID_ARTICULO = row["ID_ARTICULO"].ToString(),
                        ID_LOTE = row["ID_LOTE"].ToString(),
                        STOCK = Convert.ToDecimal(row["STOCK"]),
                        DESCRIPCION = row.Table.Columns.Contains("DESCRIPCION") ? row["DESCRIPCION"].ToString() : ""
                    });
                }

                CargarResultadosEnLista(resultados);

                if (resultados.Count == 0)
                {
                    await DisplayAlert("Info", "No hay stock en esta ubicación", "Ok");
                    entryArticulo.Focus();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async void btnBuscar_Clicked(object sender, EventArgs e)
        {
            await BuscarArticuloAsync();
        }

        private async void entryArticulo_Completed(object sender, EventArgs e)
        {
            await BuscarArticuloAsync();
        }

        private async Task BuscarArticuloAsync()
        {
            string codigoEscaneado = entryArticulo.Text?.Trim();
            string ubicacion = ObtenerUbicacionActiva();

            if (string.IsNullOrEmpty(codigoEscaneado))
            {
                await DisplayAlert("Error", "Ingrese código EAN/QR del artículo", "Ok");
                return;
            }

            // Usar BarCodeHelper para resolver el código al ID_ARTICULO
            var result = await BarCodeHelper.ProcesarCodigoProductoRefac(codigoEscaneado, v_Id_CD);
            if (!result.Exito)
            {
                await DisplayAlert("Error", result.Mensaje, "Ok");
                entryArticulo.Text = "";
                entryArticulo.Focus();
                return;
            }

            string idArticulo = result.IdArticulo;

            try
            {
                DataTable dt = string.IsNullOrEmpty(ubicacion)
                    ? await BuscarArticuloEnAlmacenAsync(idArticulo)
                    : await BuscarArticuloEnUbicacionAsync(idArticulo, ubicacion);

                if (dt.Rows.Count > 0)
                {
                    var resultados = new List<StockItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        resultados.Add(new StockItem
                        {
                            ID_UBICACION = row.Table.Columns.Contains("ID_UBICACION") ? row["ID_UBICACION"].ToString() : ubicacion,
                            ID_ARTICULO = row["ID_ARTICULO"].ToString(),
                            ID_LOTE = row["ID_LOTE"].ToString(),
                            STOCK = Convert.ToDecimal(row["STOCK"]),
                            DESCRIPCION = row.Table.Columns.Contains("DESCRIPCION") ? row["DESCRIPCION"].ToString() : ""
                        });
                    }

                    CargarResultadosEnLista(resultados);
                    entryArticulo.Text = "";
                    listViewResultados.Focus();
                }
                else
                {
                    entryArticulo.Text = "";
                    await DisplayAlert(
                        "Info",
                        string.IsNullOrEmpty(ubicacion)
                            ? "No se encontro stock para este articulo en ninguna ubicacion del almacen"
                            : "No hay stock en esta ubicación para este artículo",
                        "Ok");
                    entryArticulo.Focus();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private void listViewResultados_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e == null) return;
            selectedStockItem = e.Item as StockItem;
            // marcar la selección visualmente
            listViewResultados.SelectedItem = selectedStockItem;
        }

        private async void btnProcesar_Clicked(object sender, EventArgs e)
        {
            if (selectedStockItem == null)
            {
                await DisplayAlert("Error", "Seleccione una fila para procesar", "Ok");
                return;
            }

            string ubicacion = selectedStockItem.ID_UBICACION?.Trim();
            if (string.IsNullOrEmpty(ubicacion))
            {
                await DisplayAlert("Error", "La fila seleccionada no tiene ubicación origen", "Ok");
                return;
            }

            await Navigation.PushAsync(new MovimientoStockProcesar(selectedStockItem.ID_ARTICULO, selectedStockItem.ID_LOTE, selectedStockItem.STOCK, ubicacion, _idAlmacen, selectedStockItem.DESCRIPCION, v_Id_CD));
        }

        private void btnLimpiarUbicacion_Clicked(object sender, EventArgs e)
        {
            ActualizarEstadoUbicacion(null);
            entryUbicacion.Text = "";
            entryArticulo.Focus();
        }

        private string ObtenerUbicacionActiva()
        {
            string ubicacion = lblUbicacion.Text?.Trim();
            return string.IsNullOrEmpty(ubicacion) || ubicacion == SinUbicacionTexto ? null : ubicacion;
        }

        private void ActualizarEstadoUbicacion(string ubicacion)
        {
            bool tieneUbicacion = !string.IsNullOrWhiteSpace(ubicacion);

            lblUbicacion.Text = tieneUbicacion ? ubicacion : SinUbicacionTexto;
            lblUbicacion.TextColor = tieneUbicacion ? Color.Green : Color.Red;

            lblModoBusqueda.Text = tieneUbicacion
                ? "CON UBICACION ACTIVA - Busqueda filtrada por la ubicacion seleccionada"
                : "SIN UBICACION - Busqueda global por articulo en el almacen";
            lblModoBusqueda.TextColor = tieneUbicacion ? Color.DarkGreen : Color.Red;
        }

        private void CargarResultadosEnLista(List<StockItem> resultados)
        {
            selectedStockItem = null;
            listViewResultados.SelectedItem = null;
            listViewResultados.ItemsSource = resultados;
        }

        private async Task<DataTable> BuscarArticuloEnUbicacionAsync(string idArticulo, string ubicacion)
        {
            var dt = new DataTable();

            if (v_Id_CD == 1)
            {
                Conexion.Abrir_WMS_LIM();
                SqlCommand cmd = new SqlCommand("usp_BuscarArticuloEnUbicacion", Conexion.conectar_WMS_LIM);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = _idAlmacen;
                cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 31).Value = ubicacion;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = idArticulo;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar_WMS_LIM();
            }
            else
            {
                Conexion.Abrir_WMS_ATE();
                SqlCommand cmd = new SqlCommand("usp_BuscarArticuloEnUbicacion", Conexion.conectar_WMS_ATE);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = _idAlmacen;
                cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 31).Value = ubicacion;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = idArticulo;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar_WMS_ATE();
            }

            return dt;
        }

        private async Task<DataTable> BuscarArticuloEnAlmacenAsync(string idArticulo)
        {
            var dt = new DataTable();

            if (v_Id_CD == 1)
            {
                Conexion.Abrir_WMS_LIM();
                SqlCommand cmd = new SqlCommand("usp_BuscarArticuloEnAlmacen", Conexion.conectar_WMS_LIM);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = _idAlmacen;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = idArticulo;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar_WMS_LIM();
            }
            else
            {
                Conexion.Abrir_WMS_ATE();
                SqlCommand cmd = new SqlCommand("usp_BuscarArticuloEnAlmacen", Conexion.conectar_WMS_ATE);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = _idAlmacen;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = idArticulo;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar_WMS_ATE();
            }

            return dt;
        }

        private async Task<bool> ValidarUbicacionExiste(string idUbicacion)
        {
            try
            {
                DataTable dt = new DataTable();

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_ValidarUbicacionExiste", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = _idAlmacen;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 25).Value = idUbicacion;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_ValidarUbicacionExiste", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = _idAlmacen;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 25).Value = idUbicacion;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                return dt.Rows.Count > 0 && Convert.ToBoolean(dt.Rows[0]["EXISTE"]);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al validar ubicación: " + ex.Message, "Ok");
                return false;
            }
        }
    }

    public class StockItem
    {
        public string ID_UBICACION { get; set; }
        public string ID_ARTICULO { get; set; }
        public string ID_LOTE { get; set; }
        public decimal STOCK { get; set; }
        public string DESCRIPCION { get; set; }
    }
}
