using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using App_ConsultaStocks.Datos;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using App_ConsultaStocks.Modelo;

using System.Data.SqlClient;
using System.Data;


using ZXing.Net.Mobile.Forms;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Producto_Ubicaciones_AgregarProductodesdeubicacion : ContentPage
    {
        int v_Id_CD;
        string v_Id_Alm;

        string v_Id_Articulo = "";

        Model_UbicacionesProductos _Model_ArtUbic = new Model_UbicacionesProductos();
        public Producto_Ubicaciones_AgregarProductodesdeubicacion(Model_UbicacionesProductos _Model_ArtUbic)
        {
            InitializeComponent();

            lblNombreSede.Text = _Model_ArtUbic.DESC_CD;
            lblNombreAlmacen.Text = _Model_ArtUbic.DESC_ALM;

            v_Id_CD = _Model_ArtUbic.ID_CD;
            v_Id_Alm = _Model_ArtUbic.ID_ALM;


        }


        private void txtIdUbicacion_Completed(object sender, EventArgs e)
        {
            Ubicaciones_SelDatosUbicacion(v_Id_Alm, txtIdUbicacion.Text);

        }

        private async void btnLeerIdUbicacion_Clicked(object sender, EventArgs e)
        {
            try
            {
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txtIdUbicacion.Text = result.Text;
                    Ubicaciones_SelDatosUbicacion(v_Id_Alm, txtIdUbicacion.Text);

                }

            }
            catch (Exception ex)
            {

                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }


        private async void Btn_AgregarProducto_Clicked(object sender, EventArgs e)
        {
            Model_UbicacionesProductos _Model_ArtUbic = new Model_UbicacionesProductos();
            _Model_ArtUbic.ID_CD = v_Id_CD;
            _Model_ArtUbic.DESC_CD = lblNombreSede.Text;

            _Model_ArtUbic.ID_ALM = v_Id_Alm;
            _Model_ArtUbic.DESC_ALM = lblNombreAlmacen.Text;

            _Model_ArtUbic.ID_UBICACION = lbl_Id_Ubicacion.Text;
            _Model_ArtUbic.DESCRIPCION = lbl_DescUbicacion.Text;

            _Model_ArtUbic.TipoEvento = "Agregar";


            await Navigation.PushAsync(new Producto_Ubicaciones_MantDatos(_Model_ArtUbic));


        }

        private async void Btn_ModificarDatos_Clicked(object sender, EventArgs e)
        {

            Model_UbicacionesProductos _Model_ArtUbic = new Model_UbicacionesProductos();
            _Model_ArtUbic.ID_CD = v_Id_CD;
            _Model_ArtUbic.DESC_CD = lblNombreSede.Text;

            _Model_ArtUbic.ID_ALM = v_Id_Alm;
            _Model_ArtUbic.DESC_ALM = lblNombreAlmacen.Text;

            _Model_ArtUbic.ID_UBICACION = lbl_Id_Ubicacion.Text;
            _Model_ArtUbic.DESCRIPCION = lbl_DescUbicacion.Text;

            var selectedItem = LV_ListaUbicaciones.SelectedItem as Model_UbicacionesProductos;

            if (selectedItem == null)

            {
                await DisplayAlert("Error", "Debe seleccionar un item", "Ok");
                return;
            }

            _Model_ArtUbic.TipoEvento = "Modificar";
            if (selectedItem.ID_ARTICULO != "")
            {
                _Model_ArtUbic.ID_ARTICULO = selectedItem.ID_ARTICULO;
                _Model_ArtUbic.ART_DESCRIPCION = selectedItem.ART_DESCRIPCION;
                _Model_ArtUbic.ID_LOTE = selectedItem.ID_LOTE; // Nueva columna al final


            }

            await Navigation.PushAsync(new Producto_Ubicaciones_MantDatos(_Model_ArtUbic));

        }

        private void txtIdProductoVerificacion_Completed(object sender, EventArgs e)
        {
            v_Id_Articulo = "";
            BuscarIdProductoProducto_DesdeBarCode(txtIdProductoVerificacion.Text, v_Id_CD);
            if (v_Id_Articulo != "")
            {
                Ubicaciones_ArtUbic_SelDatosIdArticulo(v_Id_Articulo, v_Id_Alm, lbl_Id_Ubicacion.Text, v_Id_CD);
            }
            else
            {
                lblExistenciaProducto.Text = "         ";
                lblExistenciaProducto.BackgroundColor = Color.White;
            }
            txtIdProductoVerificacion.Text = "";
            lblExistenciaProducto.Focus();

        }


        private async void btnLeerIdProductoVerificacion_Clicked(object sender, EventArgs e)
        {
            try
            {
                v_Id_Articulo = "";
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txtIdProductoVerificacion.Text = result.Text;

                    BuscarIdProductoProducto_DesdeBarCode(txtIdProductoVerificacion.Text, v_Id_CD);
                    if (v_Id_Articulo != "")
                    {
                        Ubicaciones_ArtUbic_SelDatosIdArticulo(v_Id_Articulo, v_Id_Alm, lbl_Id_Ubicacion.Text, v_Id_CD);
                    }
                    else
                    {
                        lblExistenciaProducto.Text = "         ";
                        lblExistenciaProducto.BackgroundColor = Color.White;
                    }
                    txtIdProductoVerificacion.Text = "";
                    lblExistenciaProducto.Focus();

                }

            }
            catch (Exception ex)
            {

                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }



        protected override async void OnAppearing()
        {
            lblExistenciaProducto.Text = "         ";
            lblExistenciaProducto.BackgroundColor = Color.White;
            v_Id_Articulo = "";
            if (lbl_Id_Ubicacion.Text != "")
            {

                base.OnAppearing();
                Ubicaciones_SelDatosUbicacion(v_Id_Alm, lbl_Id_Ubicacion.Text);

            }
            //Subscribe();
            //vm.Theme = Settings.th.ToString();
            //await SetCardButtons(Settings.cc.Text());
        }



        //Metodos de Base de datos
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
                    lbl_Id_Ubicacion.Text = dt.Rows[0][0].ToString();
                    lbl_DescUbicacion.Text = dt.Rows[0][1].ToString();
                    txtIdUbicacion.Text = "";
                    Ubicaciones_ArtUbic_SelDatos(v_Id_Alm, lbl_Id_Ubicacion.Text);
                    LV_ListaUbicaciones.Focus();
                }
                else
                {
                    lbl_Id_Ubicacion.Text = "";
                    lbl_DescUbicacion.Text = "";
                    txtIdUbicacion.Text = "";
                    await DisplayAlert("Error!", "Ubicacion no existe!", "Ok");
                    txtIdUbicacion.Focus();
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        public void Ubicaciones_ArtUbic_SelDatos(string ID_ALMACEN, string ID_UBICACION)
        {
            try
            {
                LV_ListaUbicaciones.ItemsSource = null;
                DataTable dt = new DataTable();

                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_SelDatos", Conexion.conectar_WMS_LIM);
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
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_SelDatos", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 150).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 150).Value = ID_UBICACION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }



                int Filas = 0;
                Filas = dt.Rows.Count;

                lbl_CantSkus.Text = "0";

                if (Filas > 0)
                {
                    List<Model_UbicacionesProductos> UbicacionesProductos = new List<Model_UbicacionesProductos>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_UbicacionesProductos Model_UbicacionesProductos = new Model_UbicacionesProductos();
                        Model_UbicacionesProductos.ID_ARTICULO = dt.Rows[i][1].ToString();
                        Model_UbicacionesProductos.ART_DESCRIPCION = dt.Rows[i][2].ToString();
                        Model_UbicacionesProductos.ID_LOTE = dt.Rows[i][3].ToString(); // Nueva columna al final

                        Model_UbicacionesProductos.STOCK = dt.Rows[i][4].ToString();
                        Model_UbicacionesProductos.MAX = dt.Rows[i][5].ToString();
                        Model_UbicacionesProductos.MIN = dt.Rows[i][6].ToString();

                        UbicacionesProductos.Add(Model_UbicacionesProductos);
                    }
                    LV_ListaUbicaciones.ItemsSource = UbicacionesProductos;

                    //Calcular Totales
                    int Tot_Cantidad = 0;

                    foreach (Model_UbicacionesProductos item in UbicacionesProductos)
                    {
                        Tot_Cantidad += 1;
                    }
                    lbl_CantSkus.Text = Tot_Cantidad.ToString() + " sku(s)";
                    // LV_ListaStocks.ItemsSource = dt.Select().ToList().Select(r => new Class_StocksAlm(r["CodAlmacén"] as string, r["Almacén"] as string, r["Contabilizado"] as string, r["Reservado"] as string, r["Stock_Disponible"] as string));
                }


            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async void BuscarIdProductoProducto_DesdeBarCode(string CodBar, int Id_CD)
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
                        v_Id_Articulo = dt.Rows[0][0].ToString();

                    }
                    else
                    {
                        await DisplayAlert("Error", "Existe más de un producto con el mismo CodBar", "Ok");
                        v_Id_Articulo = "";
                        lblExistenciaProducto.Focus();
                    }

                }
                else
                {
                    v_Id_Articulo = "";
                    await DisplayAlert("Error!", "Producto no existe!", "Ok");
                    lblExistenciaProducto.Focus();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private void Ubicaciones_ArtUbic_SelDatosIdArticulo(string ID_ARTICULO, string ID_ALMACEN, string ID_UBICACION, int Id_CD)
        {
            try
            {
                DataTable dt = new DataTable();

                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_SelDatosIdArticulo", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 25).Value = ID_UBICACION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_SelDatosIdArticulo", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 25).Value = ID_UBICACION;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    lblExistenciaProducto.Text = "  Existe   ";
                    lblExistenciaProducto.BackgroundColor = Color.LightGreen;

                    //luego se buscar el item en el ListView
                    int i = 0;
                    foreach (var item in LV_ListaUbicaciones.ItemsSource)
                    {
                        i++;
                        var Dt_ArticulosUbicacion = item as Model_UbicacionesProductos;
                        if (Dt_ArticulosUbicacion.ID_ARTICULO == txtIdProductoVerificacion.Text)
                        {
                            LV_ListaUbicaciones.ScrollTo(item, ScrollToPosition.Center, true);
                            LV_ListaUbicaciones.SelectedItem = item;
                            break;
                        }

                    }

                }
                else
                {
                    lblExistenciaProducto.Text = " NO Existe ";
                    lblExistenciaProducto.BackgroundColor = Color.OrangeRed;

                }
                lblExistenciaProducto.Focus();

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }


    }
}