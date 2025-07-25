# BOLT Bootloader Simulation

This project simulates Broadcom's BOLT (Broadcom Linux Tool) bootloader used on BCM7449 SoCs in devices like the ARRIS XG1v4.

## What is BOLT?

BOLT is Broadcom's proprietary bootloader used across many STB (Set-Top Box) SoCs. It:
- Initializes the SoC (CPU, memory, caches, peripherals)
- Verifies firmware signatures (secure boot)
- Loads the next-stage image (Linux kernel, U-Boot, or other firmware)
- Provides a basic CLI for diagnostics and firmware injection

## Features

Our BOLT simulation includes:

### SoC Initialization
- ARM Cortex-A15 MP (4 cores @ 1.2GHz) setup
- L1/L2 cache initialization
- DDR3 memory initialization (128MB)
- Peripheral initialization (UART, HDMI, MoCA, Ethernet, USB, SATA, SPI/I2C)

### Firmware Loading
- ELF binary loading and parsing
- Raw binary loading fallback
- Memory mapping at correct addresses
- Entry point extraction

### Device Tree Management
- Device tree blob (DTB) creation
- Runtime DTB property modification
- DTB handoff to firmware

### BOLT CLI Commands
- `boot [-elf] <image>` - Boot firmware image
- `dump <addr> <len>` - Dump memory contents  
- `memtest` - Run memory test
- `dt show` - Display device tree
- `help` - Show available commands

## Running the Demo

### Standalone Demo
The easiest way to see BOLT in action:

```powershell
cd BoltDemo_Standalone
dotnet run
```

This will:
1. Initialize the BCM7449 SoC simulation
2. Run sample BOLT commands (memtest, dt show, dump)
3. Create a demo ARM firmware binary
4. Boot the firmware through BOLT
5. Hand off to the emulator
6. Simulate ARM instruction execution

### With Your Own Firmware
You can also boot your own firmware:

```powershell
cd BoltDemo_Standalone
dotnet run -- path/to/your/firmware.bin
```

BOLT will attempt to parse it as an ELF file, or load it as a raw binary if the ELF header is invalid.

## Integration with Main Emulator

The main ProcessorEmulator project includes:
- `BoltBootloader.cs` - Core BOLT simulation
- `BoltEmulatorBridge.cs` - Integration bridge
- `SimpleBoltBridge.cs` - Simplified bridge for demos
- New "BOLT Bootloader" tab in the main UI

### Using BOLT in the Main UI
1. Open ProcessorEmulator
2. Go to the "BOLT Bootloader" tab
3. Click "Initialize BOLT"
4. Browse for firmware file
5. Click "Load Firmware via BOLT"

## Technical Details

### Memory Layout (BCM7449)
- RAM Base: `0x00000000`
- RAM Size: `128MB (0x08000000)`
- Kernel Entry: `0x00008000` (ARM convention)
- DTB Location: `0x07F00000`

### Supported Boot Flow
1. **BOLT ROM** - Initialize SoC hardware
2. **BOLT CLI** - Optional diagnostics and commands
3. **Firmware Load** - Load ELF/binary into memory
4. **DTB Setup** - Configure device tree for handoff
5. **Emulator Handoff** - Jump to firmware entry point
6. **ARM Emulation** - Execute firmware instructions

### Device Tree Properties
- `/chosen/stdout-path` = "serial0:115200n8"
- `/chosen/bootargs` = "console=ttyS0,115200 root=/dev/mtdblock2"
- `/memory/reg` = [RAM_BASE, RAM_SIZE]
- `/cpu/compatible` = "arm,cortex-a15"

## Real Hardware Comparison

This simulation mimics real BOLT behavior seen on:
- ARRIS XG1v4 (BCM7449)
- Other Broadcom STB platforms
- RDK-V reference devices

Key differences from real hardware:
- No secure boot signature verification
- Simplified peripheral initialization
- No actual hardware register access
- DTB is simplified (real DTB is much more complex)

## Future Enhancements

- Full U-Boot integration (compile U-Boot for BCM7449)
- More complete peripheral simulation
- Secure boot simulation
- Network boot support (TFTP)
- BOLT script execution
- More comprehensive device tree
- Integration with QEMU for full-system emulation

## Use Cases

This BOLT simulation enables:
1. **Firmware Analysis** - Understanding STB boot sequences
2. **Development** - Testing firmware without real hardware  
3. **Reverse Engineering** - Analyzing Broadcom-based devices
4. **Education** - Learning embedded bootloader concepts
5. **Integration Testing** - Testing firmware handoff scenarios

The ultimate goal is to boot real XG1v4 firmware and see the pxCore splash screen! 🚀
