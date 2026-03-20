# ✅ CHECKLIST DE VALIDACIÓN - MOVIMIENTO DE STOCK

## 📋 PASO 1: Despliegue de Base de Datos

### WMS_ATE (CD=2)
- [ ] Conectar a base de datos WMS_ATE
- [ ] Ejecutar script `SQL/SP_MovimientoStock.sql`
- [ ] Verificar que se crearon 4 SPs:
  - [ ] `usp_CargarStockUbicacion`
  - [ ] `usp_BuscarArticuloEnUbicacion`
  - [ ] `usp_ValidarUbicacionExiste`
  - [ ] `usp_ValidarLoteExisteParaArticulo`
- [ ] Ejecutar scripts de prueba (comentados al final del archivo SQL)
- [ ] Verificar permisos EXEC para usuario de la app

### WMS_LIM (CD=1)
- [ ] Conectar a base de datos WMS_LIM
- [ ] Ejecutar script `SQL/SP_MovimientoStock.sql`
- [ ] Verificar que se crearon 4 SPs:
  - [ ] `usp_CargarStockUbicacion`
  - [ ] `usp_BuscarArticuloEnUbicacion`
  - [ ] `usp_ValidarUbicacionExiste`
  - [ ] `usp_ValidarLoteExisteParaArticulo`
- [ ] Ejecutar scripts de prueba
- [ ] Verificar permisos EXEC para usuario de la app
- [ ] **⚠️ CONFIRMAR ID_ALMACEN DE LIMA** (actualmente desconocido)

---

## 📋 PASO 2: Compilación y Despliegue

- [ ] Compilar proyecto sin errores
- [ ] Generar APK/IPA
- [ ] Desplegar en dispositivos de prueba
- [ ] Verificar que la app inicia correctamente

---

## 📋 PASO 3: Testing Funcional - CD ATE (ID=2, Almacén='013')

### 3.1 Selección de CD y Almacén
- [ ] Abrir app y navegar a "Movimiento de Stock"
- [ ] Verificar que aparece pantalla de selección
- [ ] Verificar que se cargan los CDs correctamente
- [ ] Seleccionar "ATE" en picker de CD
- [ ] Verificar que se cargan los almacenes de ATE
- [ ] Seleccionar almacén "013"
- [ ] Presionar "Iniciar Movimiento de Stock"
- [ ] Verificar que navega correctamente a la pantalla principal

### 3.2 Validación de Ubicación
- [ ] **Escanear/Ingresar ubicación VÁLIDA** (ej: A-01-01-01)
  - [ ] Verificar que la ubicación se valida correctamente
  - [ ] Verificar que lblUbicacion muestra la ubicación en verde
  - [ ] Verificar que se carga el stock de la ubicación
  
- [ ] **Ingresar ubicación INVÁLIDA** (ej: ZZZ-99-99-99)
  - [ ] Verificar que muestra error "La ubicación no existe..."
  - [ ] Verificar que no se carga stock

### 3.3 Cargar Stock de Ubicación
- [ ] Con ubicación válida, verificar que ListView muestra:
  - [ ] ID_ARTICULO
  - [ ] ID_LOTE
  - [ ] STOCK
  - [ ] DESCRIPCION
- [ ] Verificar que solo muestra artículos con STOCK > 0
- [ ] Verificar mensaje si ubicación sin stock

### 3.4 Búsqueda de Artículo
- [ ] **Escanear/Ingresar artículo EXISTENTE en la ubicación**
  - [ ] Verificar que filtra correctamente el ListView
  - [ ] Verificar que muestra solo ese artículo
  - [ ] Verificar que entry se limpia después de buscar
  
- [ ] **Ingresar artículo NO EXISTENTE en la ubicación**
  - [ ] Verificar mensaje "No hay stock en esta ubicación para este artículo"
  - [ ] Verificar que mantiene la lista completa de la ubicación

### 3.5 Selección y Procesamiento
- [ ] Seleccionar una fila del ListView
- [ ] Verificar que se marca visualmente
- [ ] Presionar "Procesar"
- [ ] Verificar que navega a pantalla de procesamiento con datos correctos:
  - [ ] Artículo correcto
  - [ ] Descripción correcta
  - [ ] Lote correcto
  - [ ] Stock correcto
  - [ ] Ubicación origen correcta

### 3.6 Validación de Ubicación Destino
- [ ] **Ingresar ubicación destino VÁLIDA**
  - [ ] Verificar que se valida correctamente
  - [ ] Verificar que avanza al campo de lote
  
- [ ] **Ingresar ubicación destino INVÁLIDA**
  - [ ] Verificar mensaje de error
  - [ ] Verificar que no avanza

