#Requires -Version 5.1
<#
.SYNOPSIS
    Script de build automatizado para App_ConsultaStocks (Android)
    Compila, firma y genera el APK listo para distribucion

.DESCRIPTION
    Este script automatiza todo el proceso de compilacion:
    1. Restaura paquetes NuGet
    2. Limpia builds anteriores
    3. Incrementa automaticamente el versionCode
    4. Compila en Release
    5. Firma el APK con el keystore
    6. Copia el APK final a la carpeta de salida

.USAGE
    PowerShell: .\build-android.ps1
    O simplemente hacer doble clic en el archivo

.REQUISITOS
    - Visual Studio 2019/2022 (o Build Tools)
    - MSBuild en el PATH
    - Java JDK instalado
    - Keystore configurado en build-config.psd1
#>

# Configuracion de codificacion
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# =============================================================================
# CONFIGURACION - Modifica esto segun tu entorno
# =============================================================================

$Config = @{
    # Rutas del proyecto (ajustalas a tu PC)
    ProjectRoot    = "C:\devEmpresa\App_ConsultaStocks2"  # Ruta del proyecto
    SolutionFile   = "App_ConsultaStocks2.sln"
    AndroidProject = "App_ConsultaStocks\App_ConsultaStocks.Android\App_ConsultaStocks.Android.csproj"
    
    # Configuracion del build
    Configuration  = "Release"
    Platform       = "AnyCPU"
    
    # Keystore
    # Opcion 1: DEBUG - Usa el keystore por defecto de Android (no pide password)
    # Opcion 2: PRODUCCION - Usa tu propio keystore (el script te preguntara la password)
    KeystorePath   = "$env:USERPROFILE\.android\debug.keystore"
    KeystorePass   = "android"  # Si usas debug.keystore, dejar asi
    KeyAlias       = "androiddebugkey"
    KeyPass        = "android"  # Si usas debug.keystore, dejar asi
    AskForPassword = $false     # Cambiar a $true si quieres que pregunte la password
    
    # Opciones de build
    IncrementVersionCode = $false   # Incrementar versionCode automaticamente (manual para evitar conflictos git)
    BuildAAB            = $false    # Generar tambien AAB (para Play Store)
    
    # Carpeta de salida
    OutputDirectory     = "C:\Builds\PDA"  # <-- CAMBIA ESTO
}

