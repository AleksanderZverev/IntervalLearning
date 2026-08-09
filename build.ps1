$answer = Read-Host "Start build (y/[n])"

if ($answer -ne "y") {
    return;
}

# Nginx

try {
    & ./update-keys.ps1
    if ($LASTEXITCODE -eq 1) {
        Write-Error "update-keys.ps1 failed with exit code 1"
        exit 1
    }
}
catch {
    Write-Error "An error occurred while running update-keys.ps1: $_"
    exit 1
}


## Read secrets
$secretsPath = Join-Path $PSScriptRoot "secrets.json"
$secretsTemplatePath = Join-Path $PSScriptRoot "secrets.template.json"

if (Test-Path $secretsPath) {
    $secrets = Get-Content $secretsPath -Raw | ConvertFrom-Json
} else {
    Write-Host "secrets.json is not found, using default values from secrets.template.json" -ForegroundColor Yellow
    if (-not (Test-Path $secretsTemplatePath)) {
        Write-Error "secrets.template.json not found either. Cannot continue."
        exit 1
    }
    $secrets = Get-Content $secretsTemplatePath -Raw | ConvertFrom-Json
}

## Docker compose
$date = Get-Date
$imageVersion = $date.ToString("yyyy-MM-dd")

function Apply-Secrets {
    param(
        [string]$content,
        [object]$db
    )
    $connectionString = "Host=$($db.Host);Port=$($db.Port);Database=$($db.DatabaseName);User Id=$($db.Username);Password=$($db.Password);Include Error Detail=true;"
    $content = $content.Replace('${IMAGE_VERSION}', $imageVersion)
    $content = $content.Replace('{{DB_NAME}}', $db.DatabaseName)
    $content = $content.Replace('{{DB_USER}}', $db.Username)
    $content = $content.Replace('{{DB_PASSWORD}}', $db.Password)
    $content = $content.Replace('{{DB_CONNECTION_STRING}}', $connectionString)
    return $content
}

$devContent = Get-Content .\docker-compose-template.yml -Raw
$devContent = Apply-Secrets -content $devContent -db $secrets.Development.Database
$devContent | Set-Content .\docker-compose.yml

$prodContent = Get-Content .\docker-compose.production-template.yml -Raw
$prodContent = Apply-Secrets -content $prodContent -db $secrets.Production.Database
$prodContent | Set-Content .\docker-compose.production.yml

Write-Host "Starting docker compose build..." -ForegroundColor Cyan
$buildResult = Start-Process -FilePath "docker" -ArgumentList "compose", "build" -WorkingDirectory "." -NoNewWindow -PassThru -Wait

if ($buildResult.ExitCode -ne 0) {
    Write-Host ""
    Write-Host "Build failed with exit code $($buildResult.ExitCode)." -ForegroundColor Red
    Write-Host "To see detailed error output, run manually:" -ForegroundColor Yellow
    Write-Host "  docker compose build --no-cache --progress=plain" -ForegroundColor White
} else {
    Write-Host "IL: built successfully" -ForegroundColor Green
}

Write-Host "Press enter button to exit..." -NoNewline
[System.Console]::ReadKey(1)
