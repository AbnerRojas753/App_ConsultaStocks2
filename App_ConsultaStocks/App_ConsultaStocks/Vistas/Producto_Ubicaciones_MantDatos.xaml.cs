using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using App_ConsultaStocks.Modelo;
using App_ConsultaStocks.Datos;
using System.Data.SqlClient;
using System.Data;

using ZXing.Net.Mobile.Forms;


namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Producto_Ubicaciones_MantDatos : ContentPage
    {
        int v_Id_CD;
        string v_Id_Alm;


        Model_UbicacionesProductos _Model_ArtUbic = new Model_UbicacionesProductos();
        public Producto_Ubicaciones_MantDatos(Model_UbicacionesProductos _Model_ArtUbic)
        {
            InitializeComponent();

            lblNombreSede.Text = _Model_ArtUbic.DESC_CD;
            lblNombreAlmacen.Text = _Model_ArtUbic.DESC_ALM;

            v_Id_CD = _Model_ArtUbic.ID_CD;
            v_Id_Alm = _Model_ArtUbic.ID_ALM;

            lbl_Id_Ubicacion.Text = _Model_ArtUbic.ID_UBICACION;
            lbl_DescUbicacion.Text = _Model_ArtUbic.DESCRIPCION;

            if (_Model_ArtUbic.TipoEvento == "Modificar")
            {
                lbl_Id_Articulo.Text = _Model_ArtUbic.ID_ARTICULO;
                txtIdLote.Text = _Model_ArtUbic.ID_LOTE;
                lbl_Art_Descripcion.Text = _Model_ArtUbic.ART_DESCRIPCION;
                Ubicaciones_ArtUbic_SelDatosIdArticulo(lbl_Id_Articulo.Text, v_Id_Alm, lbl_Id_Ubicacion.Text, v_Id_CD);

            }

        }

        private void txtIdUbicacion_Completed(object sender, EventArgs e)
        {
            Ubicaciones_SelDatosUbicacion(v_Id_Alm, txtIdUbicacion.Text);
        }

        private async void Btn_LeerUbicacion_Clicked(object sender, EventArgs e)
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

        private void txtIdProducto_Completed(object sender, EventArgs e)
        {
            BuscarProductos_BarCode(txtIdProducto.Text, v_Id_CD);
            Ubicaciones_ArtUbic_SelDatosIdArticulo(lbl_Id_Articulo.Text, v_Id_Alm, lbl_Id_Ubicacion.Text, v_Id_CD);
        }

        private async void btnLeerIdProducto_Clicked(object sender, EventArgs e)
        {
            try
            {
                var Scanner = new ZXing.Mobile.MobileBarcodeScanner();
                Scanner.TopText = "Leer codigo de barras...";
                Scanner.TopText = "Puede usar EAN o QR";
                var result = await Scanner.Scan();
                if (result != null)
                {
                    txtIdProducto.Text = result.Text;
                    BuscarProductos_BarCode(txtIdProducto.Text, v_Id_CD);

                }

            }
            catch (Exception ex)
            {

                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }
        private void TxtIdLote_Completed(object sender, EventArgs e)
        {
            TxtMaximo.Focus();
        }
        private void TxtMaximo_Completed(object sender, EventArgs e)
        {
            TxtMinimo.Focus();
        }

        private void TxtMinimo_Completed(object sender, EventArgs e)
        {

        }

        private async void Btn_Grabar_Clicked(object sender, EventArgs e)
        {


            if (string.IsNullOrWhiteSpace(lbl_Id_Ubicacion.Text))
            {
                await DisplayAlert("Validación", "Debe ingresar la ubicación.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(lbl_Id_Articulo.Text))
            {
                await DisplayAlert("Validación", "Debe ingresar el producto.", "OK");
                return;
            }


            if (TxtMinimo.Text == "")
            { TxtMinimo.Text = "0"; }

            if (TxtMaximo.Text == "")
            { TxtMaximo.Text = "0"; }

            Ubicaciones_ArtUbic_InsUpdDatos(
                    lbl_Id_Articulo.Text,
                    v_Id_Alm,
                    lbl_Id_Ubicacion.Text,
                    v_Id_CD,
                    int.Parse(TxtMaximo.Text),
                    int.Parse(TxtMinimo.Text),
                    txtIdLote.Text // <-- aquí se pasa el valor del lote
                );

        }

        private async void Btn_Eliminar_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Eliminar dato?", "Esta seguro de eliminar el producto", "Sí", "No");
            if (answer == true)
            {
                Ubicaciones_ArtUbic_DelDatos(lbl_Id_Articulo.Text, v_Id_Alm, lbl_Id_Ubicacion.Text, v_Id_CD);
            }
        }

        private void Btn_Limpiar_Clicked(object sender, EventArgs e)
        {
            lbl_Id_Ubicacion.Text = "";
            lbl_DescUbicacion.Text = "";

            lbl_Id_Articulo.Text = "";
            lbl_Art_Descripcion.Text = "";

            TxtMaximo.Text = "";
            TxtMinimo.Text = "";

            lblStock.Text = "";

        }




        //Metodos de Base de Datos
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
                    txtIdProducto.Focus();
                }
                else
                {
                    lbl_Id_Ubicacion.Text = "";
                    lbl_DescUbicacion.Text = "";
                    await DisplayAlert("Error!", "Ubicacion no existe!", "Ok");
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
                        lbl_Id_Articulo.Text = dt.Rows[0][0].ToString();
                        lbl_Art_Descripcion.Text = dt.Rows[0][1].ToString();
                        lbl_Art_Descripcion.TextColor = Color.Black;
                        txtIdProducto.Text = "";
                        txtIdLote.Focus();

                    }
                    else
                    {
                        DisplayAlert("Error", "Existe más de un producto con el mismo CodBar", "Ok");
                        lbl_Id_Articulo.Text = "";
                        lbl_Art_Descripcion.Text = "";
                        txtIdProducto.Focus();
                    }

                }
                else
                {
                    lbl_Id_Articulo.Text = "";
                    lbl_Art_Descripcion.Text = "";
                    await DisplayAlert("Error!", "Producto no existe!", "Ok");
                    txtIdProducto.Focus();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
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

                    lbl_ExisteUbicacion.Text = "";
                    TxtMaximo.Text = dt.Rows[0][4].ToString();
                    TxtMinimo.Text = dt.Rows[0][5].ToString();
                    lblStock.Text = dt.Rows[0][3].ToString();
                    lbl_ExisteUbicacion.Text = "Existe Ubicación para el producto";
                    lbl_ExisteUbicacion.TextColor = Color.Green;

                }
                else
                {
                    lbl_ExisteUbicacion.Text = " * Ubicación NO relacionada con el producto";
                    TxtMaximo.Text = "";
                    TxtMinimo.Text = "";
                    lblStock.Text = "";
                    lbl_ExisteUbicacion.TextColor = Color.Red;

                    //Btn_GuardarDatos.Text = "Guardar datos";
                    //Btn_GuardarDatos.BackgroundColor = Color.Red;

                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }


        private async void Ubicaciones_ArtUbic_InsUpdDatos(string ID_ARTICULO, string ID_ALMACEN, string ID_UBICACION, int Id_CD, int MAX, int MIN, string LOTE)
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
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 50).Value = LOTE; // Nuevo parámetro
                    cmd.ExecuteScalar();
                    Conexion.Cerrar_WMS_LIM();
                    await DisplayAlert("Registro OK", "Dato registrado correctamente", "OK");
                    Navigation.PopAsync();
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
                    cmd.Parameters.Add("@ID_LOTE", SqlDbType.VarChar, 50).Value = LOTE; // Nuevo parámetro
                    cmd.ExecuteScalar();
                    Conexion.Cerrar_WMS_ATE();
                    await DisplayAlert("Registro OK", "Dato registrado correctamente", "OK");
                    Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

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
                    Conexion.Cerrar_WMS_LIM();
                    await DisplayAlert("Operación OK", "Dato eliminado correctamente", "OK");

                    Navigation.PopAsync();

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
                    Conexion.Cerrar_WMS_ATE();
                    await DisplayAlert("Operación OK", "Dato eliminado correctamente", "OK");
                    Navigation.PopAsync();

                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }
    }
}