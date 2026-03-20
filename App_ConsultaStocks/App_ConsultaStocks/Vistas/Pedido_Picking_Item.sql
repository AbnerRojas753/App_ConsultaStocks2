ALTER PROCEDURE alfa_usp_pedido_picking_det_InsDatos_Lotes_v2          
@ID_PICKING         int ,          
@ID_UBICACION       varchar(31),          
@ITEMPEDIDO         bigint,          
@ID_ARTICULO        varchar(31),          
@PICK               int,          
@ORDEN              INT,          
@UBIC_OK            BIT,          
@PICK_OK            BIT,          
@ID_UBICACION_NEW   VARCHAR(31),          
@ID_PICADOR         INT = 0,          
@ID_LOTE            VARCHAR(31) = '',          
@ID_LOTE_NEW        VARCHAR(31) = ''          
AS          
BEGIN          
    SET NOCOUNT ON;          
    DECLARE @DB_ACTUAL sysname = LOWER(DB_NAME());  
  
    -- Tabla temporal para stock disponible (excluye reservas del picking actual)          
    CREATE TABLE #TempStockDisp (          
        ID_ARTICULO VARCHAR(31),          
        ID_LOTE VARCHAR(31),          
        ID_UBICACION VARCHAR(500),          
        ID_ALMACEN VARCHAR(11),          
        STOCK_TOTAL INT,          
        STOCK_RESERVADO INT,          
        STOCK_DISPONIBLE INT,          
        FECHA_VENCIMIENTO DATE,          
        ESTADO_LOTE VARCHAR(10),          
        TIENE_STOCK_DISPONIBLE BIT          
    );          
    INSERT INTO #TempStockDisp EXEC sp_Stock_Disponible @ID_ALMACEN = NULL, @ID_PICKING_EXCLUIR = @ID_PICKING;  -- NULL para todos los almacenes, excluir este picking          
    -- ==========================================          
    -- VARIABLES DE CONFIGURACIÓN          
    -- ==========================================          
    DECLARE @ID_ALMACEN varchar(31) = CASE WHEN @DB_ACTUAL = 'alfa' THEN '013' ELSE '020' END;  
    DECLARE @APLICA_VALIDACION_ARTICULO bit =  
        CASE  
            WHEN @DB_ACTUAL = 'alfa' AND @ID_ARTICULO NOT LIKE 'T%' THEN 1  
            WHEN @DB_ACTUAL <> 'alfa' AND @ID_ARTICULO LIKE 'T%' THEN 1  
            ELSE 0  
        END;  
    DECLARE @OMITIR_VALIDACIONES bit =  
        CASE  
            WHEN UPPER(@ID_ARTICULO) LIKE '15%' OR UPPER(@ID_ARTICULO) LIKE 'T15%' THEN 1  
            ELSE 0  
        END;  
    -- ==========================================          
    -- VARIABLES PARA STOCK Y VALIDACIONES          
    -- ==========================================          
    DECLARE @STOCK_UBICACION_LOTE decimal(18,2) = 0;          
    DECLARE @STOCK_TOTAL_LOTE decimal(18,2) = 0;          
    DECLARE @STOCK_RESERVADO INT = 0;  
    DECLARE @SUGERENCIAS_TOP3 varchar(2000) = '';  
    DECLARE @SUG1_UBICACION VARCHAR(500) = NULL;
    DECLARE @SUG1_LOTE VARCHAR(31) = NULL;
    DECLARE @SUG1_STOCK INT = NULL;
    DECLARE @SUG2_UBICACION VARCHAR(500) = NULL;
    DECLARE @SUG2_LOTE VARCHAR(31) = NULL;
    DECLARE @SUG2_STOCK INT = NULL;
    DECLARE @SUG3_UBICACION VARCHAR(500) = NULL;
    DECLARE @SUG3_LOTE VARCHAR(31) = NULL;
    DECLARE @SUG3_STOCK INT = NULL;
    DECLARE @SUG4_UBICACION VARCHAR(500) = NULL;
    DECLARE @SUG4_LOTE VARCHAR(31) = NULL;
    DECLARE @SUG4_STOCK INT = NULL;
    DECLARE @SUG5_UBICACION VARCHAR(500) = NULL;
    DECLARE @SUG5_LOTE VARCHAR(31) = NULL;
    DECLARE @SUG5_STOCK INT = NULL;
    DECLARE @LT_ESTADO int;          
    DECLARE @LOTE_EXISTE_EN_ARTICULO bit = 0;          
    DECLARE @LOTE_EXISTE_EN_UBICACION bit = 0;          
    -- ==========================================          
    -- VARIABLES DE VALIDACIÓN - Compatibilidad con versión anterior          
    -- ==========================================          
    IF ISNULL(@ID_UBICACION_NEW, '') = ''
        SET @ID_UBICACION_NEW = @ID_UBICACION;

    IF ISNULL(@ID_LOTE_NEW, '') = ''
        SET @ID_LOTE_NEW = @ID_LOTE;

    DECLARE @VAL_UBICACION varchar(31) = UPPER(@ID_UBICACION_NEW);          
    DECLARE @VAL_LOTE varchar(31) = UPPER(@ID_LOTE_NEW);          
    DECLARE @PISO_BUSQUEDA varchar(31) = UPPER(@ID_UBICACION_NEW);  
    DECLARE @POS_PUNTO_1 int = CHARINDEX('.', @PISO_BUSQUEDA);  
    DECLARE @POS_PUNTO_2 int = CASE WHEN @POS_PUNTO_1 > 0 THEN CHARINDEX('.', @PISO_BUSQUEDA, @POS_PUNTO_1 + 1) ELSE 0 END;  
    IF @POS_PUNTO_1 > 0 AND @POS_PUNTO_2 > 0  
        SET @PISO_BUSQUEDA = LEFT(@PISO_BUSQUEDA, @POS_PUNTO_2 - 1);  
    DECLARE @APLICAR_FILTRO_PISO BIT = CASE WHEN ISNULL(@PISO_BUSQUEDA, '') = '' THEN 0 ELSE 1 END;
    -- -- ==========================================          
    -- -- VALIDACIÓN 0: Parámetros de entrada obligatorios          
    -- -- ==========================================          
    -- IF ISNULL(@ID_PICKING, 0) = 0          
    -- BEGIN          
    --     SELECT 'ERROR' AS OUT_STATUS, 'ID_PICKING es obligatorio' AS OUT_MESSAGE;          
    --     RETURN;          
    -- END          
    -- IF ISNULL(@ID_ARTICULO, '') = ''          
    -- BEGIN          
    --     SELECT 'ERROR' AS OUT_STATUS, 'ID_ARTICULO es obligatorio' AS OUT_MESSAGE;          
    --     RETURN;          
    -- END          
    -- IF @PICK <= 0          
    -- BEGIN          
    --     SELECT 'ERROR' AS OUT_STATUS, 'PICK debe ser mayor a 0' AS OUT_MESSAGE;          
    --     RETURN;          
    -- END          
    -- ==========================================          
    -- VALIDACIÓN 0: ID_UBICACION_NEW es obligatorio          
    -- ==========================================          
    IF ISNULL(@ID_UBICACION_NEW, '') = ''          
    BEGIN          
        SELECT 'ERROR' AS OUT_STATUS, 'LA UBICACION es obligatoria' AS OUT_MESSAGE,
               @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
               @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
               @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
               @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
               @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;          
        RETURN;          
    END          
    -- ==========================================          
    IF @APLICA_VALIDACION_ARTICULO = 1 AND @OMITIR_VALIDACIONES = 0  
    BEGIN          
    -- ==========================================          
    -- VALIDACIÓN REFACTORIZADA: Usar vw_Stock_Disponible          
    -- Una sola consulta reemplaza múltiples SELECTs a diferentes tablas          
    -- ==========================================          
    IF ISNULL(@VAL_LOTE, '') <> ''          
    BEGIN          
        -- Consultar SP centralizado de stock disponible (excluye reservas del picking actual)          
        SELECT          
            @STOCK_UBICACION_LOTE = SD.STOCK_DISPONIBLE,          
            @STOCK_TOTAL_LOTE = SD.STOCK_TOTAL,          
            @LT_ESTADO = SD.ESTADO_LOTE          
        FROM #TempStockDisp SD          
        WHERE SD.ID_ARTICULO = @ID_ARTICULO          
            AND upper(SD.ID_LOTE) = upper(@VAL_LOTE)          
            AND upper(SD.ID_UBICACION) = upper(@VAL_UBICACION)          
            AND SD.ID_ALMACEN = @ID_ALMACEN;          
  
        -- Sugerencias reutilizables (TOP 3) por piso para mensajes de error  
        SELECT @SUGERENCIAS_TOP3 = STUFF((  
            SELECT ' | ' + SX.ID_UBICACION + ' / LOTE ' + ISNULL(SX.ID_LOTE, '') + ' / STOCK ' + CAST(SX.STOCK_DISPONIBLE AS varchar(20))  
            FROM (  
                SELECT TOP 3 SD.ID_UBICACION, SD.ID_LOTE, SD.STOCK_DISPONIBLE  
                FROM #TempStockDisp SD  
                WHERE SD.ID_ALMACEN = @ID_ALMACEN  
                    AND SD.ID_ARTICULO = @ID_ARTICULO  
                    -- AND (@APLICAR_FILTRO_PISO = 0 OR UPPER(SD.ID_UBICACION) LIKE @PISO_BUSQUEDA + '%')  
                    AND SD.STOCK_DISPONIBLE > 0  
                ORDER BY SD.STOCK_DISPONIBLE DESC, SD.ID_UBICACION, SD.ID_LOTE  
            ) SX  
            FOR XML PATH(''), TYPE  
        ).value('.', 'varchar(max)'), 1, 3, '');  

        ;WITH Sugerencias AS (
            SELECT TOP 5
                SD.ID_UBICACION,
                SD.ID_LOTE,
                SD.STOCK_DISPONIBLE,
                ROW_NUMBER() OVER (ORDER BY SD.STOCK_DISPONIBLE DESC, SD.ID_UBICACION, SD.ID_LOTE) AS RN
            FROM #TempStockDisp SD
            WHERE SD.ID_ALMACEN = @ID_ALMACEN
              AND SD.ID_ARTICULO = @ID_ARTICULO
                            -- AND (@APLICAR_FILTRO_PISO = 0 OR UPPER(SD.ID_UBICACION) LIKE @PISO_BUSQUEDA + '%')
              AND SD.STOCK_DISPONIBLE > 0
        )
        SELECT
            @SUG1_UBICACION = MAX(CASE WHEN RN = 1 THEN ID_UBICACION END),
            @SUG1_LOTE = MAX(CASE WHEN RN = 1 THEN ID_LOTE END),
            @SUG1_STOCK = MAX(CASE WHEN RN = 1 THEN STOCK_DISPONIBLE END),
            @SUG2_UBICACION = MAX(CASE WHEN RN = 2 THEN ID_UBICACION END),
            @SUG2_LOTE = MAX(CASE WHEN RN = 2 THEN ID_LOTE END),
            @SUG2_STOCK = MAX(CASE WHEN RN = 2 THEN STOCK_DISPONIBLE END),
            @SUG3_UBICACION = MAX(CASE WHEN RN = 3 THEN ID_UBICACION END),
            @SUG3_LOTE = MAX(CASE WHEN RN = 3 THEN ID_LOTE END),
            @SUG3_STOCK = MAX(CASE WHEN RN = 3 THEN STOCK_DISPONIBLE END),
            @SUG4_UBICACION = MAX(CASE WHEN RN = 4 THEN ID_UBICACION END),
            @SUG4_LOTE = MAX(CASE WHEN RN = 4 THEN ID_LOTE END),
            @SUG4_STOCK = MAX(CASE WHEN RN = 4 THEN STOCK_DISPONIBLE END),
            @SUG5_UBICACION = MAX(CASE WHEN RN = 5 THEN ID_UBICACION END),
            @SUG5_LOTE = MAX(CASE WHEN RN = 5 THEN ID_LOTE END),
            @SUG5_STOCK = MAX(CASE WHEN RN = 5 THEN STOCK_DISPONIBLE END)
        FROM Sugerencias;
  
        -- VALIDACIÓN 1.1: ¿Existe el LOTE para este ARTICULO+UBICACION?          
        IF @STOCK_TOTAL_LOTE IS NULL          
        BEGIN          
            -- Diagnóstico detallado: ¿El lote existe pero no en esta ubicación?          
            IF EXISTS (          
                SELECT 1          
                FROM vw_Stock_Disponible_Por_Lote SDL WITH (NOLOCK)          
                WHERE SDL.ID_ARTICULO = @ID_ARTICULO          
            AND upper(SDL.ID_LOTE) = upper(@VAL_LOTE)          
                    AND SDL.ID_ALMACEN = @ID_ALMACEN          
            )          
            BEGIN          
                SELECT 'ERROR' AS OUT_STATUS,          
                    'LOTE [' + @VAL_LOTE + '] existe para ARTICULO [' + @ID_ARTICULO + '] pero NO en UBICACION [' + @VAL_UBICACION + ']'          
                    AS OUT_MESSAGE,
                    @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
                    @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
                    @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
                    @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
                    @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;          
            END          
            ELSE          
            BEGIN          
                SELECT 'ERROR' AS OUT_STATUS,          
                    'LOTE [' + @VAL_LOTE + '] NO EXISTE para ARTICULO [' + @ID_ARTICULO + '] en ALMACEN [' + @ID_ALMACEN + ']'          
                    AS OUT_MESSAGE,
                    @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
                    @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
                    @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
                    @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
                    @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;          
            END          
            RETURN;          
        END          
        -- VALIDACIÓN 1.2: ¿El LOTE está activo?          
        -- IF @LT_ESTADO IS NULL OR @LT_ESTADO <> 1          
        -- BEGIN          
        --     SELECT 'ERROR' AS OUT_STATUS,          
        --         'LOTE [' + @VAL_LOTE + '] NO ACTIVO o NO ENCONTRADO. Estado: ' +          
        --         ISNULL(CAST(@LT_ESTADO AS varchar(10)), 'NULL') +  
        --         CASE  
        --             WHEN ISNULL(@SUGERENCIAS_TOP3, '') <> ''  
        --             THEN '. Opciones en piso [' + @PISO_BUSQUEDA + ']: ' + @SUGERENCIAS_TOP3  
        --             ELSE ''  
        --         END  
        --         AS OUT_MESSAGE;          
        --     RETURN;          
        -- END          
        -- VALIDACIÓN 1.3: ¿Hay stock DISPONIBLE en la ubicación? (ya descontadas las reservas)          
        IF ISNULL(@STOCK_UBICACION_LOTE, 0) = 0          
        BEGIN          
        --print '1'  
            -- Consultar reservas EXCLUYENDO el picking actual          
            SELECT @STOCK_RESERVADO = ISNULL(SUM(CANTIDAD_RESERVADA), 0)          
            FROM vw_Articulos_Reservados_Picking WITH (NOLOCK)          
            WHERE ID_ARTICULO = @ID_ARTICULO          
                AND upper(ID_LOTE) = upper(@VAL_LOTE)          
                AND upper(ID_UBICACION) = upper(@VAL_UBICACION)          
                AND ID_ALMACEN = @ID_ALMACEN          
                AND ID_PICKING <> @ID_PICKING;  -- Excluir reservas propias del picking actual          
  
            SELECT 'ERROR' AS OUT_STATUS,          
                'NO HAY STOCK DISPONIBLE en UBICACION [' + @VAL_UBICACION + ']/LOTE [' + @VAL_LOTE + ']. ' +          
                'Stock Total: ' + CAST(ISNULL(@STOCK_TOTAL_LOTE, 0) AS varchar(20)) + ', ' +          
                'Reservado por otros: ' + CAST(ISNULL(@STOCK_RESERVADO, 0) AS varchar(20)) +  
                CASE  
                    WHEN ISNULL(@SUGERENCIAS_TOP3, '') <> ''  
                    THEN CASE WHEN @APLICAR_FILTRO_PISO = 1 THEN '. Opciones en piso [' + @PISO_BUSQUEDA + ']: ' + @SUGERENCIAS_TOP3 ELSE '. Opciones disponibles: ' + @SUGERENCIAS_TOP3 END  
                    ELSE ''  
                END  
                AS OUT_MESSAGE,
                @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
                @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
                @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
                @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
                @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;          
        RETURN;          
        END          
        -- VALIDACIÓN 1.4: ¿El stock disponible es suficiente para el PICK?          
        IF @STOCK_UBICACION_LOTE < @PICK          
        BEGIN         
        --print 'x2'   
            -- Consultar reservas EXCLUYENDO el picking actual          
            SELECT @STOCK_RESERVADO = ISNULL(SUM(CANTIDAD_RESERVADA), 0)          
            FROM vw_Articulos_Reservados_Picking WITH (NOLOCK)          
            WHERE ID_ARTICULO = @ID_ARTICULO          
                AND  upper(ID_LOTE) = upper(@VAL_LOTE)          
                AND upper(ID_UBICACION) = upper(@VAL_UBICACION)          
                AND ID_ALMACEN = @ID_ALMACEN          
                AND ID_PICKING <> @ID_PICKING;  -- Excluir reservas propias del picking actual          
  
            SELECT 'ERROR' AS OUT_STATUS,          
                'STOCK INSUFICIENTE en UBICACION [' + @VAL_UBICACION + ']/LOTE [' + @VAL_LOTE + ']. ' +          
                'Stock Total: ' + CAST(ISNULL(@STOCK_TOTAL_LOTE, 0) AS varchar(20)) + ', ' +          
                'Reservado por otros: ' + CAST(ISNULL(@STOCK_RESERVADO, 0) AS varchar(20)) + ', ' +          
                'Disponible: ' + CAST(@STOCK_UBICACION_LOTE AS varchar(20)) + ', ' +          
                'Se intenta picar: ' + CAST(@PICK AS varchar(20)) +  
                CASE  
                    WHEN ISNULL(@SUGERENCIAS_TOP3, '') <> ''  
                    THEN CASE WHEN @APLICAR_FILTRO_PISO = 1 THEN '. Opciones en piso [' + @PISO_BUSQUEDA + ']: ' + @SUGERENCIAS_TOP3 ELSE '. Opciones disponibles: ' + @SUGERENCIAS_TOP3 END  
                    ELSE ''  
                END  
                AS OUT_MESSAGE,
                @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
                @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
                @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
                @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
                @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;          
            RETURN;          
        END          
    END          
    IF ISNULL(@VAL_UBICACION, '') = ''          
    BEGIN          
        SELECT 'ERROR' AS OUT_STATUS,          
            'UBICACION no especificada y no hay ubicación sugerida disponible'          
            AS OUT_MESSAGE,
            @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
            @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
            @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
            @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
            @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;          
        RETURN;          
    END          
    -- ==========================================          
    -- VALIDACIÓN 4: Validar que la UBICACION existe          
    -- (usa vista local dbo.DM_UBICACIONES)          
    -- ==========================================          
    IF NOT EXISTS (  
        SELECT 1  
        FROM dbo.DM_UBICACIONES  
        WHERE ID_UBICACION = @VAL_UBICACION  
            AND ID_ALMACEN = @ID_ALMACEN  
    )  
    BEGIN  
        SELECT 'ERROR' AS OUT_STATUS,  
            'UBICACION [' + @VAL_UBICACION + '] NO EXISTE en ALMACEN [' + @ID_ALMACEN + ']'  
            AS OUT_MESSAGE,
            @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
            @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
            @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
            @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
            @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;  
        RETURN;  
    END  
  
    -- ==========================================          
    -- VALIDACIÓN 5: Validar que el ARTICULO existe          
    -- (usa vista local dbo.DM_ARTICULOS)          
    -- ==========================================          
    IF NOT EXISTS (  
        SELECT 1  
        FROM dbo.DM_ARTICULOS  
        WHERE ID_ARTICULO = @ID_ARTICULO  
    )  
    BEGIN  
        SELECT 'ERROR' AS OUT_STATUS,  
            'ARTICULO [' + @ID_ARTICULO + '] NO EXISTE'  
            AS OUT_MESSAGE,
            @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
            @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
            @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
            @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
            @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;  
        RETURN;  
    END  
    END          
    -- ==========================================          
    -- LÓGICA: Insertar o actualizar detalle de picking          
    -- ==========================================          
    IF (SELECT COUNT(0) FROM dbo.ALFA_TB_PEDIDO_PICKING_DET          
        WHERE ID_PICKING = @ID_PICKING          
          AND ORDEN = @ORDEN          
          AND ID_ARTICULO = @ID_ARTICULO          
          AND ID_UBICACION = @ID_UBICACION          
          AND ITEMPEDIDO = @ITEMPEDIDO) = 0          
    BEGIN          
       INSERT INTO dbo.ALFA_TB_PEDIDO_PICKING_DET          
            (ID_PICKING, ORDEN, ID_UBICACION, ITEMPEDIDO, ID_ARTICULO, PICK)          
       VALUES          
            (@ID_PICKING, @ORDEN, @ID_UBICACION, @ITEMPEDIDO, @ID_ARTICULO, @PICK)          
    END          
    ELSE          
    BEGIN          
       UPDATE dbo.ALFA_TB_PEDIDO_PICKING_DET          
       SET PICK = @PICK          
       WHERE ID_PICKING = @ID_PICKING          
         AND ID_UBICACION = @ID_UBICACION          
         AND ORDEN = @ORDEN          
         AND ITEMPEDIDO = @ITEMPEDIDO          
         AND ID_ARTICULO = @ID_ARTICULO          
    END          
    -- ==========================================          
    -- ACTUALIZAR RUTA DE PICKING          
    -- ==========================================          
    IF @ORDEN > 0          
    BEGIN          
       UPDATE ALFA_TB_PEDIDO_PICKING_RUTA          
       SET PICK = @PICK,          
           UBIC_OK = @UBIC_OK,          
           PICK_OK = @PICK_OK,          
           ID_UBICACION_NEW = @ID_UBICACION_NEW,          
           ID_LOTE_NEW = @ID_LOTE_NEW          
       WHERE ID_PICKING = @ID_PICKING          
         AND ORDEN = @ORDEN          
       INSERT INTO ALFA_TB_PEDIDO_PICKING_RUTA_EVENTOS          
            (ID_PICKING, ORDEN, ID_UBICACION, ID_ARTICULO, PICK, UBIC_OK, PICK_OK,          
             ID_UBICACION_NEW, ID_PICADOR, FECHA_HORA, ID_LOTE, ID_LOTE_NEW)          
       VALUES    
            (@ID_PICKING, @ORDEN, @ID_UBICACION, @ID_ARTICULO, @PICK, @UBIC_OK, @PICK_OK,          
             @ID_UBICACION_NEW, @ID_PICADOR, GETDATE(), @ID_LOTE, @ID_LOTE_NEW)          
 END          
    -- ==========================================          
    -- RESULTADO: Todo validado correctamente          
    -- ==========================================          
        SELECT 'OK' AS OUT_STATUS, 'Picking registrado exitosamente' AS OUT_MESSAGE,
            @SUG1_UBICACION AS SUG1_UBICACION, @SUG1_LOTE AS SUG1_LOTE, @SUG1_STOCK AS SUG1_STOCK,
            @SUG2_UBICACION AS SUG2_UBICACION, @SUG2_LOTE AS SUG2_LOTE, @SUG2_STOCK AS SUG2_STOCK,
            @SUG3_UBICACION AS SUG3_UBICACION, @SUG3_LOTE AS SUG3_LOTE, @SUG3_STOCK AS SUG3_STOCK,
            @SUG4_UBICACION AS SUG4_UBICACION, @SUG4_LOTE AS SUG4_LOTE, @SUG4_STOCK AS SUG4_STOCK,
            @SUG5_UBICACION AS SUG5_UBICACION, @SUG5_LOTE AS SUG5_LOTE, @SUG5_STOCK AS SUG5_STOCK;          
    -- Limpiar tabla temporal          
    DROP TABLE #TempStockDisp;          
END