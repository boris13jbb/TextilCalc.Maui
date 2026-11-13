[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$projectPath = Join-Path $projectRoot 'TextilCalc.App\TextilCalc.App.csproj'
$outputDirectory = Join-Path $projectRoot 'artifacts\android'
$publishDirectory = Join-Path $projectRoot 'TextilCalc.App\bin\Release\net10.0-android\publish'

# Evita la caché sandbox corrupta de AndroidX (SavedStateHandleSupport).
$env:NUGET_PACKAGES = Join-Path $env:USERPROFILE '.nuget\packages'

$project = [xml](Get-Content -LiteralPath $projectPath -Raw)
$appVersion = $project.Project.PropertyGroup.ApplicationDisplayVersion |
    Where-Object { $_ } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($appVersion)) {
    throw 'ApplicationDisplayVersion no está configurado en TextilCalc.App.csproj.'
}

New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

Push-Location $projectRoot
try {
    Remove-Item -Recurse -Force (Join-Path $projectRoot 'TextilCalc.App\bin'), (Join-Path $projectRoot 'TextilCalc.App\obj') -ErrorAction SilentlyContinue

    dotnet restore (Join-Path $projectRoot 'TextilCalc.slnx') --force
    if ($LASTEXITCODE -ne 0) {
        throw "La restauración falló con código $LASTEXITCODE."
    }

    dotnet publish $projectPath `
        -f net10.0-android `
        -c Release `
        -p:AndroidPackageFormats=apk `
        -p:AndroidCreatePackagePerAbi=false

    if ($LASTEXITCODE -ne 0) {
        throw "La publicación Android falló con código $LASTEXITCODE."
    }

    $signedApk = Join-Path $publishDirectory 'com.gramatura.app-Signed.apk'
    if (-not (Test-Path -LiteralPath $signedApk)) {
        throw "No se encontró la APK firmada: $signedApk"
    }

    $outputApk = Join-Path $outputDirectory "TextilCalc-$appVersion.apk"
    Copy-Item -LiteralPath $signedApk -Destination $outputApk -Force
    Write-Host "APK generada correctamente: $outputApk"
}
finally {
    Pop-Location
}
