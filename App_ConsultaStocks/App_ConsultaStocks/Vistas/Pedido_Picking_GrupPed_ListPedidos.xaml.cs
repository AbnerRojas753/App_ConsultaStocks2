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
	public partial class Pedido_Picking_GrupPed_ListPedidos : ContentPage
	{

		int v_IdPicking = 0;
		public Pedido_Picking_GrupPed_ListPedidos (Model_Pedido_Picking _Model_PedidoPicking)
		{
			InitializeComponent ();
			v_IdPicking = _Model_PedidoPicking.ID_PICKING;
            Pedido_Picking_GrupoPedido_ListPedidos(v_IdPicking);


        }


        //Metodos de base de datos
        private void Pedido_Picking_GrupoPedido_ListPedidos(int ID_PICKING)
        {
            try
            {
                LV_Pedidos.ItemsSource = null;
                DataTable dt = new DataTable();


                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_GrupoPedido_ListPedidos", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                Conexion.Cerrar();

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Model_Pedido_Cabecera> Pedidos = new List<Model_Pedido_Cabecera>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Model_Pedido_Cabecera Datos_Pedido = new Model_Pedido_Cabecera();
                        Datos_Pedido.NROPEDIDO = int.Parse(dt.Rows[i][0].ToString());
                        Datos_Pedido.IDCLIENTE = dt.Rows[i][1].ToString();
                        Datos_Pedido.NOMBRECLIENTE = dt.Rows[i][2].ToString();


                        Pedidos.Add(Datos_Pedido);
                    }

                    LV_Pedidos.ItemsSource = Pedidos;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private async void btn_EliminarPedidoGrupo_Clicked(object sender, EventArgs e)
        {
            var selectedItem = LV_Pedidos.SelectedItem as Model_Pedido_Cabecera;
            if (selectedItem == null)
            {
                await DisplayAlert("Error", "Debe seleccionar un pedido", "Ok");
                return;
            }

            if (selectedItem.IDCLIENTE != "")
            {
                bool answer = await DisplayAlert("Eliminar dato?", "Esta seguro de eliminar ", "Sí", "No");
                if (answer == true)
                {
                    Pedido_Picking_GrupoPedido_DelPedido(v_IdPicking, selectedItem.NROPEDIDO);
                    Pedido_Picking_GrupoPedido_ListPedidos(v_IdPicking);
                }
            }
            


               
        }

        private void Pedido_Picking_GrupoPedido_DelPedido(int ID_PICKING, int NROPEDIDO)
        {
            try
            {
             
                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_GrupoPedido_DelPedido", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_PICKING", SqlDbType.Int).Value = ID_PICKING;
                cmd.Parameters.Add("@NROPEDIDO", SqlDbType.Int).Value = NROPEDIDO;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                cmd.ExecuteNonQuery();
                Conexion.Cerrar();
                 
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }

        }

    }
}