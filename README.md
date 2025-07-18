# Processor Emulator

A multi-purpose emulator and analysis toolkit for set-top boxes, firmware images, and embedded systems.  
Supports generic CPU/OS emulation, RDK-V/B, Uverse, DirecTV firmware analysis, filesystem probing, and more.

## Features

  Load and emulate binaries for a wide range of architectures (MIPS, ARM, x86, PowerPC, SPARC, etc.) using QEMU or custom emulators.

  (Planned) Emulate RDK-V set-top box environments for research and reverse engineering.

  (Planned) Emulate RDK-B broadband gateway environments.

  (Planned) Simulate DirecTV SWM switches and LNBs for testing and analysis.

  Analyze and identify the type of filesystem in a given image or partition.

  (Planned) Simulate a Cable Modem Termination System (CMTS) head end for DOCSIS research.

  (Planned) Emulate AT&T Uverse set-top boxes, including WinCE-based environments.

  Analyze DirecTV firmware images for structure, content, and possible vulnerabilities.

  (Planned) Read and write to Linux filesystems (ext2/3/4, JFFS2, UBIFS, etc.) from Windows.


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

## Getting Started

1. **Clone the repository:**
   ```sh
   git clone https://github.com/yourusername/Processor-Emulator.git
   ```

2. **Install QEMU:**  
   Download QEMU for your platform and place the binaries in a folder *outside* this repository (see `.gitignore`).

3. **Build the project:**  
   Open the solution in Visual Studio and build.

4. **Run:**  
   Launch the application. You will be presented with a menu of emulation and analysis options.

## Usage

- **Generic CPU/OS Emulation:**  
  Select a binary or firmware image. The tool will attempt to detect the architecture and OS, and launch the appropriate emulator.

- **Other Modes:**  
  Select the desired mode from the main menu. Some features are stubs and will display a message until implemented.

## Contributing

Contributions are welcome!  
If you want to add support for a new architecture, device, or analysis tool:

1. Fork the repository.
2. Create a new branch.
3. Add your code (see `Emulation/StubEmulators.cs` for emulator stubs).
4. Document your changes.
5. Submit a pull request.

**Guidelines:**
- Keep code modular and well-commented.
- Use stubs for unimplemented features.
- Update this README with new features or changes.

## Project Structure

- `MainWindow.xaml.cs` - Main application logic and UI event handlers.
- `Emulation/` - Emulator classes for each supported architecture.
- `Tools/` - Utilities for disassembly, filesystem analysis, etc.
- `README.md` - This file.
- `.gitignore` - Ignores QEMU binaries and other non-source files.

## Notes

- This project does **not** include QEMU binaries. Download them separately.
- Some features (RDK, Uverse, DirecTV, CMTS, Linux FS) are planned and not yet implemented.
- WinCE version detection is heuristic and may require user input.

## License

MIT License (see `LICENSE` file).

---

*This project is for research and educational purposes only. Use responsibly.*
