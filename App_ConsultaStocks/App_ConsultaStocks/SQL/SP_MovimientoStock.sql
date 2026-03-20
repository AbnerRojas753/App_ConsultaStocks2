-- =============================================
-- STORED PROCEDURES PARA MOVIMIENTO DE STOCK
-- Fecha: 2026-01-03
-- =============================================

-- =============================================
-- SP 1: Cargar stock de una ubicación
-- =============================================
IF OBJECT_ID('usp_CargarStockUbicacion', 'P') IS NOT NULL
    DROP PROCEDURE usp_CargarStockUbicacion;
GO

CREATE PROCEDURE usp_CargarStockUbicacion
    @ID_ALMACEN VARCHAR(15),
    @ID_UBICACION VARCHAR(31)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.ID_ARTICULO, 
        a.ID_LOTE, 
        a.STOCK, 
        ISNULL(d.DESCRIPCION,'') AS DESCRIPCION
    FROM ARTICULO_UBICACION a
    LEFT JOIN DM_ARTICULOS d ON RTRIM(d.ID_ARTICULO) = RTRIM(a.ID_ARTICULO)
    WHERE RTRIM(a.ID_ALMACEN) = RTRIM(@ID_ALMACEN) 
      AND RTRIM(a.ID_UBICACION) = RTRIM(@ID_UBICACION) 
      AND a.STOCK > 0
    ORDER BY a.ID_ARTICULO, a.ID_LOTE;
END
GO

-- =============================================
-- SP 2: Buscar artículo en una ubicación
-- =============================================
IF OBJECT_ID('usp_BuscarArticuloEnUbicacion', 'P') IS NOT NULL
    DROP PROCEDURE usp_BuscarArticuloEnUbicacion;
GO

CREATE PROCEDURE usp_BuscarArticuloEnUbicacion
    @ID_ALMACEN VARCHAR(15),
    @ID_UBICACION VARCHAR(31),
    @ID_ARTICULO VARCHAR(31)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.ID_ARTICULO, 
        a.ID_LOTE, 
        a.STOCK, 
        ISNULL(d.DESCRIPCION,'') AS DESCRIPCION
    FROM ARTICULO_UBICACION a
    LEFT JOIN DM_ARTICULOS d ON RTRIM(d.ID_ARTICULO) = RTRIM(a.ID_ARTICULO)
    WHERE RTRIM(a.ID_ARTICULO) = RTRIM(@ID_ARTICULO)
      AND RTRIM(a.ID_ALMACEN) = RTRIM(@ID_ALMACEN)
      AND RTRIM(a.ID_UBICACION) = RTRIM(@ID_UBICACION)
      AND a.STOCK > 0
    ORDER BY a.ID_LOTE;
END
GO

-- =============================================
-- SP 3: Validar si ubicación existe
-- =============================================
IF OBJECT_ID('usp_ValidarUbicacionExiste', 'P') IS NOT NULL
    DROP PROCEDURE usp_ValidarUbicacionExiste;
GO

CREATE PROCEDURE usp_ValidarUbicacionExiste
    @ID_ALMACEN VARCHAR(15),
    @ID_UBICACION VARCHAR(25)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @EXISTE BIT;
    
    IF EXISTS (
        SELECT 1 
        FROM DM_UBICACIONES 
        WHERE RTRIM(ID_ALMACEN) = RTRIM(@ID_ALMACEN) 
          AND RTRIM(ID_UBICACION) = RTRIM(@ID_UBICACION)
    )
        SET @EXISTE = 1
    ELSE
        SET @EXISTE = 0;
    
    -- Retornar como SELECT para DataTable
    SELECT @EXISTE AS EXISTE;
END
GO

-- =============================================
-- SP 4: Validar si lote existe para artículo
-- =============================================
IF OBJECT_ID('usp_ValidarLoteExisteParaArticulo', 'P') IS NOT NULL
    DROP PROCEDURE usp_ValidarLoteExisteParaArticulo;
GO

CREATE PROCEDURE usp_ValidarLoteExisteParaArticulo
    @ID_ARTICULO VARCHAR(31),
    @ID_LOTE VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @EXISTE BIT;
    
    IF EXISTS (
        SELECT 1 
        FROM DM_LOTES 
        WHERE RTRIM(ID_ARTICULO) = RTRIM(@ID_ARTICULO) 
          AND RTRIM(ID_LOTE) = RTRIM(@ID_LOTE)
    )
        SET @EXISTE = 1
    ELSE
        SET @EXISTE = 0;
    
    -- Retornar como SELECT para DataTable
    SELECT @EXISTE AS EXISTE;
END
GO

-- =============================================
-- SP 5: Obtener almacenes por CD (DINÁMICO)
-- =============================================
IF OBJECT_ID('usp_Almacenes_Sel_MultiCD', 'P') IS NOT NULL
    DROP PROCEDURE usp_Almacenes_Sel_MultiCD;
GO

CREATE PROCEDURE usp_Almacenes_Sel_MultiCD
    @ID_CD INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Lógica dinámica según CD
    IF @ID_CD = 1  -- LIMA
    BEGIN
        SELECT ID_ALMACEN, DESCRIPCION
        FROM dbo.DM_ALMACEN
        WHERE ID_ALMACEN = '001'  -- ALM PRINCIPAL
        ORDER BY ID_ALMACEN;
    END
    ELSE IF @ID_CD = 2  -- ATE
    BEGIN
        SELECT ID_ALMACEN, DESCRIPCION
        FROM dbo.DM_ALMACEN
        WHERE ID_ALMACEN IN ('013', '020')  -- ATE y Calle Boulevard 1030
        ORDER BY ID_ALMACEN;
    END
    ELSE IF @ID_CD = 3  -- Otro CD (ejemplo)
    BEGIN
        SELECT ID_ALMACEN, DESCRIPCION
        FROM dbo.DM_ALMACEN
        WHERE ID_ALMACEN = '020'  -- Calle Boulevard 1030
        ORDER BY ID_ALMACEN;
    END
    ELSE
    BEGIN
        -- CD no reconocido - devolver vacío
        SELECT CAST(NULL AS VARCHAR(15)) AS ID_ALMACEN, CAST(NULL AS VARCHAR(100)) AS DESCRIPCION
        WHERE 1 = 0;  -- No retorna filas
    END
END
GO

-- =============================================
-- SCRIPTS DE PRUEBA
-- =============================================
/*
-- Probar SP 1: Cargar stock de ubicación
EXEC usp_CargarStockUbicacion @ID_ALMACEN = '013', @ID_UBICACION = 'A-01-01-01';

-- Probar SP 2: Buscar artículo
EXEC usp_BuscarArticuloEnUbicacion @ID_ALMACEN = '013', @ID_UBICACION = 'A-01-01-01', @ID_ARTICULO = '00001';

-- Probar SP 3: Validar ubicación
EXEC usp_ValidarUbicacionExiste @ID_ALMACEN = '013', @ID_UBICACION = 'A-01-01-01';

-- Probar SP 4: Validar lote
EXEC usp_ValidarLoteExisteParaArticulo @ID_ARTICULO = '00001', @ID_LOTE = 'LOTE001';

-- Probar SP 5: Almacenes por CD
EXEC usp_Almacenes_Sel_MultiCD @ID_CD = 1;  -- LIMA (001)
EXEC usp_Almacenes_Sel_MultiCD @ID_CD = 2;  -- ATE (013, 020)
EXEC usp_Almacenes_Sel_MultiCD @ID_CD = 3;  -- Otro (020)
*/

PRINT 'Stored Procedures creados exitosamente.';
GO
