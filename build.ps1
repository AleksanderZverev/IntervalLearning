$answer = Read-Host "Start build (y/[n])"

if ($answer -ne "y") {
    return;
}

$buildResult = Start-Process -FilePath "docker" -ArgumentList "compose", "build" -WorkingDirectory "." -NoNewWindow -PassThru -Wait

if ($buildResult.ExitCode -ne 0) {
    Write-Error "Failed to build IL. Exit code: " + $buildResult.ExitCode
    return
}

Write-Host "IL: builded successfully" -ForegroundColor Green

Write-Host "Press enter button to exit..." -NoNewline
[System.Console]::ReadKey(1)