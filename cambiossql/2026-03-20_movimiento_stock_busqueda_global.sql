-- Cambios propuestos para MovimientoStock
-- Fuente de verdad validada contra la BD real por MCP mssql_gptor.
-- Objetivo:
-- 1. Unificar el contrato de resultados para que siempre incluya ID_UBICACION.
-- 2. Agregar busqueda global por articulo dentro del almacen seleccionado.
-- 3. Mantener la busqueda actual por ubicacion sin cambiar parametros de entrada.

IF OBJECT_ID('dbo.usp_CargarStockUbicacion', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CargarStockUbicacion;
GO

CREATE PROCEDURE dbo.usp_CargarStockUbicacion
    @ID_ALMACEN VARCHAR(15),
    @ID_UBICACION VARCHAR(31)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.ID_UBICACION,
        a.ID_ARTICULO,
        a.ID_LOTE,
        a.STOCK,
        ISNULL(d.DESCRIPCION, '') AS DESCRIPCION
    FROM ARTICULO_UBICACION a
    LEFT JOIN DM_ARTICULOS d
        ON RTRIM(d.ID_ARTICULO) = RTRIM(a.ID_ARTICULO)
    WHERE RTRIM(a.ID_ALMACEN) = RTRIM(@ID_ALMACEN)
      AND RTRIM(a.ID_UBICACION) = RTRIM(@ID_UBICACION)
      AND a.STOCK > 0
    ORDER BY a.ID_ARTICULO, a.ID_LOTE;
END
GO

IF OBJECT_ID('dbo.usp_BuscarArticuloEnUbicacion', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_BuscarArticuloEnUbicacion;
GO

CREATE PROCEDURE dbo.usp_BuscarArticuloEnUbicacion
    @ID_ALMACEN VARCHAR(15),
    @ID_UBICACION VARCHAR(31),
    @ID_ARTICULO VARCHAR(31)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.ID_UBICACION,
        a.ID_ARTICULO,
        a.ID_LOTE,
        a.STOCK,
        ISNULL(d.DESCRIPCION, '') AS DESCRIPCION
    FROM ARTICULO_UBICACION a
    LEFT JOIN DM_ARTICULOS d
        ON RTRIM(d.ID_ARTICULO) = RTRIM(a.ID_ARTICULO)
    WHERE RTRIM(a.ID_ARTICULO) = RTRIM(@ID_ARTICULO)
      AND RTRIM(a.ID_ALMACEN) = RTRIM(@ID_ALMACEN)
      AND RTRIM(a.ID_UBICACION) = RTRIM(@ID_UBICACION)
      AND a.STOCK > 0
    ORDER BY a.ID_LOTE;
END
GO

IF OBJECT_ID('dbo.usp_BuscarArticuloEnAlmacen', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_BuscarArticuloEnAlmacen;
GO

CREATE PROCEDURE dbo.usp_BuscarArticuloEnAlmacen
    @ID_ALMACEN VARCHAR(15),
    @ID_ARTICULO VARCHAR(31)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.ID_UBICACION,
        a.ID_ARTICULO,
        a.ID_LOTE,
        a.STOCK,
        ISNULL(d.DESCRIPCION, '') AS DESCRIPCION
    FROM ARTICULO_UBICACION a
    LEFT JOIN DM_ARTICULOS d
        ON RTRIM(d.ID_ARTICULO) = RTRIM(a.ID_ARTICULO)
    WHERE RTRIM(a.ID_ALMACEN) = RTRIM(@ID_ALMACEN)
      AND RTRIM(a.ID_ARTICULO) = RTRIM(@ID_ARTICULO)
      AND a.STOCK > 0
    ORDER BY a.ID_UBICACION, a.ID_LOTE;
END
GO
