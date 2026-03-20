# Análisis Completo: Ventana Movimiento de Stock

## 📋 Resumen Ejecutivo

La funcionalidad de **Movimiento de Stock** permite a los usuarios transferir productos entre ubicaciones dentro de un almacén del sistema WMS (Warehouse Management System). Es una aplicación Xamarin.Forms que conecta con bases de datos SQL Server para gestionar inventario en tiempo real.

---

## 🏗️ Arquitectura del Sistema

### Flujo de Pantallas

```
MovimientoStock_SelecCD → MovimientoStock → MovimientoStockProcesar
     (Selección)            (Consulta)         (Procesamiento)
```

### Pantallas Involucradas

| Pantalla | Archivos | Función |
|----------|----------|---------|
| **Selección CD/Almacén** | `MovimientoStock_SelecCD.xaml` / `.xaml.cs` | Punto de entrada - seleccionar sede y almacén |
| **Consulta de Stock** | `MovimientoStock.xaml` / `.xaml.cs` | Buscar y visualizar stock por ubicación |
| **Procesar Movimiento** | `MovimientoStockProcesar.xaml` / `.xaml.cs` | Ejecutar el traspaso de stock |

---

## 📱 Pantalla 1: MovimientoStock_SelecCD

### Propósito
Permite al usuario seleccionar el **Centro de Distribución (CD)** y el **Almacén** donde desea trabajar.

### Elementos de la UI
- **Picker de Sede (CD)**: Lista desplegable con los CDs disponibles
- **Picker de Almacén**: Lista desplegable que se filtra según el CD seleccionado
- **Botón "Ir a Movimiento de Stock"**: Navega a la pantalla principal

### Lógica de Negocio
- Al seleccionar un CD, se cargan dinámicamente los almacenes asociados
- Los almacenes se obtienen mediante el stored procedure `usp_Almacenes_Sel_MultiCD`
- Validación: Debe seleccionar tanto sede como almacén antes de continuar

### Mapeo de CDs
| ID_CD | Descripción | Almacenes Disponibles |
|-------|-------------|----------------------|
| 1 | LIMA | 001 (Alm Principal) |
| 2 | ATE | 013, 020 |
| 3 | Otro | 020 |

---

## 📱 Pantalla 2: MovimientoStock (Principal)

### Propósito
Pantalla principal para **consultar el stock** de una ubicación específica y seleccionar artículos para mover.

### Elementos de la UI

#### Sección Superior - Selección de Ubicación
- **Label "Ubicación Activa"**: Muestra la ubicación seleccionada (color verde cuando está activa)
- **Entry para escanear/ingresar ubicación**: Campo de texto para código de barras o escritura manual
- **Botón de escáner**: Abre el lector de códigos de barras (ZXing)

#### Sección Media - Búsqueda de Artículo
- **Entry para artículo**: Campo para escanear/ingresar EAN o código de artículo
- **Botón "Buscar"**: Filtra el stock por artículo específico

#### Sección Inferior - Lista de Resultados
- **ListView** con columnas:
  - **Descripción** (1 línea, truncada si es larga)
  - **Artículo** (ID del artículo)
  - **Lote** (Número de lote)
  - **Stock** (Cantidad disponible, formato N2)

- **Botón "Procesar"**: Navega a la pantalla de procesamiento con el item seleccionado

### Flujo de Operación

```
1. Escanear/Ingresar Ubicación
   ↓
2. Validar que existe en DM_UBICACIONES
   ↓
3. Cargar stock de la ubicación (SP: usp_CargarStockUbicacion)
   ↓
4. (Opcional) Filtrar por artículo (SP: usp_BuscarArticuloEnUbicacion)
   ↓
5. Seleccionar un item de la lista
   ↓
6. Clic en "Procesar"
```

### Clase de Modelo: StockItem

```csharp
public class StockItem
{
    public string ID_ARTICULO { get; set; }    // Código del artículo
    public string ID_LOTE { get; set; }        // Número de lote
    public decimal STOCK { get; set; }         // Cantidad disponible
    public string DESCRIPCION { get; set; }    // Descripción del artículo
}
```

### Características Técnicas
- **Soporte multi-CD**: Conecta a diferentes bases de datos según el CD (LIMA o ATE)
- **Escáner de códigos de barras**: Integración con ZXing para lectura de ubicaciones
- **Validación en tiempo real**: Verifica existencia de ubicación antes de cargar stock
- **Comunicación entre pantallas**: Usa `MessagingCenter` para recargar datos después de un movimiento

---

## 📱 Pantalla 3: MovimientoStockProcesar

### Propósito
Pantalla para **ejecutar el movimiento de stock** de un artículo/lote desde una ubicación origen a un destino.

### Elementos de la UI

