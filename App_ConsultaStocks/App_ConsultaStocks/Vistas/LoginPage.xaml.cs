using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


using App_ConsultaStocks.Datos;
using System.Data.SqlClient;
using System.Data;
using App_ConsultaStocks.Modelo;


namespace App_ConsultaStocks.Vistas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
		public LoginPage ()
		{
			InitializeComponent ();

			// Establecer ALFA por defecto
			PickerEmpresa.SelectedIndex = 0;
			AppConfig.EmpresaActual = AppConfig.Empresa.ALFA;
			ImgLogo.Source = AppConfig.GetLogoFileName();
			StackFondo.BackgroundColor = AppConfig.GetColorHeader();
		}

		private async void PickerEmpresa_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (PickerEmpresa.SelectedIndex == 0)
			{
				AppConfig.EmpresaActual = AppConfig.Empresa.ALFA;
			}
			else
			{
				AppConfig.EmpresaActual = AppConfig.Empresa.TORU;
			}

			// Actualizar logo y color dinámicamente
			ImgLogo.Source = AppConfig.GetLogoFileName();
			StackFondo.BackgroundColor = AppConfig.GetColorHeader();

			// Debug: Mostrar cambio de empresa
			//if (AppConfig.MostrarDebug)
			//{
			//	await DisplayAlert("Debug - Cambio de Empresa",
			//		$"Empresa: {AppConfig.EmpresaActual}\n" +
			//		$"Logo: {AppConfig.GetLogoFileName()}\n" +
			//		$"BD Principal: {AppConfig.GetCatalogoPrincipal()}\n" +
			//		$"BD WMS: {AppConfig.GetCatalogoWMS()}\n" +
			//		$"Servidor: {AppConfig.GetDataSource()}",
			//		"OK");
			//}
		}

		private async void CheckServidorRemoto_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			AppConfig.ServidorRemoto = e.Value;

			// Debug: Mostrar cambio de servidor
			//if (AppConfig.MostrarDebug)
			//{
			//	await DisplayAlert("Debug - Cambio de Servidor",
			//		$"Servidor Remoto: {(e.Value ? "Activado" : "Desactivado")}\n" +
			//		$"IP: {AppConfig.GetDataSource()}\n" +
			//		$"Empresa: {AppConfig.EmpresaActual}\n" +
			//		$"BD Principal: {AppConfig.GetCatalogoPrincipal()}\n" +
			//		$"BD WMS: {AppConfig.GetCatalogoWMS()}",
			//		"OK");
			//}
		}

        protected override async void OnAppearing()
        {
            //base.OnAppearing();
            //var loggedin = true;
            //if (loggedin)
            //  await Shell.Current.GoToAsync($"//{nameof(MenuPrincipal)}");
        }
        private async void Button_Clicked(object sender, EventArgs e)
        {

            Login_ValLogUsr(TxtIdUsuario.Text, TxtPassword.Text);

            ////if (TxtIdUsuario.Text == "admin" && TxtPassword.Text == "123")
            ////{
            ////    //Navigation.PushAsync(new Producto_ConsultaDatos());
            ////    // Navigation.PushAsync(new AppShell());
            ////    await Shell.Current.GoToAsync($"//{nameof(MenuPrincipal)}");
            ////    //await Shell.Current.GoToAsync(state: "//MenuPrincipal");
            ////}
            ////else
            ////{
            ////    DisplayAlert("Ops!", "Usuario o Password es incorrecto", "Ok");
            ////}


        }

        private async void Login_ValLogUsr(string ID_USUARIO,string USU_PASSWORD)
        {
            try
            {
            
                Conexion.Abrir();

                SqlCommand cmd = new SqlCommand("usp_Usuarios_Login_ValLogUsr", Conexion.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ID_USUARIO", SqlDbType.VarChar, 21).Value = ID_USUARIO;
                cmd.Parameters.Add("@USU_PASSWORD", SqlDbType.VarChar, 20).Value = USU_PASSWORD;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                string Rpta = dt.Rows[0][0].ToString();

                if (Rpta=="0")
                {
                    await Shell.Current.GoToAsync($"//{nameof(MenuPrincipal)}");
                }
                else
                {
                    await DisplayAlert("Ops!", Rpta, "Ok");
                }
                
                Conexion.Cerrar();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

    }
}