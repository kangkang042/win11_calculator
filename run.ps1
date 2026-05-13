param(
    [string]$OutputName = "ManuscriptCalculator.exe"
)

$ErrorActionPreference = "Stop"

& (Join-Path $PSScriptRoot "build.ps1") -OutputName $OutputName
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$exePath = Join-Path $PSScriptRoot ("dist\" + $OutputName)
Start-Process -FilePath $exePath
