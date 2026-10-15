# Skips the static Windows lock screen picture so you land directly on the PIN/password box.
# Works on Windows 11 Home and Pro. Called by optional-admin-tweaks.bat (needs administrator).

$key = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData'

# Method 1: official policy (Pro / Enterprise / Education).
New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Personalization' -Force | Out-Null
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Personalization' -Name NoLockScreen -Value 1 -Type DWord

# Method 2 (also works on Home): Windows resets AllowLockScreen after each sign-in,
# so a SYSTEM task sets it back to 0 at every sign-in and every unlock.
$action = New-ScheduledTaskAction -Execute 'reg.exe' `
  -Argument 'add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData" /v AllowLockScreen /t REG_DWORD /d 0 /f'

$atLogon = New-ScheduledTaskTrigger -AtLogOn
$cls = Get-CimClass -Namespace ROOT\Microsoft\Windows\TaskScheduler -ClassName MSFT_TaskSessionStateChangeTrigger
$onUnlock = New-CimInstance -CimClass $cls -ClientOnly
$onUnlock.StateChange = 8   # 8 = session unlock

$principal = New-ScheduledTaskPrincipal -UserId 'SYSTEM' -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries

Register-ScheduledTask -TaskName 'VideoLock - Skip Lock Screen' -Action $action `
  -Trigger $atLogon, $onUnlock -Principal $principal -Settings $settings -Force | Out-Null

# Apply right now too.
if (Test-Path $key) { Set-ItemProperty -Path $key -Name AllowLockScreen -Value 0 -Type DWord }

Write-Host 'Lock screen picture will be skipped. Clicking the video now goes straight to the PIN/password box.'
