# COM_INFO

Utilidad WinForms para Windows que monitoriza puertos COM desde la bandeja del sistema.

## Funcionalidad

- Se ejecuta como aplicacion de bandeja, sin interfaz de trabajo visible.
- Muestra la lista de puertos COM disponibles en el icono de notificacion.
- Resalta durante 1 minuto los puertos recien conectados con la marca `[NUEVO]`.
- Lanza una notificacion temporal cuando detecta nuevos puertos COM.
- Permite cerrar la aplicacion desde el menu contextual del icono.

## Estructura principal

- `Form1.cs`: coordinacion de la app de bandeja, tooltip, notificaciones y ciclo de vida.
- `ComPortMonitor.cs`: lectura de puertos COM y deteccion de altas/bajas.
- `PortChangeEventArgs.cs`: contrato de datos para los cambios detectados.
- `AA.iss`: instalador unificado de Inno Setup.

## Requisitos

- Windows
- .NET Framework 4.8
- MSBuild del .NET Framework o Visual Studio Build Tools
- Inno Setup 6 si se quiere generar el instalador

## Compilar en Release

Desde PowerShell, en la raiz del proyecto:

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' .\COM_INFO.sln /p:Configuration=Release
```

El ejecutable generado queda en `bin\Release\COM_INFO.exe`.

## Generar instalador

1. Compila primero en Release.
2. Abre `AA.iss` con Inno Setup.
3. Compila el script para generar `Output\COM_INFO_Setup.exe`.

El instalador:

- instala la app en `%LocalAppData%\Programs\COM_INFO`
- puede crear acceso directo en el escritorio
- puede registrar el arranque automatico del usuario actual

## Notas de mantenimiento

- El proyecto ya no depende de ClickOnce ni del certificado temporal antiguo.
- Se elimino el recurso `zu-ZA` residual porque no aportaba localizacion real y complicaba la build.
- Si quieres publicar una nueva version, actualiza la version del ensamblado y la constante `MyAppVersion` en `AA.iss`.