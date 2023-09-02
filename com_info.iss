[Setup]
AppName=COM_info
AppVersion=0.1
DefaultDirName={userdesktop}\COM_INFO
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=COM_INFO.exe
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest

[Files]
Source: "{app}\*"; DestDir: "C:\COM_INFO"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{userstartup}\COM_info"; Filename: "C:\COM_info\COM_INFO.exe"; WorkingDir: "C:\COM_INFO"; Tasks: runonstartup

[Run]
Filename: "C:\COM_info\COM_INFO.exe"; Description: "Ejecutar la aplicación al iniciar Windows"; Flags: postinstall nowait

