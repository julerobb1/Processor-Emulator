#!/usr/bin/env pwsh
# PowerShell script to run BOLT demo

Write-Host "Building BOLT Demo..." -ForegroundColor Green

try {
    # Build the demo project
    dotnet build BoltDemo.csproj --configuration Debug
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build successful! Running BOLT demo..." -ForegroundColor Green
        Write-Host ""
        
        # Run the demo
        dotnet run --project BoltDemo.csproj
    } else {
        Write-Host "Build failed. Check the errors above." -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
