@echo off
rem Run as administrator (right-click > Run as administrator).
rem 1) Skips the static lock screen picture, so after the video you go straight to the PIN/password box.
rem    (Works on Home and Pro - see skip-lock-screen.ps1, which must be in the same folder.)
rem 2) Removes the 5-second grace period in which the screen saver can be dismissed without a password.

net session >nul 2>&1
if errorlevel 1 (
  echo Please right-click this file and choose "Run as administrator".
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0skip-lock-screen.ps1"
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v ScreenSaverGracePeriod /t REG_SZ /d 0 /f

echo Done. Sign out and back in (or restart) for these to take effect.
pause
