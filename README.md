# Processor Emulator

A comprehensive multi-architecture firmware emulator and analysis toolkit for set-top boxes, embedded systems, and reverse engineering.  
Features real ARM/MIPS/x86 instruction execution, in house hypervisor, ~~complete AT&T U-verse/Mediaroom boot sequences, and advanced filesystem analysis.~~

## Key Features

### ✅ Real Firmware Execution
- **Custom ARM Hypervisor**: VMware/VirtualBox-style virtualization with real ARM Cortex-A15 instruction execution
- **ARM Instruction Emulation**: Authentic ARM opcode processing (MOV, ADD, BRANCH, LDR/STR, SWI)
- **Multi-Architecture Support**: ARM, MIPS, x86, PowerPC, SPARC with both custom and QEMU backends
- **Hardware Simulation**: Broadcom BCM7445/7449 SoC emulation with memory-mapped I/O

### ✅ Set-Top Box Platforms
- **AT&T U-verse**: Complete MIPS/WinCE emulation with Microsoft Mediaroom boot manager
- **RDK-V/RDK-B**: Cable industry reference platform emulation for research
- **DirecTV**: MIPS-based satellite receiver analysis and SWM LNB simulation  WIP 
- **Generic STB**: Universal firmware analysis for unknown set-top box platforms

### ✅ Filesystem & Analysis
- **Filesystem Probing**: Automatic detection and analysis of ext2/3/4, JFFS2, UBIFS, YAFFS, SquashFS
- **Firmware Unpacking**: ARRIS PACK1 containers, U-Boot images, WinCE nk.bin kernels
- **Security Analysis**: DOCSIS BPI+ V2 framework, CableLabs signature bypass research
- **Cross-Architecture**: RetDec integration for binary translation and reverse engineering


## Cross-Compile Binary (RetDec Integration)