### 3.7 Validación de Lote Destino
- [ ] **Ingresar lote EXISTENTE para el artículo**
  - [ ] Verificar que se valida correctamente
  - [ ] Verificar que avanza al campo cantidad
  
- [ ] **Ingresar lote NO EXISTENTE**
  - [ ] Verificar mensaje de error
  - [ ] Verificar que no avanza
  
- [ ] **Escanear QR con formato |EAN|LOTE**
  - [ ] Verificar que extrae el lote correctamente
  - [ ] Verificar validación

### 3.8 Movimiento de Stock
- [ ] **Movimiento al mismo lote**
  - [ ] Ingresar cantidad válida (menor o igual al stock)
  - [ ] Presionar "Mover"
  - [ ] Verificar que ejecuta correctamente
  - [ ] Verificar mensaje de éxito
  - [ ] Verificar que vuelve a pantalla anterior
  - [ ] Verificar que se recarga el stock automáticamente
  
- [ ] **Movimiento a lote diferente**
  - [ ] Ingresar lote destino diferente al origen
  - [ ] Verificar mensaje de confirmación
  - [ ] Confirmar movimiento
  - [ ] Verificar que ejecuta correctamente
  
- [ ] **Cantidad mayor al stock**
  - [ ] Ingresar cantidad mayor al stock disponible
  - [ ] Verificar que se ajusta automáticamente
  - [ ] Verificar mensaje de advertencia

---

## 📋 PASO 4: Testing Funcional - CD LIMA (ID=1)

**⚠️ IMPORTANTE:** Antes de ejecutar estos tests, confirmar el ID_ALMACEN de Lima

### 4.1 Repetir todos los tests del PASO 3
- [ ] Selección de CD "LIMA"
- [ ] Verificar que se cargan almacenes '001' (ALM PRINCIPAL)
- [ ] Selección de CD "ATE" 
- [ ] Verificar que se cargan almacenes '013' (ATE) y '020' (Calle Boulevard 1030)
- [ ] Todos los flujos de validación
- [ ] Todos los flujos de movimiento

---

## 📋 PASO 5: Testing de Regresión

### 5.1 Verificar que NO se rompieron otros módulos
- [ ] Pedido_Picking sigue funcionando
- [ ] Producto_Ubicaciones sigue funcionando
- [ ] WMS_RecepcionMercaderia sigue funcionando
- [ ] Reparto_CargaVehiculos sigue funcionando

---

## 📋 PASO 6: Performance y Estabilidad

### 6.1 Performance
- [ ] Medir tiempo de carga de stock (debe ser < 2 segundos)
- [ ] Medir tiempo de validación de ubicación (debe ser < 1 segundo)
- [ ] Medir tiempo de búsqueda de artículo (debe ser < 1 segundo)

### 6.2 Estabilidad
- [ ] Probar con conexión intermitente
- [ ] Probar con múltiples movimientos seguidos
- [ ] Verificar que no hay memory leaks
- [ ] Probar escaneo de códigos de barras consecutivos

---

## 📋 PASO 7: Casos Edge

### 7.1 Casos Límite
- [ ] Ubicación vacía (sin stock)
- [ ] Ubicación con 1 solo artículo
- [ ] Ubicación con muchos artículos (> 50)
- [ ] Artículo con múltiples lotes
- [ ] Mover todo el stock (cantidad = stock disponible)
- [ ] Mover cantidad decimal (ej: 2.5)

### 7.2 Interrupciones
- [ ] Interrumpir durante carga de stock (llamada telefónica)
- [ ] Interrumpir durante movimiento (llamada telefónica)
- [ ] Cambiar de app y volver
- [ ] Perder conexión durante validación

---

## 📋 PASO 8: Documentación

- [ ] Actualizar manual de usuario si existe
- [ ] Documentar ID_ALMACEN de Lima una vez confirmado
- [ ] Actualizar diagrama de arquitectura
- [ ] Documentar nuevas SPs en wiki/confluence

---

## 📋 PASO 9: Rollback Plan (Solo si hay problemas críticos)

### Si algo sale mal:
1. [ ] Restaurar AppShell.xaml a versión anterior
2. [ ] Restaurar MovimientoStock.xaml.cs a versión anterior
3. [ ] Recompilar y desplegar
4. [ ] Las SPs pueden quedarse (no afectan si no se usan)

---

## ✅ APROBACIÓN FINAL

- [ ] **Todos los tests pasaron exitosamente**
- [ ] **Performance aceptable**
- [ ] **Sin errores críticos**
- [ ] **Documentación actualizada**

**Responsable de QA:** ___________________  
**Fecha:** ___________________  
**Firma:** ___________________

---

## 📝 NOTAS Y OBSERVACIONES

```
[Espacio para notas durante el testing]




```

---

**Versión del Checklist:** 1.0  
**Última actualización:** 2026-01-03
