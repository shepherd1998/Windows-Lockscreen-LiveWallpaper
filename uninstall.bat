@echo off
rem Removes VideoLock. (To undo the admin tweaks, run the two "reg delete" lines at the bottom as administrator.)

reg add "HKCU\Control Panel\Desktop" /v ScreenSaveActive /t REG_SZ /d 0 /f >nul
reg delete "HKCU\Control Panel\Desktop" /v SCRNSAVE.EXE /f >nul 2>&1
del "%APPDATA%\Microsoft\Windows\Start Menu\Programs\VideoLock Now.lnk" >nul 2>&1
rmdir /s /q "%LOCALAPPDATA%\VideoLock" >nul 2>&1
rmdir /s /q "%APPDATA%\VideoLock" >nul 2>&1
echo VideoLock removed.

rem Admin-only undo:
rem   reg delete "HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization" /v NoLockScreen /f
rem   reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v ScreenSaverGracePeriod /f
rem   schtasks /delete /tn "VideoLock - Skip Lock Screen" /f
pause
