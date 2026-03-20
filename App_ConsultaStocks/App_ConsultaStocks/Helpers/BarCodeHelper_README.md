# BarCodeHelper - Guía de Uso

## Descripción
Helper centralizado para procesar, buscar y validar códigos de barras (EAN y QR) en toda la aplicación.

## Características
- ✅ Procesa códigos EAN simples
- ✅ Procesa códigos QR con formato `EAN|LOTE`
- ✅ Procesa códigos QR con formato `|EAN|LOTE`
- ✅ Busca el producto en base de datos
- ✅ Valida existencia y unicidad del producto
- ✅ Retorna resultado estructurado con toda la información

---

## Métodos Disponibles

### 1. `BuscarYValidarProducto` ⭐ (RECOMENDADO)
**Método completo que procesa, busca y valida en un solo paso.**

```csharp
var resultado = await BarCodeHelper.BuscarYValidarProducto(
    codigoEscaneado: "7501234|LOTE123",
    idCD: 1,  // 1=LIM, otro=ATE
    extraerLote: true  // true si necesitas el lote, false si solo EAN
);

// Verificar resultado
if (resultado.Exitoso && resultado.ProductoExiste)
{
    // ✅ Producto encontrado
    string codigo = resultado.CodigoProducto;      // "7501234"
    string lote = resultado.Lote;                  // "LOTE123"
    bool tieneLote = resultado.TieneLote;          // true
    string descripcion = resultado.DescripcionProducto;
    string um = resultado.UnidadMedida;
    string equiv = resultado.Equivalencia;
}
else
{
    // ❌ Error
    string mensaje = resultado.MensajeError;
    TipoErrorBusqueda tipoError = resultado.TipoError;
    // Posibles errores: ProductoNoExiste, ProductoDuplicado, ErrorBaseDatos, CodigoVacio
}
```

### 2. `ProcesarCodigo` (Parsing únicamente)
**Solo procesa el código sin buscar en BD.**

```csharp
var resultado = BarCodeHelper.ProcesarCodigo("7501234|LOTE123", extraerLote: true);
string codigo = resultado.CodigoProducto;  // "7501234"
string lote = resultado.Lote;              // "LOTE123"
bool tieneLote = resultado.TieneLote;      // true
```

### 3. `ExtraerCodigoProducto` (Solo EAN)
**Extrae únicamente el código del producto.**

```csharp
string codigo = BarCodeHelper.ExtraerCodigoProducto("7501234|LOTE123");
// Resultado: "7501234"
```

### 4. `ExtraerCodigoYLote` (Con parámetros out)
**Extrae código y lote usando parámetros de salida.**

```csharp
if (BarCodeHelper.ExtraerCodigoYLote("7501234|LOTE123", out string codigo, out string lote))
{
    // Tiene lote
    // codigo = "7501234", lote = "LOTE123"
}
```

---

## Ejemplos de Uso por Pantalla

### Pantalla de Picking CON Lotes (`Pedido_Picking_Item.xaml.cs`)

```csharp
private async void ProcesarCodigoProducto(string scannedCode)
{
    // Buscar y validar con lote
    var resultado = await BarCodeHelper.BuscarYValidarProducto(
        scannedCode, 
        v_Id_CD, 
        extraerLote: true
    );

    txt_IdProducto.Text = resultado.CodigoProducto;

    if (!resultado.Exitoso)
    {
        await DisplayAlert("Error!", resultado.MensajeError, "Ok");
        txt_IdProducto.Focus();
        return;
    }

    // Validar contra producto esperado
    if (resultado.CodigoProducto == lbl_IdArticulo.Text)
    {
        txt_IdProducto.BackgroundColor = Color.LawnGreen;
        
        // Si tiene lote, procesarlo
        if (resultado.TieneLote)
        {
            txt_IdLote.Text = resultado.Lote;
            ValidarLote();
        }
        
        txt_Cantidad.Focus();
    }
    else
    {
        await DisplayAlert("Error!", "Producto ERRADO!", "Ok");
    }
}
```

