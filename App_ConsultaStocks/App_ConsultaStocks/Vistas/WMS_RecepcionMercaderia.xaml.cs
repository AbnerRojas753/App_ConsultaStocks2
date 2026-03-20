using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Data.SqlClient;
using System.Data;

using App_ConsultaStocks.Modelo;
using App_ConsultaStocks.Datos;

using ZXing.Net.Mobile.Forms;
using ZXing;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WMS_RecepcionMercaderia : ContentPage
    {
        int v_Id_CD = 0;
        string v_Id_Alm = "";
        string ExisteProductoenLista = "0";
        string RecepcionFlgCls = "0";
        int v_Id_Picador = 0;
        int tipoRecepcion = 0;

        public WMS_RecepcionMercaderia(Model_WMS_RecepcionProductos _Model_WMS_RecepcionProductos)
        {
            InitializeComponent();

            v_Id_CD = _Model_WMS_RecepcionProductos.ID_CD;

            v_Id_Alm = _Model_WMS_RecepcionProductos.ID_ALMACEN;
            v_Id_Picador = _Model_WMS_RecepcionProductos.ID_ALMACENERO;

            RBtn_Proveedor.IsChecked = true;

            lbl_YaExisteEnRecepcion.IsVisible = false;
            txt_OC.Text = "";
            //txt_Cantidad_ProdDirecto.IsEnabled = false;
            //Btn_GrabarProductoDirecto.IsEnabled = false;
            //txt_Cantidad_ProdDirecto.Text = "1";
            if (tipoRecepcion != 1)
            {
                lbl_Lote.IsVisible = false;
                txt_Lote.IsVisible = false;
            }


            if (_Model_WMS_RecepcionProductos.ID_RECEPCION != 0)
            {

                if (_Model_WMS_RecepcionProductos.TIPO_RECEPCION == 1)
                {
                    RBtn_Proveedor.IsChecked = true;
                    tipoRecepcion = 1;
                }
                if (_Model_WMS_RecepcionProductos.TIPO_RECEPCION == 2)
                {
                    RBtn_Transferencia.IsChecked = true;
                    tipoRecepcion = 2;
                }
                if (_Model_WMS_RecepcionProductos.TIPO_RECEPCION == 4)
                {
                    RBtn_Inventario.IsChecked = true;
                    tipoRecepcion = 4;
                }

                txt_IdRecepcion.Text = _Model_WMS_RecepcionProductos.ID_RECEPCION.ToString();
                //DisplayAlert("Mensaje!", "Continuando proceso: " + txt_IdRecepcion.Text, "Ok");

                recep_productos_SelDatosRecepcion(int.Parse(txt_IdRecepcion.Text));
                recep_productos_Det_Sel(int.Parse(txt_IdRecepcion.Text));



                txt_IdRecepcion.Focus();
            }
            else
            {
                txt_IdRecepcion.Text = "";
            }

        }

        private async void btn_GenerarRecepcion_Clicked(object sender, EventArgs e)
        {
            int v_Tipo_Repcecion = 0;
            if (RBtn_Proveedor.IsChecked == true)
            {
                v_Tipo_Repcecion = 1;
            }
            else
            {
                if (RBtn_Transferencia.IsChecked == true)
                {
                    v_Tipo_Repcecion = 2;
                }
                else if (RBtn_Inventario.IsChecked == true)
                {
                    v_Tipo_Repcecion = 4;

                }



            }

            recep_productos_Ins("", "", v_Tipo_Repcecion, v_Id_Alm);
        }

        private void RBtn_Proveedor_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            txt_IdRecepcion.Text = "";
            LV_DetalleRecepcion.ItemsSource = null;
            lbl_IdArticulo.Text = "";
            lbl_NombreProducto.Text = "";
            txt_UM.Text = "";
            txt_Equivalencia.Text = "";
            txt_Cantidad.Text = "";
            txt_Fecha.Text = "";
            txt_IdArticulo.Focus();
            txt_Ubicacion.Text = "";
            StkLay_NombreOC.IsVisible = true;
            StkLay_NombreProveedore.IsVisible = true;
            tipoRecepcion = 1;
            lbl_Lote.IsVisible = true;
            txt_Fecha.IsEnabled = false;
            txt_Lote.IsVisible = true;
        }

        private void RBtn_Transferencia_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            txt_IdRecepcion.Text = "";
            LV_DetalleRecepcion.ItemsSource = null;
            lbl_IdArticulo.Text = "";
            lbl_NombreProducto.Text = "";
            txt_UM.Text = "";
            txt_Equivalencia.Text = "";
            txt_Cantidad.Text = "";
            txt_Fecha.Text = "";
            txt_IdArticulo.Focus();
            txt_Ubicacion.Text = "";
            StkLay_NombreOC.IsVisible = false;
            StkLay_NombreProveedore.IsVisible = false;
            tipoRecepcion = 2;
            lbl_Lote.IsVisible = false;
            txt_Fecha.IsEnabled = true;
            txt_Lote.IsVisible = false;
        }

        private void RBtn_Inventario_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            txt_IdRecepcion.Text = "";
            LV_DetalleRecepcion.ItemsSource = null;
            lbl_IdArticulo.Text = "";
            lbl_NombreProducto.Text = "";
            txt_UM.Text = "";
            txt_Equivalencia.Text = "";
            txt_Cantidad.Text = "";
            txt_Fecha.Text = "";
            txt_IdArticulo.Focus();
            txt_Ubicacion.Text = "";
            StkLay_NombreOC.IsVisible = false;
            StkLay_NombreProveedore.IsVisible = false;
            tipoRecepcion = 4;
            lbl_Lote.IsVisible = false;
            txt_Fecha.IsEnabled = true;
            txt_Lote.IsVisible = true;
        }


        private async void btn_BuscarProveedor_Clicked(object sender, EventArgs e)
        {
            if (txt_IdRecepcion.Text != "")
            {
                recep_productos_SelFlgCls(int.Parse(txt_IdRecepcion.Text));
                if (RecepcionFlgCls == "1")
                {
                    await DisplayAlert("Error", "Esta recepción ya ha sido atendida!, No se puede modificar la información", "Ok");
                    return;
                }

                Model_BusquedaDatos _Model_BusquedaDatos = new Model_BusquedaDatos();
                _Model_BusquedaDatos.Parametro1 = txt_IdRecepcion.Text;
                _Model_BusquedaDatos.Parametro2 = v_Id_CD.ToString();
                _Model_BusquedaDatos.DatoaBuscar = "Proveedores";

                await Navigation.PushAsync(new BuscarDatos(_Model_BusquedaDatos));
            }
            else
            {
                await DisplayAlert("Error", "Debe crear un correlativo de recepción", "Ok");

            }
        }

        protected override async void OnAppearing()
        {
            if (txt_IdRecepcion.Text != "")
            {
                //Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));
                recep_productos_SelDatosRecepcion(int.Parse(txt_IdRecepcion.Text));
                recep_productos_Det_Sel(int.Parse(txt_IdRecepcion.Text));
                base.OnAppearing();
            }
            //if (v_Tipo_Picking == 2)
            //{
            //    Picking_GrupoPedidos_SelCantPedidosxPicking(int.Parse(lbl_IdPicking.Text));
            //}
            //await SetCardButtons(Settings.cc.Text());
        }


        private async void txt_IdArticulo_Completed(object sender, EventArgs e)
        {
            await BuscarProductos_BarCode2(txt_IdArticulo.Text, v_Id_CD);
            if (lbl_IdArticulo.Text == "")
            {
                txt_IdArticulo.Text = "";
                txt_IdArticulo.Focus();
                return;
            }

            txt_IdArticulo.Text = "";
            txt_Cantidad.Focus();
            

            if (RBtn_Proveedor.IsChecked == true)
            {
                if (txt_OC.Text.Length != 0)
                {
                    if (txt_OC.Text != "SOC")
                    {
                        DataTable dt = new DataTable();
                        if (v_Id_CD == 1)
                        {
                            Conexion.Abrir_WMS_LIM();
                            SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_VerificaExisteProducto_OC", Conexion.conectar_WMS_LIM);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                            cmd.Parameters.Add("@ORDENCOMPRA", SqlDbType.VarChar, 31).Value = txt_OC.Text;
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(dt);
                            Conexion.Cerrar_WMS_LIM();
                        }
                        else
                        {
                            Conexion.Abrir_WMS_ATE();
                            SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_VerificaExisteProducto_OC", Conexion.conectar_WMS_ATE);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                            cmd.Parameters.Add("@ORDENCOMPRA", SqlDbType.VarChar, 31).Value = txt_OC.Text;
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(dt);
                            Conexion.Cerrar_WMS_ATE();
                        }

                        string ExisteProductoenLista2 = dt.Rows[0][0].ToString();
                        Conexion.Cerrar();
                        if (ExisteProductoenLista2 == "0")
                        {
                            DisplayAlert("Error", "El producto no existe en la Orden de Compra", "Ok");

                            lbl_IdArticulo.Text = "";
                            lbl_NombreProducto.Text = "";
                            txt_Ubicacion.Text = "";
                            txt_UM.Text = "";
                            txt_Equivalencia.Text = "";
                            lbl_IdArticulo.Text = "";
                            txt_IdArticulo.Focus();
                            return;
                        }

                    }



                }
                else
                {
                    DisplayAlert("Error", "Seleccione OC", "Ok");
                    lbl_IdArticulo.Text = "";
                    lbl_NombreProducto.Text = "";
                    txt_Ubicacion.Text = "";
                    txt_UM.Text = "";
                    txt_Equivalencia.Text = "";
                    lbl_IdArticulo.Text = "";
                    txt_IdArticulo.Focus();
                    return;

                }
                DataTable dt2 = new DataTable();
                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_VerificaSeguimientoLote", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt2);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_VerificaSeguimientoLote", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt2);
                    Conexion.Cerrar_WMS_ATE();
                }

                string ExisteFlag = dt2.Rows[0][0].ToString();
                Conexion.Cerrar();
                if (ExisteFlag == "0")
                {
                    lbl_Lote.IsVisible = false;
                    txt_Lote.IsVisible = false;
                    txt_Lote.Text = "SINLOTE";
                    txt_Fecha.Text = "01/2099";
                    Chk_fechVenc.IsVisible = false;
                    lbl_SINVENC.IsVisible = false;
                    lbl_ExisteLote.IsVisible = false;
                    lbl_Fecha.IsVisible = false;
                    txt_Fecha.IsVisible = false;

                }
                else
                {
                    lbl_Lote.IsVisible = true;
                    txt_Lote.IsVisible = true;
                    txt_Lote.Text = "";

                    lbl_Fecha.IsVisible = true;
                    txt_Fecha.IsVisible = true;
                    txt_Fecha.Text = "";

                    Chk_fechVenc.IsVisible = true;
                    lbl_SINVENC.IsVisible = true;

                }
            }


            recep_productos_Det_VerificaExisteProducto(int.Parse(txt_IdRecepcion.Text), lbl_IdArticulo.Text);

            if (ExisteProductoenLista == "0")
            {
                lbl_YaExisteEnRecepcion.IsVisible = false;
            }
            else
            {
                lbl_YaExisteEnRecepcion.IsVisible = true;
            }
        }

        private async void btn_LeerCodBarrasProducto_Clicked(object sender, EventArgs e)
        {
            try
            {
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txt_IdArticulo.Text = result.Text;
                    await BuscarProductos_BarCode2(txt_IdArticulo.Text, v_Id_CD);
                    txt_IdArticulo.Text = "";
                    // txt_Lote.Text = "";

                    recep_productos_Det_VerificaExisteProducto(int.Parse(txt_IdRecepcion.Text), lbl_IdArticulo.Text);
                    if (ExisteProductoenLista == "0")
                    {
                        lbl_YaExisteEnRecepcion.IsVisible = false;
                    }
                    else
                    {
                        lbl_YaExisteEnRecepcion.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {

                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async void btn_Limpiar_Clicked(object sender, EventArgs e)
        {
            txt_Ubicacion.Text = "";
        }


        private async void btn_GuardarLinea_Clicked(object sender, EventArgs e)
        {

            recep_productos_SelFlgCls(int.Parse(txt_IdRecepcion.Text));


            if (RecepcionFlgCls == "1")
            {
                await DisplayAlert("Error", "Esta recepción ya ha sido atendida!, No se puede modificar la información", "Ok");
                return;
            }


            if (txt_Cantidad.Text == "")
            {
                await DisplayAlert("Error", "Debe indicar una valor en cantidad", "Ok");
                return;
            }
            if (tipoRecepcion == 1)
            {

                // if (txt_Lote.Text == "")
                // {
                //     await DisplayAlert("Error", "Debe indicar un Lote", "Ok");
                //     return;
                // }
            }
            // if (  tipoRecepcion == 4)
            // {

            //     if (txt_Lote.Text == "")
            //     {
            //         await DisplayAlert("Error", "Debe indicar un Lote", "Ok");
            //         return;
            //     }
            // }

            bool isChecked = Chk_fechVenc.IsChecked;

            if (!isChecked)
            {
                if (txt_Fecha.Text == "")
                {
                    await DisplayAlert("Error", "Debe indicar una Fecha de Vencimiento ", "Ok");
                    return;
                }
            }

            // Validar ubicación obligatoria para Inventarios
            if (tipoRecepcion == 4)
            {
                if (string.IsNullOrWhiteSpace(txt_Ubicacion.Text))
                {
                    await DisplayAlert("Error", "Debe indicar una Ubicación para las recepciones de Inventario", "Ok");
                    return;
                }
            }

            //DisplayAlert("Error",txt_Ubicacion.Text,"ok");
            if (lbl_NroLinea.Text == "0")
            {
                recep_productos_Det_Ins(int.Parse(txt_IdRecepcion.Text), lbl_IdArticulo.Text, txt_UM.Text, int.Parse(txt_Equivalencia.Text), int.Parse(txt_Cantidad.Text), txt_Fecha.Text, v_Id_Picador, txt_Lote.Text, txt_Ubicacion.Text);
            }
            else
            {
                recep_productos_Det_Upd(int.Parse(txt_IdRecepcion.Text), int.Parse(lbl_NroLinea.Text), txt_UM.Text, int.Parse(txt_Equivalencia.Text), int.Parse(txt_Cantidad.Text), txt_Fecha.Text, txt_Lote.Text, txt_Ubicacion.Text);
                lbl_NroLinea.Text = "0";
            }


            recep_productos_Det_Sel(int.Parse(txt_IdRecepcion.Text));

            lbl_IdArticulo.Text = "";
            lbl_NombreProducto.Text = "";
            txt_UM.Text = "";
            txt_Equivalencia.Text = "";
            txt_Cantidad.Text = "";
            txt_Fecha.Text = "";
            txt_IdArticulo.Focus();
            lbl_YaExisteEnRecepcion.IsVisible = false;
            //txt_Ubicacion.Text = "";

            if (tipoRecepcion == 1 || tipoRecepcion == 4)
            {
                lbl_Lote.IsVisible = true;
                txt_Lote.IsVisible = true;

                //lbl_Lote.IsVisible = false;
                //txt_Lote.IsVisible = false;
            }

            txt_Lote.Text = "";
            lbl_ExisteLote.IsVisible = false;
            lbl_ExisteLote.Text = "";
            lbl_Fecha.IsVisible = true;
            txt_Fecha.IsVisible = true;
            Chk_fechVenc.IsVisible = true;
            txt_Ubicacion.Text = "";
            lbl_SINVENC.IsVisible = true;
            // Chk_fechVenc.IsChecked = false;


        }

        private async void btn_editarlinea_Clicked(object sender, EventArgs e)
        {
            recep_productos_SelFlgCls(int.Parse(txt_IdRecepcion.Text));
            if (RecepcionFlgCls == "1")
            {
                await DisplayAlert("Error", "Esta recepción ya ha sido atendida!, No se puede modificar la información", "Ok");
                return;
            }


            var selectedItem = LV_DetalleRecepcion.SelectedItem as Model_WMS_RecepcionProductos;

            if (selectedItem == null)
            {
                await DisplayAlert("Error", "Debe seleccionar un item", "Ok");
                return;
            }
            lbl_NroLinea.Text = selectedItem.NRO_LINEA.ToString();
            lbl_IdArticulo.Text = selectedItem.ID_ARTICULO;
            lbl_NombreProducto.Text = selectedItem.NOMBRE_ARTICULO;
            txt_UM.Text = selectedItem.UM;
            txt_Equivalencia.Text = selectedItem.EQUIVALENCIA.ToString();
            txt_Cantidad.Text = selectedItem.CANTIDAD.ToString();
            txt_Fecha.Text = selectedItem.FECHA_VENC;
            lbl_YaExisteEnRecepcion.IsVisible = false;
        }

        private async void btn_eliminarLinea_Clicked(object sender, EventArgs e)
        {
            recep_productos_SelFlgCls(int.Parse(txt_IdRecepcion.Text));
            if (RecepcionFlgCls == "1")
            {
                await DisplayAlert("Error", "Esta recepción ya ha sido atendida!, No se puede modificar la información", "Ok");
                return;
            }


            bool answer = await DisplayAlert("Confirmar!", "Esta seguro de eliminar el registro?", "Sí", "No");
            if (answer == false)
            {
                return;
            }


            var selectedItem = LV_DetalleRecepcion.SelectedItem as Model_WMS_RecepcionProductos;

            if (selectedItem == null)
            {
                await DisplayAlert("Error", "Debe seleccionar un item", "Ok");
                return;
            }
            recep_productos_Det_Del(int.Parse(txt_IdRecepcion.Text), selectedItem.NRO_LINEA, selectedItem.ID_ARTICULO);
            recep_productos_Det_Sel(int.Parse(txt_IdRecepcion.Text));
            lbl_YaExisteEnRecepcion.IsVisible = false;
        }
        private async void btn_CerrarRecepion_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirmar", "¿ESTÁ SEGURO? SE VAN A CONTABILIZAR LOS LOTES", "Sí", "No");
            if (!answer)
            {
                return;
            }
            recep_productos_Upd_AtencionRec(int.Parse(txt_IdRecepcion.Text));
            await Navigation.PopAsync();
        }
        private void txt_Lote_Completed(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                if (v_Id_CD == 1)
                {

                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_LotesArt_VerifExistenciaLoteArticulo_Fecha", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = txt_Lote.Text;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_LotesArt_VerifExistenciaLoteArticulo_Fecha", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = txt_Lote.Text;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                string result = dt.Rows[0][0].ToString();
                if (result != "")
                {
                    //2027-05-30
                    //24/2024
                    result = result.Substring(5, 2) + "/" + result.Substring(0, 4);

                    //existe El lote
                    txt_Fecha.IsEnabled = false;
                    lbl_ExisteLote.IsVisible = true;
                    lbl_ExisteLote.Text = "Existe el Lote";
                    lbl_ExisteLote.TextColor = Color.Green;
                    txt_Fecha.Text = result;
                    //lbl_Fecha.IsVisible = true;
                }
                else
                {
                    txt_Fecha.IsEnabled = true;
                    lbl_ExisteLote.IsVisible = true;
                    lbl_ExisteLote.Text = "No Existe el Lote";
                    lbl_ExisteLote.TextColor = Color.Red;


                    txt_Fecha.Text = "";
                    txt_Fecha.Focus();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }


        }
        private void txt_Fecha_Completed(object sender, EventArgs e)
        {
            try
            {
                string Fecha = txt_Fecha.Text;
                string Fecha2;

                if (Fecha != "")
                {
                    //DisplayAlert("dato", Fecha.ToString(), "Ok");

                    //DisplayAlert("2", Fecha.Substring(0, 2), "Ok");
                    //DisplayAlert("3", Fecha.Substring(2, 2), "Ok");

                    if (Fecha.Length == 4)
                    {
                        Fecha2 = Fecha.Substring(0, 2) + "/20" + Fecha.Substring(2, 2);
                        txt_Fecha.Text = Fecha2;

                    }
                }
            }
            catch
            {
            }


        }


        //Metodos de Base de datos

        private async void recep_productos_Ins(string NRO_OC, string ID_PROVEEDOR, int TIPO_RECEPCION, string ID_ALMACEN)
        {
            try
            {
                DataTable dt = new DataTable();
                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_InsApp", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NRO_OC", SqlDbType.VarChar, 31).Value = NRO_OC;
                    cmd.Parameters.Add("@ID_PROVEEDOR", SqlDbType.VarChar, 31).Value = ID_PROVEEDOR;
                    cmd.Parameters.Add("@TIPO_RECEPCION", SqlDbType.VarChar, 31).Value = TIPO_RECEPCION;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 31).Value = ID_ALMACEN;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_InsApp", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NRO_OC", SqlDbType.VarChar, 31).Value = NRO_OC;
                    cmd.Parameters.Add("@ID_PROVEEDOR", SqlDbType.VarChar, 31).Value = ID_PROVEEDOR;
                    cmd.Parameters.Add("@TIPO_RECEPCION", SqlDbType.VarChar, 31).Value = TIPO_RECEPCION;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 31).Value = ID_ALMACEN;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas == 1)
                {
                    txt_IdRecepcion.Text = dt.Rows[0][0].ToString();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo obtener el correlativo", "Ok");
                    txt_IdRecepcion.Text = "";
                    //txtcodigobarras.Focus();
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }


        }


        private void recep_productos_SelDatosRecepcion(int ID_RECEPCION)
        {
            try
            {
                DataTable dt = new DataTable();

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_SelDatosRecepcion", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_SelDatosRecepcion", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {


                    lbl_NombreProveedor.Text = dt.Rows[0][4].ToString();
                    //DisplayAlert("Error", dt.Rows[0][4].ToString(), "Ok");

                    //DisplayAlert("Error", dt.Rows[0][4].ToString(), "Ok");
                    DataTable dt2 = new DataTable();

                    //Conexion.Abrir();
                    //SqlCommand cmd2 = new SqlCommand("[usp_recep_productos_Sel_OC]", Conexion.conectar);
                    //cmd2.CommandType = CommandType.StoredProcedure;
                    //cmd2.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                    //SqlDataAdapter adapter2 = new SqlDataAdapter(cmd2);
                    //adapter2.Fill(dt2);
                    //Conexion.Cerrar();

                    if (v_Id_CD == 1)
                    {
                        Conexion.Abrir_WMS_LIM();
                        SqlCommand cmd2 = new SqlCommand("[usp_recep_productos_Sel_OC]", Conexion.conectar_WMS_LIM);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                        SqlDataAdapter adapter2 = new SqlDataAdapter(cmd2);
                        adapter2.Fill(dt2);
                        Conexion.Cerrar_WMS_LIM();
                    }
                    else
                    {
                        Conexion.Abrir_WMS_ATE();
                        SqlCommand cmd2 = new SqlCommand("[usp_recep_productos_Sel_OC]", Conexion.conectar_WMS_ATE);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                        SqlDataAdapter adapter2 = new SqlDataAdapter(cmd2);
                        adapter2.Fill(dt2);
                        Conexion.Cerrar_WMS_ATE();
                    }

                    int Filas2 = 0;
                    Filas2 = dt2.Rows.Count;
                    if (Filas2 > 0)
                    {
                        List<Model_Almacen> Almacenes = new List<Model_Almacen>();
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            Model_Almacen Model_Alm = new Model_Almacen();
                            Model_Alm.IdAlmacen = dt2.Rows[i][0].ToString();
                            Model_Alm.DescripcionAlmacen = dt2.Rows[i][0].ToString();
                            Almacenes.Add(Model_Alm);
                        }
                        ItemPicker_OC.ItemsSource = Almacenes;
                        ItemPicker_OC.ItemDisplayBinding = new Binding("DescripcionAlmacen");
                    }

                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");


            }
        }

        private void ItemPicker_OC_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemPicker_OC.SelectedIndex != -1)
            {
                var selectedItem = ItemPicker_OC.SelectedItem as Model_Almacen;
                var desc = selectedItem.DescripcionAlmacen;
                var id = selectedItem.IdAlmacen;
                if (desc == "Sin Orden de Compra")
                {
                    txt_OC.Text = "SOC";
                }
                else
                {
                    txt_OC.Text = desc;
                }
            }
        }

        private void OnChkUsarVencCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                lbl_Fecha.IsVisible = false;
                txt_Fecha.IsVisible = false;
                txt_Fecha.Text = "01/2099";
            }
            else
            {
                lbl_Fecha.IsVisible = true;
                txt_Fecha.IsVisible = true;
                txt_Fecha.Text = "";
                System.Diagnostics.Debug.WriteLine("CheckBox está desmarcado.");
            }
        }

        private void BuscarProductos_BarCode(string CodBar, int Id_CD)
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
                        lbl_IdArticulo.Text = Convert.ToString(dt.Rows[0][0]);
                        lbl_NombreProducto.Text = Convert.ToString(dt.Rows[0][1]);

                        //Aquí muestro las ubicaciones
                        DataTable dt_Ubicacion = new DataTable();

                        if (Id_CD == 1)
                        {
                            Conexion.Abrir_WMS_LIM();
                            SqlCommand cmd = new SqlCommand("usp_BuscarProductos_SelUbicacionAlmacenHorizontal", Conexion.conectar_WMS_LIM);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                            cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 31).Value = v_Id_Alm;
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(dt_Ubicacion);
                            Conexion.Cerrar_WMS_LIM();
                        }
                        else
                        {
                            Conexion.Abrir_WMS_ATE();
                            SqlCommand cmd = new SqlCommand("usp_BuscarProductos_SelUbicacionAlmacenHorizontal", Conexion.conectar_WMS_ATE);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                            cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 31).Value = v_Id_Alm;
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(dt_Ubicacion);
                            Conexion.Cerrar_WMS_ATE();
                        }

                        int Filas_Ubicaciones = 0;
                        Filas_Ubicaciones = dt_Ubicacion.Rows.Count;

                        //if (Filas_Ubicaciones > 0)
                        //{
                        //    txt_Ubicacion.Text = Convert.ToString(dt_Ubicacion.Rows[0][0]);
                        //}


                        if (Chk_UsarMasterPack.IsChecked == true)
                        {
                            DataTable dt_Presentaciones = new DataTable();

                            if (Id_CD == 1)
                            {
                                Conexion.Abrir_WMS_LIM();
                                SqlCommand cmd = new SqlCommand("usp_BuscarProductos_SelPresentaciones", Conexion.conectar_WMS_LIM);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                                adapter.Fill(dt_Presentaciones);
                                Conexion.Cerrar_WMS_LIM();
                            }
                            else
                            {
                                Conexion.Abrir_WMS_ATE();
                                SqlCommand cmd = new SqlCommand("usp_BuscarProductos_SelPresentaciones", Conexion.conectar_WMS_ATE);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                                adapter.Fill(dt_Presentaciones);
                                Conexion.Cerrar_WMS_ATE();
                            }

                            int Filas_Presentaciones = 0;
                            Filas_Presentaciones = dt_Presentaciones.Rows.Count;

                            if (Filas_Presentaciones > 0)
                            {
                                if (Filas_Presentaciones == 1)
                                {
                                    txt_UM.Text = Convert.ToString(dt_Presentaciones.Rows[0][2]);
                                    txt_Equivalencia.Text = Convert.ToString(dt_Presentaciones.Rows[0][3]);
                                }
                            }
                        }
                        else
                        {
                            txt_UM.Text = Convert.ToString(dt.Rows[0][2]);
                            txt_Equivalencia.Text = Convert.ToString(dt.Rows[0][3]);
                        }
                    }
                    else
                    {
                        DisplayAlert("Error", "Existe más de un producto con el mismo CodBar", "Ok");
                        //Vibrar();
                        lbl_IdArticulo.Text = "";
                        lbl_NombreProducto.Focus();
                    }
                }
                else
                {
                    DisplayAlert("Error!", "Producto no existe!", "Ok");
                    //Vibrar();
                    lbl_IdArticulo.Text = "";
                    lbl_NombreProducto.Focus();

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
                lbl_IdArticulo.Text = "";
                lbl_NombreProducto.Focus();
                return;
            }

            // Coincidencia única: replicar el comportamiento del método original
            lbl_IdArticulo.Text = resultado.IdArticulo;
            lbl_NombreProducto.Text = resultado.NombreArticulo;
            txt_Lote.Text = resultado.Lote;

            // Mostrar ubicaciones
            DataTable dt_Ubicacion = new DataTable();

            if (Id_CD == 1)
            {
                Conexion.Abrir_WMS_LIM();
                SqlCommand cmd = new SqlCommand("usp_BuscarProductos_SelUbicacionAlmacenHorizontal", Conexion.conectar_WMS_LIM);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 31).Value = v_Id_Alm;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt_Ubicacion);
                Conexion.Cerrar_WMS_LIM();
            }
            else
            {
                Conexion.Abrir_WMS_ATE();
                SqlCommand cmd = new SqlCommand("usp_BuscarProductos_SelUbicacionAlmacenHorizontal", Conexion.conectar_WMS_ATE);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 31).Value = v_Id_Alm;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt_Ubicacion);
                Conexion.Cerrar_WMS_ATE();
            }

            // Manejo de presentaciones (MasterPack) copia del original
            if (Chk_UsarMasterPack.IsChecked == true)
            {
                DataTable dt_Presentaciones = new DataTable();

                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_BuscarProductos_SelPresentaciones", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt_Presentaciones);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_BuscarProductos_SelPresentaciones", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = lbl_IdArticulo.Text;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt_Presentaciones);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas_Presentaciones = 0;
                Filas_Presentaciones = dt_Presentaciones.Rows.Count;

                if (Filas_Presentaciones > 0)
                {
                    if (Filas_Presentaciones == 1)
                    {
                        txt_UM.Text = Convert.ToString(dt_Presentaciones.Rows[0][2]);
                        txt_Equivalencia.Text = Convert.ToString(dt_Presentaciones.Rows[0][3]);
                    }
                }
            }
            else
            {
                // Si el helper no devolvió UM/Equivalencia, intentar leerlas desde el RawData
                if (resultado.RawData != null && resultado.RawData.Rows.Count == 1)
                {
                    txt_UM.Text = Convert.ToString(resultado.RawData.Rows[0][2]);
                    txt_Equivalencia.Text = Convert.ToString(resultado.RawData.Rows[0][3]);
                }
            }
        }


        private void recep_productos_Det_Sel(int ID_RECEPCION)
        {
            try
            {
                LV_DetalleRecepcion.ItemsSource = null;
                DataTable dt = new DataTable();

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_SelForPDA", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_SelForPDA", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_WMS_RecepcionProductos> RecepcionDetalle = new List<Model_WMS_RecepcionProductos>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_WMS_RecepcionProductos Model_RecepcionDetalle = new Model_WMS_RecepcionProductos();
                        Model_RecepcionDetalle.NRO_LINEA = int.Parse(dt.Rows[i][0].ToString());
                        Model_RecepcionDetalle.ID_ARTICULO = dt.Rows[i][1].ToString();
                        Model_RecepcionDetalle.NOMBRE_ARTICULO = dt.Rows[i][2].ToString();
                        Model_RecepcionDetalle.UM = dt.Rows[i][3].ToString();
                        Model_RecepcionDetalle.CANTIDAD = int.Parse(dt.Rows[i][4].ToString());
                        Model_RecepcionDetalle.FECHA_VENC = dt.Rows[i][5].ToString();
                        Model_RecepcionDetalle.EQUIVALENCIA = int.Parse(dt.Rows[i][6].ToString());
                        Model_RecepcionDetalle.ID_LOTE = dt.Rows[i][7].ToString();

                        RecepcionDetalle.Add(Model_RecepcionDetalle);
                    }
                    LV_DetalleRecepcion.ItemsSource = RecepcionDetalle;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }


        private void recep_productos_Det_Ins(

            int ID_RECEPCION, string ID_ARTICULO, string UM, int EQUIV, int CANTIDAD, string FECVENC, int IDPICADOR, string lote, string ubicacion)
        {
            string oc = txt_OC.Text;
            if (oc == "SOC")
            {
                oc = "";
            }

            bool isChecked = Chk_fechVenc.IsChecked;
            string fechaString = FECVENC;
            DateTime fechaDateTime;

            if (!isChecked)
            {
                try
                {
                    fechaDateTime = DateTime.ParseExact(fechaString, "MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Fecha incorrecta 1", ex.Message, "Ok");
                    return;
                }
            }
            else
            {
                try
                {
                    fechaDateTime = DateTime.ParseExact("01/2099", "MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Fecha incorrecta 2", ex.Message, "Ok");
                    return;
                }

            }


            try
            {

                DataTable dt = new DataTable();
                DataTable dt2 = new DataTable();
                if (v_Id_CD == 1)
                {

                    Conexion.Abrir_WMS_LIM();
                    if (tipoRecepcion == 1 || tipoRecepcion == 4)
                    {
                        
                        SqlCommand cmd2 = new SqlCommand("sp_LotesArt_InsUpdDatos", Conexion.conectar_WMS_LIM);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                        cmd2.Parameters.Add("@LT_FECVENC", SqlDbType.Date).Value = fechaDateTime;
                        cmd2.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = lote;
                        cmd2.Parameters.Add("@LT_ESTADO", SqlDbType.Int).Value = "1";
                        cmd2.Parameters.Add("@FLG_NoFecVenc", SqlDbType.Bit).Value = isChecked;
                        SqlDataAdapter adapter2 = new SqlDataAdapter(cmd2);
                        adapter2.Fill(dt2);

                    }
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_Ins", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@UM", SqlDbType.VarChar, 31).Value = UM;
                    cmd.Parameters.Add("@EQUIV", SqlDbType.Int).Value = EQUIV;
                    cmd.Parameters.Add("@CANTIDAD", SqlDbType.Int).Value = CANTIDAD;
                    cmd.Parameters.Add("@FECVENC", SqlDbType.VarChar, 31).Value = FECVENC;
                    cmd.Parameters.Add("@NRODOC", SqlDbType.VarChar, 31).Value = oc;
                    cmd.Parameters.Add("@ID_PICADOR", SqlDbType.Int).Value = IDPICADOR;
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = lote;
                    cmd.Parameters.Add("@Id_Ubicacion", SqlDbType.VarChar, 31).Value = ubicacion;
                    //@Id_Ubicacion
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();

                }
                else
                {
                    Conexion.Abrir_WMS_ATE();

                    if (tipoRecepcion == 1 || tipoRecepcion == 4)
                    {
                        SqlCommand cmd2 = new SqlCommand("sp_LotesArt_InsUpdDatos", Conexion.conectar_WMS_ATE);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                        cmd2.Parameters.Add("@LT_FECVENC", SqlDbType.Date).Value = fechaDateTime;
                        cmd2.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = lote;
                        cmd2.Parameters.Add("@LT_ESTADO", SqlDbType.Int).Value = "1";
                        cmd2.Parameters.Add("@FLG_NoFecVenc", SqlDbType.Bit).Value = isChecked;
                        SqlDataAdapter adapter2 = new SqlDataAdapter(cmd2);
                        adapter2.Fill(dt2);
                    }

                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_Ins", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.Int).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@UM", SqlDbType.VarChar, 31).Value = UM;
                    cmd.Parameters.Add("@EQUIV", SqlDbType.Int).Value = EQUIV;
                    cmd.Parameters.Add("@CANTIDAD", SqlDbType.Int).Value = CANTIDAD;
                    cmd.Parameters.Add("@FECVENC", SqlDbType.VarChar, 31).Value = FECVENC;
                    cmd.Parameters.Add("@NRODOC", SqlDbType.VarChar, 31).Value = oc;
                    cmd.Parameters.Add("@ID_PICADOR", SqlDbType.Int).Value = IDPICADOR;
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = lote;
                    cmd.Parameters.Add("@Id_Ubicacion", SqlDbType.VarChar, 31).Value = ubicacion;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }



            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");


            }
        }

        private async void recep_productos_Det_Del(int ID_RECEPCION, int NRO_LINEA, string ID_ARTICULO)
        {
            try
            {
                DataTable dt = new DataTable();
                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_Del", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@NRO_LINEA", SqlDbType.VarChar, 31).Value = NRO_LINEA;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_Del", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@NRO_LINEA", SqlDbType.VarChar, 31).Value = NRO_LINEA;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }



            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }


        }

        private async void recep_productos_Det_Upd(int ID_RECEPCION, int NROLINEA, string UM, int EQUIV, int CANTIDAD, string FECVENC, string lote, string ubicacion)
        {
            try
            {
                DataTable dt = new DataTable();
                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_Upd", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@NROLINEA", SqlDbType.VarChar, 31).Value = NROLINEA;
                    cmd.Parameters.Add("@UM", SqlDbType.VarChar, 31).Value = UM;
                    cmd.Parameters.Add("@EQUIV", SqlDbType.VarChar, 31).Value = EQUIV;
                    cmd.Parameters.Add("@CANTIDAD", SqlDbType.VarChar, 31).Value = CANTIDAD;
                    cmd.Parameters.Add("@FECVENC", SqlDbType.VarChar, 31).Value = FECVENC;
                    cmd.Parameters.Add("@NRODOC", SqlDbType.VarChar, 31).Value = txt_OC.Text;
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = lote;
                    cmd.Parameters.Add("@Id_Ubicacion", SqlDbType.VarChar, 31).Value = ubicacion;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_Upd", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@NROLINEA", SqlDbType.VarChar, 31).Value = NROLINEA;
                    cmd.Parameters.Add("@UM", SqlDbType.VarChar, 31).Value = UM;
                    cmd.Parameters.Add("@EQUIV", SqlDbType.VarChar, 31).Value = EQUIV;
                    cmd.Parameters.Add("@CANTIDAD", SqlDbType.VarChar, 31).Value = CANTIDAD;
                    cmd.Parameters.Add("@FECVENC", SqlDbType.VarChar, 31).Value = FECVENC;
                    cmd.Parameters.Add("@NRODOC", SqlDbType.VarChar, 31).Value = txt_OC.Text;
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 31).Value = lote;
                    cmd.Parameters.Add("@Id_Ubicacion", SqlDbType.VarChar, 31).Value = ubicacion;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }



            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }


        }

        private async void recep_productos_Upd_AtencionRec(int ID_RECEPCION)
        {
            try
            {
                DataTable dt = new DataTable();
                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Upd_AtencionRec", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Upd_AtencionRec", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }


        }

        private async void recep_productos_Det_VerificaExisteProducto(int ID_RECEPCION, string ID_ARTICULO)
        {
            try
            {
                DataTable dt = new DataTable();
                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_VerificaExisteProducto", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    ExisteProductoenLista = dt.Rows[0][0].ToString();
                    Conexion.Cerrar_WMS_LIM();

                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Det_VerificaExisteProducto", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    ExisteProductoenLista = dt.Rows[0][0].ToString();
                    Conexion.Cerrar_WMS_ATE();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async void recep_productos_SelFlgCls(int ID_RECEPCION)
        {
            try
            {
                DataTable dt = new DataTable();
                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_SelFlgCls", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    RecepcionFlgCls = dt.Rows[0][0].ToString();
                    Conexion.Cerrar_WMS_LIM();

                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_SelFlgCls", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    RecepcionFlgCls = dt.Rows[0][0].ToString();
                    Conexion.Cerrar_WMS_ATE();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }


    }
}