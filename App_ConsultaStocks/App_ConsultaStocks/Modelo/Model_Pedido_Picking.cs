using System;
using System.Collections.Generic;
using System.Text;

namespace App_ConsultaStocks.Modelo
{
    public class Model_Pedido_Picking
    {
        public int ID_PICKING { get; set; }
        public int TIPO_PICKING { get; set; }
        public int NROPEDIDO { get; set; } 
        public int ID_PICADOR { get; set; }
        public int CANT_LINEAS_RUTA { get; set; }
        public string HORA_INICIO { get; set; }

        public string ID_UBICACION { get; set; }
        public int ITEMPEDIDO { get; set; }
        public string ID_ARTICULO { get; set; }

        public string ART_DESCRIPCION { get; set; }
        public int CANTIDAD_PEDIDO { get; set; }


        public int PICK { get; set; }
        public int ORDEN { get; set; }
        public int ID_UBICACION_RUTA { get; set; }
        public int MST_PACK { get; set; }
        public int CANT_MASTERPACK { get; set; }
        public int CANT_UND { get; set; }
        public int CANT_TOT_EN_UND { get; set; }
        public int CANTIDADES_X_ITEM_PEDIDO { get; set; }


        public int ID_CD { get; set; }
        public string DESC_CD { get; set; }
        public string ID_ALM { get; set; }
        public string DESC_ALM { get; set; }

        public int PICKING_MANUAL { get; set; }

    }
}
