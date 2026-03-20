using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Data.SqlClient;
using System.Data;

using ZXing.Net.Mobile.Forms;
using App_ConsultaStocks.Datos;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Pedido_Picking : ContentPage
    {

        int v_Id_CD=0;
        string v_Id_Alm = "";
        int v_Id_Picador = 0;

        //int v_Id_Picking = 0;
        int v_Tipo_Picking = 1; //1 es x pedido 2 es grupo pedidos
        int v_VerSoloPendiente = 1;
        int v_CANT_LINEAS_RUTA = 0;

        int v_ITEMPEDIDO = 0;
        //Model_Pedido_Detalle _Model_PedidoDetalle = new Model_Pedido_Detalle();


        public Pedido_Picking(Model_Pedido_Picking _Model_PedidoPicking) 
        {
            InitializeComponent();

            
            v_Id_CD = _Model_PedidoPicking.ID_CD;
            lbl_NombreSede.Text = _Model_PedidoPicking.DESC_CD;

            v_Id_Alm = _Model_PedidoPicking.ID_ALM;
            v_Id_Picador = _Model_PedidoPicking.ID_PICADOR;

            Chk_VerSoloPend.IsChecked = true;
            Chk_GrupoPedidos.IsChecked = true;

            txt_Cantidad_ProdDirecto.IsEnabled = false;
            Btn_GrabarProductoDirecto.IsEnabled = false;
            txt_Cantidad_ProdDirecto.Text = "1";

            //Se identifica si viene de un proceso pendiente
            if (_Model_PedidoPicking.ID_PICKING != 0)
            {

                lbl_IdPicking.Text = _Model_PedidoPicking.ID_PICKING.ToString();
                DisplayAlert("Mensaje!", "Continuando proceso: " + lbl_IdPicking.Text, "Ok");

                Picking_GrupoPedidos_SelCantPedidosxPicking(int.Parse(lbl_IdPicking.Text));

                Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));

                txt_NroPedido_GrupoPedido.Text = "";
                txt_NroPedido_GrupoPedido.Focus();

            }
            else
            {
                lbl_IdPicking.Text = "0";
            }
            


        }


        private void txtNroPedido_Completed(object sender, EventArgs e)
        {
            Pedido_Picking_InsCab(v_Id_CD, v_Tipo_Picking,int.Parse(txtNroPedido.Text), v_Id_Alm, v_Id_Picador);
            Picking_SelCabPedido(int.Parse(txtNroPedido.Text));
            txtNroPedido.Text = "";
        }

        private async void btnLeerNroPedido_Clicked(object sender, EventArgs e)
        {
            try
            {
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txtNroPedido.Text = result.Text;
                    Pedido_Picking_InsCab(v_Id_CD, v_Tipo_Picking, int.Parse(txtNroPedido.Text), v_Id_Alm, v_Id_Picador);
                    Picking_SelCabPedido(int.Parse(txtNroPedido.Text));
                    txtNroPedido.Text = "";
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }


        private async void Btn_Iniciar_Clicked(object sender, EventArgs e)
        {
            // USAR Console.WriteLine para que aparezca en Logcat de Android
            Console.WriteLine("[DEBUG] ===== BTN INICIAR CLICKED =====");
            Console.WriteLine("[DEBUG] Btn_Iniciar_Clicked started - v_Tipo_Picking: " + v_Tipo_Picking);
            
            try
            {
                // Validar que hay un picking iniciado
                Console.WriteLine("[DEBUG] lbl_IdPicking.Text: '" + lbl_IdPicking.Text + "'");
                
                if (string.IsNullOrEmpty(lbl_IdPicking.Text) || lbl_IdPicking.Text == "0")
                {
                    Console.WriteLine("[DEBUG] ERROR: No existe proceso de picking");
                    await DisplayAlert("Error!", "No existe un proceso de picking iniciado", "Ok");
                    return;
                }
                
                if (v_Tipo_Picking == 1 )
                {
                    Console.WriteLine("[DEBUG] lbl_NroPedido.Text: '" + lbl_NroPedido.Text + "'");
                    if (lbl_NroPedido.Text == "")
                    {
                        await DisplayAlert("Error!", "Debe indicar un nro. de pedido", "Ok");
                        return;
                    }

                }

                if (v_Tipo_Picking == 2)
                {
                    Console.WriteLine("[DEBUG] lbl_CantidadPedidos_GrupoPedidos.Text: '" + lbl_CantidadPedidos_GrupoPedidos.Text + "'");
                    if (lbl_CantidadPedidos_GrupoPedidos.Text == "0")
                    {
                        await DisplayAlert("Error!", "Debe indicar los pedidos", "Ok");
                        return;
                    }
                }
                
                // Procesar picking según el tipo
                if (v_Tipo_Picking == 1)
                {
                    // Picking individual: requiere NROPEDIDO
                    Console.WriteLine("[DEBUG] Procesando picking individual - NroPedido: " + lbl_NroPedido.Text);
                    
                    if (string.IsNullOrEmpty(lbl_NroPedido.Text))
                    {
                        await DisplayAlert("Error!", "Número de pedido no válido", "Ok");
                        return;
                    }
                    
                    Console.WriteLine("[DEBUG] Parseando ID_PICKING...");
                    int idPicking = int.Parse(lbl_IdPicking.Text);
                    Console.WriteLine("[DEBUG] ID_PICKING parseado: " + idPicking);
                    
                    Console.WriteLine("[DEBUG] Parseando NROPEDIDO...");
                    int nroPedido = int.Parse(lbl_NroPedido.Text);
                    Console.WriteLine("[DEBUG] NROPEDIDO parseado: " + nroPedido);
                    
                    Picking_Procesar(v_Id_CD, v_Tipo_Picking, idPicking, nroPedido, v_Id_Alm, v_Id_Picador);
                }
                else
                {
                    // Grupo de pedidos: NROPEDIDO es NULL (se pasa 0 y el método lo convierte a DBNull)
                    Console.WriteLine("[DEBUG] Procesando grupo de pedidos - IdPicking: " + lbl_IdPicking.Text);
                    
                    Console.WriteLine("[DEBUG] Parseando ID_PICKING...");
                    int idPicking = int.Parse(lbl_IdPicking.Text);
                    Console.WriteLine("[DEBUG] ID_PICKING parseado: " + idPicking);
                    
                    Picking_Procesar(v_Id_CD, v_Tipo_Picking, idPicking, 0, v_Id_Alm, v_Id_Picador);
                }

                Console.WriteLine("[DEBUG] Creando Model_Pedido_Picking...");
                Console.WriteLine("[DEBUG] v_CANT_LINEAS_RUTA: " + v_CANT_LINEAS_RUTA);

                Model_Pedido_Picking Model_PedidoPicking = new Model_Pedido_Picking();
                Model_PedidoPicking.ID_CD = v_Id_CD;
                Model_PedidoPicking.TIPO_PICKING = v_Tipo_Picking;
                Model_PedidoPicking.ID_PICADOR = v_Id_Picador;
                Model_PedidoPicking.ID_ALM = v_Id_Alm;

                Console.WriteLine("[DEBUG] Parseando ID_PICKING para modelo...");
                Model_PedidoPicking.ID_PICKING = int.Parse(lbl_IdPicking.Text);
                Console.WriteLine("[DEBUG] ID_PICKING asignado: " + Model_PedidoPicking.ID_PICKING);
                
                Model_PedidoPicking.CANT_LINEAS_RUTA = v_CANT_LINEAS_RUTA;
                Model_PedidoPicking.PICKING_MANUAL = 0;

                Console.WriteLine("[DEBUG] Navegando a Pedido_Picking_Item con ID_PICKING: " + Model_PedidoPicking.ID_PICKING);
                await Navigation.PushAsync(new Pedido_Picking_Item(Model_PedidoPicking));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DEBUG] ===== EXCEPTION =====");
                Console.WriteLine("[DEBUG] Message: " + ex.Message);
                Console.WriteLine("[DEBUG] StackTrace: " + ex.StackTrace);
                Console.WriteLine("[DEBUG] =====================");
                await DisplayAlert("Error", "Error al iniciar picking: " + ex.Message, "Ok");
            }
        }


        private async void Btn_PickingManual_Clicked(object sender, EventArgs e)
        {
            if (lbl_IdPicking.Text == "0")
            {
                await DisplayAlert("Error!", "Debe iniciar un proceso de Picking", "Ok");
                return;
            }

            //Picking_Procesar(v_Id_CD,v_Tipo_Picking,int.Parse(lbl_IdPicking.Text ),int.Parse(lbl_NroPedido.Text), v_Id_Alm, v_Id_Picador);

            Model_Pedido_Picking Model_PedidoPicking = new Model_Pedido_Picking();

            Model_PedidoPicking.ID_CD = v_Id_CD;
            Model_PedidoPicking.NROPEDIDO = 0;
            Model_PedidoPicking.ID_PICADOR = v_Id_Picador;
            Model_PedidoPicking.ID_ALM = v_Id_Alm;

            Model_PedidoPicking.ID_PICKING = int.Parse(lbl_IdPicking.Text); 
            Model_PedidoPicking.CANT_LINEAS_RUTA = 0;

            Model_PedidoPicking.PICKING_MANUAL = 1;

            var selectedItem = LV_DetallePedido.SelectedItem as Model_Pedido_Detalle;
            if (selectedItem == null)
            {
                await DisplayAlert("Error", "Debe seleccionar un item", "Ok");
                return;
            }

            if (selectedItem.ID_ARTICULO != "")
            {
                Model_PedidoPicking.ID_ARTICULO = selectedItem.ID_ARTICULO;
                Model_PedidoPicking.ITEMPEDIDO = selectedItem.ITEMPEDIDO;
                Model_PedidoPicking.ART_DESCRIPCION= selectedItem.ART_DESCRIPCION;
                Model_PedidoPicking.CANTIDAD_PEDIDO = selectedItem.CANTIDAD;
            }

            await Navigation.PushAsync(new Pedido_Picking_Item(Model_PedidoPicking));
        }

        protected override async void OnAppearing()
        {
            if (lbl_IdPicking.Text != "0")
            {
                Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente,int.Parse(lbl_IdPicking.Text));
                base.OnAppearing();
            }
            if (v_Tipo_Picking ==2)
            {
                Picking_GrupoPedidos_SelCantPedidosxPicking(int.Parse(lbl_IdPicking.Text));
            }
            //await SetCardButtons(Settings.cc.Text());
        }

        private void Chk_VerSoloPend_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            
            if (Chk_VerSoloPend.IsChecked == true)
            {
                v_VerSoloPendiente = 1;
            }
            else
            {
                v_VerSoloPendiente = 0;
            }

            if (lbl_IdPicking.Text != "0")
            {
                    Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));                
            }
             
        }

        private async void txt_NroPedido_GrupoPedido_Completed(object sender, EventArgs e)
        {
            Debug.WriteLine("[DEBUG] txt_NroPedido_GrupoPedido_Completed started");
            
            if (lbl_IdPicking.Text == "0")
            {
                Debug.WriteLine("[DEBUG] Creating new picking");
                Pedido_Picking_InsCab(v_Id_CD,v_Tipo_Picking,0,v_Id_Alm, v_Id_Picador);
            }
            
            Debug.WriteLine("[DEBUG] Calling Picking_GrupoPedidos_InsPedido");
            bool resultado = await Picking_GrupoPedidos_InsPedido(int.Parse(lbl_IdPicking.Text), int.Parse(txt_NroPedido_GrupoPedido.Text));
            
            if (!resultado)
            {
                Debug.WriteLine("[DEBUG] Picking_GrupoPedidos_InsPedido failed - stopping execution");
                txt_NroPedido_GrupoPedido.Text = "";
                txt_NroPedido_GrupoPedido.Focus();
                return;
            }
            
            Debug.WriteLine("[DEBUG] Picking_GrupoPedidos_InsPedido success - continuing");
            Picking_GrupoPedidos_SelCantPedidosxPicking(int.Parse(lbl_IdPicking.Text));

            Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));

            txt_NroPedido_GrupoPedido.Text = "";
            txt_NroPedido_GrupoPedido.Focus();
            
            Debug.WriteLine("[DEBUG] txt_NroPedido_GrupoPedido_Completed completed");
        }

        private async void btnLeerNroPedido_GrupoPedido_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[DEBUG] btnLeerNroPedido_GrupoPedido_Clicked started");
            
            var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
            Scanner.TopText = "Leer codigo de barras...";
            Scanner.TopText = "Puede usar EAN o QR";
            var result = await Scanner.Scan();
            if (result != null)
            {
                Debug.WriteLine("[DEBUG] Barcode scanned: " + result.Text);
                txt_NroPedido_GrupoPedido.Text = result.Text;

                if (lbl_IdPicking.Text == "0")
                {
                    Debug.WriteLine("[DEBUG] Creating new picking");
                    Pedido_Picking_InsCab(v_Id_CD, v_Tipo_Picking, 0, v_Id_Alm, v_Id_Picador);
                }
                
                Debug.WriteLine("[DEBUG] Calling Picking_GrupoPedidos_InsPedido");
                bool resultado = await Picking_GrupoPedidos_InsPedido(int.Parse(lbl_IdPicking.Text), int.Parse(txt_NroPedido_GrupoPedido.Text));
                
                if (!resultado)
                {
                    Debug.WriteLine("[DEBUG] Picking_GrupoPedidos_InsPedido failed - stopping execution");
                    txt_NroPedido_GrupoPedido.Text = "";
                    txt_NroPedido_GrupoPedido.Focus();
                    return;
                }
                
                Debug.WriteLine("[DEBUG] Picking_GrupoPedidos_InsPedido success - continuing");
                Picking_GrupoPedidos_SelCantPedidosxPicking(int.Parse(lbl_IdPicking.Text));

                Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));

                txt_NroPedido_GrupoPedido.Text = "";
                txt_NroPedido_GrupoPedido.Focus();
            }
            
            Debug.WriteLine("[DEBUG] btnLeerNroPedido_GrupoPedido_Clicked completed");
        }

        private async void Btn_GrupoPedidos_VerPedido_Clicked(object sender, EventArgs e)
        {
            if (lbl_CantidadPedidos_GrupoPedidos.Text == "0")
            {
                await DisplayAlert("Error!", "No existen pedidos", "Ok");
                return;
            }

            Model_Pedido_Picking Model_PedidoPicking = new Model_Pedido_Picking();
            Model_PedidoPicking.ID_PICKING = int.Parse(lbl_IdPicking.Text);

            await Navigation.PushAsync(new Pedido_Picking_GrupPed_ListPedidos(Model_PedidoPicking));
        }



      

        private void Chk_GrupoPedidos_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            InicializeControles();
        }

        private void InicializeControles()
        {
            lbl_IdPicking.Text = "0";
            lbl_NroPedido.Text = "";
            txtNroPedido.Text = "";
            lbl_RucCliente.Text = "";
            lbl_NombreCliente.Text = "";
            txt_NroPedido_GrupoPedido.Text = "";
            lbl_CantidadPedidos_GrupoPedidos.Text = "0";

            LV_DetallePedido.ItemsSource = null;

            lblTotPedido.Text = "0";
            lblTotPick.Text = "0";

            if (Chk_GrupoPedidos.IsChecked == true)
            {
                v_Tipo_Picking = 2;
                txtNroPedido.IsEnabled = false;
                btnLeerNroPedido.IsEnabled = false;
                txt_NroPedido_GrupoPedido.IsEnabled = true;
                btnLeerNroPedido_GrupoPedido.IsEnabled = true;
                Btn_GrupoPedidos_VerPedido.IsEnabled = true;

            }
            else
            {
                v_Tipo_Picking = 1;
                txtNroPedido.IsEnabled = true;
                btnLeerNroPedido.IsEnabled = true;
                txt_NroPedido_GrupoPedido.IsEnabled = false;
                btnLeerNroPedido_GrupoPedido.IsEnabled = false;
                Btn_GrupoPedidos_VerPedido.IsEnabled = false;
            }
        }
        private void Btn_Terminar_Clicked(object sender, EventArgs e)
        {
            Pedido_Picking_Terminar(int.Parse(lbl_IdPicking.Text));
            InicializeControles();

        }


      
        private void Chk_CantManual_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (Chk_CantManual.IsChecked == true)
            {
                txt_Cantidad_ProdDirecto.IsEnabled = true;
                Btn_GrabarProductoDirecto.IsEnabled = true;
                txt_Cantidad_ProdDirecto.Text = "";
                txt_Cantidad_ProdDirecto.Focus();
            }
            else
            {
                txt_Cantidad_ProdDirecto.IsEnabled = false;
                Btn_GrabarProductoDirecto.IsEnabled = false;
                txt_Cantidad_ProdDirecto.Text = "1";
                txt_IdProducto_Directo.Focus();
            }
        }


        private void Btn_GrabarProductoDirecto_Clicked(object sender, EventArgs e)
        {
           
            pedido_picking_det_InsDatos_SinCtrlUbicacion(int.Parse(lbl_IdPicking.Text), v_ITEMPEDIDO, txt_IdProducto_Directo.Text, int.Parse(txt_Cantidad_ProdDirecto.Text));
            txt_IdProducto_Directo.Text = "";
            txt_Cantidad_ProdDirecto.Text = "1";
            Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));
            txt_IdProducto_Directo.Focus();

        }

        private void txt_IdProducto_Directo_Completed(object sender, EventArgs e)
        {
            v_ITEMPEDIDO = 0;

            BuscarProductos_BarCode(txt_IdProducto_Directo.Text, v_Id_CD);
            if (txt_IdProducto_Directo.Text != "")
            {
                //buscar si el codigo esta en la lista
                int ExistProdenPedido = 0;

                foreach (var item in LV_DetallePedido.ItemsSource)
                {
                    var Dt_DetallePedido = item as Model_Pedido_Detalle;
                    if (Dt_DetallePedido.ID_ARTICULO == txt_IdProducto_Directo.Text)
                    {
                        ExistProdenPedido = 1;

                        v_ITEMPEDIDO = Dt_DetallePedido.ITEMPEDIDO;
                        break;
                    }

                }

                if (ExistProdenPedido == 1)
                {
                    if (Chk_CantManual.IsChecked == false)
                    {
                        pedido_picking_det_InsDatos_SinCtrlUbicacion(int.Parse(lbl_IdPicking.Text), v_ITEMPEDIDO, txt_IdProducto_Directo.Text, int.Parse(txt_Cantidad_ProdDirecto.Text));
                        txt_IdProducto_Directo.Text = "";
                        txt_Cantidad_ProdDirecto.Text = "1";
                        Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));
                        txt_IdProducto_Directo.Focus();
                    }
                    else
                    {
                        txt_Cantidad_ProdDirecto.Focus();
                    }
                }
                else
                {
                    DisplayAlert("Error!", "Producto NO ESTA EN EL PEDIDO!", "Ok");
                    //Vibrar();
                    txt_IdProducto_Directo.Text = "";
                    txt_IdProducto_Directo.Focus();
                }
            }
        }


        private void txt_Cantidad_ProdDirecto_Completed(object sender, EventArgs e)
        {
            Btn_GrabarProductoDirecto.Focus();
        }



        private async void  btnLeerIdProductoDirecto_Clicked(object sender, EventArgs e)
        {
            var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
            Scanner.TopText = "Leer codigo de barras...";
            Scanner.TopText = "Puede usar EAN o QR";
            var result = await Scanner.Scan();
            if (result != null)
            {
                txt_IdProducto_Directo.Text = result.Text;

                v_ITEMPEDIDO = 0;

                BuscarProductos_BarCode(txt_IdProducto_Directo.Text, v_Id_CD);
                if (txt_IdProducto_Directo.Text != "")
                {
                    //buscar si el codigo esta en la lista
                    int ExistProdenPedido = 0;

                    foreach (var item in LV_DetallePedido.ItemsSource)
                    {
                        var Dt_DetallePedido = item as Model_Pedido_Detalle;
                        if (Dt_DetallePedido.ID_ARTICULO == txt_IdProducto_Directo.Text)
                        {
                            ExistProdenPedido = 1;

                            v_ITEMPEDIDO = Dt_DetallePedido.ITEMPEDIDO;
                            break;
                        }

                    }

                    if (ExistProdenPedido == 1)
                    {
                        if (Chk_CantManual.IsChecked == false)
                        {
                            pedido_picking_det_InsDatos_SinCtrlUbicacion(int.Parse(lbl_IdPicking.Text), v_ITEMPEDIDO, txt_IdProducto_Directo.Text, int.Parse(txt_Cantidad_ProdDirecto.Text));
                            txt_IdProducto_Directo.Text = "";
                            txt_Cantidad_ProdDirecto.Text = "1";
                            Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));
                            txt_IdProducto_Directo.Focus();
                        }
                        else
                        {
                            txt_Cantidad_ProdDirecto.Focus();
                        }
                    }
                    else
                    {
                        DisplayAlert("Error!", "Producto NO ESTA EN EL PEDIDO!", "Ok");
                        //Vibrar();
                        txt_IdProducto_Directo.Text = "";
                        txt_IdProducto_Directo.Focus();
                    }
                }

            }
        }


        //Metodos de Base de datos


        private void Picking_SelCabPedido(int NROPEDIDO)
        {
            try
            {
                lbl_NroPedido.Text = "";
                Conexion.Abrir();

                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_SelCabPedido", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NROPEDIDO", SqlDbType.Int).Value = NROPEDIDO;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    lbl_NroPedido.Text = Convert.ToString(dt.Rows[0][0]);
                    lbl_RucCliente.Text = Convert.ToString(dt.Rows[0][1]);
                    lbl_NombreCliente.Text = Convert.ToString(dt.Rows[0][2]);
                    Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking,lbl_NroPedido.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));
                    //InfoProductosStocks(Convert.ToString(dt.Rows[0][0]));
                    //txtcodigobarras.Text = "";
                }
                else
                {
                    DisplayAlert("Error", "No existe Nro de Pedido", "Ok");
                }

                Conexion.Cerrar();

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private void Picking_SelDetallePedido( int ID_CD,int TIPO_PICKING, string NroPedido,int SoloPend,int ID_PICKING)
        {
            try
            {
                LV_DetallePedido.ItemsSource = null;
                DataTable dt = new DataTable();

                    
                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_SelDetallePedido", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_CD", SqlDbType.VarChar, 150).Value = ID_CD;
                cmd.Parameters.Add("@TIPO_PICKING", SqlDbType.Int, 150).Value = TIPO_PICKING;
                cmd.Parameters.Add("@NroPedido", SqlDbType.VarChar, 150).Value = NroPedido;
                cmd.Parameters.Add("@SoloPend", SqlDbType.Int).Value = SoloPend;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;

                
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();
             
                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_Pedido_Detalle> PedidoDetalle = new List<Model_Pedido_Detalle>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_Pedido_Detalle Model_PedidoDetalle = new Model_Pedido_Detalle();
                        Model_PedidoDetalle.ITEMPEDIDO = int.Parse(dt.Rows[i][0].ToString());
                        Model_PedidoDetalle.ID_ARTICULO = dt.Rows[i][1].ToString();
                        Model_PedidoDetalle.ART_DESCRIPCION = dt.Rows[i][2].ToString();
                        Model_PedidoDetalle.CANTIDAD = int.Parse(dt.Rows[i][3].ToString());
                        Model_PedidoDetalle.PICK= int.Parse(dt.Rows[i][4].ToString());
                        PedidoDetalle.Add(Model_PedidoDetalle);
                    }
                    LV_DetallePedido.ItemsSource = PedidoDetalle;

                    //Calcular Totales
                    int Tot_Cantidad= 0;
                    int Tot_Pick = 0;

                    foreach (Model_Pedido_Detalle item in PedidoDetalle)
                    {
                        Tot_Cantidad += item.CANTIDAD;
                        Tot_Pick += item.PICK;
                    }

                    lblTotPedido.Text = Tot_Cantidad.ToString();
                    lblTotPick.Text = Tot_Pick.ToString();

                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private void Picking_Procesar(int ID_CD, int TIPO_PICKING,int ID_PICKING, int NROPEDIDO, string ID_ALM, int ID_PICADOR)
        {
            Console.WriteLine("[DEBUG] Picking_Procesar started - ID_CD: " + ID_CD + ", TIPO_PICKING: " + TIPO_PICKING + ", ID_PICKING: " + ID_PICKING + ", NROPEDIDO: " + NROPEDIDO + ", ID_ALM: " + ID_ALM + ", ID_PICADOR: " + ID_PICADOR);
            try
            {
                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_Procesar_Lotes", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_CD", SqlDbType.Int).Value = ID_CD;
                cmd.Parameters.Add("@TIPO_PICKING", SqlDbType.Int).Value = TIPO_PICKING;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                
                // IMPORTANTE: Si NROPEDIDO es 0, enviarlo como NULL al SP
                if (NROPEDIDO == 0)
                {
                    Console.WriteLine("[DEBUG] NROPEDIDO es 0, enviando NULL al SP (grupo de pedidos)");
                    cmd.Parameters.Add("@NROPEDIDO", SqlDbType.Int).Value = DBNull.Value;
                }
                else
                {
                    Console.WriteLine("[DEBUG] NROPEDIDO: " + NROPEDIDO);
                    cmd.Parameters.Add("@NROPEDIDO", SqlDbType.Int).Value = NROPEDIDO;
                }
                
                cmd.Parameters.Add("@ID_ALM", SqlDbType.VarChar, 15).Value = ID_ALM;
                cmd.Parameters.Add("@ID_PICADOR", SqlDbType.Int).Value = ID_PICADOR;
                cmd.Parameters.Add("@FORZAR_RECALCULO", SqlDbType.Bit).Value = 0;
                

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                int Filas = 0;
                Filas = dt.Rows.Count;
                Console.WriteLine("[DEBUG] SP ejecutado - Filas retornadas: " + Filas);

                if (Filas > 0)
                {
                    // DEBUG: Ver cuántas columnas y qué valores retorna el SP
                    Console.WriteLine("[DEBUG] Columnas retornadas: " + dt.Columns.Count);
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        Console.WriteLine("[DEBUG] Columna[" + i + "] - Nombre: '" + dt.Columns[i].ColumnName + "', Tipo: " + dt.Columns[i].DataType.Name + ", Valor: '" + (dt.Rows[0][i] == DBNull.Value ? "NULL" : dt.Rows[0][i].ToString()) + "'");
                    }
                    
                    lbl_IdPicking.Text = dt.Rows[0][0].ToString();
                    v_CANT_LINEAS_RUTA = int.Parse(dt.Rows[0][1].ToString());
                    Console.WriteLine("[DEBUG] Picking procesado - ID_PICKING: " + lbl_IdPicking.Text + ", CANT_LINEAS_RUTA: " + v_CANT_LINEAS_RUTA);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("[DEBUG] Exception in Picking_Procesar: " + ex.Message);
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        
        private void Picking_GrupoPedidos_SelCantPedidosxPicking(int ID_PICKING)
        {
            try
            {
                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_GrupoPedidos_SelCantPedidosxPicking", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                lbl_CantidadPedidos_GrupoPedidos.Text = dt.Rows[0][0].ToString() ;

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private void Pedido_Picking_InsCab(int ID_CD,int TIPO_PICKING,int NROPEDIDO, string ID_ALM,int ID_PICADOR)
        {
            try
            {
                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_InsCab", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_CD", SqlDbType.Int).Value = ID_CD;
                cmd.Parameters.Add("@TIPO_PICKING", SqlDbType.Int).Value = TIPO_PICKING;
                cmd.Parameters.Add("@NROPEDIDO", SqlDbType.VarChar).Value = NROPEDIDO;
                cmd.Parameters.Add("@ID_ALM", SqlDbType.VarChar).Value = ID_ALM;
                cmd.Parameters.Add("@ID_PICADOR", SqlDbType.Int).Value = ID_PICADOR;

                


                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    lbl_IdPicking.Text = dt.Rows[0][0].ToString();
                }
 

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async Task<bool> Picking_GrupoPedidos_InsPedido(int ID_PICKING, int NROPEDIDO)
        {
            Debug.WriteLine("[DEBUG] Picking_GrupoPedidos_InsPedido started - ID_PICKING: " + ID_PICKING + ", NROPEDIDO: " + NROPEDIDO);
            try
            {
                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_GrupoPedidos_InsPedido", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@NROPEDIDO", SqlDbType.Int).Value = NROPEDIDO;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                // Verificar si el SP retornó resultado
                if (dt.Rows.Count > 0)
                {
                    string status = dt.Rows[0]["STATUS"].ToString();
                    string message = dt.Rows[0]["MESSAGE"].ToString();
                    
                    Debug.WriteLine("[DEBUG] SP Result - STATUS: " + status + ", MESSAGE: " + message);
                    
                    if (status == "ERROR")
                    {
                        await DisplayAlert("Atención", message, "Ok");
                        Debug.WriteLine("[DEBUG] Picking_GrupoPedidos_InsPedido failed");
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("[DEBUG] Picking_GrupoPedidos_InsPedido success - " + message);
                        return true;
                    }
                }
                
                Debug.WriteLine("[DEBUG] Picking_GrupoPedidos_InsPedido - No result from SP");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[DEBUG] Exception in Picking_GrupoPedidos_InsPedido: " + ex.Message);
                await DisplayAlert("Error", "Error al agregar el pedido. Por favor, intente nuevamente.", "Ok");
                return false;
            }
        }

        private void Pedido_Picking_Terminar(int ID_PICKING)
        {
            try
            { 
                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_Terminar", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING; 
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                cmd.ExecuteNonQuery(); 
                Conexion.Cerrar();

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private void pedido_picking_det_InsDatos_SinCtrlUbicacion(int ID_PICKING,int ITEMPEDIDO,string ID_ARTICULO,int PICK)
        {
            try
            {
                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_pedido_picking_det_InsDatos_SinCtrlUbicacion", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@ITEMPEDIDO", SqlDbType.Int).Value = ITEMPEDIDO;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar,31).Value = ID_ARTICULO;
                cmd.Parameters.Add("@PICK", SqlDbType.Int).Value = PICK;
                
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                cmd.ExecuteNonQuery();
                Conexion.Cerrar();

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
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
                        txt_IdProducto_Directo.Text = Convert.ToString(dt.Rows[0][0]);                       
                    }
                    else
                    {
                        DisplayAlert("Error", "Existe más de un producto con el mismo CodBar", "Ok");
                       //Vibrar();
                        txt_IdProducto_Directo.Text = "";
                        txt_IdProducto_Directo.Focus();

                    }
                }
                else
                {
                    await DisplayAlert("Error!", "Producto no existe!", "Ok");
                    //Vibrar();
                    txt_IdProducto_Directo.Text = "";
                    txt_IdProducto_Directo.Focus();

                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

    }
}