### Pantalla de Picking SIN Lotes (`Pedido_RepAlm_Picking_Item.xaml.cs`)

```csharp
private async void ProcesarCodigoProducto(string scannedCode)
{
    // Buscar y validar sin lote
    var resultado = await BarCodeHelper.BuscarYValidarProducto(
        scannedCode, 
        v_Id_CD, 
        extraerLote: false  // ❌ No necesitamos lote
    );

    txt_IdProducto.Text = resultado.CodigoProducto;

    if (!resultado.Exitoso)
    {
        await DisplayAlert("Error!", resultado.MensajeError, "Ok");
        return;
    }

    if (resultado.CodigoProducto == lbl_IdArticulo.Text)
    {
        txt_IdProducto.BackgroundColor = Color.LawnGreen;
        txt_Cantidad.Focus();
    }
}
```

### Pantalla de Recepción de Mercadería

```csharp
private async void txt_IdArticulo_Completed(object sender, EventArgs e)
{
    var resultado = await BarCodeHelper.BuscarYValidarProducto(
        txt_IdArticulo.Text, 
        v_Id_CD, 
        extraerLote: true  // Recepciones manejan lotes
    );

    if (!resultado.Exitoso)
    {
        await DisplayAlert("Error", resultado.MensajeError, "Ok");
        txt_IdArticulo.Focus();
        return;
    }

    lbl_IdArticulo.Text = resultado.CodigoProducto;
    lbl_NombreProducto.Text = resultado.DescripcionProducto;
    txt_UM.Text = resultado.UnidadMedida;
    txt_Equivalencia.Text = resultado.Equivalencia;
    
    // Si viene con lote, pre-llenarlo
    if (resultado.TieneLote)
    {
        txt_Lote.Text = resultado.Lote;
    }
}
```

---

## Tipos de Error

```csharp
public enum TipoErrorBusqueda
{
    Ninguno,              // ✅ Sin error
    ProductoNoExiste,     // ❌ Producto no encontrado en BD
    ProductoDuplicado,    // ❌ Múltiples productos con mismo código
    ErrorBaseDatos,       // ❌ Error de conexión o SQL
    CodigoVacio          // ❌ Código vacío o nulo
}
```

---

## Formatos Soportados

| Formato de Entrada | Código Extraído | Lote Extraído | Ejemplo |
|-------------------|-----------------|---------------|---------|
| `7501234` | `7501234` | `""` | EAN simple |
| `7501234\|LOTE123` | `7501234` | `LOTE123` | QR con lote |
| `\|7501234\|LOTE123` | `7501234` | `LOTE123` | QR formato alternativo |

---

## Ventajas de Usar el Helper

1. **✅ Código Centralizado**: Un solo lugar para mantener la lógica
2. **✅ Validación Completa**: Procesa + busca + valida en un paso
3. **✅ Mensajes Consistentes**: Errores estandarizados en toda la app
4. **✅ Testeable**: Fácil de probar unitariamente
5. **✅ Reutilizable**: Usar en cualquier pantalla que escanee códigos
6. **✅ Flexible**: Soporta múltiples formatos de QR y EAN

---

## Migración desde Código Antiguo

### ANTES ❌
```csharp
// Código duplicado en cada pantalla
string eanCode = scannedCode;
if (scannedCode.Contains("|"))
{
    var parts = scannedCode.Split('|');
    // ... lógica compleja ...
}
BuscarProductos_BarCode(eanCode, v_Id_CD);
```

### AHORA ✅
```csharp
// Una sola línea, hace todo
var resultado = await BarCodeHelper.BuscarYValidarProducto(scannedCode, v_Id_CD, extraerLote: true);
```

---

## Notas Importantes

- ⚠️ Siempre usa `await` cuando llames a `BuscarYValidarProducto`
- ⚠️ Verifica `resultado.Exitoso` antes de usar los datos
- ⚠️ El parámetro `extraerLote` debe ser `true` solo si tu pantalla usa lotes
- ⚠️ El helper maneja automáticamente las conexiones LIM/ATE según `idCD`

---

## Soporte

Para dudas o mejoras, contactar al equipo de desarrollo.
