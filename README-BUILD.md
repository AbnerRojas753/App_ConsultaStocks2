# Guía de Build Automatizado - App_ConsultaStocks (Android)

## 📦 Archivos incluidos

| Archivo | Descripción |
|---------|-------------|
| `build-android.ps1` | Script principal de PowerShell |
| `build-android.bat` | Ejecutable para doble clic (llama al PS) |
| `README-BUILD.md` | Esta guía |

## ⚙️ Configuración inicial

### 1. Editar el script

Abre `build-android.ps1` en cualquier editor de texto (VS Code, Notepad++, etc.) y modifica la sección **CONFIGURACIÓN**:

```powershell
$Config = @{
    # Ruta donde tienes el proyecto en tu PC
    ProjectRoot    = "C:\Proyectos\App_ConsultaStocks2"
    
    # Carpeta donde quieres los APK generados
    OutputDirectory = "C:\Builds\PDA"
    
    # Datos de tu keystore (para firmar el APK)
    KeystorePath   = "C:\Keys\miapp.keystore"
    KeystorePass   = "tu_password"
    KeyAlias       = "mi_alias"
    KeyPass        = "tu_password"
}
```

### 2. Ubicar tu keystore

Si no tienes un keystore, créalo con:

```bash
# Desde Developer Command Prompt de Visual Studio
keytool -genkey -v -keystore miapp.keystore -alias mi_alias -keyalg RSA -keysize 2048 -validity 10000
```

## 🚀 Cómo usar

### Opción A: Doble clic (recomendado)
1. Guarda `build-android.bat` en la misma carpeta que tu proyecto
2. Haz doble clic en `build-android.bat`

### Opción B: PowerShell
```powershell
# Desde la carpeta del proyecto
.\build-android.ps1
```

### Opción C: Con parámetros (avanzado)
```powershell
# Puedes modificar el script para aceptar parámetros
.\build-android.ps1 -Configuration "Release" -IncrementVersion
```

## 📋 Qué hace el script

1. ✅ **Verifica requisitos** - Busca MSBuild, NuGet, Java
2. ✅ **Restaura NuGet** - Descarga dependencias automáticamente
3. ✅ **Limpia builds anteriores** - Borra bin/obj para build limpio
4. ✅ **Incrementa versión** - Suma +1 al versionCode automáticamente
5. ✅ **Compila Release** - Build optimizado para producción
6. ✅ **Firma el APK** - Usa tu keystore configurado
7. ✅ **Copia resultado** - Guarda el APK con nombre descriptivo

## 📁 Estructura de salida

```
C:\Builds\PDA\
├── PDA_v2_20240320_143022.apk      ← APK firmado v2
├── PDA_v3_20240321_091530.apk      ← APK firmado v3
└── mapping_20240320_143022.txt     ← Archivo de mapeo ProGuard (si aplica)
```

## 🔧 Solución de problemas

### "No se encontró MSBuild"
- Instala Visual Studio 2019/2022 con workload de "Mobile development with .NET"
- O instala solo [Build Tools for Visual Studio](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022)

### "Keystore no encontrado"
- Verifica que la ruta en `KeystorePath` sea correcta
- Si no tienes keystore, el APK se generará sin firma (solo para debug)

### "Error al restaurar paquetes NuGet"
- Abre Visual Studio y compila una vez manualmente para que descargue todo
- O ejecuta: `nuget restore App_ConsultaStocks2.sln`

### "Error de Java/JDK"
- Asegúrate de tener Java JDK 11 o 17 instalado
- Configura la variable de entorno `JAVA_HOME`

## 🔄 Integración continua (opcional)

Puedes usar este mismo script en:
- **GitHub Actions**
- **Azure DevOps**
- **Jenkins**
- **GitLab CI**

Ejemplo para ejecutar en CI sin ventana interactiva:
```powershell
powershell -ExecutionPolicy Bypass -File build-android.ps1 -NonInteractive
```

## 📱 Información del proyecto detectada

| Propiedad | Valor |
|-----------|-------|
| **App Name** | PDA |
| **Package** | com.companyname.app_consultastocks1 |
| **Versión actual** | 1.0 (code: 1) |
| **Target SDK** | Android 12.0 (API 31) |
| **Min SDK** | Android 4.4 (API 20) |
| **Xamarin.Forms** | 5.0.0.2196 |

---

¿Preguntas? El script tiene comentarios detallados en cada paso.
