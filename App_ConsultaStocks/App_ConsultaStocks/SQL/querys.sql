ALTER PROCEDURE [dbo].[usp_ResumenBultos_PorClienteDireccion_AmbasBD]              
(  
    @FechaReparto DATE  
)  
AS              
BEGIN              
    SET NOCOUNT ON;              
  
    ---------------------------------------------------------------------------        
    -- TABLA TEMPORAL PARA GUARDAR RESULTADOS DE AMBAS BASES        
    ---------------------------------------------------------------------------        
    CREATE TABLE #RESULTADOS (        
        FUENTE NVARCHAR(10),        
        IDCLIENTE NVARCHAR(50),        
        NOMBRE_CLIENTE NVARCHAR(200),        
        IDDIRECCION NVARCHAR(100),        
        COD_VEHICULO NVARCHAR(50),        
        MARCA_VEHICULO NVARCHAR(100),        
        PLACA_VEHICULO NVARCHAR(50),        
        TOTAL_DESPACHOS INT,        
        TOTAL_BULTOS INT,        
        TOTAL_BULTOSCARGADOS DECIMAL(10,2),        
        BULTOS_PENDIENTES INT,        
        TOTAL_BOLSAS INT,        
        TOTAL_CAJAS INT        
    );        
  
    ---------------------------------------------------------------------------        
    -- 🔹 1. RESULTADOS ALFA        
    ---------------------------------------------------------------------------        
    INSERT INTO #RESULTADOS (        
        IDCLIENTE, NOMBRE_CLIENTE, IDDIRECCION,        
        COD_VEHICULO, MARCA_VEHICULO, PLACA_VEHICULO,        
        TOTAL_DESPACHOS, TOTAL_BULTOS, TOTAL_BULTOSCARGADOS,        
        BULTOS_PENDIENTES, TOTAL_BOLSAS, TOTAL_CAJAS        
    )        
    EXEC dbo.usp_ResumenBultos_PorClienteDireccion @FechaReparto, 'ALFA';        
  
    UPDATE #RESULTADOS        
    SET FUENTE = 'ALFA'        
    WHERE FUENTE IS NULL;        
  
    ---------------------------------------------------------------------------        
    -- 🔹 2. RESULTADOS GPTOR        
    ---------------------------------------------------------------------------        
    INSERT INTO #RESULTADOS (        
        IDCLIENTE, NOMBRE_CLIENTE, IDDIRECCION,        
        COD_VEHICULO, MARCA_VEHICULO, PLACA_VEHICULO,        
        TOTAL_DESPACHOS, TOTAL_BULTOS, TOTAL_BULTOSCARGADOS,        
        BULTOS_PENDIENTES, TOTAL_BOLSAS, TOTAL_CAJAS        
    )        
    EXEC dbo.usp_ResumenBultos_PorClienteDireccion @FechaReparto, 'GPTOR';        
  
    UPDATE #RESULTADOS        
    SET FUENTE = 'GPTOR'        
    WHERE FUENTE IS NULL  
      AND IDCLIENTE IS NOT NULL;        
  
    ---------------------------------------------------------------------------        
    -- 🔹 RESULTADO FINAL DETALLADO        
    ---------------------------------------------------------------------------        
    SELECT         
        FUENTE,        
        IDCLIENTE,        
        NOMBRE_CLIENTE,        
        IDDIRECCION,        
        COD_VEHICULO,        
        MARCA_VEHICULO,        
        PLACA_VEHICULO,        
        TOTAL_DESPACHOS,        
        TOTAL_BULTOS,        
        TOTAL_BULTOSCARGADOS,        
        BULTOS_PENDIENTES,        
        TOTAL_BOLSAS,        
        TOTAL_CAJAS        
    FROM #RESULTADOS        
    ORDER BY COD_VEHICULO, NOMBRE_CLIENTE, IDDIRECCION;        
  
    ---------------------------------------------------------------------------        
    -- 🔹 RESUMEN AGRUPADO POR MARCA DE VEHÍCULO        
    ---------------------------------------------------------------------------        
    SELECT         
        MARCA_VEHICULO,    
        SUM(TOTAL_BULTOS) AS TOTAL_BULTOS,        
        SUM(TOTAL_BULTOSCARGADOS) AS TOTAL_CARGADOS,        
        SUM(BULTOS_PENDIENTES) AS PENDIENTES,        
        SUM(TOTAL_BOLSAS) AS TOTAL_BOLSAS,        
        SUM(TOTAL_CAJAS) AS TOTAL_CAJAS        
    FROM #RESULTADOS      
    GROUP BY MARCA_VEHICULO;        
  
