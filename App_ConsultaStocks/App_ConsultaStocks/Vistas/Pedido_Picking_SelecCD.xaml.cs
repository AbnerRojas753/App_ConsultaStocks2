using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using App_ConsultaStocks.Modelo;
using App_ConsultaStocks.Datos;
using System.Data.SqlClient;
using System.Data;

namespace App_ConsultaStocks.Vistas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Pedido_Picking_SelecCD : ContentPage
	{ 
		int v_Id_CD = 0;
		string v_Nombre_CD;

        string v_Id_Alm = "";
        string v_Nombre_Alm;

        int v_Id_Picador = 0;

        int v_Id_Picking_Recuperar = 0;
        public Pedido_Picking_SelecCD ()
		{
			InitializeComponent ();
			// ✅ La inicialización se hace en OnAppearing para evitar duplicados
		}
		
		private void LimpiarSelectores()
		{
			// Resetear variables
			v_Id_CD = 0;
			v_Nombre_CD = "";
			v_Id_Alm = "";
			v_Nombre_Alm = "";
			v_Id_Picador = 0;
			
			// Limpiar controles visuales
			ItemPicker_CD.SelectedIndex = -1;
			ItemPicker_CD.ItemsSource = null;
			
			ItemPicker_Almacen.SelectedIndex = -1;
			ItemPicker_Almacen.ItemsSource = null;
			
			txtIdPicador.Text = "";
			lbl_NombrePicador.Text = "";
			lbl_CantProcesosPendientes.Text = "";
			LV_ProcesosPendientes.ItemsSource = null;
		}
		
		protected override void OnAppearing()
		{
			base.OnAppearing();
			
			// ✅ LIMPIAR cada vez que aparece la página - evita datos "pegados"
			LimpiarSelectores();
			
			// ✅ CARGAR datos frescos de la BD según la empresa actual
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

        private async void txtIdPicador_Completed(object sender, EventArgs e)
        {
            Debug.WriteLine("[DEBUG] txtIdPicador_Completed started");

            if (Convert.ToString(v_Id_CD) == "" || v_Id_CD == 0)
            {
                Debug.WriteLine("[DEBUG] Error: Debe seleccionar una sede");
                await DisplayAlert("Error!", "Debe seleccionar una sede", "Ok");
                return;
            }

            if (txtIdPicador.Text is null || txtIdPicador.Text.Trim() == "")
            {
                Debug.WriteLine("[DEBUG] Error: Debe indicar un Picador!");
                await DisplayAlert("Error!", "Debe indicar un Picador!", "Ok");
                return;
            }

            Debug.WriteLine("[DEBUG] Calling GetAlmacenero with ID: " + txtIdPicador.Text);
            GetAlmacenero(int.Parse(txtIdPicador.Text));

            if (lbl_NombrePicador.Text is null || lbl_NombrePicador.Text == "")
            {
                Debug.WriteLine("[DEBUG] Error: Picador no existe");
                await DisplayAlert("Error!", "Picador no existe", "Ok");
                return;
            }

            Debug.WriteLine("[DEBUG] Calling Pedido_Picking_SelPcsPendxPicador with ID_CD: " + v_Id_CD + ", ID_PICADOR: " + txtIdPicador.Text);
            Pedido_Picking_SelPcsPendxPicador(v_Id_CD,int.Parse(txtIdPicador.Text));

            if (lbl_CantProcesosPendientes.Text != "0")
            {
                Debug.WriteLine("[DEBUG] Advertencia: Existen procesos pendientes - Cantidad: " + lbl_CantProcesosPendientes.Text);
                await DisplayAlert("Advertencia!","Existen procesos pendientes" , "Ok");
            }

            Debug.WriteLine("[DEBUG] txtIdPicador_Completed completed");
        }

        private async void Btn_IraPicking_Clicked(object sender, EventArgs e)
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


            Pedido_Picking_SelPcsPendxPicador(v_Id_CD, int.Parse(txtIdPicador.Text));

            if (lbl_CantProcesosPendientes.Text != "0")
            {
                bool answer = await DisplayAlert("Advertencia!", "Existen procesos pendientes, esta seguro continuar?", "Sí", "No");
                if (answer == false)
                {
                    return;
                }

            }

            Model_Pedido_Picking _Model_PedidoPicking = new Model_Pedido_Picking();
            _Model_PedidoPicking.ID_CD = v_Id_CD;
            _Model_PedidoPicking.DESC_CD = v_Nombre_CD;

            _Model_PedidoPicking.ID_ALM = v_Id_Alm;
            _Model_PedidoPicking.DESC_ALM = v_Nombre_Alm;

            _Model_PedidoPicking.ID_PICADOR = v_Id_Picador;
            
            _Model_PedidoPicking.ID_PICKING = 0;

            await Navigation.PushAsync(new Pedido_Picking(_Model_PedidoPicking));


        }

        private async void Btn_ContinuarPicking_Clicked(object sender, EventArgs e)
        {
            v_Id_Picking_Recuperar = 0;
            var selectedItem = LV_ProcesosPendientes.SelectedItem as Model_Pedido_Picking;

            if (selectedItem== null)
            {
                await DisplayAlert("Error", "Debe seleccionar un proceso", "Ok");
                return;
            }
            
            v_Id_Picking_Recuperar = selectedItem.ID_PICKING;
            if (v_Id_Picking_Recuperar == 0)
            {
                await DisplayAlert("Error", "Debe seleccionar un proceso", "Ok");
                return;
            }


            Model_Pedido_Picking _Model_PedidoPicking = new Model_Pedido_Picking();
            _Model_PedidoPicking.ID_CD = v_Id_CD;
            _Model_PedidoPicking.DESC_CD = v_Nombre_CD;

            _Model_PedidoPicking.ID_ALM = v_Id_Alm;
            _Model_PedidoPicking.DESC_ALM = v_Nombre_Alm;

            _Model_PedidoPicking.ID_PICADOR = v_Id_Picador;
            _Model_PedidoPicking.ID_PICKING = selectedItem.ID_PICKING;

            await Navigation.PushAsync(new Pedido_Picking(_Model_PedidoPicking));
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

        private async void GetAlmacenero(int IdAlmacenero)
        {
            Debug.WriteLine("[DEBUG] GetAlmacenero started with IdAlmacenero: " + IdAlmacenero);
            try
            {
                DataTable dt = new DataTable();
                    
                Debug.WriteLine("[DEBUG] Opening connection and executing query");
                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_GetAlmacenero", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdAlmacenero", SqlDbType.Int).Value = IdAlmacenero;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();
                
                int Filas = 0;
                Filas = dt.Rows.Count;
                Debug.WriteLine("[DEBUG] Query executed, rows returned: " + Filas);

                if (Filas > 0)
                {
                    v_Id_Picador = int.Parse(dt.Rows[0][0].ToString());
                    lbl_NombrePicador.Text = dt.Rows[0][1].ToString();
                    Debug.WriteLine("[DEBUG] Almacenero found - ID: " + v_Id_Picador + ", Nombre: " + lbl_NombrePicador.Text);
                }   
                else
                {
                    v_Id_Picador = 0;
                    lbl_NombrePicador.Text = "";
                    Debug.WriteLine("[DEBUG] No Almacenero found for ID: " + IdAlmacenero);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("[DEBUG] Exception in GetAlmacenero: " + ex.Message);
                DisplayAlert("Error", ex.Message, "Ok");
            }
            Debug.WriteLine("[DEBUG] GetAlmacenero completed");
        }

        private void Pedido_Picking_SelPcsPendxPicador(int ID_CD, int ID_PICADOR)
        {
            try
            {
                LV_ProcesosPendientes.ItemsSource = null;
                DataTable dt = new DataTable();


                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_SelPcsPendxPicador", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_CD", SqlDbType.Int).Value = ID_CD;
                cmd.Parameters.Add("@ID_PICADOR", SqlDbType.Int).Value = ID_PICADOR;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                int Filas = 0;
                Filas = dt.Rows.Count;

                lbl_CantProcesosPendientes.Text = Filas.ToString();

                if (Filas > 0)
                {
                    List<Model_Pedido_Picking> PickingPendientes = new List<Model_Pedido_Picking>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_Pedido_Picking Datos_PickingPendientes = new Model_Pedido_Picking();
                        Datos_PickingPendientes.ID_PICKING = int.Parse(dt.Rows[i][0].ToString());
                        Datos_PickingPendientes.NROPEDIDO = int.Parse(dt.Rows[i][1].ToString());
                        Datos_PickingPendientes.HORA_INICIO = dt.Rows[i][2].ToString();


                        PickingPendientes.Add(Datos_PickingPendientes);
                    }

                    LV_ProcesosPendientes.ItemsSource = PickingPendientes;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }
         
      
    }
}