Start-Process -FilePath "gitex" -WorkingDirectory "." -WindowStyle Maximized
Start-Process -FilePath "./back/IntervalLearningApi.sln" -WindowStyle Maximized
Start-Process code -WorkingDirectory "." -ArgumentList "--new-window", "./interval-learning-web" -WindowStyle Hidden
Start-Process powershell.exe -ArgumentList "-f", "./dev.ps1" -WorkingDirectory "./interval-learning-web"