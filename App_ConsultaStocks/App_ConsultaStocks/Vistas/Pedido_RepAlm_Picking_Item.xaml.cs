using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;


using System.Data.SqlClient;
using System.Data;
using ZXing.Net.Mobile.Forms;
using App_ConsultaStocks.Datos;
using App_ConsultaStocks.Helpers;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Pedido_RepAlm_Picking_Item : ContentPage
    {
        int v_Id_CD = 0;
        string v_Id_Alm = "";
        int v_CANT_LINEAS_RUTA = 0;
        int v_IdContador_Ruta = 0;
        int v_IdPicking = 0;
        int v_TipoPicking = 1;
        string v_Filtro = "";
        string v_Tipo = "";
        int var_OrdenReal = 0;
        int v_id_Picador = 0;

        private class SugerenciaPick
        {
            public string Ubicacion { get; set; }
            public string Lote { get; set; }
            public string Stock { get; set; }
        }

        private List<SugerenciaPick> ObtenerSugerenciasPick(DataTable dt)
        {
            var sugerencias = new List<SugerenciaPick>();
            if (dt == null || dt.Rows.Count == 0)
            {
                return sugerencias;
            }

            DataRow row = dt.Rows[0];
            for (int i = 1; i <= 5; i++)
            {
                string colUbic = $"SUG{i}_UBICACION";
                string colLote = $"SUG{i}_LOTE";
                string colStock = $"SUG{i}_STOCK";

                if (!dt.Columns.Contains(colUbic))
                {
                    continue;
                }

                string ubicacion = row[colUbic]?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(ubicacion))
                {
                    continue;
                }

                sugerencias.Add(new SugerenciaPick
                {
                    Ubicacion = ubicacion,
                    Lote = dt.Columns.Contains(colLote) ? (row[colLote]?.ToString()?.Trim() ?? string.Empty) : string.Empty,
                    Stock = dt.Columns.Contains(colStock) ? (row[colStock]?.ToString()?.Trim() ?? string.Empty) : string.Empty
                });
            }

            return sugerencias;
        }

        private async Task<bool> MostrarSugerenciasPick(string message, List<SugerenciaPick> sugerencias)
        {
            if (sugerencias == null || sugerencias.Count == 0)
            {
                return false;
            }

            var opciones = sugerencias
                .Select(s => $"Ubic: {s.Ubicacion} | Lote: {s.Lote} | Stock: {s.Stock}")
                .ToArray();

            string seleccion = await DisplayActionSheet(message, "Cancelar", null, opciones);
            if (string.IsNullOrWhiteSpace(seleccion) || seleccion == "Cancelar")
            {
                return false;
            }

            int index = Array.IndexOf(opciones, seleccion);
            if (index < 0)
            {
                return false;
            }

            var opcion = sugerencias[index];
            txt_IdUbicacion.Text = opcion.Ubicacion;
            txt_IdLote.Text = opcion.Lote;
            txt_IdUbicacion.IsReadOnly = false;
            txt_IdLote.IsReadOnly = false;
            txt_IdUbicacion.BackgroundColor = Color.LawnGreen;
            txt_IdLote.BackgroundColor = Color.LawnGreen;

            bool mostrarLote = !string.IsNullOrWhiteSpace(opcion.Lote) || !string.IsNullOrWhiteSpace(lbl_IdLote.Text);
            stkLote.IsVisible = mostrarLote;
            lbl_Lote.IsVisible = mostrarLote;

            txt_Cantidad.Focus();
            return true;
        }

        public Pedido_RepAlm_Picking_Item(Model_Pedido_Picking _Model_PedidoPicking, string Filtro, string Tipo, string orden)
        {
            InitializeComponent();

            btnAnterior.Text = " < < < ";
            v_IdPicking = _Model_PedidoPicking.ID_PICKING;
            v_Filtro = Filtro;
            v_Tipo = Tipo;
            v_Id_CD = _Model_PedidoPicking.ID_CD;
            v_Id_Alm = _Model_PedidoPicking.ID_ALM;
            v_TipoPicking = 1;
            v_CANT_LINEAS_RUTA = _Model_PedidoPicking.CANT_LINEAS_RUTA;
            lbl_CANT_LINEAS_RUTA.Text = v_CANT_LINEAS_RUTA.ToString();
            v_id_Picador = _Model_PedidoPicking.ID_PICADOR;
            lbl_Orden.Text = orden;
            v_IdContador_Ruta = int.Parse(lbl_Orden.Text);
            Picking_SelDatosRutaxOrden(v_IdPicking, int.Parse(lbl_Orden.Text));
            txt_IdUbicacion.Focus();
        }

        private void Vibrar()
        {
            var duracion = TimeSpan.FromSeconds(0.5);
            Vibration.Vibrate(duracion);
        }


        private void btnAnterior_Clicked(object sender, EventArgs e)
        {

            InicializeControls();
            if (v_IdContador_Ruta > 1)
            {
                v_IdContador_Ruta = v_IdContador_Ruta - 1;
                lbl_Orden.Text = v_IdContador_Ruta.ToString();
                Picking_SelDatosRutaxOrden(v_IdPicking, int.Parse(lbl_Orden.Text));
            }


        }

        private void btnSiguiente_Clicked(object sender, EventArgs e)
        {

            InicializeControls();

            if (v_IdContador_Ruta < v_CANT_LINEAS_RUTA)
            {
                v_IdContador_Ruta = v_IdContador_Ruta + 1;
                lbl_Orden.Text = v_IdContador_Ruta.ToString();
                Picking_SelDatosRutaxOrden(v_IdPicking, int.Parse(lbl_Orden.Text));

                if (Chk_SoloPendientes.IsChecked == true)
                {
                    while (lbl_CantUND.Text == lbl_AtendidoUND.Text)
                    {
                        v_IdContador_Ruta = v_IdContador_Ruta + 1;
                        lbl_Orden.Text = v_IdContador_Ruta.ToString();
                        Picking_SelDatosRutaxOrden(v_IdPicking, int.Parse(lbl_Orden.Text));
                    }
                }




            }
        }


        private void txt_IdUbicacion_Completed(object sender, EventArgs e)
        {

            Ubicaciones_SelDatosUbicacion(v_Id_Alm, txt_IdUbicacion.Text);
        }


        private async void btnLeerFotoUbicacion_Clicked(object sender, EventArgs e)
        {
            try
            {
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txt_IdUbicacion.Text = result.Text;
                    Ubicaciones_SelDatosUbicacion(v_Id_Alm, txt_IdUbicacion.Text);
                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }
        }


        private async void txt_IdProducto_Completed(object sender, EventArgs e)
        {
            await BuscarProductos_BarCode2(txt_IdProducto.Text, v_Id_CD);

        }

        private void txt_IdLote_Completed(object sender, EventArgs e)
        {
            // Solo validar si hay un lote esperado
            if (!string.IsNullOrWhiteSpace(lbl_IdLote.Text) && !string.IsNullOrWhiteSpace(txt_IdLote.Text))
            {
                if (string.Equals(txt_IdLote.Text.Trim(), lbl_IdLote.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    txt_IdLote.BackgroundColor = Color.LawnGreen;
                    txt_Cantidad.Focus();
                }
                else
                {
                    txt_IdLote.BackgroundColor = Color.Red;
                    txt_Cantidad.Focus();
                }
            }
            else
            {
                txt_Cantidad.Focus();
            }
        }


        private async void btnLeerFotoProducto_Clicked(object sender, EventArgs e)
        {
            try
            {
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txt_IdProducto.Text = result.Text;
                    await BuscarProductos_BarCode2(txt_IdProducto.Text, v_Id_CD);
                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async void btnLeerFotoLote_Clicked(object sender, EventArgs e)
        {
            try
            {
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txt_IdLote.Text = result.Text;
                    if (!string.IsNullOrWhiteSpace(lbl_IdLote.Text) && !string.IsNullOrWhiteSpace(txt_IdLote.Text))
                    {
                        if (txt_IdLote.Text.Trim() == lbl_IdLote.Text.Trim())
                        {
                            txt_IdLote.BackgroundColor = Color.LawnGreen;
                            txt_Cantidad.Focus();
                        }
                        else
                        {
                            txt_IdLote.BackgroundColor = Color.Red;
                            DisplayAlert("Error", "El lote no coincide con el esperado.", "Ok");
                            txt_IdLote.Focus();
                        }
                    }
                    else
                    {
                        txt_Cantidad.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }
        }



        private void btnLimpiar_Clicked(object sender, EventArgs e)
        {

            InicializeControls();
            ////txt_IdUbicacion.Text = "";
            ////txt_IdProducto.Text = "";
            ////txt_Cantidad.Text = "";

            ////txt_IdProducto.BackgroundColor = Color.White;
            ////txt_IdUbicacion.BackgroundColor = Color.White;

            ////txt_IdProducto.IsReadOnly = false;
            ////txt_IdUbicacion.IsReadOnly = false;

            ////txt_IdUbicacion.Focus();
        }

        private void InicializeControls()
        {
            txt_IdUbicacion.Text = "";
            txt_IdProducto.Text = "";
            txt_Cantidad.Text = "";
            txt_IdLote.Text = "";

            txt_IdProducto.BackgroundColor = Color.White;
            txt_IdUbicacion.BackgroundColor = Color.White;
            txt_IdLote.BackgroundColor = Color.White;

            txt_IdProducto.IsReadOnly = false;
            txt_IdUbicacion.IsReadOnly = false;
            txt_IdLote.IsReadOnly = false;

            txt_IdUbicacion.Focus();
        }


        private void txt_Cantidad_Completed(object sender, EventArgs e)
        {
            btn_GuardarPick.Focus();
        }



        private async void btn_GuardarPick_Clicked(object sender, EventArgs e)
        {

            if (txt_IdUbicacion.Text is null || txt_IdUbicacion.Text.Trim() == "")
            {
                await DisplayAlert("Error!", "Debe ingresar ubicación!", "Ok");
                return;
            }

            if (txt_IdProducto.Text is null || txt_IdProducto.Text.Trim() == "")
            {
                await DisplayAlert("Error!", "Debe ingresar producto!", "Ok");
                return;
            }

            if (txt_Cantidad.Text is null || txt_Cantidad.Text.Trim() == "")
            {
                await DisplayAlert("Error!", "Debe ingresar cantidad!", "Ok");
                return;
                txt_Cantidad.Focus();
            }

            if (txt_IdLote.Text is null)
            {
                txt_IdLote.Text = "";
            }

            if (lbl_Orden.Text == "0")
            {
                if (int.Parse(txt_Cantidad.Text) > (int.Parse(lbl_CantUND.Text) - int.Parse(lbl_AtendidoUND.Text)))
                {
                    await DisplayAlert("Error!", "Cantidad excede lo pedido!!", "Ok");
                    return;
                    txt_Cantidad.Focus();
                }
            }
            if (int.Parse(txt_Cantidad.Text) > int.Parse(lbl_CantUND.Text))
            {
                await DisplayAlert("Error!", "Cantidad excede lo pedido!!", "Ok");
                return;
                txt_Cantidad.Focus();
            }
            else
            {

                Boolean Ubic_Ok; Boolean Pick_Ok;
                Ubic_Ok = true; Pick_Ok = true;

                if (txt_IdUbicacion.BackgroundColor == Color.Red) { Ubic_Ok = false; }
                if (int.Parse(lbl_CantUND.Text) != int.Parse(txt_Cantidad.Text)) { Pick_Ok = false; }

                // Lógica: ID_UBICACION e ID_LOTE = lo que sugiere la ruta (lbl_*)
                //         ID_UBICACION_NEW e ID_LOTE_NEW = lo que ingresa el usuario SIEMPRE (txt_*)
                bool guardadoExitoso = await Picking_Det_InsDatos(v_IdPicking,
                    lbl_IdUbicacion_Ruta.Text,              // ID_UBICACION (sugerido por ruta)
                    int.Parse(lbl_ITEMPEDIDO.Text),
                    txt_IdProducto.Text,
                    int.Parse(txt_Cantidad.Text),
                    var_OrdenReal,
                    Ubic_Ok,
                    Pick_Ok,
                    txt_IdUbicacion.Text.Trim(),            // ID_UBICACION_NEW (ingresado por usuario)
                    lbl_IdLote.Text,                        // ID_LOTE (sugerido por ruta)
                    txt_IdLote.Text.Trim());                // ID_LOTE_NEW (ingresado por usuario)

                // Si hubo error en el guardado, detener aquí y no avanzar
                if (!guardadoExitoso)
                {
                    return;
                }

                // Solo si el guardado fue exitoso, limpiar y avanzar
                string v_IdUbicacion_Ant = "";
                v_IdUbicacion_Ant = txt_IdUbicacion.Text.Trim();

                txt_IdUbicacion.Text = "";
                txt_IdProducto.Text = "";
                txt_Cantidad.Text = "";
                txt_IdLote.Text = "";

                txt_IdProducto.BackgroundColor = Color.White;
                txt_IdUbicacion.BackgroundColor = Color.White;
                txt_IdLote.BackgroundColor = Color.White;

                txt_IdProducto.IsReadOnly = false;
                txt_IdUbicacion.IsReadOnly = false;
                txt_IdLote.IsReadOnly = false;

                //aqui pasa al siguiente item
                if (v_IdContador_Ruta < v_CANT_LINEAS_RUTA)
                {
                    v_IdContador_Ruta = v_IdContador_Ruta + 1;
                    lbl_Orden.Text = v_IdContador_Ruta.ToString();
                    Picking_SelDatosRutaxOrden(v_IdPicking, int.Parse(lbl_Orden.Text));

                    if (v_IdUbicacion_Ant == lbl_IdUbicacion_Ruta.Text.Trim())
                    {
                        txt_IdUbicacion.Text = v_IdUbicacion_Ant;
                        txt_IdProducto.Focus();
                    }
                    else
                    {
                        txt_IdUbicacion.Focus();
                    }

                    //////Aqui pongo colores a los caracteres diferentes
                    ////string[] Ubic_Anterior = v_IdUbicacion_Ant.Split('.');
                    ////string[] Ubic_Nueva = lbl_IdUbicacion_Ruta.Text.Split('.');

                    ////int v_Count_Segmentos_Max_Compare = 0;
                    ////v_Count_Segmentos_Max_Compare = Ubic_Anterior.Count();
                    ////if (Ubic_Nueva.Count() > Ubic_Anterior.Count() ) { v_Count_Segmentos_Max_Compare = Ubic_Nueva.Count(); };

                    ////for (int i = 0; i < v_Count_Segmentos_Max_Compare; i++)
                    ////{
                    ////    if (Ubic_Anterior[i].ToString() != Ubic_Nueva[i].ToString())
                    ////    {
                    ////        DisplayAlert("Error", Ubic_Anterior[i].ToString(), "Ok");
                    ////        DisplayAlert("Error", Ubic_Nueva[i].ToString(), "Ok");
                    ////    }
                    ////}

                    //foreach (string SegmentosUbicAnt in Ubic_Anterior)
                    //{
                    //    Console.WriteLine(SegmentosUbicAnt);
                    //}

                }
                else
                {
                    if (v_IdContador_Ruta == v_CANT_LINEAS_RUTA)
                    {
                        await Navigation.PopAsync();
                    }
                }


            }

        }

        private async void btnEliminar_Clicked(object sender, EventArgs e)
        {
            //bool answer = await DisplayAlert("Eliminar dato?", "Esta seguro de eliminar ", "Sí", "No");
            //if (answer == true)
            //{
            //    alfa_usp_Picking_Det_DelDatos(v_IdPicking, int.Parse(lbl_ITEMPEDIDO.Text), int.Parse(lbl_Orden.Text));
            //    Picking_SelDatosRutaxOrden(v_IdPicking, int.Parse(lbl_Orden.Text));
            //    InicializeControls();
            //}
        }



        //Metodos de Base de datos

        private void Picking_SelDatosRutaxOrden(int ID_PICKING, int ORDEN)

        {
            try
            {

                Conexion.Abrir();

                SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Seleccionar_Ruta", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPicking", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@ORDEN", SqlDbType.Int).Value = ORDEN;
                cmd.Parameters.Add("@FILTRO", SqlDbType.VarChar, 150).Value = v_Filtro;
                cmd.Parameters.Add("@TIPO", SqlDbType.VarChar, 150).Value = v_Tipo;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                int Filas = 0;
                Filas = dt.Rows.Count;
                if (Filas > 0)
                {
                    var_OrdenReal = Convert.ToInt32(dt.Rows[0][1]);
                    lbl_IdUbicacion_Ruta.Text = Convert.ToString(dt.Rows[0][2]);
                    lbl_ITEMPEDIDO.Text = Convert.ToString(dt.Rows[0][3]);
                    lbl_IdArticulo.Text = Convert.ToString(dt.Rows[0][4]);
                    lbl_DescArticulo.Text = Convert.ToString(dt.Rows[0][5]);
                    lbl_CantUND.Text = Convert.ToString(dt.Rows[0][6]);
                    //lbl_CantMst.Text = Convert.ToString(dt.Rows[0][7]);
                    //lbl_Equivalencias.Text = Convert.ToString(dt.Rows[0][8]);
                    lbl_AtendidoUND.Text = Convert.ToString(dt.Rows[0][9]);
                    // Columna 10 es ID_LOTE2 (filler), columna 11 es row_num, columna 12 es ID_LOTE
                    lbl_IdLote.Text = Convert.ToString(dt.Rows[0][12]);

                    // Manejar visibilidad de controles de lote
                    if (string.IsNullOrWhiteSpace(lbl_IdLote.Text))
                    {
                        stkLote.IsVisible = false;
                        lbl_Lote.IsVisible = false;
                    }
                    else
                    {
                        stkLote.IsVisible = true;
                        lbl_Lote.IsVisible = true;
                    }

                    Picking_SelCantSKUsxUbicacion(v_IdPicking, lbl_IdUbicacion_Ruta.Text);
                    Picking_SelDatosAdicionalesxItem(v_IdPicking, v_Id_Alm, int.Parse(lbl_ITEMPEDIDO.Text), lbl_IdArticulo.Text, int.Parse(lbl_Orden.Text));
                }
                else
                {
                    DisplayAlert("Error", "No existen datos", "Ok");
                }

                Conexion.Cerrar();

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async void Ubicaciones_SelDatosUbicacion(string ID_ALMACEN, string ID_UBICACION)
        {
            try
            {
                DataTable dt = new DataTable();

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_SelDatosUbicacion", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 150).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 150).Value = ID_UBICACION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_SelDatosUbicacion", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 150).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 150).Value = ID_UBICACION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    if (lbl_IdUbicacion_Ruta.Text.TrimEnd() != txt_IdUbicacion.Text)
                    {
                        Vibrar();
                        txt_IdUbicacion.BackgroundColor = Color.Red;

                    }

                    txt_IdProducto.Focus();
                    txt_IdUbicacion.IsReadOnly = true;

                }
                else
                {
                    txt_IdUbicacion.Text = "";
                    Vibrar();
                    await DisplayAlert("Error!", "Ubicacion no existe!", "Ok");
                    txt_IdUbicacion.Focus();
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async void BuscarProductos_BarCode(string CodBar, int Id_CD)
        {
            try
            {
                DataTable dt = new DataTable();

                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_BuscarProductos_BarCode", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CodBar", SqlDbType.VarChar, 31).Value = CodBar;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_BuscarProductos_BarCode", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CodBar", SqlDbType.VarChar, 31).Value = CodBar;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    if (Filas == 1)
                    {

                        txt_IdProducto.Text = Convert.ToString(dt.Rows[0][0]);

                        if (txt_IdProducto.Text == lbl_IdArticulo.Text)
                        {
                            txt_IdProducto.BackgroundColor = Color.LawnGreen;
                            txt_IdProducto.IsReadOnly = true;
                            VerifExistenciaIdArticuloUbicacion(txt_IdProducto.Text, v_Id_Alm, txt_IdUbicacion.Text, v_Id_CD);
                            txt_Cantidad.Focus();
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Producto ERRADO !", "Ok");
                            Vibrar();
                            txt_IdProducto.BackgroundColor = Color.White;
                            txt_IdUbicacion.BackgroundColor = Color.White;
                            txt_IdProducto.Text = "";
                            txt_IdProducto.Focus();
                        }

                    }
                    else
                    {
                        DisplayAlert("Error", "Existe más de un producto con el mismo CodBar", "Ok");
                        Vibrar();
                        txt_IdProducto.BackgroundColor = Color.White;
                        txt_IdUbicacion.BackgroundColor = Color.White;
                        txt_IdProducto.Text = "";
                        txt_IdProducto.Focus();

                    }
                }
                else
                {
                    await DisplayAlert("Error!", "Producto no existe!", "Ok");
                    Vibrar();
                    txt_IdProducto.BackgroundColor = Color.White;
                    txt_IdUbicacion.BackgroundColor = Color.White;
                    txt_IdProducto.Text = "";
                    txt_IdProducto.Focus();

                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        // Nueva versión que reutiliza el helper centralizado
        private async Task BuscarProductos_BarCode2(string CodBar, int Id_CD)
        {
            var resultado = await App_ConsultaStocks.Helpers.BarCodeHelper.ProcesarCodigoProductoRefac(CodBar, Id_CD);

            if (!resultado.Exito)
            {
                await DisplayAlert("Error", resultado.Mensaje, "Ok");
                Vibrar();
                txt_IdProducto.BackgroundColor = Color.White;
                txt_IdUbicacion.BackgroundColor = Color.White;
                txt_IdProducto.Text = "";
                txt_IdProducto.Focus();
                return;
            }

            // Comparar el Id devuelto por el helper con el Id pedido (lbl_IdArticulo)
            string idDevuelto = (resultado.IdArticulo ?? "").Trim();
            string idPedido = (lbl_IdArticulo.Text ?? "").Trim();

            if (string.IsNullOrEmpty(idDevuelto))
            {
                await DisplayAlert("Error!", "Producto no identificado.", "Ok");
                Vibrar();
                txt_IdProducto.BackgroundColor = Color.White;
                txt_IdProducto.Text = "";
                txt_IdProducto.Focus();
                return;
            }

            if (!string.Equals(idDevuelto, idPedido, StringComparison.OrdinalIgnoreCase))
            {
                txt_IdProducto.BackgroundColor = Color.Red;
                Vibrar();
                await DisplayAlert("Error!", "Producto ERRADO!", "Ok");
                txt_IdProducto.Text = "";
                txt_IdProducto.Focus();
                return;
            }

            // Coincide con lo pedido: marcar verde y continuar
            txt_IdProducto.Text = idDevuelto;
            txt_IdProducto.BackgroundColor = Color.LawnGreen;
            txt_IdProducto.IsReadOnly = true;
            VerifExistenciaIdArticuloUbicacion(txt_IdProducto.Text, v_Id_Alm, txt_IdUbicacion.Text, v_Id_CD);

            if (!string.IsNullOrWhiteSpace(lbl_IdLote.Text))
            {
                stkLote.IsVisible = true;
                lbl_Lote.IsVisible = true;
                txt_IdLote.Focus();
            }
            else
            {
                txt_Cantidad.Focus();
            }

            // Si el helper devuelve lote, validar contra lote pedido
            if (!string.IsNullOrEmpty(resultado.Lote))
            {
                txt_IdLote.Text = resultado.Lote;
                stkLote.IsVisible = true;
                lbl_Lote.IsVisible = true;

                if (!string.IsNullOrWhiteSpace(lbl_IdLote.Text))
                {
                    if (string.Equals(txt_IdLote.Text.Trim(), lbl_IdLote.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        txt_IdLote.BackgroundColor = Color.LawnGreen;
                        txt_Cantidad.Focus();
                    }
                    else
                    {
                        txt_IdLote.BackgroundColor = Color.Red;
                        await DisplayAlert("Error", "El lote no coincide con el esperado.", "Ok");
                        txt_IdLote.Focus();
                    }
                }
                else
                {
                    txt_Cantidad.Focus();
                }
            }
        }


        private async void VerifExistenciaIdArticuloUbicacion(string ID_ARTICULO, string ID_ALMACEN, string ID_UBICACION, int Id_CD)
        {
            try
            {
                DataTable dt = new DataTable();

                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_VerifExistenciaIdArticulo", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 31).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 31).Value = ID_UBICACION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_VerifExistenciaIdArticulo", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 31).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 31).Value = ID_UBICACION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    txt_IdUbicacion.BackgroundColor = Color.LawnGreen;
                }
                else
                {
                    txt_IdUbicacion.BackgroundColor = Color.Red;

                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }


        private async Task<bool> Picking_Det_InsDatos(int ID_PICKING, string ID_UBICACION, int ITEMPEDIDO, string ID_ARTICULO, int PICK, int ORDEN, Boolean UBIC_OK, Boolean PICK_OK, string ID_UBICACION_NEW, string ID_LOTE, string ID_LOTE_NEW)
        {
            try
            {
                Conexion.Abrir();

                // Llamar al stored procedure (la SP ahora devuelve un resultset con OUT_STATUS/OUT_MESSAGE)
                SqlCommand cmd = new SqlCommand("alfa_usp_pedido_picking_det_InsDatos_Lotes_v2", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 31).Value = ID_UBICACION;
                cmd.Parameters.Add("@ITEMPEDIDO", SqlDbType.BigInt).Value = ITEMPEDIDO;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                cmd.Parameters.Add("@PICK", SqlDbType.Int).Value = PICK;
                cmd.Parameters.Add("@ORDEN", SqlDbType.Int).Value = ORDEN;
                cmd.Parameters.Add("@UBIC_OK", SqlDbType.Bit).Value = UBIC_OK;
                cmd.Parameters.Add("@PICK_OK", SqlDbType.Bit).Value = PICK_OK;

                // Trim whitespace only; do NOT alter content (the stored procedure expects the exact lote text)
                if (!string.IsNullOrEmpty(ID_UBICACION_NEW)) ID_UBICACION_NEW = ID_UBICACION_NEW.Trim();
                if (!string.IsNullOrEmpty(ID_LOTE)) ID_LOTE = ID_LOTE.Trim();
                if (!string.IsNullOrEmpty(ID_LOTE_NEW)) ID_LOTE_NEW = ID_LOTE_NEW.Trim();

                cmd.Parameters.Add("@ID_UBICACION_NEW", SqlDbType.VarChar, 31).Value = ID_UBICACION_NEW ?? string.Empty;
                cmd.Parameters.Add("@ID_PICADOR", SqlDbType.Int).Value = v_id_Picador;
                cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = ID_LOTE ?? string.Empty;
                cmd.Parameters.Add("@ID_LOTE_NEW", SqlDbType.VarChar, 31).Value = ID_LOTE_NEW ?? string.Empty;

                // Ejecutar y leer el resultset que devuelve la SP
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                Conexion.Cerrar();

                string status = "OK";
                string message = string.Empty;

                if (dt.Rows.Count > 0)
                {
                    // La SP retorna columnas OUT_STATUS y OUT_MESSAGE en el primer row
                    if (dt.Columns.Contains("OUT_STATUS"))
                        status = dt.Rows[0]["OUT_STATUS"]?.ToString() ?? "OK";
                    if (dt.Columns.Contains("OUT_MESSAGE"))
                        message = dt.Rows[0]["OUT_MESSAGE"]?.ToString() ?? string.Empty;
                }

                List<SugerenciaPick> sugerencias = ObtenerSugerenciasPick(dt);

                // Manejo del resultado
                if (!string.IsNullOrWhiteSpace(status) && status.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
                {
                    Vibrar();

                    if (sugerencias.Count > 0)
                    {
                        await MostrarSugerenciasPick(string.IsNullOrWhiteSpace(message) ? "Error al guardar pick." : message, sugerencias);
                    }
                    else
                    {
                        await DisplayAlert("Error", string.IsNullOrWhiteSpace(message) ? "Error al guardar pick." : message, "Ok");
                    }

                    return false; // Retornar false en caso de error
                }
                else
                {
                    // OK: si existe mensaje informativo, mostrarlo
                    // if (!string.IsNullOrWhiteSpace(message))
                    // {
                    //     await DisplayAlert("Información", message, "Ok");
                    // }
                    return true; // Retornar true en caso de éxito
                }

            }
            catch (Exception ex)
            {
                try { Conexion.Cerrar(); } catch { }
                await DisplayAlert("Error", ex.Message, "Ok");
                return false; // Retornar false en caso de excepción
            }

        }


        private async void Picking_SelCantSKUsxUbicacion(int ID_PICKING, string ID_UBICACION)
        {
            try
            {
                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_SelCantSKUsxUbicacion", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 31).Value = ID_UBICACION;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                int Filas = 0;
                Filas = dt.Rows.Count;

                lbl_CantSkusxUbicacion.Text = "";

                if (Filas > 0)
                {
                    lbl_CantSkusxUbicacion.Text = "  ( " + Convert.ToString(dt.Rows[0][0]) + " Sku's ) ";
                }



            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async void Picking_SelDatosAdicionalesxItem(int ID_PICKING, string ID_ALM, int ITEMPEDIDO, string ID_ARTICULO, int ORDEN)
        {
            try
            {
                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_SelDatosAdicionalesxItem", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@ID_ALM", SqlDbType.VarChar, 31).Value = ID_ALM;
                cmd.Parameters.Add("@ITEMPEDIDO", SqlDbType.BigInt, 31).Value = ITEMPEDIDO;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                cmd.Parameters.Add("@ORDEN", SqlDbType.Int).Value = ORDEN;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                int Filas = 0;
                Filas = dt.Rows.Count;

                lbl_Stock.Text = "";

                if (Filas > 0)
                {
                    lbl_Stock.Text = Convert.ToString(dt.Rows[0][0]);
                    if (ORDEN == 0)
                    {
                        //await DisplayAlert("Error", Convert.ToString(dt.Rows[0][1]), "Ok");
                        lbl_AtendidoUND.Text = Convert.ToString(dt.Rows[0][1]);
                    }

                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private void alfa_usp_Picking_Det_DelDatos(int ID_PICKING, int ITEMPEDIDO, int ORDEN)
        {
            //try
            //{

            //    Conexion.Abrir();

            //    SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_Det_DelDatos", Conexion.conectar);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
            //    cmd.Parameters.Add("@ITEMPEDIDO", SqlDbType.BigInt).Value = ITEMPEDIDO;
            //    cmd.Parameters.Add("@ORDEN", SqlDbType.Int).Value = ORDEN;

            //    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            //    cmd.ExecuteScalar();

            //    Conexion.Cerrar();

            //}
            //catch (Exception ex)
            //{
            //    DisplayAlert("Error", ex.Message, "Ok");
            //}

        }


        private void pedido_Picking_SelPedidosxRutaItemOrden(int ID_CD, int ID_PICKING, int ORDEN)
        {
            try
            {
                LV_CantxPedido.ItemsSource = null;
                DataTable dt = new DataTable();


                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Seleccionar_Ruta", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPicking", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@ORDEN", SqlDbType.Int).Value = ORDEN;
                cmd.Parameters.Add("@FILTRO", SqlDbType.VarChar, 150).Value = v_Filtro;
                cmd.Parameters.Add("@TIPO", SqlDbType.VarChar, 150).Value = v_Tipo;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                int Filas = 0;
                Filas = dt.Rows.Count;


                if (Filas > 0)
                {
                    List<Model_Pedido_Picking> CantxPedido = new List<Model_Pedido_Picking>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_Pedido_Picking Datos_CantxPedido = new Model_Pedido_Picking();
                        Datos_CantxPedido.NROPEDIDO = int.Parse(dt.Rows[i][0].ToString());
                        Datos_CantxPedido.CANTIDADES_X_ITEM_PEDIDO = int.Parse(dt.Rows[i][1].ToString());
                        CantxPedido.Add(Datos_CantxPedido);
                    }

                    LV_CantxPedido.ItemsSource = CantxPedido;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

    }
}