#### Sección Origen (Frame azul)
- **Ubicación origen**: Mostrada como label
- **Descripción del artículo**: Texto grande (hasta 3 líneas)
- **Código de artículo**: Label pequeño
- **Lote y Stock**: En la misma línea

#### Sección Destino (Frame rosa)
- **Ubicación destino**: Entry + botón de escáner
- **Lote destino**: Entry + botón de escáner
- **Cantidad a mover**: Entry numérico + label "Max: XX.XX"
- **Botón "Mover"**: Ejecuta el movimiento

### Flujo de Operación

```
1. Recibe datos del artículo seleccionado (artículo, lote, stock, ubicación)
   ↓
2. Usuario ingresa/escanea ubicación destino
   ↓
3. Validar ubicación destino existe (SP: usp_ValidarUbicacionExiste)
   ↓
4. Usuario ingresa/escanea lote destino
   ↓
5. Validar lote existe para el artículo (SP: usp_ValidarLoteExisteParaArticulo)
   ↓
6. Usuario ingresa cantidad a mover (validar ≤ stock origen)
   ↓
7. Si lote origen ≠ lote destino → Confirmación del usuario
   ↓
8. Ejecutar movimiento (SP: usp_MoverStock_Entre_Lotes)
   ↓
9. Notificar a pantalla anterior para recargar
   ↓
10. Regresar a pantalla anterior
```

### Validaciones Realizadas

| Validación | Mensaje de Error |
|------------|------------------|
| Ubicación destino no existe | "La ubicación no existe en el maestro de ubicaciones" |
| Lote destino no existe | "El lote no existe para este artículo en el maestro de lotes" |
| Cantidad vacía | "Debe ingresar la cantidad a mover" |
| Cantidad no numérica | "La cantidad debe ser un número válido" |
| Cantidad ≤ 0 | "La cantidad debe ser mayor a cero" |
| Cantidad > stock origen | Se ajusta automáticamente al máximo con advertencia |
| Lote destino vacío (si difiere del origen) | "Debe ingresar el lote de destino" |

### Parámetros del Stored Procedure `usp_MoverStock_Entre_Lotes`

```sql
@ID_ARTICULO          -- Artículo a mover
@ID_ALMACEN           -- Almacén
@ID_UBICACION_ORIG    -- Ubicación de origen
@ID_LOTE_ORIG         -- Lote de origen
@ID_UBICACION_DEST    -- Ubicación de destino
@ID_LOTE_DEST         -- Lote de destino
@CANTIDAD             -- Cantidad a mover
@USUARIO              -- Usuario que ejecuta (Environment.UserName)
@CREAR_LOTE_DESTINO   -- Flag para crear lote si no existe (1 = sí)
```

---

## 🗄️ Stored Procedures (SQL)

### 1. usp_CargarStockUbicacion
**Propósito**: Obtener todo el stock disponible en una ubicación específica.

```sql
-- Parámetros
@ID_ALMACEN VARCHAR(15)
@ID_UBICACION VARCHAR(31)

-- Retorna
ID_ARTICULO, ID_LOTE, STOCK, DESCRIPCION

-- Tablas
ARTICULO_UBICACION (principal)
DM_ARTICULOS (LEFT JOIN para descripción)

-- Filtros
- Almacén y ubicación coinciden
- Stock > 0
```

### 2. usp_BuscarArticuloEnUbicacion
**Propósito**: Filtrar stock de un artículo específico en una ubicación.

```sql
-- Parámetros
@ID_ALMACEN VARCHAR(15)
@ID_UBICACION VARCHAR(31)
@ID_ARTICULO VARCHAR(31)

-- Retorna
ID_ARTICULO, ID_LOTE, STOCK, DESCRIPCION
```

### 3. usp_ValidarUbicacionExiste
**Propósito**: Verificar si una ubicación existe en el maestro.

```sql
-- Parámetros
@ID_ALMACEN VARCHAR(15)
@ID_UBICACION VARCHAR(25)

-- Retorna
EXISTE (BIT: 1 = existe, 0 = no existe)

-- Tabla
DM_UBICACIONES
```

### 4. usp_ValidarLoteExisteParaArticulo
**Propósito**: Verificar si un lote está registrado para un artículo.

```sql
-- Parámetros
@ID_ARTICULO VARCHAR(31)
@ID_LOTE VARCHAR(150)

-- Retorna
EXISTE (BIT)

-- Tabla
DM_LOTES
```

### 5. usp_Almacenes_Sel_MultiCD
**Propósito**: Obtener almacenes disponibles según el Centro de Distribución.

```sql
-- Parámetros
@ID_CD INT

-- Retorna
ID_ALMACEN, DESCRIPCION

-- Lógica condicional por CD
```

