using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.Data.SqlClient;
using System.Data;

using App_ConsultaStocks.Datos;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks.Vistas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BuscarDatos : ContentPage
	{
        int Id_CD = 0;
        int Id_Recepcion = 0;
        string v_DatoaBuscar = "";
        string v_Id_Alm = "";

        public BuscarDatos (Model_BusquedaDatos _Model_BusquedaDatos)
		{
			InitializeComponent ();
            Id_Recepcion = int.Parse(_Model_BusquedaDatos.Parametro1);
            Id_CD = int.Parse(_Model_BusquedaDatos.Parametro2);
            v_DatoaBuscar = _Model_BusquedaDatos.DatoaBuscar;
            v_Id_Alm = _Model_BusquedaDatos.Parametro3;

            if (v_DatoaBuscar == "RecepcionesMercaderia")
            {
                StackLayout_TipoRecepcion.IsVisible = true;
            }

        }

        private void btnBuscar_Clicked(object sender, EventArgs e)
        {
            if (v_DatoaBuscar == "Proveedores")
            {
                Compras_Proveedores_SelDatos(txtBuscar.Text);
            }
            if (v_DatoaBuscar == "RecepcionesMercaderia")
            {
                int v_Tipo_Recepcion = 0;
                if (RBtn_Proveedor.IsChecked == true)
                {
                    v_Tipo_Recepcion = 1;
                }
                else
                {
                    if (RBtn_Transferencia.IsChecked == true)
                    {
                        v_Tipo_Recepcion = 2;
                    }
                }

                string Buscar = "";
                if (txtBuscar.Text=="" )
                {
                    Buscar = " ";
                }
                recep_productos_Search_App(Buscar, v_Tipo_Recepcion);
            }
        }

        private async void btn_Seleccionar_Clicked(object sender, EventArgs e)
        {

            var selectedItem = LV_Buscar.SelectedItem as Model_BusquedaDatos;
            if (selectedItem == null)
            {
                await DisplayAlert("Error", "Debe seleccionar un registro", "Ok");
                return;
            }

            if (selectedItem.Retorno1 != "")
            {
                //bool answer = await DisplayAlert("Eliminar dato?", "Esta seguro de eliminar ", "Sí", "No");
                //if (answer == true)
                //{

                if (v_DatoaBuscar == "Proveedores")
                {
                    recep_productos_UpdProveedor(Id_Recepcion, selectedItem.Retorno1);
                    await Navigation.PopAsync();
                }

                if (v_DatoaBuscar == "RecepcionesMercaderia")
                {
                    int v_Id_Recepcion_Recuperar = 0;
                    int v_Tipo_Recepcion = 0;

                    if (RBtn_Proveedor.IsChecked == true)
                    {
                        v_Tipo_Recepcion = 1;
                    }
                    else
                    {
                        if (RBtn_Transferencia.IsChecked == true)
                        {
                            v_Tipo_Recepcion = 2;
                        }
                    }

                    v_Id_Recepcion_Recuperar = int.Parse(selectedItem.Retorno1);
                    
                    if (v_Id_Recepcion_Recuperar == 0)
                    {
                        await DisplayAlert("Error", "Debe seleccionar un proceso", "Ok");
                        return;
                    }

                    Model_WMS_RecepcionProductos _Model_WMS_RecepcionProductos = new Model_WMS_RecepcionProductos();
                    _Model_WMS_RecepcionProductos.ID_CD = Id_CD;
                    _Model_WMS_RecepcionProductos.ID_ALMACEN = v_Id_Alm;
                    _Model_WMS_RecepcionProductos.ID_RECEPCION = v_Id_Recepcion_Recuperar;
                    _Model_WMS_RecepcionProductos.TIPO_RECEPCION = v_Tipo_Recepcion;

                    await Navigation.PushAsync(new WMS_RecepcionMercaderia(_Model_WMS_RecepcionProductos));

                }
            }
        }



        //Metodos de base de datos
        private void Compras_Proveedores_SelDatos(string Buscar)
        {
            try
            {
                LV_Buscar.ItemsSource = null;
                DataTable dt = new DataTable();

                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_Compras_Proveedores_SelDatos", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Buscar", SqlDbType.VarChar, 31).Value = Buscar;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_Compras_Proveedores_SelDatos", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Buscar", SqlDbType.VarChar, 31).Value = Buscar;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_BusquedaDatos> BuscarDatos = new List<Model_BusquedaDatos>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_BusquedaDatos Datos_Buscar = new Model_BusquedaDatos();
                        Datos_Buscar.Retorno1 = dt.Rows[i][0].ToString();
                        Datos_Buscar.Retorno2= dt.Rows[i][1].ToString();

                        BuscarDatos.Add(Datos_Buscar);
                    }

                    LV_Buscar.ItemsSource = BuscarDatos;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }


        private void recep_productos_UpdProveedor(int ID_RECEPCION,string ID_PROVEEDOR)
        {
            try
            {
                DataTable dt = new DataTable();

                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_UpdProveedor", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@ID_PROVEEDOR", SqlDbType.VarChar, 31).Value = ID_PROVEEDOR;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_UpdProveedor", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_RECEPCION", SqlDbType.VarChar, 31).Value = ID_RECEPCION;
                    cmd.Parameters.Add("@ID_PROVEEDOR", SqlDbType.VarChar, 31).Value = ID_PROVEEDOR;
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

        private void recep_productos_Search_App(string Buscar,int Tipo)
        {
            try
            {
                LV_Buscar.ItemsSource = null;
                DataTable dt = new DataTable();

                if (Id_CD == 1)
                {
                    Conexion.Abrir_WMS_LIM();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Search_App", Conexion.conectar_WMS_LIM);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Buscar", SqlDbType.VarChar, 31).Value = Buscar;
                    cmd.Parameters.Add("@Tipo", SqlDbType.Int).Value = Tipo;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_LIM();
                }
                else
                {
                    Conexion.Abrir_WMS_ATE();
                    SqlCommand cmd = new SqlCommand("usp_recep_productos_Search_App", Conexion.conectar_WMS_ATE);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Buscar", SqlDbType.VarChar, 31).Value = Buscar;
                    cmd.Parameters.Add("@Tipo", SqlDbType.Int).Value = Tipo;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    Conexion.Cerrar_WMS_ATE();
                }

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_BusquedaDatos> BuscarDatos = new List<Model_BusquedaDatos>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_BusquedaDatos Datos_Buscar = new Model_BusquedaDatos();
                        Datos_Buscar.Retorno1 = dt.Rows[i][0].ToString();
                        Datos_Buscar.Retorno2 = dt.Rows[i][1].ToString();

                        BuscarDatos.Add(Datos_Buscar);
                    }

                    LV_Buscar.ItemsSource = BuscarDatos;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }




    }
}