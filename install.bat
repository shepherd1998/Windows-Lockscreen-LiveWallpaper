@echo off
setlocal
title VideoLock installer

set "FW=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319"
set "CSC=%FW%\csc.exe"
set "OUT=%LOCALAPPDATA%\VideoLock"

if not exist "%CSC%" (
  echo Could not find the .NET Framework C# compiler at:
  echo   %CSC%
  pause
  exit /b 1
)
if not exist "%~dp0VideoLock.cs" (
  echo VideoLock.cs must be in the same folder as this installer.
  pause
  exit /b 1
)
if not exist "%OUT%" mkdir "%OUT%"

echo [1/4] Compiling VideoLock...
"%CSC%" /nologo /target:winexe /optimize+ /lib:"%FW%\WPF" ^
  /r:PresentationFramework.dll /r:PresentationCore.dll /r:WindowsBase.dll ^
  /r:System.Xaml.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll ^
  /out:"%OUT%\VideoLock.scr" "%~dp0VideoLock.cs"
if errorlevel 1 (
  echo Compile failed.
  pause
  exit /b 1
)

rem Screen saver paths with spaces can fail, so register the 8.3 short path.
for %%I in ("%OUT%\VideoLock.scr") do set "SHORT=%%~sI"

echo [2/4] Setting VideoLock as your screen saver (starts after 5 minutes idle)...
reg add "HKCU\Control Panel\Desktop" /v SCRNSAVE.EXE /t REG_SZ /d "%SHORT%" /f >nul
reg add "HKCU\Control Panel\Desktop" /v ScreenSaveActive /t REG_SZ /d 1 /f >nul
reg add "HKCU\Control Panel\Desktop" /v ScreenSaverIsSecure /t REG_SZ /d 1 /f >nul
reg add "HKCU\Control Panel\Desktop" /v ScreenSaveTimeOut /t REG_SZ /d 300 /f >nul

echo [3/4] Creating the Ctrl+Alt+L "lock with video" shortcut...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$s=(New-Object -ComObject WScript.Shell).CreateShortcut([Environment]::GetFolderPath('StartMenu')+'\Programs\VideoLock Now.lnk');" ^
  "$s.TargetPath='%OUT%\VideoLock.scr'; $s.Arguments='/s'; $s.Hotkey='CTRL+ALT+L'; $s.Description='Show video, then lock'; $s.Save()"

echo [4/4] Pick your video (MP4 works best)...
"%OUT%\VideoLock.scr" /c

echo.
echo Done. Press Ctrl+Alt+L to try it, or wait 5 minutes idle.
echo Change the idle time in: Settings ^> Personalization ^> Lock screen ^> Screen saver.
echo For a smoother unlock, run optional-admin-tweaks.bat as administrator.
pause
