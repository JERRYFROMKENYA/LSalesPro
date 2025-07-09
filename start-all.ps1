# PowerShell script to start all microservices and the API Gateway in separate windows

# Start AuthService
Start-Process powershell -ArgumentList "dotnet clean;dotnet restore; dotnet build; dotnet run" -WindowStyle Normal

# Start AuthService
Start-Process powershell -ArgumentList "cd src\AuthService\AuthService.Api; dotnet run" -WindowStyle Normal

# Start InventoryService
Start-Process powershell -ArgumentList "cd src\InventoryService\InventoryService.Api; dotnet run" -WindowStyle Normal

# Start SalesService
Start-Process powershell -ArgumentList "cd src\SalesService\SalesService.Api; dotnet run" -WindowStyle Normal

# Start API Gateway
Start-Process powershell -ArgumentList "cd src\ApiGateway; dotnet run" -WindowStyle Normal

