$ErrorActionPreference = "Stop"
$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$srcPlugin = Join-Path $projectDir "com.dofus.zaap.sdPlugin"
$soomfonManifest = Join-Path $projectDir "manifests\manifest.soomfon.json"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  Installation du Plugin : Dofus Zaap (/zaap)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

# 0. Arrêt d'un éventuel processus DofusHelper verrouillant le dossier
cmd /c "taskkill /F /IM DofusHelper.exe >nul 2>&1"
Start-Sleep -Milliseconds 300

# 1. Compilation de DofusHelper.exe si nécessaire
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$helperExe = Join-Path $srcPlugin "bin\DofusHelper.exe"
if (Test-Path $csc) {
    Write-Host "Compilation de DofusHelper.exe..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path (Join-Path $srcPlugin "bin") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $srcPlugin "static") | Out-Null
    & $csc /nologo /optimize+ /target:exe /out:$helperExe /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll (Join-Path $projectDir "src\DofusHelper.cs")
}

# 2. Installation pour Elgato Stream Deck
$elgatoPluginsDir = "$env:APPDATA\Elgato\StreamDeck\Plugins"
if (Test-Path "$env:APPDATA\Elgato\StreamDeck") {
    New-Item -ItemType Directory -Force -Path $elgatoPluginsDir | Out-Null
    $destElgato = Join-Path $elgatoPluginsDir "com.dofus.zaap.sdPlugin"
    if (Test-Path $destElgato) {
        Remove-Item -Recurse -Force $destElgato -ErrorAction SilentlyContinue
    }
    New-Item -ItemType Directory -Force -Path $destElgato | Out-Null
    Copy-Item -Path (Join-Path $srcPlugin "*") -Destination $destElgato -Recurse -Force
    $nestedElgato = Join-Path $destElgato "com.dofus.zaap.sdPlugin"
    if (Test-Path $nestedElgato) {
        Remove-Item -Recurse -Force $nestedElgato -ErrorAction SilentlyContinue
    }
    Write-Host "[OK] Installé pour Elgato Stream Deck : $destElgato" -ForegroundColor Green
}

# 3. Installation pour Soomfon / HotSpot StreamDock / Mirabox
$streamDockDir = "$env:APPDATA\HotSpot\StreamDock\plugins"
if (Test-Path "$env:APPDATA\HotSpot\StreamDock") {
    New-Item -ItemType Directory -Force -Path $streamDockDir | Out-Null
    $destSoomfon = Join-Path $streamDockDir "com.dofus.zaap.sdPlugin"
    if (Test-Path $destSoomfon) {
        Remove-Item -Recurse -Force $destSoomfon -ErrorAction SilentlyContinue
    }
    New-Item -ItemType Directory -Force -Path $destSoomfon | Out-Null
    Copy-Item -Path (Join-Path $srcPlugin "*") -Destination $destSoomfon -Recurse -Force
    $nestedSoomfon = Join-Path $destSoomfon "com.dofus.zaap.sdPlugin"
    if (Test-Path $nestedSoomfon) {
        Remove-Item -Recurse -Force $nestedSoomfon -ErrorAction SilentlyContinue
    }
    Copy-Item -Path $soomfonManifest -Destination (Join-Path $destSoomfon "manifest.json") -Force
    Write-Host "[OK] Installé pour Soomfon / StreamDock : $destSoomfon" -ForegroundColor Green
}

Write-Host ""
Write-Host "Installation terminée ! Redémarrez votre logiciel Stream Deck ou Soomfon (Stream Controller / StreamDock) pour voir la catégorie 'Dofus Zaap' dans la liste de droite." -ForegroundColor Cyan
