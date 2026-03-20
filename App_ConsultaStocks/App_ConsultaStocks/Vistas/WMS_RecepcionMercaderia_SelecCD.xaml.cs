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

namespace App_ConsultaStocks.Vistas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class WMS_RecepcionMercaderia_SelecCD : ContentPage
	{
        int v_Id_CD = 0;
        string v_Nombre_CD;

        string v_Id_Alm = "";
        string v_Nombre_Alm;

        
        int v_Id_Recepcion_Recuperar = 0;
        int v_Tipo_Recepcion= 0;
        int v_Id_Picador = 0;


        public WMS_RecepcionMercaderia_SelecCD()
		{
			InitializeComponent ();
            SelCentrodeDistribucion();
        }

        private void ItemPicker_CD_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemPicker_CD.SelectedIndex != -1)
            {
                var selectedItem = ItemPicker_CD.SelectedItem as Model_CentrodeDistribucion;
                v_Id_CD = int.Parse(selectedItem.IdCD);
                v_Nombre_CD = selectedItem.DescripcionCD;

                Almacenes_Sel(v_Id_CD);
                v_Id_Alm = "";

                LV_ProcesosPendientes.ItemsSource = null;
                lbl_CantProcesosPendientes.Text = "0";
            }
        }

        private void ItemPicker_Almacen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemPicker_Almacen.SelectedIndex != -1)
            {
                var selectedItem = ItemPicker_Almacen.SelectedItem as Model_Almacen;
                v_Id_Alm = selectedItem.IdAlmacen;
                v_Nombre_Alm = selectedItem.DescripcionAlmacen;

                WMSRecepciones_SelPcsPendxUsuario(v_Id_CD, v_Id_Alm);
            }
        }

        private async void Btn_IraRecepcion_Clicked(object sender, EventArgs e)
        {
            if (Convert.ToString(v_Id_CD) == "" || v_Id_CD == 0)
            {
                await DisplayAlert("Error!", "Debe seleccionar una sede", "Ok");
                return;
            }


            WMSRecepciones_SelPcsPendxUsuario(v_Id_CD, v_Id_Alm);

            if (lbl_CantProcesosPendientes.Text != "0")
            {
                bool answer = await DisplayAlert("Advertencia!", "Existen procesos pendientes, esta seguro continuar?", "Sí", "No");
                if (answer == false)
                {
                    return;
                }

            }


            Model_WMS_RecepcionProductos _Model_WMS_RecepcionProductos = new Model_WMS_RecepcionProductos();
            _Model_WMS_RecepcionProductos.ID_CD = v_Id_CD;
            //_Model_WMS_RecepcionProductos.DESC_CD = v_Nombre_CD;

            _Model_WMS_RecepcionProductos.ID_ALMACEN = v_Id_Alm;
            ////_Model_WMS_RecepcionProductos.DESC_ALM = v_Nombre_Alm;
            _Model_WMS_RecepcionProductos.ID_ALMACENERO = v_Id_Picador;

            _Model_WMS_RecepcionProductos.ID_RECEPCION = 0;

            await Navigation.PushAsync(new WMS_RecepcionMercaderia(_Model_WMS_RecepcionProductos));
            //await Navigation.PushAsync(new WMS_RecepcionMercaderia());

        }

        private async void Btn_ContinuarRecepcion_Clicked(object sender, EventArgs e)
        {
            v_Id_Recepcion_Recuperar = 0;
            v_Tipo_Recepcion = 0;

            var selectedItem = LV_ProcesosPendientes.SelectedItem as Model_WMS_RecepcionProductos;

            if (selectedItem == null)
            {
                await DisplayAlert("Error", "Debe seleccionar un proceso", "Ok");
                return;
            }

            v_Id_Recepcion_Recuperar = selectedItem.ID_RECEPCION;
            v_Tipo_Recepcion = selectedItem.TIPO_RECEPCION;
            if (v_Id_Recepcion_Recuperar == 0)
            {
                await DisplayAlert("Error", "Debe seleccionar un proceso", "Ok");
                return;
            }

             
            Model_WMS_RecepcionProductos _Model_WMS_RecepcionProductos = new Model_WMS_RecepcionProductos();
            _Model_WMS_RecepcionProductos.ID_CD = v_Id_CD;
            _Model_WMS_RecepcionProductos.ID_ALMACEN = v_Id_Alm;
            _Model_WMS_RecepcionProductos.ID_RECEPCION = v_Id_Recepcion_Recuperar;
            _Model_WMS_RecepcionProductos.TIPO_RECEPCION= v_Tipo_Recepcion;
            _Model_WMS_RecepcionProductos.ID_ALMACENERO = v_Id_Picador;

            await Navigation.PushAsync(new WMS_RecepcionMercaderia(_Model_WMS_RecepcionProductos));


        }

        protected override async void OnAppearing()
        {

           
            if (Convert.ToString(v_Id_CD) != "" && v_Id_Alm != "")
            {
                WMSRecepciones_SelPcsPendxUsuario(v_Id_CD, v_Id_Alm);
                base.OnAppearing();
            }
             
        }

        private async void btn_BucarRecepcion_Clicked(object sender, EventArgs e)
        {

            Model_BusquedaDatos _Model_BusquedaDatos = new Model_BusquedaDatos();
            _Model_BusquedaDatos.Parametro1 = "0";
            _Model_BusquedaDatos.Parametro2 = v_Id_CD.ToString();
            _Model_BusquedaDatos.Parametro3 = v_Id_Alm;
            _Model_BusquedaDatos.DatoaBuscar = "RecepcionesMercaderia";

                await Navigation.PushAsync(new BuscarDatos(_Model_BusquedaDatos));
            
        }

        private async void txtIdPicador_Completed(object sender, EventArgs e)
        {
            if (Convert.ToString(v_Id_CD) == "" || v_Id_CD == 0)
            {
                await DisplayAlert("Error!", "Debe seleccionar una sede", "Ok");
                return;
            }
            if (txtIdPicador.Text is null || txtIdPicador.Text.Trim() == "")
            {
                await DisplayAlert("Error!", "Debe indicar un Picador!", "Ok");
                return;
            }

            GetAlmacenero(int.Parse(txtIdPicador.Text));
            if (lbl_NombrePicador.Text is null || lbl_NombrePicador.Text == "")
            {
                await DisplayAlert("Error!", "Picador no existe", "Ok");
                return;
            }
        }


        //Metodos de Base de datos
        private void SelCentrodeDistribucion()
        {
            try
            {
                Conexion.Abrir();

                SqlCommand cmd = new SqlCommand("usp_recep_productos_SelLugarRecepcion", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_CentrodeDistribucion> CDs = new List<Model_CentrodeDistribucion>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_CentrodeDistribucion Model_CD = new Model_CentrodeDistribucion();
                        Model_CD.IdCD = dt.Rows[i][0].ToString();
                        Model_CD.DescripcionCD = dt.Rows[i][1].ToString();
                        CDs.Add(Model_CD);
                    }

                    ItemPicker_CD.ItemsSource = CDs;
                    ItemPicker_CD.ItemDisplayBinding = new Binding("DescripcionCD");

                }

                Conexion.Cerrar();

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async void GetAlmacenero(int IdAlmacenero)
        {
            try
            {

                DataTable dt = new DataTable();

                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_GetAlmacenero", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdAlmacenero", SqlDbType.Int).Value = IdAlmacenero;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    v_Id_Picador = int.Parse(dt.Rows[0][0].ToString());
                    lbl_NombrePicador.Text = dt.Rows[0][1].ToString();
                }
                else
                {
                    v_Id_Picador = 0;
                    lbl_NombrePicador.Text = "";
                }



            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private void Almacenes_Sel(int Id_CD)
        {
            try
            {

                DataTable dt = new DataTable();
                Conexion.Abrir_WMS_ATE();
                SqlCommand cmd = new SqlCommand("usp_Almacenes_Sel_MultiCD", Conexion.conectar_WMS_ATE);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_CD", SqlDbType.Int).Value = Id_CD;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar_WMS_ATE();


                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_Almacen> Almacenes = new List<Model_Almacen>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_Almacen Model_Alm = new Model_Almacen();
                        Model_Alm.IdAlmacen = dt.Rows[i][0].ToString();
                        Model_Alm.DescripcionAlmacen = dt.Rows[i][1].ToString();
                        Almacenes.Add(Model_Alm);
                    }
                    ItemPicker_Almacen.ItemsSource = Almacenes;
                    ItemPicker_Almacen.ItemDisplayBinding = new Binding("DescripcionAlmacen");
                }



            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }
        private void WMSRecepciones_SelPcsPendxUsuario(int ID_CD, string LOCNCODE)
        {
            try
            {
                LV_ProcesosPendientes.ItemsSource = null;
                DataTable dt = new DataTable();
                 
                if (v_Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_SelPcsRecepcionPend", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@LOCNCODE", SqlDbType.VarChar,31).Value = LOCNCODE;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_SelPcsRecepcionPend", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@LOCNCODE", SqlDbType.VarChar, 31).Value = LOCNCODE;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }


                int Filas = 0;
                Filas = dt.Rows.Count;

                lbl_CantProcesosPendientes.Text = Filas.ToString();

                if (Filas > 0)
                {
                    List<Model_WMS_RecepcionProductos> RecepcionesPendientes = new List<Model_WMS_RecepcionProductos>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_WMS_RecepcionProductos Datos_ProcesosPendientes = new Model_WMS_RecepcionProductos();
                        Datos_ProcesosPendientes.ID_RECEPCION = int.Parse(dt.Rows[i][0].ToString());
                        Datos_ProcesosPendientes.ID_PROVEEDOR= dt.Rows[i][1].ToString();
                        Datos_ProcesosPendientes.FECHA_HORA_INICIO= dt.Rows[i][2].ToString();
                        Datos_ProcesosPendientes.TIPO_RECEPCION = int.Parse(dt.Rows[i][3].ToString());


                        RecepcionesPendientes.Add(Datos_ProcesosPendientes);
                    }

                    LV_ProcesosPendientes.ItemsSource = RecepcionesPendientes;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        
    }
}