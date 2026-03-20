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
        private readonly int v_Id_CD;
        private readonly string _idAlmacen;
        // seleccionado en la lista
        private StockItem selectedStockItem = null;

        public MovimientoStock(int idCD, string idAlmacen)
        {
            InitializeComponent();
            v_Id_CD = idCD;
            _idAlmacen = idAlmacen;

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
                // Guardar la ubicación activa en el label
                lblUbicacion.Text = ubicacion;
                lblUbicacion.TextColor = Color.Green;

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
                        // Guardar la ubicación activa en el label
                        lblUbicacion.Text = ubicacion;
                        lblUbicacion.TextColor = Color.Green;

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
                        ID_ARTICULO = row["ID_ARTICULO"].ToString(),
                        ID_LOTE = row["ID_LOTE"].ToString(),
                        STOCK = Convert.ToDecimal(row["STOCK"]),
                        DESCRIPCION = row.Table.Columns.Contains("DESCRIPCION") ? row["DESCRIPCION"].ToString() : ""
                    });
                }

                // Limpiar selección anterior al cargar nueva lista
                selectedStockItem = null;
                listViewResultados.ItemsSource = resultados;

                if (resultados.Count == 0)
                {
                    lblUbicacion.Text = "(Sin ubicación)";
                    lblUbicacion.TextColor = Color.Red;
                    await DisplayAlert("Info", "No hay stock en esta ubicación", "Ok");
                    entryUbicacion.Focus();
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
            string ubicacion = lblUbicacion.Text?.Trim();

            // Validar que hay una ubicación activa
            if (string.IsNullOrEmpty(ubicacion) || ubicacion == "(Sin ubicación)")
            {
                await DisplayAlert("Error", "Primero debe seleccionar una ubicación", "Ok");
                entryUbicacion.Focus();
                return;
            }

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
                DataTable dt = new DataTable();

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

                int Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<StockItem> resultados = new List<StockItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        resultados.Add(new StockItem
                        {
                            ID_ARTICULO = row["ID_ARTICULO"].ToString(),
                            ID_LOTE = row["ID_LOTE"].ToString(),
                            STOCK = Convert.ToDecimal(row["STOCK"]),
                            DESCRIPCION = row.Table.Columns.Contains("DESCRIPCION") ? row["DESCRIPCION"].ToString() : ""
                        });
                    }

                    // Limpiar selección anterior al filtrar lista
                    selectedStockItem = null;
                    listViewResultados.ItemsSource = resultados;

                    // Limpiar el entry de artículo para permitir buscar otro
                    entryArticulo.Text = "";
                    listViewResultados.Focus();
                }
                else
                {
                    // No limpiar la lista completa, mantener el stock de la ubicación
                    // Solo limpiar el entry de artículo para permitir buscar otro
                    entryArticulo.Text = "";
                    await DisplayAlert("Info", "No hay stock en esta ubicación para este artículo", "Ok");
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

            string ubicacion = lblUbicacion.Text?.Trim();
            if (string.IsNullOrEmpty(ubicacion) || ubicacion == "(Sin ubicación)")
            {
                await DisplayAlert("Error", "No hay una ubicación activa", "Ok");
                entryUbicacion.Focus();
                return;
            }

            // Navegar a la pantalla de procesamiento pasando los datos necesarios (incluye descripción)
            await Navigation.PushAsync(new MovimientoStockProcesar(selectedStockItem.ID_ARTICULO, selectedStockItem.ID_LOTE, selectedStockItem.STOCK, ubicacion, _idAlmacen, selectedStockItem.DESCRIPCION, v_Id_CD));
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
        public string ID_ARTICULO { get; set; }
        public string ID_LOTE { get; set; }
        public decimal STOCK { get; set; }
        public string DESCRIPCION { get; set; }
    }
}