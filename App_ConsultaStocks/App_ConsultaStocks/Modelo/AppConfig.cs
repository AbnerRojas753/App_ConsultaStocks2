using System;
using Xamarin.Forms;

namespace App_ConsultaStocks.Modelo
{
    public static class AppConfig
    {
        public enum Empresa { ALFA, TORU }

        public static Empresa EmpresaActual { get; set; } = Empresa.ALFA;

        // Variable para controlar si se conecta al servidor remoto o local
        public static bool ServidorRemoto { get; set; } = false; // false = local (192.168.1.2), true = remoto (190.116.61.194)

        // Variable para controlar si se muestran los mensajes de debug
        public static bool MostrarDebug { get; set; } = true; // Cambiar a false para ocultar

        // Colores por empresa
        public static Color GetColorPrincipal()
        {
            return EmpresaActual == Empresa.ALFA
                ? Color.FromHex("#2196F3")  // Azul para ALFA
                : Color.FromHex("#076300");  // Verde oscuro para TORU
        }

        public static Color GetColorHeader()
        {
            return EmpresaActual == Empresa.ALFA
                ? Color.Red
                : Color.Green;
        }

        // Logos por empresa
        public static string GetLogoFileName()
        {
            return EmpresaActual == Empresa.ALFA
                ? "LogoAlfa.jpg"
                : "LogoToru.png";
        }

        // Nombres de bases de datos
        public static string GetCatalogoPrincipal()
        {
            return EmpresaActual == Empresa.ALFA
                ? "ALFA"
                : "GPTOR";
        }

        public static string GetCatalogoWMS()
        {
            return EmpresaActual == Empresa.ALFA
                ? "WMS_ALFA"
                : "WMS_TORU";
        }

        // IP del servidor
        public static string GetDataSource()
        {
            return ServidorRemoto
                ? "190.116.61.194"  // Servidor remoto
                : "192.168.1.2";    // Servidor local (por defecto)
        }
    }
}
