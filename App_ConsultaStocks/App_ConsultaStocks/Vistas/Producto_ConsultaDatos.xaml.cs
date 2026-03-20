using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App_ConsultaStocks.Datos;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Data.SqlClient;
using System.Data;


using ZXing.Net.Mobile.Forms;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Producto_ConsultaDatos : ContentPage
    {
        public Producto_ConsultaDatos()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            txtcodigobarras.Focus();
        }


        private void txtcodigobarras_Completed(object sender, EventArgs e)
        {
            Data_GetDatosProdcutos(txtcodigobarras.Text);
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
                    Data_GetDatosProdcutos(txtcodigobarras.Text);
                }

            }
            catch (Exception ex)
            {

                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }


        private void Btn_LimpiarDatos_Clicked(object sender, EventArgs e)
        {
            txtcodigobarras.Text = "";
            Lbl_Codigo.Text = "";
            lblDescripcionProducto.Text = "";
            Lbl_StockTotal.Text = "";
            LV_ListaStocks.ItemsSource = null;
            txtcodigobarras.Focus();

        }
      

        //Metodos de Base de datos
        private void Data_GetDatosProdcutos(string Search)
        {
            try
            {
                lblDescripcionProducto.Text="";
                Conexion.Abrir();

                SqlCommand cmd = new SqlCommand("usp_QuickFind_InfoProductosStocks_General_v2", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Search", SqlDbType.VarChar, 150).Value = Search;
                
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                int Filas =0;
                Filas = dt.Rows.Count;
                if (Filas>0)
                {
                    Lbl_Codigo.Text= Convert.ToString(dt.Rows[0][0]);
                    lblDescripcionProducto.Text =  Convert.ToString(dt.Rows[0][2]);
                    Lbl_StockTotal.Text=Convert.ToString( Convert.ToInt16(dt.Rows[0][3]));
                    InfoProductosStocks(Convert.ToString(dt.Rows[0][0]));
                    txtcodigobarras.Text = "";
                    lblDescripcionProducto.Focus();
                }
                else
                {
                    DisplayAlert("Error", "Producto NO Registrado", "Ok");
                }
                 
                Conexion.Cerrar();
               
            }
            catch (Exception ex)
            {
                DisplayAlert ("Error", ex.Message, "Ok");
            }

        }
     

        private void InfoProductosStocks(string ITEMNMBR)
        {
            try
            {
                Conexion.Abrir();

                SqlCommand cmd = new SqlCommand("usp_QuickFind_InfoProductosStocks", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ITEMNMBR", SqlDbType.VarChar, 150).Value = ITEMNMBR;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                int Filas = 0;
                Filas = dt.Rows.Count;

                if (Filas > 0)
                {
                    List<Class_StocksAlm> Stocks = new List<Class_StocksAlm>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Class_StocksAlm Stks = new Class_StocksAlm();
                        Stks.CodAlmacén = dt.Rows[i][0].ToString();
                        Stks.Almacén = dt.Rows[i][1].ToString();
                        Stks.Contabilizado = dt.Rows[i][2].ToString();
                        Stks.Reservado = dt.Rows[i][3].ToString();
                        Stks.Stock_Disponible = dt.Rows[i][4].ToString();
                        Stocks.Add(Stks);
                    }
                    LV_ListaStocks.ItemsSource = Stocks;
                                  
                    // LV_ListaStocks.ItemsSource = dt.Select().ToList().Select(r => new Class_StocksAlm(r["CodAlmacén"] as string, r["Almacén"] as string, r["Contabilizado"] as string, r["Reservado"] as string, r["Stock_Disponible"] as string));
                }

                Conexion.Cerrar();

            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "Ok");
            }
            //@ITEMNMBR
        }

    
    }
}