# =============================================================================
# FUNCIONES AUXILIARES
# =============================================================================

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-ErrorMsg {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-WarningMsg {
    param([string]$Message)
    Write-Host "[ADVERTENCIA] $Message" -ForegroundColor Yellow
}

function Test-Prerequisites {
    Write-Header "VERIFICANDO REQUISITOS"
    
    # Verificar MSBuild
    $msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
    if (-not $msbuild) {
        # Buscar MSBuild en ubicaciones comunes
        $possiblePaths = @(
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe"
        )
        
        foreach ($path in $possiblePaths) {
            if (Test-Path $path) {
                $script:MSBuildPath = $path
                Write-Success "MSBuild encontrado en: $path"
                break
            }
        }
        
        if (-not $script:MSBuildPath) {
            Write-ErrorMsg "No se encontro MSBuild. Tienes Visual Studio instalado?"
            exit 1
        }
    } else {
        $script:MSBuildPath = $msbuild.Source
        Write-Success "MSBuild encontrado en PATH"
    }
    
    # Verificar NuGet
    $nuget = Get-Command nuget -ErrorAction SilentlyContinue
    if (-not $nuget) {
        # Intentar encontrar nuget.exe
        $nugetPaths = @(
            "${env:ProgramFiles}\NuGet\nuget.exe",
            "${env:LOCALAPPDATA}\Microsoft\VisualStudio\Packages\_nuget\nuget.exe"
        )
        foreach ($path in $nugetPaths) {
            if (Test-Path $path) {
                $script:NuGetPath = $path
                Write-Success "NuGet encontrado en: $path"
                break
            }
        }
        
        if (-not $script:NuGetPath) {
            Write-WarningMsg "NuGet no encontrado. Intentando usar restauracion integrada..."
            $script:NuGetPath = $null
        }
    } else {
        $script:NuGetPath = $nuget.Source
    }
    
    # Verificar existencia del proyecto
    $fullProjectPath = Join-Path $Config.ProjectRoot $Config.AndroidProject
    if (-not (Test-Path $fullProjectPath)) {
        Write-ErrorMsg "No se encontro el proyecto en: $fullProjectPath"
        Write-Host "Verifica la ruta en la configuracion (ProjectRoot)" -ForegroundColor Yellow
        exit 1
    }
    Write-Success "Proyecto encontrado: $($Config.AndroidProject)"
    
    # Verificar keystore si esta configurado
    if ($Config.KeystorePath -and -not (Test-Path $Config.KeystorePath)) {
        Write-WarningMsg "Keystore no encontrado en: $($Config.KeystorePath)"
        Write-Host "El APK se generara pero NO estara firmado para distribucion" -ForegroundColor Yellow
    }
}

function Invoke-VersionIncrement {
    Write-Header "ACTUALIZANDO VERSION"
    
    $manifestPath = Join-Path $Config.ProjectRoot "App_ConsultaStocks\App_ConsultaStocks.Android\Properties\AndroidManifest.xml"
    
    if (-not (Test-Path $manifestPath)) {
        Write-WarningMsg "No se encontro AndroidManifest.xml"
        return
    }
    
    [xml]$manifest = Get-Content $manifestPath
    $currentVersionCode = [int]$manifest.manifest.versionCode
    $currentVersionName = $manifest.manifest.versionName
    
    Write-Host "Version actual: $currentVersionName (code: $currentVersionCode)" -ForegroundColor Gray
    
    if ($Config.IncrementVersionCode) {
        $newVersionCode = $currentVersionCode + 1
        $manifest.manifest.versionCode = $newVersionCode.ToString()
        
        $manifest.Save($manifestPath)
        Write-Success "VersionCode actualizado: $currentVersionCode -> $newVersionCode"
    } else {
        Write-Host "Incremento de version desactivado" -ForegroundColor Gray
    }
}

function Invoke-NuGetRestore {
    Write-Header "RESTAURANDO PAQUETES NUGET"
    
    $solutionPath = Join-Path $Config.ProjectRoot $Config.SolutionFile
    
    if ($script:NuGetPath) {
        & $script:NuGetPath restore $solutionPath
    } else {
        # Usar MSBuild para restaurar
        & $script:MSBuildPath $solutionPath /t:Restore /p:Configuration=$($Config.Configuration) /v:quiet
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Paquetes NuGet restaurados"
    } else {
        Write-ErrorMsg "Error al restaurar paquetes NuGet"
        exit 1
    }
}

function Invoke-CleanBuild {
    Write-Header "LIMPIANDO BUILDS ANTERIORES"
    
    $projectPath = Join-Path $Config.ProjectRoot $Config.AndroidProject
    
    & $script:MSBuildPath $projectPath `
        /t:Clean `
        /p:Configuration=$($Config.Configuration) `
        /p:Platform=$($Config.Platform) `
        /v:quiet
    
    # Limpiar carpetas bin y obj manualmente tambien
    $androidProjectDir = Split-Path $projectPath -Parent
    $foldersToClean = @("bin", "obj")
    foreach ($folder in $foldersToClean) {
        $fullPath = Join-Path $androidProjectDir $folder
        if (Test-Path $fullPath) {
            Remove-Item $fullPath -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    
    Write-Success "Builds anteriores limpiados"
}

function Invoke-BuildAPK {
    Write-Header "COMPILANDO APK"
    
    $projectPath = Join-Path $Config.ProjectRoot $Config.AndroidProject
    
    # Parametros base
    $buildParams = @(
        $projectPath
        "/t:SignAndroidPackage"
        "/p:Configuration=$($Config.Configuration)"
        "/p:Platform=$($Config.Platform)"
        "/p:AndroidPackageFormat=apk"
        "/p:EmbedAssembliesIntoApk=true"
        "/p:AndroidUseSharedRuntime=false"
        "/v:minimal"
    )
    
    # Agregar firma si tenemos keystore
    if ($Config.KeystorePath -and (Test-Path $Config.KeystorePath)) {
        $buildParams += "/p:AndroidKeyStore=true"
        $buildParams += "/p:AndroidSigningKeyStore=$($Config.KeystorePath)"
        $buildParams += "/p:AndroidSigningStorePass=$($Config.KeystorePass)"
        $buildParams += "/p:AndroidSigningKeyAlias=$($Config.KeyAlias)"
        $buildParams += "/p:AndroidSigningKeyPass=$($Config.KeyPass)"
        Write-Host "Firmando APK con keystore..." -ForegroundColor Gray
    } else {
        $buildParams += "/p:AndroidKeyStore=false"
        Write-WarningMsg "Compilando APK sin firma (debug)"
    }
    
    # Ejecutar build
    & $script:MSBuildPath @buildParams
    
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Error en la compilacion. Revisa los mensajes de arriba."
        exit 1
    }
    
    Write-Success "APK compilado correctamente"
}

function Copy-OutputFiles {
    Write-Header "COPIANDO ARCHIVOS DE SALIDA"
    
    # Crear directorio de salida
    if (-not (Test-Path $Config.OutputDirectory)) {
        New-Item -ItemType Directory -Path $Config.OutputDirectory -Force | Out-Null
    }
    
    $projectDir = Join-Path $Config.ProjectRoot "App_ConsultaStocks\App_ConsultaStocks.Android"
    $binPath = Join-Path $projectDir "bin\$($Config.Configuration)"
    
    # Buscar el APK firmado
    $apkFiles = Get-ChildItem -Path $binPath -Filter "*-Signed.apk" -ErrorAction SilentlyContinue
    if (-not $apkFiles) {
        $apkFiles = Get-ChildItem -Path $binPath -Filter "*.apk" -ErrorAction SilentlyContinue
    }
    
    if ($apkFiles) {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $versionCode = ""
        
        # Intentar leer el versionCode del manifest
        $manifestPath = Join-Path $projectDir "Properties\AndroidManifest.xml"
        if (Test-Path $manifestPath) {
            [xml]$manifest = Get-Content $manifestPath
            $versionCode = "_v$($manifest.manifest.versionCode)"
        }
        
        foreach ($apk in $apkFiles) {
            $newName = "PDA$versionCode`_$timestamp.apk"
            $destination = Join-Path $Config.OutputDirectory $newName
            Copy-Item $apk.FullName $destination -Force
            Write-Success "APK copiado a: $destination"
        }
        
        # Tambien copiar el archivo de mapeo si existe (ProGuard)
        $mappingFile = Join-Path $binPath "mapping.txt"
        if (Test-Path $mappingFile) {
            Copy-Item $mappingFile (Join-Path $Config.OutputDirectory "mapping_$timestamp.txt") -Force
        }
    } else {
        Write-ErrorMsg "No se encontraron archivos APK en: $binPath"
        exit 1
    }
}

function Show-Summary {
    Write-Header "BUILD COMPLETADO"
    
    Write-Host ""
    Write-Host "Archivos generados en:" -ForegroundColor White
    Write-Host "   $($Config.OutputDirectory)" -ForegroundColor Cyan
    
    Write-Host ""
    Write-Host "Proximos pasos:" -ForegroundColor White
    Write-Host "   1. Instala el APK en tu dispositivo:" -ForegroundColor Gray
    Write-Host "      adb install -r 'ruta\del\apk.apk'" -ForegroundColor DarkCyan
    Write-Host "   2. O copia el APK a tu dispositivo e instalalo" -ForegroundColor Gray
    Write-Host "   3. Para Play Store, sube el AAB (si lo generaste)" -ForegroundColor Gray
    
    Write-Host ""
    Write-Host "Tip:" -ForegroundColor Yellow
    Write-Host "   Este script se puede ejecutar desde CI/CD o como tarea programada" -ForegroundColor Gray
    
    # Abrir carpeta de salida
    $openFolder = Read-Host "Abrir carpeta de salida? (S/N)"
    if ($openFolder -eq 'S' -or $openFolder -eq 's') {
        Invoke-Item $Config.OutputDirectory
    }
}

# =============================================================================
# EJECUCION PRINCIPAL
# =============================================================================

$ErrorActionPreference = "Stop"

# Titulo de la ventana
$host.ui.RawUI.WindowTitle = "Build App_ConsultaStocks (Android)"

try {
    Clear-Host
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "           BUILD AUTOMATIZADO - App_ConsultaStocks             " -ForegroundColor Cyan
    Write-Host "                        Android (Xamarin)                       " -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Verificar configuracion
    if ($Config.ProjectRoot -eq "C:\Proyectos\App_ConsultaStocks2" -or 
        $Config.OutputDirectory -eq "C:\Builds\PDA") {
        Write-WarningMsg "Estas usando rutas por defecto. Edita el archivo y configura tus rutas reales."
        Write-Host "Busca la seccion 'CONFIGURACION' en este archivo y modifica:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "  - ProjectRoot: Ruta donde clonaste el proyecto" -ForegroundColor White
        Write-Host "  - OutputDirectory: Donde quieres los APK" -ForegroundColor White
        Write-Host "  - KeystorePath, KeystorePass, KeyAlias, KeyPass: Datos de firma" -ForegroundColor White
        Write-Host ""
        $continue = Read-Host "Continuar de todos modos? (S/N)"
        if ($continue -ne 'S' -and $continue -ne 's') {
            exit 0
        }
    }
    
    # Preguntar password si esta configurado
    if ($Config.AskForPassword) {
        Write-Host ""
        Write-Host "Introduce las credenciales del keystore:" -ForegroundColor Yellow
        if ($Config.KeystorePath -and (Test-Path $Config.KeystorePath)) {
            Write-Host "Usando: $($Config.KeystorePath)" -ForegroundColor Gray
        }
        $Config.KeystorePass = Read-Host "Password del keystore" -AsSecureString | ForEach-Object { [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($_)) }
        $Config.KeyPass = Read-Host "Password del alias (Enter si es igual)" -AsSecureString | ForEach-Object { 
            $pass = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($_))
            if ([string]::IsNullOrWhiteSpace($pass)) { $Config.KeystorePass } else { $pass }
        }
    }
    
    # Ejecutar pasos
    Test-Prerequisites
    Invoke-NuGetRestore
    Invoke-CleanBuild
    Invoke-VersionIncrement
    Invoke-BuildAPK
    Copy-OutputFiles
    Show-Summary
    
} catch {
    Write-ErrorMsg "Error inesperado: $_"
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
} finally {
    Write-Host ""
    Write-Host "Presiona cualquier tecla para cerrar..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
