# Check if Chocolatey is installed
if (!(Get-Command choco -ErrorAction SilentlyContinue)) {
    Write-Host "Chocolatey is not installed. Please install Chocolatey from https://chocolatey.org/install and try again."
    exit 1
}

# Check if mkcert is installed
if (Get-Command mkcert -ErrorAction SilentlyContinue) {
    Write-Host "mkcert is already installed, skipping installation."
} else {
    Write-Host "Installing mkcert..."
    choco install mkcert -y
}

# Check if mkcert root certificate is installed
$rootCertPath = "$env:LOCALAPPDATA\mkcert\rootCA.pem"
if (Test-Path $rootCertPath) {
    Write-Host "mkcert root certificate is already installed, skipping installation."
} else {
    Write-Host "Installing mkcert root certificate..."
    mkcert -install
}

# Skip regeneration if certificates exist and are less than 1 year old
if ((Test-Path "fullchain.pem") -and (Test-Path "privkey.pem")) {
    $age = (Get-Date) - (Get-Item "fullchain.pem").LastWriteTime
    if ($age.TotalDays -lt 365) {
        Write-Host "Certificates are up to date (created $([int]$age.TotalDays) days ago), skipping regeneration."
        exit 0
    }
}

# Remove old certificates if they exist
if (Test-Path "fullchain.pem") { Remove-Item "fullchain.pem" }
if (Test-Path "privkey.pem") { Remove-Item "privkey.pem" }

# Create certificate for localhost
Write-Host "Creating certificate for localhost..."
mkcert localhost

# Rename certificate files
Write-Host "Renaming certificates..."
if (Test-Path "localhost.pem") {
    Rename-Item -Path "localhost.pem" -NewName "fullchain.pem"
} else {
    Write-Host "Error: File localhost.pem not found"
    exit 1
}

if (Test-Path "localhost-key.pem") {
    Rename-Item -Path "localhost-key.pem" -NewName "privkey.pem"
} else {
    Write-Host "Error: File localhost-key.pem not found"
    exit 1
}

Write-Host "Script completed successfully!"