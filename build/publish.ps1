param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Output = "artifacts/app",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root

try {
    Write-Host "Installing web dependencies..."
    pnpm --dir web install --frozen-lockfile --ignore-scripts

    Write-Host "Building web assets..."
    pnpm --dir web build

    Write-Host "Publishing desktop app ($Configuration / $Runtime)..."
    dotnet publish src/TuitionDesk.Desktop `
        --configuration $Configuration `
        --runtime $Runtime `
        --self-contained true `
        -p:PublishSingleFile=false `
        -o $Output

    if (-not $SkipInstaller) {
        $isccCommand = Get-Command iscc -ErrorAction SilentlyContinue
        $iscc = if ($isccCommand) { $isccCommand.Source } else { $null }

        if (-not $iscc) {
            $candidates = @(
                (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"),
                "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
                "C:\Program Files\Inno Setup 6\ISCC.exe"
            )
            $iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
        }

        if ($iscc) {
            Write-Host "Building installer..."
            & $iscc (Join-Path $root "build\installer.iss")
        }
        else {
            Write-Warning "Inno Setup 6 was not found. Install it to produce the .exe installer, or run with -SkipInstaller."
        }
    }

    Write-Host "Published to $root\$Output"
}
finally {
    Pop-Location
}
