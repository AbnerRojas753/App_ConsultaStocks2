using System;
using System.Collections.Generic;
using System.Text;

namespace App_ConsultaStocks.Modelo
{
    public class Model_UbicacionesProductos
    {
        public string ID_UBICACION { get; set; }
        public string DESCRIPCION { get; set; }
        public string STOCK { get; set; }
        public string MAX { get; set; }
        public string MIN { get; set; }

        public string ID_ARTICULO { get; set; }
        public string ART_DESCRIPCION { get; set; }


        public int ID_CD { get; set; }
        public string DESC_CD { get; set; }

        public string ID_ALM { get; set; }
        public string DESC_ALM { get; set; }

        public string TipoEvento { get; set; }

        public string ID_LOTE { get; set; }


    }
}
