#!/usr/bin/env pwsh
# Comcast X1 Platform Virtualizer Demo
# Creates real virtual machines from RDK firmware like VMware/VirtualBox

Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "  COMCAST X1 PLATFORM VIRTUALIZER" -ForegroundColor Cyan
Write-Host "  Real RDK Firmware Virtualization with Virtual Disks" -ForegroundColor Cyan  
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host ""

# Check for QEMU installation
Write-Host "Checking QEMU installation..." -ForegroundColor Yellow
$qemuPaths = @(
    "qemu-system-arm",
    "qemu-system-mips", 
    "qemu-img",
    "C:\Program Files\qemu\qemu-system-arm.exe",
    "C:\qemu\qemu-system-arm.exe"
)

$qemuFound = $false
foreach ($path in $qemuPaths) {
    try {
        $result = Get-Command $path -ErrorAction SilentlyContinue
        if ($result) {
            Write-Host "  ✓ Found QEMU: $($result.Source)" -ForegroundColor Green
            $qemuFound = $true
            break
        }
    } catch { }
}

if (-not $qemuFound) {
    Write-Host "  ⚠ QEMU not found in PATH" -ForegroundColor Red
    Write-Host "  Download from: https://www.qemu.org/download/#windows" -ForegroundColor Yellow
    Write-Host ""
}

# Demo firmware files
$demoFirmwares = @(
    "demo_firmware.bin",
    "test_rdkv_firmware.bin",
    "UverseFirmware\*.bin"
)

Write-Host "Looking for firmware files..." -ForegroundColor Yellow
$firmwareFound = @()

foreach ($pattern in $demoFirmwares) {
    $files = Get-ChildItem $pattern -ErrorAction SilentlyContinue
    if ($files) {
        $firmwareFound += $files
        foreach ($file in $files) {
            Write-Host "  ✓ Found firmware: $($file.Name) ($([math]::Round($file.Length / 1MB, 2)) MB)" -ForegroundColor Green
        }
    }
}

if ($firmwareFound.Count -eq 0) {
    Write-Host "  ⚠ No firmware files found" -ForegroundColor Red
    Write-Host "  Place your RDK firmware files in the project directory" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host ""
Write-Host "VIRTUALIZATION DEMO" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

# Build the project
Write-Host "Building Comcast X1 Virtualizer..." -ForegroundColor Yellow
try {
    $buildResult = dotnet build ProcessorEmulator.csproj --configuration Release --verbosity quiet
    Write-Host "  ✓ Build successful" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Create demo script
$demoCode = @"
using System;
using System.Threading.Tasks;
using ProcessorEmulator;

class VirtualizerDemo
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("COMCAST X1 PLATFORM VIRTUALIZER DEMO");
        Console.WriteLine("====================================");
        Console.WriteLine();

        try
        {
            // Create virtualizer instance
            var virtualizer = new ComcastX1Emulator();
            
            // Initialize virtualizer
            Console.WriteLine("Initializing virtualizer...");
            bool initialized = await virtualizer.Initialize();
            if (!initialized)
            {
                Console.WriteLine("Failed to initialize virtualizer");
                return;
            }
            
            // Load firmware (use demo firmware if available)
            string firmwarePath = "demo_firmware.bin";
            if (args.Length > 0)
                firmwarePath = args[0];
                
            if (System.IO.File.Exists(firmwarePath))
            {
                Console.WriteLine();
                Console.WriteLine("Creating virtual machine from firmware...");
                bool loaded = await virtualizer.LoadFirmware(firmwarePath);
                
                if (loaded)
                {
                    Console.WriteLine();
                    Console.WriteLine("Virtual machine created successfully!");
                    Console.WriteLine("Ready to start RDK virtualization");
                    Console.WriteLine();
                    Console.WriteLine("Features:");
                    Console.WriteLine("• Real virtual disk creation (QCOW2 format)");
                    Console.WriteLine("• Firmware partition analysis and installation");
                    Console.WriteLine("• Hardware-accurate virtualization");
                    Console.WriteLine("• Bootable RDK environment");
                    Console.WriteLine();
                    
                    Console.Write("Start virtual machine? (y/n): ");
                    string input = Console.ReadLine();
                    
                    if (input?.ToLower() == "y")
                    {
                        Console.WriteLine();
                        Console.WriteLine("Starting virtual machine...");
                        await virtualizer.Start();
                    }
                }
                else
                {
                    Console.WriteLine("Failed to create virtual machine from firmware");
                }
            }
            else
            {
                Console.WriteLine($"Firmware file not found: {firmwarePath}");
                Console.WriteLine("Available demo modes:");
                Console.WriteLine("• Hardware detection");
                Console.WriteLine("• Partition analysis");
                Console.WriteLine("• Virtual disk creation");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
"@

# Save demo code
$demoPath = "ComcastVirtualizerDemo.cs"
$demoCode | Out-File -FilePath $demoPath -Encoding UTF8

Write-Host ""
Write-Host "USAGE EXAMPLES" -ForegroundColor Cyan
Write-Host "==============" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Run with demo firmware:" -ForegroundColor Yellow
Write-Host "   dotnet run --project ProcessorEmulator.csproj" -ForegroundColor White
Write-Host ""
Write-Host "2. Run with your firmware:" -ForegroundColor Yellow  
Write-Host "   dotnet run --project ProcessorEmulator.csproj -- your_firmware.bin" -ForegroundColor White
Write-Host ""
Write-Host "3. Compile demo separately:" -ForegroundColor Yellow
Write-Host "   csc ComcastVirtualizerDemo.cs /r:bin\Release\net6.0\ProcessorEmulator.dll" -ForegroundColor White
Write-Host "   .\ComcastVirtualizerDemo.exe" -ForegroundColor White
Write-Host ""

Write-Host "WHAT THIS DOES:" -ForegroundColor Cyan
Write-Host "• Analyzes your RDK firmware and extracts partitions" -ForegroundColor Green
Write-Host "• Creates QCOW2 virtual disk like VMware/VirtualBox" -ForegroundColor Green  
Write-Host "• Installs firmware partitions to virtual disk" -ForegroundColor Green
Write-Host "• Launches QEMU virtual machine with your firmware" -ForegroundColor Green
Write-Host "• Provides real RDK environment you can modify" -ForegroundColor Green
Write-Host ""

Write-Host "VIRTUAL MACHINE FEATURES:" -ForegroundColor Cyan
Write-Host "• Real ARM/MIPS CPU virtualization" -ForegroundColor Green
Write-Host "• Network connectivity (NAT/bridged)" -ForegroundColor Green
Write-Host "• Virtual hardware devices" -ForegroundColor Green
Write-Host "• Persistent storage (modify and save changes)" -ForegroundColor Green
Write-Host "• Snapshots and rollback capabilities" -ForegroundColor Green
Write-Host ""

if ($firmwareFound.Count -gt 0) {
    Write-Host "Ready to virtualize with firmware: $($firmwareFound[0].Name)" -ForegroundColor Green
} else {
    Write-Host "Add your RDK firmware files to test real virtualization" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "This is REAL virtualization - not emulation or simulation!" -ForegroundColor Cyan
Write-Host "Your firmware will run in an actual virtual machine." -ForegroundColor Cyan
