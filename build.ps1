$answer = Read-Host "Start build (y/[n])"

if ($answer -ne "y") {
    return;
}

$date = Get-Date
$imageVersion = $date.ToString("yyyy-MM-dd")

(Get-Content .\docker-compose-template.yml).Replace('${IMAGE_VERSION}', $imageVersion) | Set-Content .\docker-compose.yml
(Get-Content .\docker-compose.production-template.yml).Replace('${IMAGE_VERSION}', $imageVersion) | Set-Content .\docker-compose.production.yml

$buildResult = Start-Process -FilePath "docker" -ArgumentList "compose", "build" -WorkingDirectory "." -NoNewWindow -PassThru -Wait

if ($buildResult.ExitCode -ne 0) {
    Write-Error "Failed to build IL. Exit code: " + $buildResult.ExitCode
} else {
    Write-Host "IL: builded successfully" -ForegroundColor Green
}

Write-Host "Press enter button to exit..." -NoNewline
[System.Console]::ReadKey(1)