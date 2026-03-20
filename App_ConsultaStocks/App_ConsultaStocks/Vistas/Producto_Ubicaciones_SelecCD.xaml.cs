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
	public partial class Producto_Ubicaciones_SelecCD : ContentPage
	{
        int v_Id_CD  = 0 ;
        string v_Id_Alm = "" ;

        string v_Nombre_CD;
        string v_Nombre_Alm;

        public Producto_Ubicaciones_SelecCD ()
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
            }
        }

        private void ItemPicker_Almacen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemPicker_Almacen.SelectedIndex != -1)
            {
                var selectedItem = ItemPicker_Almacen.SelectedItem as Model_Almacen;
                v_Id_Alm = selectedItem.IdAlmacen;
                v_Nombre_Alm = selectedItem.DescripcionAlmacen;
            }
        }

        private async void Btn_IngresaraUbicaciones_Clicked(object sender, EventArgs e)
        {

            if (Convert.ToString(v_Id_CD) == "" || v_Id_CD == 0)
            {
                await DisplayAlert("Error!", "Debe seleccionar una sede", "Ok");
                return;
            }

            if (v_Id_Alm == "" || v_Id_Alm is null)
            {
                await DisplayAlert("Error!", "Debe seleccionar un almacén", "Ok");
                return;
            }



            Model_UbicacionesProductos _Model_ArtUbic = new Model_UbicacionesProductos();
            _Model_ArtUbic.ID_CD = v_Id_CD;
            _Model_ArtUbic.DESC_CD = v_Nombre_CD;

            _Model_ArtUbic.ID_ALM = v_Id_Alm;
            _Model_ArtUbic.DESC_ALM = v_Nombre_Alm;


            await Navigation.PushAsync(new Producto_Ubicaciones_AgregarProductodesdeubicacion(_Model_ArtUbic));

        }


        private async void Btn_UbicacionesxProducto_Clicked(object sender, EventArgs e)
        {

            if (Convert.ToString(v_Id_CD) == "" || v_Id_CD == 0)
            {
                await DisplayAlert("Error!", "Debe seleccionar una sede", "Ok");
                return;
            }

            if (v_Id_Alm == "" || v_Id_Alm is null)
            {
                await DisplayAlert("Error!", "Debe seleccionar un almacén", "Ok");
                return;
            }



            Model_UbicacionesProductos _Model_ArtUbic = new Model_UbicacionesProductos();
            _Model_ArtUbic.ID_CD = v_Id_CD;
            _Model_ArtUbic.DESC_CD = v_Nombre_CD;

            _Model_ArtUbic.ID_ALM = v_Id_Alm;
            _Model_ArtUbic.DESC_ALM = v_Nombre_Alm;


            await Navigation.PushAsync(new Producto_Ubicaciones(_Model_ArtUbic));


        }

        private async void Btn_SinUbica_Clicked(object sender, EventArgs e)
        {
            if (Convert.ToString(v_Id_CD) == "" || v_Id_CD == 0)
            {
                await DisplayAlert("Error!", "Debe seleccionar una sede", "Ok");
                return;
            }
            if (v_Id_Alm == "" || v_Id_Alm is null)
            {
                await DisplayAlert("Error!", "Debe seleccionar un almacén", "Ok");
                return;
            }
            Model_UbicacionesProductos _Model_ArtUbic = new Model_UbicacionesProductos();
            _Model_ArtUbic.ID_CD = v_Id_CD;
            _Model_ArtUbic.DESC_CD = v_Nombre_CD;
            _Model_ArtUbic.ID_ALM = v_Id_Alm;
            _Model_ArtUbic.DESC_ALM = v_Nombre_Alm;
            await Navigation.PushAsync(new Producto_Ubicaciones_ProdSinUbicacion(_Model_ArtUbic));
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

      
    }
}