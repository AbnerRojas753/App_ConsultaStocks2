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
    public partial class Producto_Ubicaciones_ProdSinUbicacion : ContentPage
    {
        int v_Id_CD;
        string v_Id_Alm;
        Model_UbicacionesProductos _Model_ArtUbicx = new Model_UbicacionesProductos();

        public Producto_Ubicaciones_ProdSinUbicacion(Model_UbicacionesProductos _Model_ArtUbic)
        {
            InitializeComponent();
            lblNombreSede.Text = _Model_ArtUbic.DESC_CD;
            lblNombreAlmacen.Text = _Model_ArtUbic.DESC_ALM;

            v_Id_CD = _Model_ArtUbic.ID_CD;
            v_Id_Alm = _Model_ArtUbic.ID_ALM;
            //Ubicaciones_ArtUbic_SelUbicacionesArticulo(v_Id_Alm,  v_Id_CD);
            _Model_ArtUbicx = _Model_ArtUbic;
        }

        protected override async void OnAppearing()
        {

            if (Tipo1RadioButton.IsChecked)
            {
                Ubicaciones_ArtUbic_SelUbicacionesArticulo(v_Id_Alm, v_Id_CD, 6);
            }
            else if (Tipo2RadioButton.IsChecked)
            {
                Ubicaciones_ArtUbic_SelUbicacionesArticulo(v_Id_Alm, v_Id_CD, 4);
            }


        }


        private void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;

            if (e.Value)
            {
                int radioButtonValue = 0;

                if (radioButton == Tipo1RadioButton)
                {
                    radioButtonValue = 6;
                }
                else if (radioButton == Tipo2RadioButton)
                {
                    radioButtonValue = 4;
                }
                Ubicaciones_ArtUbic_SelUbicacionesArticulo(v_Id_Alm, v_Id_CD, radioButtonValue);

            }

        }

        private void Ubicaciones_ArtUbic_SelUbicacionesArticulo(string ID_ALMACEN, int Id_CD, int tipo)
        {
            try
            {
                LV_ListaUbicaciones.ItemsSource = null;
                DataTable dt = new DataTable();

                Conexion.Abrir();

                SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_SinUbica", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@TIPO", SqlDbType.Int).Value = tipo;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();



                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_UbicacionesProductos> UbicacionesProductos = new List<Model_UbicacionesProductos>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_UbicacionesProductos Model_UbicacionesProductos = new Model_UbicacionesProductos();
                        Model_UbicacionesProductos.ID_UBICACION = dt.Rows[i][0].ToString();
                        Model_UbicacionesProductos.DESCRIPCION = dt.Rows[i][1].ToString();
                        Model_UbicacionesProductos.STOCK = dt.Rows[i][2].ToString();
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

        private async void Btn_SinUbica_Clicked(object sender, EventArgs e)
        {

            if (LV_ListaUbicaciones.SelectedItem == null)
            {
                return;
            }

            var selectedItem = LV_ListaUbicaciones.SelectedItem as Model_UbicacionesProductos;
            string cod = selectedItem.ID_UBICACION;
            string descr = selectedItem.DESCRIPCION;

            if (Tipo1RadioButton.IsChecked)
            {
                await Navigation.PushAsync(new Producto_Ubicaciones(_Model_ArtUbicx));
            }
            else if (Tipo2RadioButton.IsChecked)
            {
                await Navigation.PushAsync(new Producto_Ubicaciones(_Model_ArtUbicx));
            }
        }

    }
}