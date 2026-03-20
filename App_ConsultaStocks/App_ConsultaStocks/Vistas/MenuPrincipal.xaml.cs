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
    public partial class MenuPrincipal : ContentPage
    {
        public MenuPrincipal()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Cargar logo dinámicamente según la empresa seleccionada cada vez que aparece
            ImgLogoMenu.Source = AppConfig.GetLogoFileName();
        }
    }
}