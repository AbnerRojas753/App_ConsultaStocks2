using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Data.SqlClient;
using System.Data;

using ZXing.Net.Mobile.Forms;
using App_ConsultaStocks.Datos;
using App_ConsultaStocks.Modelo;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;


namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Pedido_RepAlm_Picking : ContentPage
    {
        int v_Id_CD = 0;
        string v_Id_Alm = "";
        int v_Id_Picador = 0;
        int varorden = 0;
        //int v_Id_Picking = 0;
        int v_Tipo_Picking = 1; //1 es x pedido 2 es grupo pedidos
        int v_VerSoloPendiente = 1;
        int v_CANT_LINEAS_RUTA = 0;
        int v_cant_lineas = 0;
        int v_ITEMPEDIDO = 0;
        string v_tipo_Sug = "";

        public Pedido_RepAlm_Picking(Model_Pedido_Picking _Model_PedidoPicking)
        {
            InitializeComponent();

            v_Id_CD = _Model_PedidoPicking.ID_CD;
            lbl_NombreSede.Text = _Model_PedidoPicking.DESC_CD;
            lbl_IdPicking.Text = "";
            v_Id_Alm = _Model_PedidoPicking.ID_ALM;
            v_Id_Picador = _Model_PedidoPicking.ID_PICADOR;

            Chk_VerSoloPend.IsChecked = true;
            txt_Cantidad_ProdDirecto.Text = "";

            if (_Model_PedidoPicking.ID_PICKING != 0)
            {

                lbl_IdPicking.Text = _Model_PedidoPicking.ID_PICKING.ToString();
                Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_IdPicking.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));

            }
            else
            {
                lbl_IdPicking.Text = "0";
            }

        }

        protected override async void OnAppearing()
        {
            Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_IdPicking.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));
        }

        //private void ItemPicker_Piso_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (ItemPicker_Piso.SelectedIndex != -1)
        //    {

        //        LV_DetalleSugerido.ItemsSource = null;
        //        ItemPicker_Rack.ItemsSource = null;
        //        Sel_Rack(lbl_IdPicking.Text);
        //    }

        //}SelectedIndexChanged="ItemPicker_Piso_SelectedIndexChanged"
        //SelectedIndexChanged="ItemPicker_Rack_SelectedIndexChanged"
        // SelectedIndexChanged="ItemPicker_Tipo_SelectedIndexChanged"

        private void ItemPicker_Tipo_Focused(object sender, FocusEventArgs e)
        {
            LV_DetalleSugerido.ItemsSource = null;
            ItemPicker_Rack.ItemsSource = null;
            ItemPicker_Piso.ItemsSource = null;
            Sel_Tipo();
        }

        private void ItemPicker_Piso_Focused(object sender, FocusEventArgs e)
        {
            if (ItemPicker_Tipo.SelectedIndex != -1)
            {
                LV_DetalleSugerido.ItemsSource = null;
                ItemPicker_Rack.ItemsSource = null;
                Sel_Pisos(lbl_IdPicking.Text);
            }
        }

        private void ItemPicker_Rack_Focused(object sender, FocusEventArgs e)
        {
            if (ItemPicker_Piso.SelectedIndex != -1)
            {
                Sel_Rack(lbl_IdPicking.Text);
            }
        }
        private void ItemPicker_Tipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemPicker_Tipo.SelectedIndex != -1)
            {



                var selectedItem = ItemPicker_Tipo.SelectedItem as Model_Almacen;
                var desc = selectedItem.DescripcionAlmacen;
                var id = selectedItem.IdAlmacen;
                v_tipo_Sug = desc;

                Console.WriteLine("[DEBUG] Tipo seleccionado: " + desc);

                //DisplayAlert("Mensaje!", "TEST " + selectedItem.DescripcionAlmacen + selectedItem.IdAlmacen, "Ok");
                if (desc == "SIN UBICA")
                {
                    //ItemPicker_Piso.IsVisible = false;
                    //ItemPicker_Rack.IsVisible = false;
                    stackLayoutPiso.IsVisible = false;
                    stackLayoutRack.IsVisible = false;



                    lblFiltro.Text = "";
                    Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_IdPicking.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));

                }
                else
                {
                    stackLayoutPiso.IsVisible = true;
                    stackLayoutRack.IsVisible = true;

                }
            }
        }

        private void ItemPicker_Rack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemPicker_Rack.SelectedIndex != -1)
            {
                var selectedItem = ItemPicker_Rack.SelectedItem as Model_Almacen;
                selectedItem.DescripcionAlmacen = selectedItem.IdAlmacen;
                ItemPicker_Rack.SelectedItem = selectedItem;


                var ID = selectedItem.IdAlmacen;
                var Descripcion = selectedItem.DescripcionAlmacen;
                //DisplayAlert("Mensaje!", "Iniciando Proceso: " + selectedItem.DescripcionAlmacen + selectedItem.IdAlmacen, "Ok");

                lblFiltro.Text = ID;

                Console.WriteLine("[DEBUG] Rack seleccionado: " + ID);

                Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_IdPicking.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));

            }
        }

        private void ItemPicker_Piso_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemPicker_Piso.SelectedIndex != -1)
            {
                var selectedItem = ItemPicker_Piso.SelectedItem as Model_Almacen;
                var ID = selectedItem.IdAlmacen;
                var Descripcion = selectedItem.DescripcionAlmacen;
                lblFiltro.Text = ID;

                Console.WriteLine("[DEBUG] Piso seleccionado: " + ID);

                Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_IdPicking.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));
            }
        }

        private void Sel_Tipo()
        {

            try
            {
                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Get_Tipos", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPicking", SqlDbType.Int).Value = lbl_IdPicking.Text;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();
                int Filas = 0;
                Filas = dt.Rows.Count;
                if (Filas > 0)
                {
                    //DisplayAlert("Mensaje! >"+Filas.ToString(), "Continuando proceso: " + lbl_IdPicking.Text, "Ok");

                    List<Model_Almacen> Almacenes = new List<Model_Almacen>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_Almacen Model_Alm = new Model_Almacen();

                        Model_Alm.IdAlmacen = dt.Rows[i][1].ToString();
                        Model_Alm.DescripcionAlmacen = dt.Rows[i][1].ToString();
                        Almacenes.Add(Model_Alm);
                    }
                    ItemPicker_Tipo.ItemsSource = Almacenes;
                    ItemPicker_Tipo.ItemDisplayBinding = new Binding("DescripcionAlmacen");
                }



            }
            catch (Exception ex)
            {
                DisplayAlert("Error ItemPicker_tipo", ex.Message, "Ok");
            }



        }


        private void Sel_Pisos(string ID_PICKING)
        {
            if (ItemPicker_Tipo.SelectedIndex != -1)
            {
                var selectedItem = ItemPicker_Tipo.SelectedItem as Model_Almacen;
                var ID = selectedItem.IdAlmacen;
                var Descripcion = selectedItem.DescripcionAlmacen;
                DataTable dt = new DataTable();

                try
                {

                    Conexion.Abrir();
                    SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Get_Pisos", Conexion.conectar);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TIPO", SqlDbType.VarChar).Value = Descripcion;
                    cmd.Parameters.Add("@IdPicking", SqlDbType.Int).Value = ID_PICKING;


                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar();



                    int Filas = 0;
                    Filas = dt.Rows.Count;

                    if (Filas > 0)
                    {
                        List<Model_Almacen> Almacenes = new List<Model_Almacen>();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            Model_Almacen Model_Alm = new Model_Almacen();
                            Model_Alm.IdAlmacen = dt.Rows[i][0].ToString();
                            Model_Alm.DescripcionAlmacen = dt.Rows[i][0].ToString();


                            // CONSULTAR PROGRESO
                            Conexion.Abrir();
                            SqlCommand cmd2 = new SqlCommand("alfa_usp_Sugerido_Picking_Get_Progress", Conexion.conectar);
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Parameters.Add("@Filtro", SqlDbType.VarChar).Value = dt.Rows[i][0].ToString();
                            cmd2.Parameters.Add("@Tipo", SqlDbType.VarChar).Value = Descripcion;
                            cmd2.Parameters.Add("@IdPicking", SqlDbType.Int).Value = ID_PICKING;
                            SqlDataAdapter adapter2 = new SqlDataAdapter(cmd2);
                            DataTable dt2 = new DataTable();
                            adapter2.Fill(dt2);
                            Conexion.Cerrar();
                            if (dt2.Rows.Count > 0)
                            {
                                DataRow rowprog = dt2.Rows[0];
                                string Progreso = rowprog[0].ToString();
                                Model_Alm.DescripcionAlmacen += "                " + Progreso;
                            }



                            Almacenes.Add(Model_Alm);
                        }
                        ItemPicker_Piso.ItemsSource = Almacenes;
                        ItemPicker_Piso.ItemDisplayBinding = new Binding("DescripcionAlmacen");
                        ItemPicker_Rack.ItemsSource = null;
                    }



                }
                catch (Exception ex)
                {
                    ItemPicker_Piso.ItemsSource = null;
                    DisplayAlert("Error ItemPicker_Piso", ex.Message, "Ok");
                }

            }


        }
        private void Sel_Rack(string ID_PICKING)
        {
            if (ItemPicker_Piso.SelectedIndex != -1)
            {



                var selectedItem2 = ItemPicker_Piso.SelectedItem as Model_Almacen;
                var ID2 = selectedItem2.IdAlmacen;
                var Descripcion2 = selectedItem2.IdAlmacen;

                ItemPicker_Rack.ItemsSource = null;

                DataTable dt = new DataTable();


                try
                {

                    Conexion.Abrir();
                    SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Get_Racks", Conexion.conectar);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@TIPO", SqlDbType.VarChar).Value = v_tipo_Sug;
                    cmd.Parameters.Add("@PISO", SqlDbType.VarChar).Value = Descripcion2;
                    cmd.Parameters.Add("@IdPicking", SqlDbType.Int).Value = ID_PICKING;


                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar();



                    int Filas = 0;
                    Filas = dt.Rows.Count;

                    if (Filas > 0)
                    {
                        List<Model_Almacen> Almacenes = new List<Model_Almacen>();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            Model_Almacen Model_Alm = new Model_Almacen();
                            Model_Alm.IdAlmacen = dt.Rows[i][0].ToString();
                            Model_Alm.DescripcionAlmacen = dt.Rows[i][0].ToString();


                            // CONSULTAR PROGRESO
                            Conexion.Abrir();
                            SqlCommand cmd2 = new SqlCommand("alfa_usp_Sugerido_Picking_Get_Progress", Conexion.conectar);
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Parameters.Add("@Filtro", SqlDbType.VarChar).Value = dt.Rows[i][0].ToString();
                            cmd2.Parameters.Add("@Tipo", SqlDbType.VarChar).Value = v_tipo_Sug;
                            cmd2.Parameters.Add("@IdPicking", SqlDbType.Int).Value = ID_PICKING;
                            SqlDataAdapter adapter2 = new SqlDataAdapter(cmd2);
                            DataTable dt2 = new DataTable();
                            adapter2.Fill(dt2);
                            Conexion.Cerrar();
                            if (dt2.Rows.Count > 0)
                            {
                                DataRow rowprog = dt2.Rows[0];
                                string Progreso = rowprog[0].ToString();
                                Model_Alm.DescripcionAlmacen += "            " + Progreso;
                            }

                            Almacenes.Add(Model_Alm);
                        }
                        ItemPicker_Rack.ItemsSource = Almacenes;
                        ItemPicker_Rack.ItemDisplayBinding = new Binding("DescripcionAlmacen");
                    }



                }
                catch (Exception ex)
                {
                    ItemPicker_Rack.ItemsSource = null;

                    DisplayAlert("Error ItemPicker_Rack", ex.Message, "Ok");
                }

            }


        }

        private void Picking_SelDetallePedido(int ID_CD, int TIPO_PICKING, string NroPedido, int SoloPend, int ID_PICKING)
        {
            try
            {
                LV_DetalleSugerido.ItemsSource = null;
                DataTable dt = new DataTable();
                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Detalle", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_CD", SqlDbType.VarChar, 150).Value = ID_CD;
                cmd.Parameters.Add("@TIPO_PICKING", SqlDbType.Int, 150).Value = TIPO_PICKING;
                cmd.Parameters.Add("@NroPedido", SqlDbType.VarChar, 150).Value = NroPedido;
                cmd.Parameters.Add("@SoloPend", SqlDbType.Int).Value = SoloPend;
                cmd.Parameters.Add("@IdPicking", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@FILTRO", SqlDbType.VarChar).Value = lblFiltro.Text;
                cmd.Parameters.Add("@TIPO", SqlDbType.VarChar).Value = v_tipo_Sug;

                Console.WriteLine("[DEBUG] Ejecutando store procedure: alfa_usp_Sugerido_Picking_Detalle con parámetros - ID_CD: " + ID_CD + ", TIPO_PICKING: " + TIPO_PICKING + ", NroPedido: " + NroPedido + ", SoloPend: " + SoloPend + ", IdPicking: " + ID_PICKING + ", FILTRO: " + lblFiltro.Text + ", TIPO: " + v_tipo_Sug);

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
                        Model_PedidoDetalle.PICK = int.Parse(dt.Rows[i][4].ToString());
                        PedidoDetalle.Add(Model_PedidoDetalle);
                    }
                    LV_DetalleSugerido.ItemsSource = PedidoDetalle;

                    int Tot_Cantidad = 0;
                    int Tot_Pick = 0;
                    int Tot_Art = 0;
                    foreach (Model_Pedido_Detalle item in PedidoDetalle)
                    {
                        Tot_Cantidad += item.CANTIDAD;
                        Tot_Pick += item.PICK;
                        Tot_Art += 1;
                    }
                    lblTotPedido.Text = Tot_Cantidad.ToString();
                    lblTotPick.Text = Tot_Pick.ToString();
                    v_CANT_LINEAS_RUTA = Tot_Art;
                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error CD", ex.Message, "Ok");
            }

        }




        private async void Btn_Iniciar_Clicked(object sender, EventArgs e)
        {

            Model_Pedido_Picking Model_PedidoPicking = new Model_Pedido_Picking();
            Model_PedidoPicking.ID_CD = v_Id_CD;
            Model_PedidoPicking.TIPO_PICKING = v_Tipo_Picking;
            Model_PedidoPicking.ID_PICADOR = v_Id_Picador;
            Model_PedidoPicking.ID_ALM = v_Id_Alm;
            Model_PedidoPicking.ID_PICKING = int.Parse(lbl_IdPicking.Text);
            Model_PedidoPicking.CANT_LINEAS_RUTA = v_CANT_LINEAS_RUTA;
            Model_PedidoPicking.PICKING_MANUAL = 0;
            await Navigation.PushAsync(new Pedido_RepAlm_Picking_Item(Model_PedidoPicking, lblFiltro.Text, v_tipo_Sug, "1"));
        }


        private async void Btn_PickingManual_Clicked(object sender, EventArgs e)
        {
            if (lbl_IdPicking.Text == "0")
            {
                await DisplayAlert("Error!", "Debe iniciar un proceso de Picking", "Ok");
                return;
            }


            var selectedItem = LV_DetalleSugerido.SelectedItem as Model_Pedido_Detalle;
            if (selectedItem == null)
            {

                await DisplayAlert("Error", "Debe seleccionar un item", "Ok");
                return;
            }
            else
            {
                int numeroOrden = LV_DetalleSugerido.ItemsSource.Cast<Model_Pedido_Detalle>().ToList().IndexOf(selectedItem) + 1;

                Model_Pedido_Picking Model_PedidoPicking = new Model_Pedido_Picking();
                Model_PedidoPicking.ID_CD = v_Id_CD;
                Model_PedidoPicking.NROPEDIDO = 0;
                Model_PedidoPicking.ID_PICADOR = v_Id_Picador;
                Model_PedidoPicking.ID_ALM = v_Id_Alm;
                Model_PedidoPicking.ID_PICKING = int.Parse(lbl_IdPicking.Text);
                Model_PedidoPicking.CANT_LINEAS_RUTA = v_CANT_LINEAS_RUTA;
                Model_PedidoPicking.PICKING_MANUAL = 1;

                await Navigation.PushAsync(new Pedido_RepAlm_Picking_Item(Model_PedidoPicking, lblFiltro.Text, v_tipo_Sug, numeroOrden.ToString()));

            }
        }

        private void Btn_GrabarProductoDirecto_Clicked(object sender, EventArgs e)
        {
            try
            {

                int ExistProdenPedido = Check_Producto();
                if (ExistProdenPedido == 1)
                {
                    Btn_GrabarProductoDirecto.Focus();
                }
                else
                {
                    DisplayAlert("Error!", "Producto NO ESTA EN EL PEDIDO!", "Ok");
                    //Vibrar();
                    txt_IdProducto_Directo.Text = "";
                    txt_IdProducto_Directo.Focus();
                    txt_IdProducto_Directo.Text = "";
                    txt_Cantidad_ProdDirecto.Text = "";
                    return;
                }

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Seleccionar_Ruta", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPicking", SqlDbType.Int).Value = int.Parse(lbl_IdPicking.Text);
                cmd.Parameters.Add("@ORDEN", SqlDbType.Int).Value = varorden;
                cmd.Parameters.Add("@FILTRO", SqlDbType.VarChar, 150).Value = lblFiltro.Text;
                cmd.Parameters.Add("@TIPO", SqlDbType.VarChar, 150).Value = v_tipo_Sug;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                Conexion.Cerrar();
                int Filas = 0;
                Filas = dt.Rows.Count;
                if (Filas > 0)
                {
                    int var_OrdenReal = Convert.ToInt32(dt.Rows[0][1]);
                    if (var_OrdenReal != 0)
                    {
                        Picking_Det_InsDatos(int.Parse(lbl_IdPicking.Text), "", 0, txt_IdProducto_Directo.Text, int.Parse(txt_Cantidad_ProdDirecto.Text), var_OrdenReal, false, false, "");
                        Picking_SelDetallePedido(v_Id_CD, v_Tipo_Picking, lbl_IdPicking.Text, v_VerSoloPendiente, int.Parse(lbl_IdPicking.Text));
                        txt_IdProducto_Directo.Focus();
                    }
                    else
                    {
                        DisplayAlert("Error encontrando el articulo", "/Orden Absoluto" + var_OrdenReal.ToString(), "Ok");

                    }
                }
                else
                {
                    DisplayAlert("Error encontrando el articulo", "/No se encontro el nro de orden", "Ok");
                }


            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }
            txt_IdProducto_Directo.Text = "";
            txt_Cantidad_ProdDirecto.Text = "";




        }



        private void InicializeControles()
        {
            lbl_IdPicking.Text = "0";
            //lbl_NroPedido.Text = "";
            //txtNroPedido.Text = "";
            //lbl_RucCliente.Text = "";
            //lbl_NombreCliente.Text = "";
            //txt_NroPedido_GrupoPedido.Text = "";
            //lbl_CantidadPedidos_GrupoPedidos.Text = "0";

            LV_DetalleSugerido.ItemsSource = null;

            lblTotPedido.Text = "0";
            lblTotPick.Text = "0";

            //if (Chk_GrupoPedidos.IsChecked == true)
            //{
            //    v_Tipo_Picking = 2;
            //    txtNroPedido.IsEnabled = false;
            //    btnLeerNroPedido.IsEnabled = false;
            //    txt_NroPedido_GrupoPedido.IsEnabled = true;
            //    btnLeerNroPedido_GrupoPedido.IsEnabled = true;
            //    Btn_GrupoPedidos_VerPedido.IsEnabled = true;

            //}
            //else
            //{
            //    v_Tipo_Picking = 1;
            //    txtNroPedido.IsEnabled = true;
            //    btnLeerNroPedido.IsEnabled = true;
            //    txt_NroPedido_GrupoPedido.IsEnabled = false;
            //    btnLeerNroPedido_GrupoPedido.IsEnabled = false;
            //    Btn_GrupoPedidos_VerPedido.IsEnabled = false;
            //}
        }
        private void Btn_Terminar_Clicked(object sender, EventArgs e)
        {
            //Pedido_Picking_Terminar(int.Parse(lbl_IdPicking.Text));
            InicializeControles();

        }


        private void Picking_Det_InsDatos(int ID_PICKING, string ID_UBICACION, int ITEMPEDIDO, string ID_ARTICULO, int PICK, int ORDEN, Boolean UBIC_OK, Boolean PICK_OK, string ID_UBICACION_NEW)
        {
            try
            {

                Conexion.Abrir();

                SqlCommand cmd = new SqlCommand("alfa_usp_pedido_picking_det_InsDatos2", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar).Value = ID_UBICACION;
                cmd.Parameters.Add("@ITEMPEDIDO", SqlDbType.BigInt).Value = ITEMPEDIDO;
                cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar).Value = ID_ARTICULO;
                cmd.Parameters.Add("@PICK", SqlDbType.Int).Value = PICK;
                cmd.Parameters.Add("@ORDEN", SqlDbType.Int).Value = ORDEN;
                cmd.Parameters.Add("@UBIC_OK", SqlDbType.Bit).Value = UBIC_OK;
                cmd.Parameters.Add("@PICK_OK", SqlDbType.Bit).Value = PICK_OK;
                cmd.Parameters.Add("@ID_UBICACION_NEW", SqlDbType.VarChar, 31).Value = ID_UBICACION_NEW;


                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                cmd.ExecuteScalar();

                Conexion.Cerrar();

            }
            catch (Exception ex)
            {
                DisplayAlert("Error Insertando Articulo", ex.Message, "Ok");
            }

        }


        private int Check_Producto()
        {
            v_ITEMPEDIDO = 0;

            BuscarProductos_BarCode(txt_IdProducto_Directo.Text, v_Id_CD);
            int ExistProdenPedido = 0;

            if (txt_IdProducto_Directo.Text != "")
            {
                //buscar si el codigo esta en la lista
                varorden = 1;
                foreach (var item in LV_DetalleSugerido.ItemsSource)
                {
                    var Dt_DetallePedido = item as Model_Pedido_Detalle;
                    //DisplayAlert("Debug", "Comparando " + Dt_DetallePedido.ID_ARTICULO.Trim() + " con " + txt_IdProducto_Directo.Text.Trim(), "Ok");
                    if (Dt_DetallePedido.ID_ARTICULO.Trim() == txt_IdProducto_Directo.Text.Trim())
                    {
                        ExistProdenPedido = 1;
                        v_ITEMPEDIDO = Dt_DetallePedido.ITEMPEDIDO;
                        break;
                    }
                    varorden = varorden + 1;
                }
            }
            return ExistProdenPedido;



        }
        private void txt_IdProducto_Directo_Completed(object sender, EventArgs e)
        {
            BuscarProductos_BarCode(txt_IdProducto_Directo.Text, v_Id_CD);
            //txt_Cantidad_ProdDirecto.Focus();
        }


        private void txt_Cantidad_ProdDirecto_Completed(object sender, EventArgs e)
        {




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
                        txt_IdProducto_Directo.Text = Convert.ToString(dt.Rows[0][0]);
                        txt_Cantidad_ProdDirecto.Focus();
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
                    DisplayAlert("Error!", "Producto no existe!", "Ok");
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