# COM_INFO

Monitor de puertos COM para Windows que funciona desde la bandeja del sistema.

Version actual: `0.2.1`

## Hace esto

- muestra los puertos COM disponibles en el tooltip del icono
- marca los puertos nuevos con `[NUEVO]` durante 1 minuto
- lanza una notificacion temporal cuando detecta nuevos puertos
- se cierra desde el menu contextual del icono

## Compilar

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' .\COM_INFO.sln /p:Configuration=Release
```

Genera: `bin\Release\COM_INFO.exe`

## Instalador

El script de Inno Setup es `AA.iss`.

Con una build Release lista, genera `Output\COM_INFO_Setup.exe`.