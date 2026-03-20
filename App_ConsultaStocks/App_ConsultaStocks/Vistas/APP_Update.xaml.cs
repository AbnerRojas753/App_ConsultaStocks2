using System;
 
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;
using System.IO;
using EzSmb;

namespace App_ConsultaStocks.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class APP_Update : ContentPage
    {
        public APP_Update()
        {
            InitializeComponent();
            DescargarAPK();

        }

        public async void DescargarAPK()
        {
            var file = await Node.GetNode(@"192.168.1.4\users\Sistemas\Aplicaciones_Actualizadas\APP\appToru.apk", "Administrador", "9Chino2017");
            string localFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "temp.apk");

            using (var stream = await file.Read())
            {
                using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
            
            DisplayAlert("Se ha descargado el APK ", "", "Aceptar");

             

            IApkInstaller(localFilePath);




        }

        protected override async void OnAppearing()
        {
            DescargarAPK();
        }

        public async void IApkInstaller(string apkFilePath)
        {


            try
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(apkFilePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Se produjo", ex.Message, "OK");
            }
        }


    }
}