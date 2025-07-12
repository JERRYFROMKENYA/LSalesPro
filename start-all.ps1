# PowerShell script to start all microservices and the API Gateway in the background

# Start AuthService
Start-Process dotnet -ArgumentList "run --no-build" -WorkingDirectory "src\AuthService\AuthService.Api" -NoNewWindow
Start-Sleep -Seconds 5

# Start InventoryService
Start-Process dotnet -ArgumentList "run --no-build" -WorkingDirectory "src\InventoryService\InventoryService.Api" -NoNewWindow
Start-Sleep -Seconds 5

# Start SalesService
Start-Process dotnet -ArgumentList "run --no-build" -WorkingDirectory "src\SalesService\SalesService.Api" -NoNewWindow
Start-Sleep -Seconds 5

# Start API Gateway
Start-Process dotnet -ArgumentList "run --no-build" -WorkingDirectory "src\ApiGateway" -NoNewWindow
Start-Sleep -Seconds 5

Write-Host "All services started in the background."
