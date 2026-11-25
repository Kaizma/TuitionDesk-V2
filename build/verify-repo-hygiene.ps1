$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$trackedFiles = @(& git -C $repoRoot ls-files)
if ($LASTEXITCODE -ne 0) {
    throw "Could not list tracked files with git ls-files."
}

$forbiddenPathPattern = '(?i)(^|/)(bin|obj|node_modules|dist|artifacts|\.vs|\.idea|\.vscode|\.nuget|\.dotnet|\.pnpm-store|\.npm-cache|coverage|tmp)(/|$)'
$forbiddenFilePattern = '(?i)(^|/)\.env(?:\..*)?$|\.(?:db|db-wal|db-shm|log|user|suo)$'
$textExtensions = @(
    '.cs', '.csproj', '.slnx', '.props', '.targets', '.xaml', '.xml', '.json', '.md',
    '.yml', '.yaml', '.ts', '.tsx', '.js', '.jsx', '.css', '.html', '.ps1', '.iss',
    '.config', '.editorconfig', '.gitattributes', '.gitignore', '.txt', '.sql', '.sh',
    '.toml', '.ini', '.lock'
)
$secretPatterns = @(
    @{ Name = 'private key'; Pattern = '-----BEGIN (?:RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----' },
    @{ Name = 'AWS access key ID'; Pattern = '\b(?:AKIA|ASIA)[A-Z0-9]{16}\b' },
    @{ Name = 'GitHub token'; Pattern = '\b(?:gh[pousr]_[A-Za-z0-9_]{30,}|github_pat_[A-Za-z0-9_]{30,})\b' },
    @{ Name = 'Google API key'; Pattern = '\bAIza[0-9A-Za-z_-]{35}\b' }
)
$findings = [System.Collections.Generic.List[string]]::new()

foreach ($relativePath in $trackedFiles) {
    $normalizedPath = $relativePath.Replace('\', '/')

    if ($normalizedPath -match $forbiddenPathPattern -or $normalizedPath -match $forbiddenFilePattern) {
        $findings.Add("Tracked generated/local file: $relativePath")
        continue
    }

    $extension = [System.IO.Path]::GetExtension($relativePath)
    if ($extension -and $extension.ToLowerInvariant() -notin $textExtensions) {
        continue
    }

    $absolutePath = Join-Path $repoRoot $relativePath
    if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) {
        continue
    }

    $bytes = [System.IO.File]::ReadAllBytes($absolutePath)
    $sampleLength = [Math]::Min($bytes.Length, 4096)
    if ($sampleLength -gt 0 -and ($bytes[0..($sampleLength - 1)] -contains 0)) {
        continue
    }

    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($absolutePath)) {
        $lineNumber++
        foreach ($secretPattern in $secretPatterns) {
            if ($line -match $secretPattern.Pattern) {
                $findings.Add("Possible $($secretPattern.Name): ${relativePath}:${lineNumber}")
            }
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Error "Repository hygiene check failed:`n - $($findings -join "`n - ")"
    exit 1
}

Write-Host "Repository hygiene check passed ($($trackedFiles.Count) tracked files scanned)."
