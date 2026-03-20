using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using App_ConsultaStocks.Datos;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovimientoStockProcesar : ContentPage
    {
        private readonly string _idArticulo;
        private readonly string _idLote;
        private readonly decimal _stockOrigen;
        private readonly string _origenUbicacion;
        private readonly string _idAlmacen;
        private readonly string _origenTransito;
        private readonly int v_Id_CD;

        public MovimientoStockProcesar(string idArticulo, string idLote, decimal stockOrigen, string origenUbicacion, string idAlmacen, string descripcion, int idCD = 2)
        {
            InitializeComponent();

            _idArticulo = idArticulo ?? "";
            _idLote = idLote ?? "";
            _stockOrigen = stockOrigen;
            _origenUbicacion = origenUbicacion ?? "";
            _idAlmacen = idAlmacen ?? "013"; // por defecto ATE
            _origenTransito = origenUbicacion; // para notificación
            v_Id_CD = idCD;

            lbl_Articulo.Text = _idArticulo;
            lbl_Descripcion.Text = descripcion ?? "";
            lbl_Lote.Text = _idLote;
            lbl_Stock.Text = _stockOrigen.ToString("N2", CultureInfo.InvariantCulture);
            lbl_OrigenUbicacion.Text = _origenUbicacion;
            lbl_MaxCantidad.Text = $"Max: {_stockOrigen:N2}";
        }

        private async void txt_DestinoUbicacion_Completed(object sender, EventArgs e)
        {
            string ubicacion = txt_DestinoUbicacion.Text?.Trim();
            if (string.IsNullOrEmpty(ubicacion))
            {
                txt_DestinoLote.Focus();
                return;
            }

            // Validar que la ubicación existe en el maestro
            bool existeUbicacion = await ValidarUbicacionExiste(ubicacion, _idAlmacen);
            if (existeUbicacion)
            {
                txt_DestinoLote.Focus();
            }
            else
            {
                txt_DestinoUbicacion.Text = "";
                await DisplayAlert("Error", "La ubicación no existe en el maestro de ubicaciones", "Ok");
                txt_DestinoUbicacion.Focus();
            }
        }

        private async void txt_DestinoLote_Completed(object sender, EventArgs e)
        {
            string texto = txt_DestinoLote.Text?.Trim();
            if (string.IsNullOrEmpty(texto))
            {
                txt_CantidadMover.Focus();
                return;
            }

            // Extraer el lote si tiene formato separador+ean+separador+lote
            string loteExtraido = texto;
            if (texto.Contains("|"))
            {
                var parts = texto.Split('|');
                if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2]))
                {
                    loteExtraido = parts[2].Trim();
                }
            }

            // Validar que el lote existe para este artículo
            bool existeLote = await ValidarLoteExisteParaArticulo(loteExtraido, _idArticulo);
            if (existeLote)
            {
                txt_CantidadMover.Focus();
                txt_DestinoLote.Text = loteExtraido;
            }
            else
            {
                txt_DestinoLote.Text = "";
                await DisplayAlert("Error", "El lote no existe para este artículo en el maestro de lotes", "Ok");
                txt_DestinoLote.Focus();
            }
        }

        private void txt_CantidadMover_Completed(object sender, EventArgs e)
        {
            // Validar que no supere el stock origen y ajustar si es necesario
            if (decimal.TryParse(txt_CantidadMover.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
            {
                if (v > _stockOrigen)
                {
                    txt_CantidadMover.Text = _stockOrigen.ToString("N2", CultureInfo.InvariantCulture);
                    DisplayAlert("Advertencia", $"La cantidad no puede superar el stock disponible ({_stockOrigen:N2}). Se ajustó al máximo.", "Ok");
                }
            }
            btnMover.Focus();
        }

        private async void btnLeerDestino_Clicked(object sender, EventArgs e)
        {
            try
            {
                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                scanner.TopText = "Leer codigo de barras...";
                var result = await scanner.Scan();
                if (result != null)
                {
                    string ubicacion = result.Text?.Trim();
                    if (string.IsNullOrEmpty(ubicacion))
                        return;

                    // Validar que la ubicación existe en el maestro
                    bool existeUbicacion = await ValidarUbicacionExiste(ubicacion, _idAlmacen);
                    if (existeUbicacion)
                    {
                        txt_DestinoUbicacion.Text = ubicacion;
                        try { txt_DestinoLote.Focus(); } catch { }
                    }
                    else
                    {
                        txt_DestinoUbicacion.Text = "";
                        await DisplayAlert("Error", "La ubicación no existe en el maestro de ubicaciones", "Ok");
                        txt_DestinoUbicacion.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async void btnLeerDestinoLote_Clicked(object sender, EventArgs e)
        {
            try
            {
                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                scanner.TopText = "Leer lote...";
                var result = await scanner.Scan();
                if (result != null)
                {
                    string texto = result.Text?.Trim();
                    if (string.IsNullOrEmpty(texto))
                        return;

                    // Extraer el lote si tiene formato separador+ean+separador+lote
                    string loteExtraido = texto;
                    if (texto.Contains("|"))
                    {
                        var parts = texto.Split('|');
                        if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2]))
                        {
                            loteExtraido = parts[2].Trim();
                        }
                    }

                    // Validar que el lote existe para este artículo
                    bool existeLote = await ValidarLoteExisteParaArticulo(loteExtraido, _idArticulo);
                    if (existeLote)
                    {
                        txt_DestinoLote.Text = loteExtraido;
                        try { txt_CantidadMover.Focus(); } catch { }
                    }
                    else
                    {
                        txt_DestinoLote.Text = "";
                        await DisplayAlert("Error", "El lote no existe para este artículo en el maestro de lotes", "Ok");
                        txt_DestinoLote.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async Task<bool> ValidarUbicacionExiste(string idUbicacion, string idAlmacen)
        {
            try
            {
                DataTable dt = new DataTable();

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_ValidarUbicacionExiste", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = idAlmacen;
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
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = idAlmacen;
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

        private async Task<bool> ValidarLoteExisteParaArticulo(string idLote, string idArticulo)
        {
            try
            {
                DataTable dt = new DataTable();

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_ValidarLoteExisteParaArticulo", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = idArticulo;
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 150).Value = idLote;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_ValidarLoteExisteParaArticulo", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = idArticulo;
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 150).Value = idLote;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                return dt.Rows.Count > 0 && Convert.ToBoolean(dt.Rows[0]["EXISTE"]);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al validar lote: " + ex.Message, "Ok");
                return false;
            }
        }

        private async void btnMover_Clicked(object sender, EventArgs e)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txt_DestinoUbicacion.Text))
            {
                await DisplayAlert("Advertencia", "Debe ingresar la ubicación de destino.", "Ok");
                return;
            }

            if (string.IsNullOrWhiteSpace(txt_CantidadMover.Text))
            {
                await DisplayAlert("Advertencia", "Debe ingresar la cantidad a mover", "Ok");
                return;
            }

            if (!decimal.TryParse(txt_CantidadMover.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal cantidad))
            {
                await DisplayAlert("Advertencia", "La cantidad debe ser un número válido", "Ok");
                return;
            }

            if (cantidad <= 0)
            {
                await DisplayAlert("Advertencia", "La cantidad debe ser mayor a cero", "Ok");
                return;
            }

            string loteDestino = (txt_DestinoLote.Text ?? "").Trim();
            string loteOrigen = (_idLote ?? "").Trim();

            // Permitir mover a lote distinto si el lote destino existe para el artículo.
            if (!string.Equals(loteOrigen, loteDestino, StringComparison.OrdinalIgnoreCase))
            {
                // Si no se ingresó lote destino, pedir que lo ingrese
                if (string.IsNullOrWhiteSpace(loteDestino))
                {
                    await DisplayAlert("Advertencia", "Debe ingresar el lote de destino.", "Ok");
                    return;
                }

                // Validar existencia del lote destino para el artículo
                bool existeLoteDestino = await ValidarLoteExisteParaArticulo(loteDestino, _idArticulo);
                if (!existeLoteDestino)
                {
                    await DisplayAlert("Advertencia", "El lote de destino no existe para este artículo.", "Ok");
                    return;
                }

                // Confirmación del usuario por seguridad (evita errores humanos)
                bool confirmar = await DisplayAlert("Confirmar", $"Mover {cantidad:N2}u desde lote '{loteOrigen}' hacia lote '{loteDestino}'?", "Sí", "No");
                if (!confirmar) return;
            }

            // Ejecutar movimiento mediante stored procedure centralizada (valida FK, crea lote destino si corresponde)
            try
            {
                // Validar que la ubicación destino existe en maestro (refuerzo adicional)
                bool existeUbicDestino = await ValidarUbicacionExiste(txt_DestinoUbicacion.Text.Trim(), _idAlmacen);
                if (!existeUbicDestino)
                {
                    await DisplayAlert("Error", "La ubicación destino no existe en el maestro de ubicaciones.", "Ok");
                    return;
                }

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    using (SqlCommand cmd = new SqlCommand("usp_MoverStock_Entre_Lotes", Conexion.conectar_WMS_LIM))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ID_ARTICULO", _idArticulo);
                        cmd.Parameters.AddWithValue("@ID_ALMACEN", _idAlmacen);
                        cmd.Parameters.AddWithValue("@ID_UBICACION_ORIG", _origenUbicacion);
                        cmd.Parameters.AddWithValue("@ID_LOTE_ORIG", _idLote ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ID_UBICACION_DEST", txt_DestinoUbicacion.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID_LOTE_DEST", loteDestino ?? string.Empty);
                        cmd.Parameters.AddWithValue("@CANTIDAD", cantidad);
                        cmd.Parameters.AddWithValue("@USUARIO", Environment.UserName ?? string.Empty);
                        cmd.Parameters.AddWithValue("@CREAR_LOTE_DESTINO", 1);

                        DataTable dt = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);

                        Conexion.Cerrar_WMS_LIM();

                        if (dt.Rows.Count > 0 && dt.Columns.Contains("OUT_STATUS") && dt.Rows[0]["OUT_STATUS"].ToString().Equals("ERROR", StringComparison.OrdinalIgnoreCase))
                        {
                            string msg = dt.Rows[0].ItemArray.Length > 1 ? dt.Rows[0][1].ToString() : "Error en movimiento";
                            await DisplayAlert("Error", msg, "Ok");
                            return;
                        }
                    }
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    using (SqlCommand cmd = new SqlCommand("usp_MoverStock_Entre_Lotes", Conexion.conectar_WMS_ATE))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ID_ARTICULO", _idArticulo);
                        cmd.Parameters.AddWithValue("@ID_ALMACEN", _idAlmacen);
                        cmd.Parameters.AddWithValue("@ID_UBICACION_ORIG", _origenUbicacion);
                        cmd.Parameters.AddWithValue("@ID_LOTE_ORIG", _idLote ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ID_UBICACION_DEST", txt_DestinoUbicacion.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID_LOTE_DEST", loteDestino ?? string.Empty);
                        cmd.Parameters.AddWithValue("@CANTIDAD", cantidad);
                        cmd.Parameters.AddWithValue("@USUARIO", Environment.UserName ?? string.Empty);
                        cmd.Parameters.AddWithValue("@CREAR_LOTE_DESTINO", 1);

                        DataTable dt = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dt);

                        Conexion.Cerrar_WMS_ATE();

                        if (dt.Rows.Count > 0 && dt.Columns.Contains("OUT_STATUS") && dt.Rows[0]["OUT_STATUS"].ToString().Equals("ERROR", StringComparison.OrdinalIgnoreCase))
                        {
                            string msg = dt.Rows[0].ItemArray.Length > 1 ? dt.Rows[0][1].ToString() : "Error en movimiento";
                            await DisplayAlert("Error", msg, "Ok");
                            return;
                        }
                    }
                }

                // Notificar para recargar
                MessagingCenter.Send(this, "StockMovido", _origenTransito ?? "");

                await DisplayAlert("Información", "Stock movido correctamente.", "Ok");
                await Navigation.PopAsync();
                return;
            }
            catch (Exception ex)
            {
                try { Conexion.Cerrar_WMS_ATE(); } catch { }
                await DisplayAlert("Error", ex.Message, "Ok");
                return;
            }
        }
    }
}