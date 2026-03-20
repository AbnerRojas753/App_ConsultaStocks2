using System;
using System.Collections.Generic;
using System.Text;

namespace App_ConsultaStocks.Modelo
{
    public class Model_Pedido_Detalle
    {
        public int NROPEDIDO { get; set; }
        public int ITEMPEDIDO { get; set; }
        public string ID_ARTICULO { get; set; }
        public string ART_DESCRIPCION { get; set; }
        public int CANTIDAD { get; set; }
        public int PICK { get; set; }

        public int ID_CD { get; set; }
        public string DESC_CD { get; set; }

        public string ID_UBICACION_PREDET { get; set; }
        public string ID_UBICACION_PICK { get; set; }

        public string ID_ALM { get; set; }
        public string DESC_ALM { get; set; }




    }
}