END;  
go

alter PROCEDURE [dbo].[usp_ResumenBultos_PorClienteDireccion]              
(  
    @FechaReparto DATE,  
    @BaseDatos NVARCHAR(10) = 'ALFA'  
)  
AS              
BEGIN  
    SET NOCOUNT ON;  
  
    -------------------------------------------------------------------------  
    -- 🔹🔹🔹   LÓGICA GPTOR COMPLETA   🔹🔹🔹  
    -------------------------------------------------------------------------  
    IF @BaseDatos = 'GPTOR'  
    BEGIN  
          
        -------------------------------------------------------------------  
        -- PASO 1: PEDIDOS DEL DÍA  
        -------------------------------------------------------------------  
        SELECT   
            ped.IDCLIENTE,   
            rep.NroPedido  
        INTO #BULTOS_PEDREPARTOS_PEDIDOS_GPTOR              
        FROM GPTOR.dbo.ALFA_Pedidos_Repartos_Cab rep              
        INNER JOIN GPTOR.dbo.DIN_TB_CABECERA_PEDIDO ped   
            ON rep.NroPedido = ped.NroPedido              
        WHERE rep.FechaReparto = @FechaReparto;          
  
          
        -------------------------------------------------------------------  
        -- PASO 2: DESPACHOS FINALIZADOS  
        -------------------------------------------------------------------  
        SELECT DISTINCT  
            PED.IDCLIENTE,  
            CAB.ID_DESPACHO,  
            CAB.ID_CD  
        INTO #BULTOS_PEDREPARTOS_GPTOR  
        FROM GPTOR.dbo.ALFA_Pedidos_Repartos_Cab REP  
        INNER JOIN GPTOR.dbo.DIN_TB_CABECERA_PEDIDO PED   
            ON REP.NroPedido = PED.NroPedido              
        INNER JOIN GPTOR.dbo.ALFA_TB_PEDIDO_DESPACHO_GRUPOPEDIDOS GRUPO   
            ON REP.NroPedido = GRUPO.NroPedido              
        INNER JOIN GPTOR.dbo.ALFA_TB_PEDIDO_DESPACHO_CAB CAB   
            ON GRUPO.ID_DESPACHO = CAB.ID_DESPACHO              
        WHERE REP.FechaReparto = @FechaReparto  
          AND CAB.ID_CD = '2';  
  
  
        -------------------------------------------------------------------  
        -- PASO 3: DIRECCIÓN + VEHÍCULO POR DESPACHO  
        -------------------------------------------------------------------  
        SELECT DISTINCT   
            D.IDCLIENTE,  
            C.ID_DIRECCION AS IDDIRECCION,  
            D.ID_DESPACHO,  
            C.Vehiculo AS COD_VEHICULO,  
            VEH.Campo1 AS MARCA_VEHICULO,  
            VEH.Campo2 AS PLACA_VEHICULO  
        INTO #PEDIDOS_CON_DIRECCION_GPTOR  
        FROM #BULTOS_PEDREPARTOS_GPTOR D  
        INNER JOIN GPTOR.dbo.ALFA_TB_PEDIDO_DESPACHO_GRUPOPEDIDOS GP  
            ON GP.ID_DESPACHO = D.ID_DESPACHO  
        INNER JOIN GPTOR.dbo.ALFA_Pedidos_Repartos_Cab C  
            ON C.NroPedido = GP.NroPedido  
        INNER JOIN GPTOR.dbo.DIN_TB_CABECERA_PEDIDO PED   
            ON C.NroPedido = PED.NroPedido  
        LEFT JOIN GPTOR.dbo.ALFA_TB_DATOSAUXILIARES VEH   
            ON VEH.Id = C.Vehiculo   
            AND VEH.Concepto = 'VEHICULOS'  
        WHERE C.FechaReparto = @FechaReparto  
            AND PED.IDCLIENTE NOT IN ('CL000001');  
  
  
        -------------------------------------------------------------------  
        -- VALIDACIÓN  
        -------------------------------------------------------------------  
        IF EXISTS (  
            SELECT IDCLIENTE, IDDIRECCION  
            FROM #PEDIDOS_CON_DIRECCION_GPTOR  
            GROUP BY IDCLIENTE, IDDIRECCION  
            HAVING COUNT(DISTINCT COD_VEHICULO) > 1  
        )  
        BEGIN  
            DECLARE @ClienteG VARCHAR(20), @DireccionG VARCHAR(50);        
            SELECT TOP 1  
                @ClienteG = IDCLIENTE,  
                @DireccionG = IDDIRECCION  
            FROM (  
                SELECT IDCLIENTE, IDDIRECCION  
                FROM #PEDIDOS_CON_DIRECCION_GPTOR  
                GROUP BY IDCLIENTE, IDDIRECCION  
                HAVING COUNT(DISTINCT COD_VEHICULO) > 1  
            ) X;  
  
            RAISERROR(  
                'ERROR GPTOR: El cliente %s en la dirección %s tiene más de un vehículo asignado.',   
                16, 1, @ClienteG, @DireccionG  
         );  
            RETURN;  
        END;  
  
  
        -------------------------------------------------------------------  
        -- PASO 4: RESUMEN POR DESPACHO  
        -------------------------------------------------------------------  
        SELECT   
            D.ID_DESPACHO,  
            SUM(CAST(B.CANTIDAD AS INT)) AS TOTAL_BULTOS,  
            SUM(CASE WHEN B.ID_TIPOBULTO = 2 THEN CAST(B.CANTIDAD AS INT) ELSE 0 END) AS TOTAL_CAJAS,  
            SUM(CASE WHEN B.ID_TIPOBULTO = 1 THEN CAST(B.CANTIDAD AS INT) ELSE 0 END) AS TOTAL_BOLSAS  
        INTO #BULTOS_POR_DESPACHO_GPTOR  
        FROM #BULTOS_PEDREPARTOS_GPTOR D  
        INNER JOIN GPTOR.dbo.ALFA_TB_PEDIDO_DESPACHO_BULTOS B  
            ON B.ID_DESPACHO = D.ID_DESPACHO  
        GROUP BY D.ID_DESPACHO;  
  
        -- 🔹 CORREGIDO: Ahora filtra por FECHA + VEHICULO para evitar duplicaciones  
        SELECT   
            BC.ID_DESPACHO,  
            BC.COD_VEHICULO,  
            COUNT(*) AS TOTAL_BULTOS_CARGADOS  
        INTO #BULTOS_CARGADOS_GPTOR  
        FROM GPTOR.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA BC  
        WHERE CAST(BC.FECHA_REPARTO AS DATE) = @FechaReparto  
        GROUP BY BC.ID_DESPACHO, BC.COD_VEHICULO;  
  
        SELECT DISTINCT  
            IDCLIENTE,  
            IDDIRECCION,  
            COD_VEHICULO,  
            MARCA_VEHICULO,  
            PLACA_VEHICULO,  
            ID_DESPACHO  
        INTO #AGRUPACION_GPTOR  
        FROM #PEDIDOS_CON_DIRECCION_GPTOR;  
  
        SELECT  
            A.IDCLIENTE,  
            CLI.CUSTNAME AS NOMBRE_CLIENTE,  
            A.IDDIRECCION,  
            A.COD_VEHICULO,  
            A.MARCA_VEHICULO,  
            A.PLACA_VEHICULO,  
            COUNT(DISTINCT A.ID_DESPACHO) AS TOTAL_DESPACHOS,  
            SUM(ISNULL(B.TOTAL_BULTOS,0)) AS TOTAL_BULTOS,  
            SUM(ISNULL(C.TOTAL_BULTOS_CARGADOS,0)) AS TOTAL_BULTOS_CARGADOS,  
            SUM(ISNULL(B.TOTAL_BULTOS,0)) - SUM(ISNULL(C.TOTAL_BULTOS_CARGADOS,0)) AS BULTOS_PENDIENTES,  
            SUM(ISNULL(B.TOTAL_BOLSAS,0)) AS TOTAL_BOLSAS,  
            SUM(ISNULL(B.TOTAL_CAJAS,0)) AS TOTAL_CAJAS  
        FROM #AGRUPACION_GPTOR A  
        LEFT JOIN #BULTOS_POR_DESPACHO_GPTOR B ON A.ID_DESPACHO = B.ID_DESPACHO  
        LEFT JOIN #BULTOS_CARGADOS_GPTOR C   
            ON A.ID_DESPACHO = C.ID_DESPACHO   
            AND A.COD_VEHICULO = C.COD_VEHICULO  -- 🔹 JOIN POR VEHICULO  
        LEFT JOIN GPTOR.dbo.RM00101 CLI ON A.IDCLIENTE = CLI.CUSTNMBR  
        GROUP BY  
            A.IDCLIENTE, CLI.CUSTNAME, A.IDDIRECCION,  
            A.COD_VEHICULO, A.MARCA_VEHICULO, A.PLACA_VEHICULO  
        ORDER BY  
            A.PLACA_VEHICULO, CLI.CUSTNAME, A.IDCLIENTE, A.IDDIRECCION;  
  
        DROP TABLE #BULTOS_PEDREPARTOS_PEDIDOS_GPTOR;  
        DROP TABLE #BULTOS_PEDREPARTOS_GPTOR;  
        DROP TABLE #PEDIDOS_CON_DIRECCION_GPTOR;  
        DROP TABLE #BULTOS_POR_DESPACHO_GPTOR;  
        DROP TABLE #BULTOS_CARGADOS_GPTOR;  
        DROP TABLE #AGRUPACION_GPTOR;  
  
        RETURN;  
    END  
  
  
    -------------------------------------------------------------------------  
    -- 🔹🔹🔹   LÓGICA ALFA COMPLETA (ORIGINAL TUYA)   🔹🔹🔹  
    -------------------------------------------------------------------------  
    ELSE  
    BEGIN  
  
        -------------------------------------------------------------------  
        -- PASO 1: PEDIDOS DEL DÍA  
        -------------------------------------------------------------------  
        SELECT   
            ped.IDCLIENTE,   
            rep.NroPedido  
        INTO #BULTOS_PEDREPARTOS_PEDIDOS              
        FROM ALFA.dbo.ALFA_Pedidos_Repartos_Cab rep              
        INNER JOIN ALFA.dbo.DIN_TB_CABECERA_PEDIDO ped   
            ON rep.NroPedido = ped.NroPedido              
        WHERE rep.FechaReparto = @FechaReparto;          
  
          
        -------------------------------------------------------------------  
        -- PASO 2: DESPACHOS FINALIZADOS  
        -------------------------------------------------------------------  
  SELECT DISTINCT  
            PED.IDCLIENTE,  
            CAB.ID_DESPACHO,  
            CAB.ID_CD  
        INTO #BULTOS_PEDREPARTOS  
        FROM ALFA.dbo.ALFA_Pedidos_Repartos_Cab REP  
        INNER JOIN ALFA.dbo.DIN_TB_CABECERA_PEDIDO PED   
            ON REP.NroPedido = PED.NroPedido              
        INNER JOIN ALFA.dbo.ALFA_TB_PEDIDO_DESPACHO_GRUPOPEDIDOS GRUPO   
            ON REP.NroPedido = GRUPO.NroPedido              
        INNER JOIN ALFA.dbo.ALFA_TB_PEDIDO_DESPACHO_CAB CAB   
            ON GRUPO.ID_DESPACHO = CAB.ID_DESPACHO              
        WHERE REP.FechaReparto = @FechaReparto  
          AND CAB.ID_CD = '2';  
  
  
        -------------------------------------------------------------------  
        -- PASO 3: DIRECCIÓN + VEHÍCULO POR DESPACHO  
        -------------------------------------------------------------------  
        SELECT DISTINCT   
            D.IDCLIENTE,  
            C.ID_DIRECCION AS IDDIRECCION,  
            D.ID_DESPACHO,  
            C.Vehiculo AS COD_VEHICULO,  
            VEH.Campo1 AS MARCA_VEHICULO,  
            VEH.Campo2 AS PLACA_VEHICULO  
        INTO #PEDIDOS_CON_DIRECCION  
        FROM #BULTOS_PEDREPARTOS D  
        INNER JOIN ALFA.dbo.ALFA_TB_PEDIDO_DESPACHO_GRUPOPEDIDOS GP  
            ON GP.ID_DESPACHO = D.ID_DESPACHO  
        INNER JOIN ALFA.dbo.ALFA_Pedidos_Repartos_Cab C  
            ON C.NroPedido = GP.NroPedido  
        INNER JOIN ALFA.dbo.DIN_TB_CABECERA_PEDIDO PED   
            ON C.NroPedido = PED.NroPedido  
        LEFT JOIN ALFA.dbo.ALFA_TB_DATOSAUXILIARES VEH   
            ON VEH.Id = C.Vehiculo   
            AND VEH.Concepto = 'VEHICULOS'  
        WHERE C.FechaReparto = @FechaReparto  
            AND PED.IDCLIENTE NOT IN ('CL000001');  
  
  
        -------------------------------------------------------------------  
        -- VALIDACIÓN  
        -------------------------------------------------------------------  
        IF EXISTS (  
            SELECT IDCLIENTE, IDDIRECCION  
            FROM #PEDIDOS_CON_DIRECCION  
            GROUP BY IDCLIENTE, IDDIRECCION  
            HAVING COUNT(DISTINCT COD_VEHICULO) > 1  
        )  
        BEGIN  
            DECLARE @ClienteA VARCHAR(20), @DireccionA VARCHAR(50);        
            SELECT TOP 1  
                @ClienteA = IDCLIENTE,  
                @DireccionA = IDDIRECCION  
            FROM (  
                SELECT IDCLIENTE, IDDIRECCION  
                FROM #PEDIDOS_CON_DIRECCION  
                GROUP BY IDCLIENTE, IDDIRECCION  
                HAVING COUNT(DISTINCT COD_VEHICULO) > 1  
            ) X;  
  
            RAISERROR(  
                'ERROR ALFA: El cliente %s en la dirección %s tiene más de un vehículo asignado.',   
                16, 1, @ClienteA, @DireccionA  
            );  
            RETURN;  
        END;  
  
  
        -------------------------------------------------------------------  
        -- PASO 4: RESUMEN POR DESPACHO  
        -------------------------------------------------------------------  
        SELECT   
            D.ID_DESPACHO,  
            SUM(CAST(B.CANTIDAD AS INT)) AS TOTAL_BULTOS,  
            SUM(CASE WHEN B.ID_TIPOBULTO = 2 THEN CAST(B.CANTIDAD AS INT) ELSE 0 END) AS TOTAL_CAJAS,  
            SUM(CASE WHEN B.ID_TIPOBULTO = 1 THEN CAST(B.CANTIDAD AS INT) ELSE 0 END) AS TOTAL_BOLSAS  
        INTO #BULTOS_POR_DESPACHO  
        FROM #BULTOS_PEDREPARTOS D  
        INNER JOIN ALFA.dbo.ALFA_TB_PEDIDO_DESPACHO_BULTOS B  
            ON B.ID_DESPACHO = D.ID_DESPACHO  
        GROUP BY D.ID_DESPACHO;  
  
        -- 🔹 CORREGIDO: Ahora filtra por FECHA + VEHICULO para evitar duplicaciones  
        SELECT   
            BC.ID_DESPACHO,  
            BC.COD_VEHICULO,  
            COUNT(*) AS TOTAL_BULTOS_CARGADOS  
        INTO #BULTOS_CARGADOS  
        FROM ALFA.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA BC  
        WHERE CAST(BC.FECHA_REPARTO AS DATE) = @FechaReparto  
        GROUP BY BC.ID_DESPACHO, BC.COD_VEHICULO;  
  
        SELECT DISTINCT  
            IDCLIENTE,  
            IDDIRECCION,  
 COD_VEHICULO,  
            MARCA_VEHICULO,  
            PLACA_VEHICULO,  
            ID_DESPACHO  
        INTO #AGRUPACION  
        FROM #PEDIDOS_CON_DIRECCION;  
  
        SELECT  
            A.IDCLIENTE,  
            CLI.CUSTNAME AS NOMBRE_CLIENTE,  
            A.IDDIRECCION,  
            A.COD_VEHICULO,  
            A.MARCA_VEHICULO,  
            A.PLACA_VEHICULO,  
            COUNT(DISTINCT A.ID_DESPACHO) AS TOTAL_DESPACHOS,  
            SUM(ISNULL(B.TOTAL_BULTOS,0)) AS TOTAL_BULTOS,  
            SUM(ISNULL(C.TOTAL_BULTOS_CARGADOS,0)) AS TOTAL_BULTOS_CARGADOS,  
            SUM(ISNULL(B.TOTAL_BULTOS,0)) - SUM(ISNULL(C.TOTAL_BULTOS_CARGADOS,0)) AS BULTOS_PENDIENTES,  
            SUM(ISNULL(B.TOTAL_BOLSAS,0)) AS TOTAL_BOLSAS,  
            SUM(ISNULL(B.TOTAL_CAJAS,0)) AS TOTAL_CAJAS  
        FROM #AGRUPACION A  
        LEFT JOIN #BULTOS_POR_DESPACHO B ON A.ID_DESPACHO = B.ID_DESPACHO  
        LEFT JOIN #BULTOS_CARGADOS C   
            ON A.ID_DESPACHO = C.ID_DESPACHO   
            AND A.COD_VEHICULO = C.COD_VEHICULO  -- 🔹 JOIN POR VEHICULO  
        LEFT JOIN ALFA.dbo.RM00101 CLI ON A.IDCLIENTE = CLI.CUSTNMBR  
        GROUP BY  
            A.IDCLIENTE, CLI.CUSTNAME, A.IDDIRECCION,  
            A.COD_VEHICULO, A.MARCA_VEHICULO, A.PLACA_VEHICULO  
        ORDER BY  
            A.PLACA_VEHICULO, CLI.CUSTNAME, A.IDCLIENTE, A.IDDIRECCION;  
  
        DROP TABLE #BULTOS_PEDREPARTOS_PEDIDOS;  
        DROP TABLE #BULTOS_PEDREPARTOS;  
        DROP TABLE #PEDIDOS_CON_DIRECCION;  
        DROP TABLE #BULTOS_POR_DESPACHO;  
        DROP TABLE #BULTOS_CARGADOS;  
        DROP TABLE #AGRUPACION;  
  
        RETURN;  
    END  
  
