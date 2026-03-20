#Requires -Version 5.1
<#
.SYNOPSIS
    Script de build automatizado para App_ConsultaStocks (Android)
    Compila, firma y genera el APK listo para distribución

.DESCRIPTION
    Este script automatiza todo el proceso de compilación:
    1. Restaura paquetes NuGet
    2. Limpia builds anteriores
    3. Incrementa automáticamente el versionCode
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

# Configuración de codificación
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# ═════════════════════════════════════════════════════════════════════════════
# CONFIGURACIÓN - Modifica esto según tu entorno
# ═════════════════════════════════════════════════════════════════════════════

$Config = @{
    # Rutas del proyecto (ajústalas a tu PC)
    ProjectRoot    = "C:\Proyectos\App_ConsultaStocks2"  # <-- CAMBIA ESTO
    SolutionFile   = "App_ConsultaStocks2.sln"
    AndroidProject = "App_ConsultaStocks\App_ConsultaStocks.Android\App_ConsultaStocks.Android.csproj"
    
    # Configuración del build
    Configuration  = "Release"
    Platform       = "AnyCPU"
    
    # Keystore (ajústalos a tus datos)
    KeystorePath   = ""  # Ej: "C:\Keys\miapp.keystore"
    KeystorePass   = ""  # Password del keystore
    KeyAlias       = ""  # Alias de la clave
    KeyPass        = ""  # Password de la clave (generalmente igual al keystore)
    
    # Opciones de build
    IncrementVersionCode = $true    # Incrementar versionCode automáticamente
    BuildAAB            = $false    # Generar también AAB (para Play Store)
    
    # Carpeta de salida
    OutputDirectory     = "C:\Builds\PDA"  # <-- CAMBIA ESTO
}

# ═════════════════════════════════════════════════════════════════════════════
# FUNCIONES AUXILIARES
# ═════════════════════════════════════════════════════════════════════════════

function Write-Header {
    param([string]$Message)
    Write-Host "`n═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
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
            Write-Error "No se encontró MSBuild. ¿Tienes Visual Studio instalado?"
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
            Write-Warning "NuGet no encontrado. Intentando usar restauración integrada..."
            $script:NuGetPath = $null
        }
    } else {
        $script:NuGetPath = $nuget.Source
    }
    
    # Verificar existencia del proyecto
    $fullProjectPath = Join-Path $Config.ProjectRoot $Config.AndroidProject
    if (-not (Test-Path $fullProjectPath)) {
        Write-Error "No se encontró el proyecto en: $fullProjectPath"
        Write-Host "Verifica la ruta en la configuración (ProjectRoot)" -ForegroundColor Yellow
        exit 1
    }
    Write-Success "Proyecto encontrado: $($Config.AndroidProject)"
    
    # Verificar keystore si está configurado
    if ($Config.KeystorePath -and -not (Test-Path $Config.KeystorePath)) {
        Write-Warning "Keystore no encontrado en: $($Config.KeystorePath)"
        Write-Host "El APK se generará pero NO estará firmado para distribución" -ForegroundColor Yellow
    }
}

