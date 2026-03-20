using System;
using System.Collections.Generic;
using System.Text;

namespace App_ConsultaStocks.Modelo
{
    public class Model_WMS_RecepcionProductos
    {
        public int ID_RECEPCION { get; set; }
        public int TIPO_RECEPCION { get; set; }
        public string FECHA_HORA_INICIO { get; set; }
        public string FECHA_HORA_FIN { get; set; }
        public string ID_PROVEEDOR { get; set; }
        public string ID_ALMACEN { get; set; }
        public string ID_LOTE { get; set; }
        public string HORA_INICIO { get; set; }

        public int NRO_LINEA { get; set; }
        public string ID_ARTICULO { get; set; }
        public string NOMBRE_ARTICULO { get; set; }
        public string UM { get; set; }
        public int EQUIVALENCIA { get; set; }
        public int CANTIDAD { get; set; }
        public string FECHA_VENC { get; set; }


        public int ID_CD { get; set; }
        //public string DESC_CD { get; set; }
        //public string ID_ALM { get; set; }
        //public string DESC_ALM { get; set; }

        public int ID_ALMACENERO { get; set; }
    }


}

