using App_ConsultaStocks.Datos;
using App_ConsultaStocks.Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Pedido_RepAlm_SelecCD : ContentPage
    {
        int v_Id_CD = 0;
        string v_Nombre_CD;

        string v_Id_Alm = "";
        string v_Nombre_Alm;

        int v_Id_Picador = 0;

        int v_Id_Picking_Recuperar = 0;
        public Pedido_RepAlm_SelecCD()
        {
            InitializeComponent();
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
            v_Id_Picking_Recuperar = 0;
            
            // Limpiar controles visuales
            ItemPicker_CD.SelectedIndex = -1;
            ItemPicker_CD.ItemsSource = null;
            
            ItemPicker_Almacen.SelectedIndex = -1;
            ItemPicker_Almacen.ItemsSource = null;
            
            ItemPicker_NroSug.SelectedIndex = -1;
            ItemPicker_NroSug.ItemsSource = null;
            
            ItemPicker_NroHoja.SelectedIndex = -1;
            ItemPicker_NroHoja.ItemsSource = null;
            
            txtIdPicador.Text = "";
            lbl_NombrePicador.Text = "";
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // ✅ LIMPIAR cada vez que aparece la página - evita datos "pegados"
            LimpiarSelectores();
            
            // ✅ CARGAR datos frescos de la BD según la empresa actual
            SelCentrodeDistribucion();
            Sel_NroSug();
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


            //Pedido_Picking_SelPcsPendxPicador(v_Id_CD, int.Parse(txtIdPicador.Text));

            //if (lbl_CantProcesosPendientes.Text != "0")
            //{
            //    await DisplayAlert("Advertencia!", "Existen procesos pendientes", "Ok");
            //}


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


            //Pedido_Picking_SelPcsPendxPicador(v_Id_CD, int.Parse(txtIdPicador.Text));

            //if (lbl_CantProcesosPendientes.Text != "0")
            //{
            //    bool answer = await DisplayAlert("Advertencia!", "Existen procesos pendientes, esta seguro continuar?", "Sí", "No");
            //    if (answer == false)
            //    {
            //        return;
            //    }

            //}


            Model_Pedido_Picking _Model_PedidoPicking = new Model_Pedido_Picking();
            _Model_PedidoPicking.ID_CD = v_Id_CD;
            _Model_PedidoPicking.DESC_CD = v_Nombre_CD;

            _Model_PedidoPicking.ID_ALM = v_Id_Alm;
            _Model_PedidoPicking.DESC_ALM = v_Nombre_Alm;

            _Model_PedidoPicking.ID_PICADOR = v_Id_Picador;


            var selectedItem = ItemPicker_NroSug.SelectedItem as Model_Almacen;
            var ID = selectedItem.IdAlmacen;
            var Descripcion = selectedItem.DescripcionAlmacen;

            var selectedItem2 = ItemPicker_NroHoja.SelectedItem as Model_Almacen;
            var ID2 = selectedItem2.IdAlmacen;
            var Descripcion2 = selectedItem2.DescripcionAlmacen;

            Conexion.Abrir();
            SqlCommand cmd2 = new SqlCommand("alfa_usp_Sugerido_Picking_Sel_Picking", Conexion.conectar);
            cmd2.CommandType = CommandType.StoredProcedure;
            cmd2.Parameters.Add("@NROSUG", SqlDbType.VarChar).Value = Descripcion;
            cmd2.Parameters.Add("@NROHOJA", SqlDbType.VarChar).Value = Descripcion2;
            SqlDataAdapter adapter2 = new SqlDataAdapter(cmd2);
            DataTable dt2 = new DataTable();
            adapter2.Fill(dt2);
            Conexion.Cerrar();

            if (dt2.Rows.Count > 0)
            {
                //DataRow rowprog = dt2.Rows[0];
                //string Progreso = rowprog[0].ToString();
                //_Model_PedidoPicking.ID_PICKING = Progreso;
                _Model_PedidoPicking.ID_PICKING = int.TryParse(dt2.Rows.Count > 0 ? dt2.Rows[0][0].ToString() : null, out int progresoInt) ? progresoInt : 0;
                await Navigation.PushAsync(new Pedido_RepAlm_Picking(_Model_PedidoPicking));

            }






        }

        private void Pedido_Picking_InsCab(int ID_CD, string NROSUGERIDO, string ID_ALM, int ID_PICADOR)
        {
            //try
            //{
            //    DataTable dt = new DataTable();

            //    Conexion.Abrir();
            //    SqlCommand cmd = new SqlCommand("alfa_usp_Pedido_Picking_InsCab2", Conexion.conectar);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.Add("@ID_CD", SqlDbType.Int).Value = ID_CD;
            //    cmd.Parameters.Add("@NROSUGERIDO", SqlDbType.VarChar).Value = NROSUGERIDO;
            //    cmd.Parameters.Add("@ID_ALM", SqlDbType.VarChar).Value = ID_ALM;
            //    cmd.Parameters.Add("@ID_PICADOR", SqlDbType.Int).Value = ID_PICADOR;
            //    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            //    adapter.Fill(dt);

            //    Conexion.Cerrar();

            //    int Filas = 0;
            //    Filas = dt.Rows.Count;

            //    if (Filas > 0)
            //    {
            //        lbl_IdPicking.Text = dt.Rows[0][0].ToString();

            //        DisplayAlert("Exito Creando Sugerido", "Exito Creando Sugerido", "Ok");

            //        //Conexion.Abrir();
            //        //SqlCommand cmd2 = new SqlCommand("[alfa_usp_Pedido_Picking_Procesar2]", Conexion.conectar);
            //        //cmd2.CommandType = CommandType.StoredProcedure;
            //        //cmd2.Parameters.Add("@ID_CD", SqlDbType.Int).Value = ID_CD;
            //        //cmd2.Parameters.Add("@ID_ALM", SqlDbType.VarChar).Value = ID_ALM;
            //        //cmd2.Parameters.Add("@ID_PICADOR", SqlDbType.Int).Value = ID_PICADOR;
            //        //cmd2.ExecuteNonQuery();
            //        //Conexion.Cerrar();

            //        //Navigation.PushAsync(new Sugerido_Picking(lbl_IdPicking.Text));

            //    }
            //    else
            //    {
            //        DisplayAlert("Error Creando Sugerido", "Error Creando Sugerido", "Ok");


            //    }


            //}
            //catch (Exception ex)
            //{
            //    DisplayAlert("Error", ex.Message, "Ok");
            //}

        }
        //private async void Btn_ContinuarPicking_Clicked(object sender, EventArgs e)
        //{
        //    v_Id_Picking_Recuperar = 0;
        //    var selectedItem = LV_ProcesosPendientes.SelectedItem as Model_Pedido_Picking;

        //    if (selectedItem == null)
        //    {
        //        await DisplayAlert("Error", "Debe seleccionar un proceso", "Ok");
        //        return;
        //    }

        //    v_Id_Picking_Recuperar = selectedItem.ID_PICKING;
        //    if (v_Id_Picking_Recuperar == 0)
        //    {
        //        await DisplayAlert("Error", "Debe seleccionar un proceso", "Ok");
        //        return;
        //    }


        //    Model_Pedido_Picking _Model_PedidoPicking = new Model_Pedido_Picking();
        //    _Model_PedidoPicking.ID_CD = v_Id_CD;
        //    _Model_PedidoPicking.DESC_CD = v_Nombre_CD;

        //    _Model_PedidoPicking.ID_ALM = v_Id_Alm;
        //    _Model_PedidoPicking.DESC_ALM = v_Nombre_Alm;

        //    _Model_PedidoPicking.ID_PICADOR = v_Id_Picador;
        //    _Model_PedidoPicking.ID_PICKING = selectedItem.ID_PICKING;

        //    await Navigation.PushAsync(new Pedido_Picking(_Model_PedidoPicking));
        //}

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

        private void Sel_NroSug()
        {
            try
            {

                DataTable dt = new DataTable();
                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Sel_NroSUG", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
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
                        Almacenes.Add(Model_Alm);
                    }
                    ItemPicker_NroSug.ItemsSource = Almacenes;
                    ItemPicker_NroSug.ItemDisplayBinding = new Binding("DescripcionAlmacen");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error SugPicker", ex.Message, "Ok");
            }

        }

        private void ItemPicker_NroSug_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemPicker_NroSug.SelectedIndex != -1)
            {
                Sel_NroHoja();
            }
        }

        private void Sel_NroHoja()
        {
            try
            {
                var selectedItem = ItemPicker_NroSug.SelectedItem as Model_Almacen;
                var ID = selectedItem.IdAlmacen;
                var Descripcion = selectedItem.DescripcionAlmacen;

                DataTable dt = new DataTable();
                Conexion.Abrir();
                SqlCommand cmd = new SqlCommand("alfa_usp_Sugerido_Picking_Sel_NroHoja", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NROSUG", SqlDbType.VarChar).Value = Descripcion;
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
                        Almacenes.Add(Model_Alm);
                    }
                    ItemPicker_NroHoja.ItemsSource = Almacenes;
                    ItemPicker_NroHoja.ItemDisplayBinding = new Binding("DescripcionAlmacen");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error SugPicker", ex.Message, "Ok");
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
    }
}