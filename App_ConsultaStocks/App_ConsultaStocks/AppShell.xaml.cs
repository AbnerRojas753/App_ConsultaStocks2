using App_ConsultaStocks.VistaModelo;
using App_ConsultaStocks.Vistas;
using App_ConsultaStocks.Modelo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace App_ConsultaStocks
{
    public class AppShellViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _logoSource;
        public string LogoSource
        {
            get => _logoSource;
            set
            {
                _logoSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogoSource)));
            }
        }

        private Color _colorHeader;
        public Color ColorHeader
        {
            get => _colorHeader;
            set
            {
                _colorHeader = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorHeader)));
            }
        }

        public void ActualizarDatos()
        {
            LogoSource = AppConfig.GetLogoFileName();
            ColorHeader = AppConfig.GetColorHeader();
        }
    }

    public partial class AppShell : Xamarin.Forms.Shell
    {
        private AppShellViewModel _viewModel;

        public AppShell()
        {
            InitializeComponent();

            _viewModel = new AppShellViewModel();
            this.BindingContext = _viewModel;

            Routing.RegisterRoute(nameof(MenuPrincipal), typeof(MenuPrincipal));
            //Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

            // Aplicar colores y logo dinámicos
            ActualizarColores();

            // Actualizar colores cuando navegue
            this.Navigating += (s, e) => ActualizarColores();
            this.Navigated += (s, e) => ActualizarColores();
        }

        private void ActualizarColores()
        {
            Color colorHeader = AppConfig.GetColorHeader();

            this.BackgroundColor = colorHeader;
            _viewModel.ActualizarDatos();

            // Actualizar color del NavBar dinámicamente para cada ShellContent
            foreach (var item in this.Items)
            {
                Shell.SetBackgroundColor(item, colorHeader);
                Shell.SetForegroundColor(item, Color.White);
                Shell.SetTitleColor(item, Color.White);
            }

            if (AppConfig.MostrarDebug)
            {
                Console.WriteLine($"[APPSHELL] ActualizarColores - Color: {colorHeader}");
                Console.WriteLine($"[APPSHELL] ActualizarColores - Logo: {AppConfig.GetLogoFileName()}");
                Console.WriteLine($"[APPSHELL] ActualizarColores - Empresa actual: {AppConfig.EmpresaActual}");
            }
        }

      

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}
