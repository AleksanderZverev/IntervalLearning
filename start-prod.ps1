$startProcess = Start-Process -FilePath "docker" -ArgumentList "compose", "-f", "docker-compose.yml", "-f", "docker-compose.production.yml", "up", "-d" -WorkingDirectory "." -NoNewWindow -PassThru -Wait

if ($startProcess.ExitCode -ne 0) {
    Write-Host "Failed to start IL. Exit code: $($startProcess.ExitCode)" -ForegroundColor Red
} else {
    Write-Host "IL: started successfully" -ForegroundColor Green
}

Write-Host "Press enter button to exit..." -NoNewline
[System.Console]::ReadKey(1)