### 6. usp_MoverStock_Entre_Lotes (No incluido en el archivo)
**Propósito**: Ejecutar el movimiento físico de stock entre ubicaciones/lotes.

```sql
-- Parámetros (inferidos del código)
@ID_ARTICULO, @ID_ALMACEN, @ID_UBICACION_ORIG, @ID_LOTE_ORIG
@ID_UBICACION_DEST, @ID_LOTE_DEST, @CANTIDAD, @USUARIO, @CREAR_LOTE_DESTINO

-- Retorna
OUT_STATUS (OK/ERROR), mensaje de error si aplica
```

---

## 🔧 Configuración de Conexiones

### Soporte Multi-Base de Datos

El sistema soporta dos centros de distribución con bases de datos diferentes:

| CD | Método de Conexión | Base de Datos |
|----|-------------------|---------------|
| 1 (LIMA) | `Conexion.Abrir_WMS_LIM()` | WMS_LIM |
| 2 (ATE) | `Conexion.Abrir_WMS_ATE()` | WMS_ATE |

### Patrón de Uso
```csharp
if (v_Id_CD == 1)
{
    Conexion.Abrir_WMS_LIM();
    // ... ejecutar comando
    Conexion.Cerrar_WMS_LIM();
}
else
{
    Conexion.Abrir_WMS_ATE();
    // ... ejecutar comando
    Conexion.Cerrar_WMS_ATE();
}
```

---

## 📊 Tablas de Base de Datos Involucradas

| Tabla | Propósito |
|-------|-----------|
| `ARTICULO_UBICACION` | Stock actual por artículo, lote, almacén y ubicación |
| `DM_ARTICULOS` | Maestro de artículos (descripción) |
| `DM_UBICACIONES` | Maestro de ubicaciones del almacén |
| `DM_LOTES` | Maestro de lotes por artículo |
| `DM_ALMACEN` | Maestro de almacenes |

---

## 🔐 Seguridad y Validaciones

### Validaciones de Negocio
1. **Ubicación debe existir**: Se valida contra DM_UBICACIONES antes de cualquier operación
2. **Lote debe existir**: Se valida contra DM_LOTES para el artículo específico
3. **Stock suficiente**: La cantidad no puede superar el stock disponible en origen
4. **Confirmación de cambio de lote**: Si el lote destino es diferente al origen, se pide confirmación al usuario

### Usuario
- Se captura `Environment.UserName` para auditoría del movimiento

---

## 🎯 Casos de Uso Principales

### Caso 1: Consultar Stock de Ubicación
1. Usuario escanea ubicación
2. Sistema muestra todo el stock (artículos, lotes, cantidades)
3. Usuario puede filtrar por artículo específico

### Caso 2: Mover Stock Mismo Lote
1. Usuario selecciona artículo de la lista
2. Ingresa ubicación destino
3. Ingresa misma cantidad (o parcial)
4. Sistema ejecuta el movimiento

### Caso 3: Mover Stock a Diferente Lote
1. Usuario selecciona artículo de la lista
2. Ingresa ubicación destino
3. Ingresa lote destino diferente
4. Sistema pide confirmación
5. Usuario confirma
6. Sistema ejecuta el movimiento

### Caso 4: Mismo Lote, Diferente Ubicación
1. Usuario selecciona artículo
2. Ingresa nueva ubicación destino
3. Mantiene mismo lote
4. Cantidad parcial o total
5. Ejecuta movimiento

---

## 🚀 Tecnologías Utilizadas

| Componente | Tecnología |
|------------|------------|
| Framework UI | Xamarin.Forms |
| Base de Datos | SQL Server |
| Conexión DB | ADO.NET (SqlConnection, SqlCommand) |
| Escáner Código de Barras | ZXing.Net.Mobile.Forms |
| Arquitectura | Code-behind (sin MVVM) |
| Patrón de Comunicación | MessagingCenter |

---

## 📝 Notas de Implementación

### Fortalezas
- ✅ Validaciones robustas en múltiples capas
- ✅ Soporte para múltiples CDs y almacenes
- ✅ Integración con escáner de códigos de barras
- ✅ Feedback visual claro (colores, mensajes)
- ✅ Comunicación entre pantallas para actualización en tiempo real

### Áreas de Mejora Potenciales
- 🔄 Migrar a arquitectura MVVM para mejor separación de responsabilidades
- 🔄 Implementar logging de errores más robusto
- 🔄 Agregar historial de movimientos realizados
- 🔄 Implementar offline-first con sincronización
- 🔄 Agregar confirmación antes de procesar movimiento

---

## 📅 Información del Sistema

- **Fecha de análisis**: 19 de marzo de 2026
- **Framework**: Xamarin.Forms
- **Patrón**: Code-behind
- **Base de datos**: SQL Server (WMS_LIM / WMS_ATE)