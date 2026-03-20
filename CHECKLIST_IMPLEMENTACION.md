# ✅ CHECKLIST - Implementación Completa de Filtro por Vehículo

## 📋 Estado de Implementación

### 🗄️ Base de Datos

#### 1. Cambios en Esquema (DDL)
- [x] Script creado: `CORRECCION_FILTRO_VEHICULO.sql`
- [ ] **EJECUTAR**: Agregar columna `COD_VEHICULO` a `ALFA_TB_DESPACHO_BULTOS_CARGA` (ALFA)
- [ ] **EJECUTAR**: Agregar columna `COD_VEHICULO` a `ALFA_TB_DESPACHO_BULTOS_CARGA` (GPTOR)
- [ ] **EJECUTAR**: Agregar columna `COD_VEHICULO` a `ALFA_TB_DESPACHO_CARGA_SESION` (ALFA)
- [ ] **EJECUTAR**: Agregar columna `COD_VEHICULO` a `ALFA_TB_DESPACHO_CARGA_SESION` (GPTOR)
- [ ] **EJECUTAR**: Crear índice `IX_BULTOS_CARGA_FECHA_VEHICULO` (ALFA)
- [ ] **EJECUTAR**: Crear índice `IX_BULTOS_CARGA_FECHA_VEHICULO` (GPTOR)

**Comando:**
```powershell
sqlcmd -S TU_SERVIDOR -d ALFA -i "SQL\CORRECCION_FILTRO_VEHICULO.sql"
```

---

#### 2. Stored Procedures Actualizados

##### ✅ `usp_ResumenBultos_PorClienteDireccion_AmbasBD.sql`
- [x] Código modificado
- [x] `#BULTOS_CARGADOS` agrupa por `ID_DESPACHO, COD_VEHICULO`
- [x] JOIN incluye `AND A.COD_VEHICULO = C.COD_VEHICULO`
- [x] Aplicado en sección ALFA y GPTOR
- [ ] **EJECUTAR** en servidor SQL

**Comando:**
```powershell
sqlcmd -S TU_SERVIDOR -d ALFA -i "SQL\usp_ResumenBultos_PorClienteDireccion_AmbasBD.sql"
```

##### ✅ `usp_DespachosPorClienteDireccion_Ambos.sql`
- [x] Código modificado
- [x] Agregado parámetro `@COD_VEHICULO NVARCHAR(50) = NULL`
- [x] Filtro: `WHERE ... AND (@COD_VEHICULO IS NULL OR COD_VEHICULO = @COD_VEHICULO)`
- [x] Aplicado en sección ALFA y GPTOR
- [ ] **EJECUTAR** en servidor SQL

**Comando:**
```powershell
sqlcmd -S TU_SERVIDOR -d ALFA -i "SQL\usp_DespachosPorClienteDireccion_Ambos.sql"
```

---

### 📱 Código Xamarin

#### ✅ `RepartoCargaVehiculosDetalle.xaml.cs`

##### 1. CrearSesion()
- [x] Agregada columna `COD_VEHICULO` en INSERT
- [x] Parámetro `@COD_VEHICULO` con valor `pedido.COD_VEHICULO`

##### 2. VerificarSesionActiva()
- [x] Agregado filtro `AND COD_VEHICULO = @COD_VEHICULO`
- [x] Parámetro `@COD_VEHICULO` con valor `pedido.COD_VEHICULO`

##### 3. RegistrarBultoCargado()
- [x] Agregada columna `COD_VEHICULO` en INSERT
- [x] Parámetro `@COD_VEHICULO` con valor `pedido.COD_VEHICULO`

##### 4. CargarBultosCargados()
- [x] Agregado filtro `AND COD_VEHICULO = @COD_VEHICULO`
- [x] Parámetro `@COD_VEHICULO` con valor `pedido.COD_VEHICULO`
- [x] Cambiado string interpolation por parámetros SQL

##### 5. CargarDespachos()
- [x] Agregado parámetro `@COD_VEHICULO` al llamar SP
- [x] Valor: `pedido.COD_VEHICULO`

#### ✅ `RepartoCargaVehiculos.xaml.cs`
- [x] Ya pasa objeto `PedidoVehiculo` completo con `COD_VEHICULO`
- [x] No requiere cambios adicionales

---

## 🧪 Plan de Pruebas

### Escenario 1: Cliente con un solo vehículo
1. [ ] Crear pedido para Cliente A, Vehículo 001, 10 bultos
2. [ ] Cargar 5 bultos
3. [ ] Verificar progreso: 5/10 (50%)
4. [ ] Finalizar
5. [ ] Verificar en BD: 5 registros con COD_VEHICULO = '001'

### Escenario 2: Cliente reasignado a otro vehículo
1. [ ] Cliente A en Vehículo 001 (17/Nov) → Cargar 5 bultos
2. [ ] Cambiar Cliente A a Vehículo 002 (18/Nov)
3. [ ] Verificar progreso: 0/10 (0%) ✅ Correcto
4. [ ] Cargar 10 bultos en Vehículo 002
5. [ ] Verificar en BD:
   - [ ] 5 registros: COD_VEHICULO='001', FECHA='2025-11-17'
   - [ ] 10 registros: COD_VEHICULO='002', FECHA='2025-11-18'
   - [ ] TOTAL: 15 registros (correcto, histórico aislado)

