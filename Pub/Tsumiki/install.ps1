if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole("Administrators")) { Start-Process powershell.exe "-File `"$PSCommandPath`"" -Verb RunAs; exit }

$vst3Path = Join-Path $PSScriptRoot "app\Tsumiki.vst3"
$presetPath = Join-Path $PSScriptRoot "app\Tsumiki"

Unblock-File $vst3Path
New-Item -ItemType Directory -Path "C:\Program Files\Common Files\VST3" -Force
Copy-Item -Path $vst3Path -Destination "C:\Program Files\Common Files\VST3\Tsumiki.vst3"

New-Item -ItemType Directory -Path "C:\ProgramData\VST3 Presets\Minori" -Force
Copy-Item -Path $presetPath -Destination "C:\ProgramData\VST3 Presets\Minori\Tsumiki" -Recurse -Force

Pause