function Invoke-VersionIncrement {
    Write-Header "ACTUALIZANDO VERSIÓN"
    
    $manifestPath = Join-Path $Config.ProjectRoot "App_ConsultaStocks\App_ConsultaStocks.Android\Properties\AndroidManifest.xml"
    
    if (-not (Test-Path $manifestPath)) {
        Write-Warning "No se encontró AndroidManifest.xml"
        return
    }
    
    [xml]$manifest = Get-Content $manifestPath
    $currentVersionCode = [int]$manifest.manifest.'versionCode'
    $currentVersionName = $manifest.manifest.'versionName'
    
    Write-Host "Versión actual: $currentVersionName (code: $currentVersionCode)" -ForegroundColor Gray
    
    if ($Config.IncrementVersionCode) {
        $newVersionCode = $currentVersionCode + 1
        $manifest.manifest.'versionCode' = $newVersionCode.ToString()
        
        # Opcional: actualizar versionName (ej: 1.0 -> 1.0.1)
        # $newVersionName = "$currentVersionName.$newVersionCode"
        # $manifest.manifest.'versionName' = $newVersionName
        
        $manifest.Save($manifestPath)
        Write-Success "VersionCode actualizado: $currentVersionCode → $newVersionCode"
    } else {
        Write-Host "Incremento de versión desactivado" -ForegroundColor Gray
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
        Write-Error "Error al restaurar paquetes NuGet"
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
    
    # Limpiar carpetas bin y obj manualmente también
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
    
    # Parámetros base
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
        Write-Warning "Compilando APK sin firma (debug)"
    }
    
    # Ejecutar build
    & $script:MSBuildPath @buildParams
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error en la compilación. Revisa los mensajes de arriba."
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
            $versionCode = "_v$($manifest.manifest.'versionCode')"
        }
        
        foreach ($apk in $apkFiles) {
            $newName = "PDA$versionCode`_$timestamp.apk"
            $destination = Join-Path $Config.OutputDirectory $newName
            Copy-Item $apk.FullName $destination -Force
            Write-Success "APK copiado a: $destination"
        }
        
        # También copiar el archivo de mapeo si existe (ProGuard)
        $mappingFile = Join-Path $binPath "mapping.txt"
        if (Test-Path $mappingFile) {
            Copy-Item $mappingFile (Join-Path $Config.OutputDirectory "mapping_$timestamp.txt") -Force
        }
    } else {
        Write-Error "No se encontraron archivos APK en: $binPath"
        exit 1
    }
}

function Show-Summary {
    Write-Header "BUILD COMPLETADO"
    
    Write-Host "`n📁 Archivos generados en:" -ForegroundColor White
    Write-Host "   $($Config.OutputDirectory)" -ForegroundColor Cyan
    
    Write-Host "`n📱 Próximos pasos:" -ForegroundColor White
    Write-Host "   1. Instala el APK en tu dispositivo:" -ForegroundColor Gray
    Write-Host "      adb install -r 'ruta\del\apk.apk'" -ForegroundColor DarkCyan
    Write-Host "   2. O copia el APK a tu dispositivo e instálalo" -ForegroundColor Gray
    Write-Host "   3. Para Play Store, sube el AAB (si lo generaste)" -ForegroundColor Gray
    
    Write-Host "`n💡 Tip:" -ForegroundColor Yellow
    Write-Host "   Este script se puede ejecutar desde CI/CD o como tarea programada" -ForegroundColor Gray
    
    # Abrir carpeta de salida
    $openFolder = Read-Host "`n¿Abrir carpeta de salida? (S/N)"
    if ($openFolder -eq 'S' -or $openFolder -eq 's') {
        Invoke-Item $Config.OutputDirectory
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# EJECUCIÓN PRINCIPAL
# ═════════════════════════════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"

# Título de la ventana
$host.ui.RawUI.WindowTitle = "Build App_ConsultaStocks (Android)"

try {
    Clear-Host
    Write-Host @"
╔═══════════════════════════════════════════════════════════════╗
║                                                               ║
║           BUILD AUTOMATIZADO - App_ConsultaStocks             ║
║                        Android (Xamarin)                      ║
║                                                               ║
╚═══════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan
    
    # Verificar configuración
    if ($Config.ProjectRoot -eq "C:\Proyectos\App_ConsultaStocks2" -or 
        $Config.OutputDirectory -eq "C:\Builds\PDA") {
        Write-Warning "Estás usando rutas por defecto. Edita el archivo y configura tus rutas reales."
        Write-Host "Busca la sección 'CONFIGURACIÓN' en este archivo y modifica:`n" -ForegroundColor Yellow
        Write-Host "  - ProjectRoot: Ruta donde clonaste el proyecto" -ForegroundColor White
        Write-Host "  - OutputDirectory: Donde quieres los APK" -ForegroundColor White
        Write-Host "  - KeystorePath, KeystorePass, KeyAlias, KeyPass: Datos de firma" -ForegroundColor White
        Write-Host ""
        $continue = Read-Host "¿Continuar de todos modos? (S/N)"
        if ($continue -ne 'S' -and $continue -ne 's') {
            exit 0
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
    Write-Error "Error inesperado: $_"
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
} finally {
    Write-Host "`nPresiona cualquier tecla para cerrar..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
