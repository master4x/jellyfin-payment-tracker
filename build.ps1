<#
.SYNOPSIS
    Baut das Plugin, packt die DLL als ZIP und aktualisiert checksum/timestamp/sourceUrl in manifest.json.
    (Datei bewusst ASCII-only, damit sie auch unter Windows PowerShell 5.1 korrekt geparst wird.)

.EXAMPLE
    ./build.ps1 -Version 1.0.0.0 -Repo "master4x/jellyfin-payment-tracker"
#>
param(
    [string]$Version = "1.0.0.0",
    [string]$Repo = "master4x/jellyfin-payment-tracker"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$project = Join-Path $root "Jellyfin.Plugin.PaymentTracker"
$artifacts = Join-Path $root "artifacts"
$publish = Join-Path $artifacts "publish"
$zipName = "payment-tracker_$Version.zip"
$zipPath = Join-Path $artifacts $zipName

if (Test-Path $artifacts) { Remove-Item -Recurse -Force $artifacts }
dotnet publish $project -c Release -o $publish -p:Version=$Version -p:AssemblyVersion=$Version -p:FileVersion=$Version
if ($LASTEXITCODE -ne 0) { throw "Build fehlgeschlagen" }

# Nur die Plugin-DLL gehoert ins Paket - alle Jellyfin-Assemblies liefert der Server.
Compress-Archive -Path (Join-Path $publish "Jellyfin.Plugin.PaymentTracker.dll") -DestinationPath $zipPath -Force

$md5 = (Get-FileHash $zipPath -Algorithm MD5).Hash.ToLowerInvariant()
$manifestPath = Join-Path $root "manifest.json"
$utf8 = New-Object System.Text.UTF8Encoding($false)
$manifest = [System.IO.File]::ReadAllText($manifestPath, $utf8) | ConvertFrom-Json
$entry = @($manifest)[0].versions | Where-Object { $_.version -eq $Version }
if (-not $entry) { throw "Version $Version fehlt in manifest.json - bitte zuerst einen Eintrag anlegen." }

$entry.checksum = $md5
$entry.timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$entry.sourceUrl = "https://github.com/$Repo/releases/download/v$Version/$zipName"
$json = ConvertTo-Json -InputObject @($manifest) -Depth 10
[System.IO.File]::WriteAllText($manifestPath, $json, $utf8)

Write-Host "Paket:    $zipPath"
Write-Host "MD5:      $md5"
Write-Host "Manifest aktualisiert. ZIP als Release-Asset 'v$Version' hochladen und manifest.json committen."
