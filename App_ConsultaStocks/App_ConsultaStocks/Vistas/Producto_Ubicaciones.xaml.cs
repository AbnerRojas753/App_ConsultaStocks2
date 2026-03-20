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


namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Producto_Ubicaciones : ContentPage
    {
        int v_Id_CD;
        string v_Id_Alm;


        //  Model_UbicacionesProductos _Model_ArtUbic = new Model_UbicacionesProductos();

        public Producto_Ubicaciones(Model_UbicacionesProductos _Model_ArtUbic)
        {
            InitializeComponent();
            lblNombreSede.Text = _Model_ArtUbic.DESC_CD;
            lblNombreAlmacen.Text = _Model_ArtUbic.DESC_ALM;

            v_Id_CD = _Model_ArtUbic.ID_CD;
            v_Id_Alm = _Model_ArtUbic.ID_ALM;
        }


        private void txtcodigobarras_Completed(object sender, EventArgs e)
        {
            BuscarProductos_BarCode(txtcodigobarras.Text, v_Id_CD);
        }


        private async void btnLeerCodBar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txtcodigobarras.Text = result.Text;
                    BuscarProductos_BarCode(txtcodigobarras.Text, v_Id_CD);
                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }
        }



        private void txtIdUbicacion_Completed(object sender, EventArgs e)
        {

            //Ubicaciones_SelDatosUbicacion(v_Id_Alm, txtIdUbicacion.Text);

            //if (lbl_IdUbicacion.Text != "")
            //{
            //    Ubicaciones_ArtUbic_SelDatosIdArticulo(Lbl_Codigo.Text, v_Id_Alm, lbl_IdUbicacion.Text, v_Id_CD);
            //}

        }

        private async void btnLeerUbicacion_Clicked(object sender, EventArgs e)
        {
            //try
            //{
            //    var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
            //    Scanner.TopText = "Leer codigo de barras...";
            //    Scanner.TopText = "Puede usar EAN o QR";
            //    var result = await Scanner.Scan();
            //    if (result != null)
            //    {
            //        txtIdUbicacion.Text = result.Text;

            //        Ubicaciones_SelDatosUbicacion(v_Id_Alm, txtIdUbicacion.Text);

            //        if (lbl_IdUbicacion.Text != "")
            //        {
            //            Ubicaciones_ArtUbic_SelDatosIdArticulo(Lbl_Codigo.Text, v_Id_Alm, lbl_IdUbicacion.Text, v_Id_CD);
            //        }

            //    }

            //}
            //catch (Exception ex)
            //{
            //    DisplayAlert("Error", ex.Message, "Ok");
            //}
        }

        private void Btn_Limpiar_Clicked(object sender, EventArgs e)
        {
            LimpriarDatos();

        }



        private async void Btn_GuardarDatos_Clicked(object sender, EventArgs e)
        {

            //if (txtIdUbicacion.Text == "" && TxtMaximo.Text == "" && TxtMinimo.Text == "")
            //{
            //    await DisplayAlert("Error!", "Complete los datos", "Ok");
            //    return;

            //}
            //Ubicaciones_ArtUbic_InsUpdDatos(Lbl_Codigo.Text, v_Id_Alm, lbl_IdUbicacion.Text, v_Id_CD, int.Parse(TxtMaximo.Text), int.Parse(TxtMinimo.Text));
            //Ubicaciones_ArtUbic_SelUbicacionesArticulo(v_Id_Alm, Lbl_Codigo.Text, v_Id_CD);
            //LimpriarDatos();

        }

        private async void Btn_EliminarDatos_Clicked(object sender, EventArgs e)
        {
            //bool answer = await DisplayAlert("Eliminar dato?", "Esta seguro de eliminar el producto", "Sí", "No");
            //if (answer == true)
            //{
            //    Ubicaciones_ArtUbic_DelDatos(Lbl_Codigo.Text, v_Id_Alm, lbl_IdUbicacion.Text, v_Id_CD);
            //}
        }



        //Metodos de Base de datos
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

                        Lbl_Codigo.Text = dt.Rows[0][0].ToString();
                        lblDescripcionProducto.Text = dt.Rows[0][1].ToString();

                        Ubicaciones_ArtUbic_SelUbicacionesArticulo(v_Id_Alm, Lbl_Codigo.Text, v_Id_CD);
                        txtcodigobarras.Text = "";
                        Lbl_Codigo.Focus();


                    }
                    else
                    {
                        DisplayAlert("Error", "Existe más de un producto con el mismo CodBar", "Ok");
                        Lbl_Codigo.Text = "";
                        lblDescripcionProducto.Text = "";
                        txtcodigobarras.Focus();
                    }

                }
                else
                {
                    Lbl_Codigo.Text = "";
                    lblDescripcionProducto.Text = "";
                    txtcodigobarras.Text = "";
                    await DisplayAlert("Error!", "Producto no existe!", "Ok");
                    txtcodigobarras.Focus();
                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }


        private void Ubicaciones_ArtUbic_SelUbicacionesArticulo(string ID_ALMACEN, string ITEMNMBR, int Id_CD)
        {
            try
            {
                LV_ListaUbicaciones.ItemsSource = null;
                DataTable dt = new DataTable();

                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_SelUbicacionesArticulo", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 150).Value = ITEMNMBR;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 150).Value = ID_ALMACEN;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_SelUbicacionesArticulo", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 150).Value = ITEMNMBR;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 150).Value = ID_ALMACEN;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }



                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_UbicacionesProductos> UbicacionesProductos = new List<Model_UbicacionesProductos>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_UbicacionesProductos Model_UbicacionesProductos = new Model_UbicacionesProductos();
                        Model_UbicacionesProductos.ID_UBICACION = dt.Rows[i][0].ToString();
                        // Model_UbicacionesProductos.DESCRIPCION = dt.Rows[i][1].ToString();
                        Model_UbicacionesProductos.STOCK = dt.Rows[i][2].ToString();
                        Model_UbicacionesProductos.MAX = dt.Rows[i][3].ToString();
                        Model_UbicacionesProductos.MIN = dt.Rows[i][4].ToString();
                        Model_UbicacionesProductos.ID_LOTE = dt.Rows[i][5].ToString(); // Nueva columna


                        UbicacionesProductos.Add(Model_UbicacionesProductos);
                    }
                    LV_ListaUbicaciones.ItemsSource = UbicacionesProductos;

                    // LV_ListaStocks.ItemsSource = dt.Select().ToList().Select(r => new Class_StocksAlm(r["CodAlmacén"] as string, r["Almacén"] as string, r["Contabilizado"] as string, r["Reservado"] as string, r["Stock_Disponible"] as string));
                }

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

                //if (Filas > 0)
                //{
                //    lbl_IdUbicacion.Text = dt.Rows[0][0].ToString();
                //    txtIdUbicacion.Text = "";
                //    lbl_IdUbicacion.Focus();
                //}
                //else
                //{
                //    lbl_IdUbicacion.Text = "";
                //    await DisplayAlert("Error!", "Ubicacion no existe!", "Ok");
                //    txtIdUbicacion.Text = "";

                //}

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

                //if (Filas > 0)
                //{
                //    lbl_IdUbicacion.Text = dt.Rows[0][2].ToString();
                //    TxtMaximo.Text = dt.Rows[0][4].ToString();
                //    TxtMinimo.Text = dt.Rows[0][5].ToString();
                //    Btn_GuardarDatos.Text = "Modificar datos";
                //    Btn_GuardarDatos.BackgroundColor = Color.Yellow;

                //}
                //else
                //{

                //    TxtMaximo.Text = "";
                //    TxtMinimo.Text = "";
                //    Btn_GuardarDatos.Text = "Guardar datos";
                //    Btn_GuardarDatos.BackgroundColor = Color.LightGray;

                //}

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async void Ubicaciones_ArtUbic_InsUpdDatos(string ID_ARTICULO, string ID_ALMACEN, string ID_UBICACION, int Id_CD, int MAX, int MIN)
        {
            try
            {
                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_InsUpdDatos", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 25).Value = ID_UBICACION;
                    cmd.Parameters.Add("@MAX", SqlDbType.Int).Value = MAX;
                    cmd.Parameters.Add("@MIN", SqlDbType.Int).Value = MIN;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    cmd.ExecuteScalar();
                    await DisplayAlert("Registro OK", "Dato registrado correctamente", "OK");
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_InsUpdDatos", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 25).Value = ID_UBICACION;
                    cmd.Parameters.Add("@MAX", SqlDbType.Int).Value = MAX;
                    cmd.Parameters.Add("@MIN", SqlDbType.Int).Value = MIN;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    cmd.ExecuteScalar();
                    await DisplayAlert("Registro OK", "Dato registrado correctamente", "OK");
                    Conexion.Cerrar_WMS_ATE();

                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }


        private void LimpriarDatos()
        {
            //txtIdUbicacion.Text = "";
            //lbl_IdUbicacion.Text = "";
            //TxtMaximo.Text = "";
            //TxtMinimo.Text = "";
            //Btn_GuardarDatos.Text = "Guardar datos";
            //Btn_GuardarDatos.BackgroundColor = Color.LightGray;
            //lbl_IdUbicacion.Focus();
        }



        private async void Ubicaciones_ArtUbic_DelDatos(string ID_ARTICULO, string ID_ALMACEN, string ID_UBICACION, int Id_CD)
        {
            try
            {
                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_DelDatos", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 25).Value = ID_UBICACION;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    cmd.ExecuteScalar();
                    await DisplayAlert("Operación OK", "Dato eliminado correctamente", "OK");

                    Conexion.Cerrar_WMS_LIM();

                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_Ubicaciones_ArtUbic_DelDatos", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_ARTICULO", SqlDbType.VarChar, 31).Value = ID_ARTICULO;
                    cmd.Parameters.Add("@ID_ALMACEN", SqlDbType.VarChar, 15).Value = ID_ALMACEN;
                    cmd.Parameters.Add("@ID_UBICACION", SqlDbType.VarChar, 25).Value = ID_UBICACION;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    cmd.ExecuteScalar();
                    await DisplayAlert("Operación OK", "Dato eliminado correctamente", "OK");
                    Conexion.Cerrar_WMS_ATE();

                }
                Ubicaciones_ArtUbic_SelUbicacionesArticulo(v_Id_Alm, Lbl_Codigo.Text, v_Id_CD);
                LimpriarDatos();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

    }
}