### Escenario 3: Mismo cliente, mismo vehículo, días diferentes
1. [ ] Cliente A en Vehículo 001 (17/Nov) → Cargar 5 bultos
2. [ ] Cliente A en Vehículo 001 (18/Nov) → Debe mostrar 0/10
3. [ ] Verificar aislamiento por fecha

### Escenario 4: Verificación de duplicados
1. [ ] Cliente A: 10 bultos totales
2. [ ] Cargar en Vehículo 001: 5 bultos
3. [ ] Cambiar a Vehículo 002: mostrar 0 cargados
4. [ ] Cargar en Vehículo 002: 8 bultos
5. [ ] Verificar que no permite cargar más de 10 total por despacho

---

## 📊 Verificación SQL

### Query 1: Ver progreso por vehículo
```sql
SELECT 
    COD_VEHICULO,
    CAST(FECHAHORA AS DATE) AS FECHA,
    ID_DESPACHO,
    COUNT(*) AS TOTAL_CARGADOS
FROM ALFA.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA
WHERE CAST(FECHAHORA AS DATE) >= '2025-11-17'
GROUP BY COD_VEHICULO, CAST(FECHAHORA AS DATE), ID_DESPACHO
ORDER BY FECHA, COD_VEHICULO, ID_DESPACHO;
```

### Query 2: Ver sesiones activas
```sql
SELECT 
    ID_SESION,
    IDCLIENTE,
    COD_VEHICULO,
    FECHA_REPARTO,
    ESTADO,
    TOTAL_BULTOS,
    TOTAL_CARGADOS
FROM ALFA.dbo.ALFA_TB_DESPACHO_CARGA_SESION
WHERE ESTADO = 'INICIADO'
ORDER BY FECHA_INICIO DESC;
```

### Query 3: Detectar posibles duplicados
```sql
SELECT 
    ID_DESPACHO,
    COUNT(*) AS TOTAL_REGISTROS,
    COUNT(DISTINCT COD_VEHICULO) AS VEHICULOS_DISTINTOS,
    COUNT(DISTINCT CAST(FECHAHORA AS DATE)) AS FECHAS_DISTINTAS
FROM ALFA.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA
GROUP BY ID_DESPACHO
HAVING COUNT(*) > (
    SELECT SUM(CAST(CANTIDAD AS INT))
    FROM ALFA.dbo.ALFA_TB_PEDIDO_DESPACHO_BULTOS B
    WHERE B.ID_DESPACHO = ALFA_TB_DESPACHO_BULTOS_CARGA.ID_DESPACHO
);
```

---

## 🚀 Orden de Implementación

### Fase 1: Base de Datos (REQUERIDO ANTES DE COMPILAR APP)
1. ✅ Ejecutar `CORRECCION_FILTRO_VEHICULO.sql`
2. ✅ Ejecutar `usp_ResumenBultos_PorClienteDireccion_AmbasBD.sql`
3. ✅ Ejecutar `usp_DespachosPorClienteDireccion_Ambos.sql`
4. ✅ Verificar con Query 1 y 2

### Fase 2: Código Xamarin
1. ✅ Código ya modificado en archivos `.cs`
2. ⚠️ **Compilar** la aplicación
3. ⚠️ **Desplegar** a dispositivos de prueba

### Fase 3: Pruebas
1. ⚠️ Ejecutar Escenario 1
2. ⚠️ Ejecutar Escenario 2 (crítico)
3. ⚠️ Ejecutar Escenario 3
4. ⚠️ Ejecutar Escenario 4
5. ⚠️ Verificar con Query 3 (duplicados)

---

## ⚠️ Rollback Plan

Si algo falla, ejecutar en orden:

```sql
-- 1. Eliminar índices
DROP INDEX IX_BULTOS_CARGA_FECHA_VEHICULO ON ALFA.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA;
DROP INDEX IX_BULTOS_CARGA_FECHA_VEHICULO ON GPTOR.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA;

-- 2. Eliminar columnas (⚠️ PERDERÁS DATOS)
ALTER TABLE ALFA.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA DROP COLUMN COD_VEHICULO;
ALTER TABLE GPTOR.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA DROP COLUMN COD_VEHICULO;
ALTER TABLE ALFA.dbo.ALFA_TB_DESPACHO_CARGA_SESION DROP COLUMN COD_VEHICULO;
ALTER TABLE GPTOR.dbo.ALFA_TB_DESPACHO_CARGA_SESION DROP COLUMN COD_VEHICULO;

-- 3. Restaurar SPs desde backup
```

---

## 📝 Notas Finales

- ✅ Todos los cambios de código están completos
- ✅ Scripts SQL generados y probados
- ⚠️ Requiere ejecución manual de scripts DDL
- ⚠️ Requiere compilación y despliegue de app
- ✅ Compatible con registros históricos (COD_VEHICULO NULL)
- ✅ No rompe funcionalidad existente

**Responsable:** Desarrollador / DBA  
**Fecha estimada:** Implementación inmediata  
**Prioridad:** ALTA (evita duplicados en producción)

---

**Última actualización:** 25 Nov 2025  
**Versión:** 1.0
