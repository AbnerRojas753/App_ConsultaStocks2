using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using App_ConsultaStocks.Vistas;
using App_ConsultaStocks.Modelo;

namespace App_ConsultaStocks
{
    public partial class App : Application
    {
        // Configuración para saltar login automáticamente
        // Cambiar a false si se quiere forzar login incluso en DEBUG
        private const bool SKIP_LOGIN_IN_DEBUG = true;

        public App()
        {
            InitializeComponent();

#if DEBUG
            // En modo DEBUG, saltar login si está configurado
            if (SKIP_LOGIN_IN_DEBUG)
            {
                // Inicializar AppConfig con valores por defecto
                AppConfig.EmpresaActual = AppConfig.Empresa.ALFA; // Usar ALFA por defecto
                AppConfig.ServidorRemoto = false; // Usar servidor local por defecto

                MainPage = new AppShell();
                // Navegar automáticamente al menú principal
                Task.Run(async () =>
                {
                    await Task.Delay(100); // Pequeño delay para asegurar que la UI esté lista
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Shell.Current.GoToAsync($"//{nameof(App_ConsultaStocks.Vistas.MenuPrincipal)}");
                    });
                });
            }
            else
            {
                MainPage = new AppShell();
            }
#else
            // En modo RELEASE, usar el flujo normal con login
            MainPage = new AppShell();
#endif
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
