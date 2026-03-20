using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Aplicar color dinámico según la empresa seleccionada cada vez que aparece
            FrameHeader.BackgroundColor = AppConfig.GetColorPrincipal();

            if (AppConfig.MostrarDebug)
            {
                Console.WriteLine($"[MAINPAGE] OnAppearing - Color aplicado: {AppConfig.GetColorPrincipal()}");
                Console.WriteLine($"[MAINPAGE] OnAppearing - Empresa actual: {AppConfig.EmpresaActual}");
            }
        }
    }
}
