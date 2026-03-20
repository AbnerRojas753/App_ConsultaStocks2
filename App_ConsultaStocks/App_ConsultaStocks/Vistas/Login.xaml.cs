using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();

            // Establecer TORU por defecto
            PickerEmpresa.SelectedIndex = 1;
            AppConfig.EmpresaActual = AppConfig.Empresa.TORU;
            ImgLogo.Source = AppConfig.GetLogoFileName();
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

            // Actualizar logo dinámicamente
            ImgLogo.Source = AppConfig.GetLogoFileName();

            // Debug: Mostrar cambio de empresa
            //if (AppConfig.MostrarDebug)
            //{
            //    await DisplayAlert("Debug - Cambio de Empresa",
            //        $"Empresa: {AppConfig.EmpresaActual}\n" +
            //        $"Logo: {AppConfig.GetLogoFileName()}\n" +
            //        $"BD Principal: {AppConfig.GetCatalogoPrincipal()}\n" +
            //        $"BD WMS: {AppConfig.GetCatalogoWMS()}\n" +
            //        $"Servidor: {AppConfig.GetDataSource()}",
            //        "OK");
            //}
        }

        private async void CheckServidorRemoto_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            AppConfig.ServidorRemoto = e.Value;

            // Debug: Mostrar cambio de servidor
            //if (AppConfig.MostrarDebug)
            //{
            //    await DisplayAlert("Debug - Cambio de Servidor",
            //        $"Servidor Remoto: {(e.Value ? "Activado" : "Desactivado")}\n" +
            //        $"IP: {AppConfig.GetDataSource()}\n" +
            //        $"Empresa: {AppConfig.EmpresaActual}\n" +
            //        $"BD Principal: {AppConfig.GetCatalogoPrincipal()}\n" +
            //        $"BD WMS: {AppConfig.GetCatalogoWMS()}",
            //        "OK");
            //}
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (TxtIdUsuario.Text == "admin" && TxtPassword.Text=="123" )
            {
                Navigation.PushAsync(new Producto_ConsultaDatos());
            }
            else
            {
                DisplayAlert("Ops!", "Usuario o Password es incorrecto", "Ok");
            }


        }

    }
}