This tool uses the [RetDec decompiler](https://github.com/avast/retdec) CLI to perform static cross-architecture translation of raw binaries.

Prerequisites:
1. Install RetDec on your system and ensure `retdec-decompiler` is in your PATH.
   - Windows: Download the latest [RetDec Windows release](https://github.com/avast/retdec/releases) and add its `bin` folder to your PATH.
   - Linux/macOS: Follow RetDec build instructions or use package manager if available.
2. Supported source/target architectures: x86 (32-bit), x64 (64-bit), ARM, ARM64, MIPS.

Usage:
1. In the app UI, select **Cross-Compile Binary**.
2. Choose an input file (`.bin`, `.exe`, etc.).
3. Pick the target architecture from the dropdown.
4. The tool will invoke:
   ```
   retdec-decompiler --mode raw -e <source> -t <target> -o <output> <input>
   ```
5. The resulting file will be saved and opened in a result window.

Note: Raw mode preserves only code sections. PE/ELF headers are not rebuilt. For full executable formats, integrate a header-rebuilder or patcher on the output.

## YAFFS Filesystem Extraction

This project can extract YAFFS filesystem images using the external `unyaffs` tool. To enable YAFFS extraction:

1. Clone or download the `unyaffs` repository:
   ```powershell
   git clone https://github.com/jbruchon/unyaffs.git
   cd unyaffs
   # Build with MinGW or on Linux
   make
   ```
2. Copy the resulting `unyaffs` (or `unyaffs.exe` on Windows) binary into your PATH or into the same folder as the emulator executable.
3. When running in **extract** mode, YAFFS images will be passed to `unyaffs` for real file extraction. If the tool is not found, the fallback stub will run.

## Installation & Quick Start

### Prerequisites
- **Windows**: .NET 6 or later
- **Visual Studio**: 2022 or VS Code with C# extension
- **Optional**: UnicornEngine for enhanced ARM emulation
- **Optional**: RetDec decompiler for cross-architecture binary analysis

### Build & Run
1. **Clone the repository:**
   ```sh
   git clone https://github.com/julerobb1/Processor-Emulator.git
   cd Processor-Emulator
   ```

2. **Build the project:**
   ```sh
   dotnet build ProcessorEmulator.csproj --configuration Release
   ```

3. **Run the emulator:**
   ```sh
   dotnet run --project ProcessorEmulator.csproj
   ```

### Usage Examples

#### ARM Firmware Emulation
1. Launch the application
2. Select **"Custom Hypervisor"** from the main menu
3. Choose your ARM firmware file (.bin, .img, .elf)
4. Watch real ARM instruction execution in the VMware-style hypervisor window

#### AT&T U-verse Analysis
1. Select **"Uverse Box Emulator"** 
2. Load a U-verse firmware image
3. Experience complete WinCE + Microsoft Mediaroom boot sequence
4. Analyze IPTV platform components and services

#### RDK-V Platform Research
1. Choose **"RDK-V Emulator"**
2. Load RDK-V firmware for Broadcom BCM7445/7449
3. Monitor ARM Cortex-A15 instruction execution
4. Analyze cable industry reference software stack

## Contributing

We welcome contributions to enhance the Processor-Emulator project! 

### Development Guidelines
1. **Target `dev` branch** for all pull requests
2. **Follow IChipsetEmulator pattern** for new platform emulators
3. **Implement real execution** - avoid synthetic/fake boot sequences
4. **Test thoroughly** - verify firmware loading and execution on Windows/.NET 6
5. **Document changes** - update README and include screenshots for UI changes

### Code Standards
- **Real Firmware Focus**: Implement authentic instruction execution, not simulation
- **Error Handling**: Use `ErrorManager` with appropriate error codes and humor
- **UI Threading**: Use `Application.Current.Dispatcher.Invoke()` for background-to-UI updates
- **Chunked Loading**: Handle large firmware files with streaming to avoid memory limits

### Adding New Platforms
1. Create emulator class implementing `IChipsetEmulator`
2. Add platform detection logic in `MainWindow.xaml.cs`
3. Implement authentic boot sequence with visual display
4. Test with real firmware images from the target platform

## Educational Purpose

This project is designed for **research and educational purposes only**:
- **Reverse Engineering Education**: Learn ARM/MIPS instruction sets and embedded systems
- **Security Research**: Study cable/satellite industry firmware protection mechanisms  
- **Platform Analysis**: Understand set-top box architectures and IPTV systems
- **Academic Use**: Support computer science and cybersecurity curriculum

**Use responsibly and ethically in compliance with applicable laws and regulations.**

## License

MIT License - see `LICENSE` file for details.

## Support

- **Issues**: Report bugs and feature requests via GitHub Issues
- **Discussions**: Technical questions and research collaboration
- **Documentation**: See `.github/copilot-instructions.md` for AI development guidelines

---

*"The lesson is, never try." - Homer Simpson*  
*Educational/archival firmware emulation for the next generation of security researchers.*

## Architecture Overview

### Core Components
- **`MainWindow.xaml.cs`**: Main UI with 25+ emulation options and platform selection
- **`VirtualMachineHypervisor.cs`**: VMware-style ARM hypervisor with real instruction execution
- **`MediaroomBootManager.cs`**: Complete Microsoft Mediaroom/WinCE boot sequence implementation
- **`/Emulation/HomebrewEmulator.cs`**: Advanced ARM emulator with BCM7449 SoC simulation
- **`ErrorManager.cs`**: Centralized error handling with educational pop culture references

### Emulator Interface Pattern
All emulators implement the `IChipsetEmulator` interface:
```csharp
public interface IChipsetEmulator
{
    string ChipsetName { get; }
    bool Initialize(string configPath);
    byte[] ReadRegister(uint address);
    void WriteRegister(uint address, byte[] data);
}
```

### Real vs. Synthetic Execution
This project emphasizes **authentic firmware execution** over simulation:
- Real ARM instruction decoding and processing - PARTIALLY WORKS 
- Actual memory-mapped I/O simulation - PARTIALLY WORKS 
- Hardware-accurate boot sequences
- Chunked firmware loading for large images (>100MB) - WIP

## Advanced Features

### Security Research
- **DOCSIS 4.0**: BPI+ V2 security framework for cable modem research
- **Firmware Unpacking**: ARRIS PACK1 container extraction and analysis
- **Signature Bypass**: CableLabs PKCS#7 certificate research techniques
- **Memory Protection**: ARM security state and privilege level emulation

### Development Tools
- **Real-time Logging**: Live ARM instruction traces with register states - WIP 
- **Filesystem Mounting**: Direct access to embedded Linux filesystems from Windows - NON WSL
- **Cross-compilation**: RetDec integration for architecture translation
- **Debug Interface**: Memory dumps, register inspection, execution control - WIP

## Notes & Status

- This project emphasizes **real firmware execution** over QEMU dependency
- ARM instruction emulation is **PARTIALLY WORKING** with custom hypervisor
- Memory-mapped I/O simulation is **PARTIALLY WORKING** 
- Large firmware file handling (>100MB) uses **chunked streaming** to avoid .NET limits
- Some advanced features are **WORK IN PROGRESS** - see status indicators above
- Windows/.NET 6 focused - Linux support not currently implemented

## License

MIT License (see `LICENSE` file for details).

## Unintended Behavior: AVR File Extraction

### Description
The Processor Emulator has demonstrated the ability to analyze `.avr` files and extract their contents. This behavior was discovered while analyzing firmware files from Denon AVR devices.

### Steps to Reproduce
1. Navigate to the `Analysis` tab in the Processor Emulator.
2. Feed a `.avr` file into the `analyze FW` functionality.
3. Observe the extracted files in the output directory.

### Output Details
The extracted files include:
- Configuration settings
- Product IDs
- MCU details
- Other related data

### Significance
This feature provides insights into the structure and configuration of `.avr` files, enabling reverse engineering and deeper analysis of firmware.

### Next Steps
- Enhance robustness to handle edge cases.
- Implement additional features to parse and interpret extracted files.
- Provide user feedback during the extraction process.