END;  
  

go

---------------------------------------------------------------------
-- Manejo de sesiones e inserción de bultos vía SP
---------------------------------------------------------------------

CREATE OR ALTER PROCEDURE dbo.usp_SesionCarga_ObtenerActiva
(
    @BaseDatos     NVARCHAR(10),
    @IDCLIENTE     NVARCHAR(50),
    @ID_DIRECCION  NVARCHAR(100),
    @FECHA_REPARTO DATE,
    @COD_VEHICULO  NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF UPPER(@BaseDatos) = 'GPTOR'
    BEGIN
        SELECT TOP 1 ID_SESION, USUARIO_INICIO, FECHA_INICIO
        FROM GPTOR.dbo.ALFA_TB_DESPACHO_CARGA_SESION
        WHERE IDCLIENTE = @IDCLIENTE
          AND ID_DIRECCION = @ID_DIRECCION
          AND FECHA_REPARTO = @FECHA_REPARTO
          AND COD_VEHICULO = @COD_VEHICULO
          AND ESTADO = 'INICIADO'
        ORDER BY FECHA_INICIO DESC;
    END
    ELSE
    BEGIN
        SELECT TOP 1 ID_SESION, USUARIO_INICIO, FECHA_INICIO
        FROM ALFA.dbo.ALFA_TB_DESPACHO_CARGA_SESION
        WHERE IDCLIENTE = @IDCLIENTE
          AND ID_DIRECCION = @ID_DIRECCION
          AND FECHA_REPARTO = @FECHA_REPARTO
          AND COD_VEHICULO = @COD_VEHICULO
          AND ESTADO = 'INICIADO'
        ORDER BY FECHA_INICIO DESC;
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SesionCarga_Crear
(
    @BaseDatos     NVARCHAR(10),
    @IDCLIENTE     NVARCHAR(50),
    @ID_DIRECCION  NVARCHAR(100),
    @FECHA_REPARTO DATE,
    @USUARIO       NVARCHAR(50),
    @TOTAL_BULTOS  INT,
    @COD_VEHICULO  NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NuevoId INT;

    IF UPPER(@BaseDatos) = 'GPTOR'
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM GPTOR.INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'ALFA_TB_DESPACHO_CARGA_SESION'
        )
        BEGIN
            RAISERROR('La tabla ALFA_TB_DESPACHO_CARGA_SESION no existe en GPTOR.', 16, 1);
            RETURN;
        END;

        INSERT INTO GPTOR.dbo.ALFA_TB_DESPACHO_CARGA_SESION
            (IDCLIENTE, ID_DIRECCION, FECHA_REPARTO, FECHA_INICIO, USUARIO_INICIO, ESTADO, TOTAL_BULTOS, TOTAL_CARGADOS, COD_VEHICULO)
        VALUES (@IDCLIENTE, @ID_DIRECCION, @FECHA_REPARTO, GETDATE(), @USUARIO, 'INICIADO', @TOTAL_BULTOS, 0, @COD_VEHICULO);

        SET @NuevoId = SCOPE_IDENTITY();
        SELECT 'OK' AS STATUS, @NuevoId AS ID_SESION;
    END
    ELSE
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM ALFA.INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'ALFA_TB_DESPACHO_CARGA_SESION'
        )
        BEGIN
            RAISERROR('La tabla ALFA_TB_DESPACHO_CARGA_SESION no existe en ALFA.', 16, 1);
            RETURN;
        END;

        INSERT INTO ALFA.dbo.ALFA_TB_DESPACHO_CARGA_SESION
            (IDCLIENTE, ID_DIRECCION, FECHA_REPARTO, FECHA_INICIO, USUARIO_INICIO, ESTADO, TOTAL_BULTOS, TOTAL_CARGADOS, COD_VEHICULO)
        VALUES (@IDCLIENTE, @ID_DIRECCION, @FECHA_REPARTO, GETDATE(), @USUARIO, 'INICIADO', @TOTAL_BULTOS, 0, @COD_VEHICULO);

        SET @NuevoId = SCOPE_IDENTITY();
        SELECT 'OK' AS STATUS, @NuevoId AS ID_SESION;
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_BultoCargado_Registrar
(
    @BaseDatos     NVARCHAR(10),
    @ID_DESPACHO   INT,
    @ID_BULTO      INT,
    @ID_SESION     INT,
    @FECHA_REPARTO DATE,
    @USUARIO       NVARCHAR(50),
    @COD_VEHICULO  NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF UPPER(@BaseDatos) = 'GPTOR'
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM GPTOR.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA
            WHERE ID_DESPACHO = @ID_DESPACHO
              AND ID_BULTO = @ID_BULTO
              AND FECHA_REPARTO = @FECHA_REPARTO
              AND COD_VEHICULO = @COD_VEHICULO
        )
        BEGIN
            SELECT 'DUPLICADO' AS STATUS;
            RETURN;
        END;

        INSERT INTO GPTOR.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA
            (ID_DESPACHO, ID_BULTO, ID_SESION, FECHAHORA, FECHA_REPARTO, USUARIO, COD_VEHICULO)
        VALUES (@ID_DESPACHO, @ID_BULTO, @ID_SESION, GETDATE(), @FECHA_REPARTO, @USUARIO, @COD_VEHICULO);

        UPDATE GPTOR.dbo.ALFA_TB_DESPACHO_CARGA_SESION
        SET TOTAL_CARGADOS = TOTAL_CARGADOS + 1
        WHERE ID_SESION = @ID_SESION;
    END
    ELSE
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM ALFA.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA
            WHERE ID_DESPACHO = @ID_DESPACHO
              AND ID_BULTO = @ID_BULTO
              AND FECHA_REPARTO = @FECHA_REPARTO
              AND COD_VEHICULO = @COD_VEHICULO
        )
        BEGIN
            SELECT 'DUPLICADO' AS STATUS;
            RETURN;
        END;

        INSERT INTO ALFA.dbo.ALFA_TB_DESPACHO_BULTOS_CARGA
            (ID_DESPACHO, ID_BULTO, ID_SESION, FECHAHORA, FECHA_REPARTO, USUARIO, COD_VEHICULO)
        VALUES (@ID_DESPACHO, @ID_BULTO, @ID_SESION, GETDATE(), @FECHA_REPARTO, @USUARIO, @COD_VEHICULO);

        UPDATE ALFA.dbo.ALFA_TB_DESPACHO_CARGA_SESION
        SET TOTAL_CARGADOS = TOTAL_CARGADOS + 1
        WHERE ID_SESION = @ID_SESION;
    END;

    SELECT 'OK' AS STATUS;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SesionCarga_Finalizar
(
    @BaseDatos NVARCHAR(10),
    @ID_SESION INT,
    @USUARIO   NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF UPPER(@BaseDatos) = 'GPTOR'
    BEGIN
        UPDATE GPTOR.dbo.ALFA_TB_DESPACHO_CARGA_SESION
        SET FECHA_FIN = GETDATE(),
            USUARIO_FIN = @USUARIO,
            ESTADO = 'FINALIZADO'
        WHERE ID_SESION = @ID_SESION;
    END
    ELSE
    BEGIN
        UPDATE ALFA.dbo.ALFA_TB_DESPACHO_CARGA_SESION
        SET FECHA_FIN = GETDATE(),
            USUARIO_FIN = @USUARIO,
            ESTADO = 'FINALIZADO'
        WHERE ID_SESION = @ID_SESION;
    END;

    SELECT 'OK' AS STATUS;
END;
GO
