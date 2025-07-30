# Comcast X1 Domain Parser Demo Script
# This script demonstrates the Comcast firmware discovery functionality

Write-Host "🚀 COMCAST X1 DOMAIN PARSER DEMO" -ForegroundColor Cyan
Write-Host "═════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check if ProcessorEmulator.exe exists
$exePath = ".\bin\Debug\net6.0-windows\ProcessorEmulator.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "❌ ProcessorEmulator.exe not found at: $exePath" -ForegroundColor Red
    Write-Host "💡 Run 'dotnet build' first to compile the project" -ForegroundColor Yellow
    exit 1
}

Write-Host "📊 Starting Comcast domain analysis..." -ForegroundColor Green
Write-Host ""

# Create a temporary C# file to run the demo
$tempFile = "temp_domain_demo.cs"
$tempCode = @"
using System;
using System.Threading.Tasks;
using ProcessorEmulator.Tools;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("🎯 Running Comcast Domain Parser Demo...");
        Console.WriteLine();
        
        try
        {
            // Run the quick demo first
            ComcastDomainParserDemo.RunQuickDemo();
            
            Console.WriteLine();
            Console.WriteLine("Press any key to run full analysis...");
            Console.ReadKey();
            Console.WriteLine();
            
            // Run the full demo
            await ComcastDomainParserDemo.RunDemo();
        }
        catch (Exception ex)
        {
            Console.WriteLine(`$"❌ Error: {ex.Message}");
        }
        
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
"@

# Write the temp file
$tempCode | Out-File -FilePath $tempFile -Encoding UTF8

Write-Host "🔧 Compiling and running domain parser..." -ForegroundColor Yellow

try {
    # Compile and run the demo
    dotnet run --project ProcessorEmulator.csproj -- $tempFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Domain analysis completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Domain analysis failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error running domain parser: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # Clean up temp file
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force
    }
}

Write-Host ""
Write-Host "📁 Check the current directory for exported files:" -ForegroundColor Cyan
Write-Host "   • comcast_analysis_*.json" -ForegroundColor White
Write-Host "   • comcast_firmware_urls_*.txt" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Review the generated firmware URLs" -ForegroundColor White
Write-Host "   2. Use URLs to download Comcast X1 firmware" -ForegroundColor White
Write-Host "   3. Load firmware into the X1 Emulator" -ForegroundColor White
Write-Host "   4. Start real emulation with backend connectivity" -ForegroundColor White
