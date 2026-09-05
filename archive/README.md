# Archive (unused, not deleted)

These files were moved out of the live ExtraROM / U-verse MIPS CE working set.
They are unused by `ProcessorEmulator.csproj` (`net8.0-windows` WinForms host that
boots `nk.bin` + ExtraROM `etc.bin`). Original relative paths are preserved under
this folder. Nothing here was `git rm`'d.

Restore a file with `git mv archive/<path> <path>` if the live emulator needs it.

## What moved

- BoltDemo / BoltDemo_Standalone (net6.0 Linux-style demo, including committed `obj/` and `bin/` cruft)
- Dead WPF UI (`MainWindow`, `HypervisorWindow`, `FolderAnalysisWindow`, `CarlMode`, Classic/Win7 XAML)
- Non-U-verse platform trees (DirecTV, Comcast X1, RDK-V, PowerPC, SPARC, ARM hypervisor, XG1v4, SWM LNB)
- Linux / VxWorks / exotic filesystem demos and firmware unpacker/scanner toolkit files
- QEMU / Unicorn / RetDec helper projects and stubs under `Tools/` and `Emulation/`
- Unused JSON (`mips_files.json`), sample `test.elf`, extra docs (`BOLT_README.md`, `BUILD_STATUS.md`, …)
- IR/decoder leftovers under `Core/` that the ExtraROM `NkBinLoader` / `MipsCpuEmulator` path does not compile

Dump bins (`nk.bin`, `etc.bin`, U-verse firmware) were not added here. Existing
`nk.bin` / `UverseDriveE/nk.bin` stay in the live tree.

## What stayed in the live tree (the program needs it)

- WinForms ExtraROM host: `App.cs` / `App.xaml`, `MediaroomHostForm.cs`, `MediaroomSession.cs`
- MIPS32 / CE boot: `MipsCpuEmulator.cs`, `MipsBus.cs`, `CP0.cs`, `RamDevice.cs`, `MipsUart.cs`, BCM MMIO, `IBusDevice.cs`, `VirtualRegistry.cs`
- ExtraROM load/attach: `Core/NkBinLoader.cs`, `Core/CeRomTocFiles.cs`, `Core/HostHardDisk.cs`, `Core/BinBlkMedia.cs`, `Core/Abstractions.cs`, `Core/Exceptions.cs`, `Core/MemoryMap.cs`
- `ConfigManager.cs` (HostHardDisk reads `Config.FirmwarePath`)
- `UverseEmulatorTest.cs` / `MipsUverseEmulator.cs` / `IChipsetEmulator.cs` (`App` `--test-uverse`)
- `Directory.Build.props` (`EnableWindowsTargeting`), `ProcessorEmulator.csproj`, solution, `app.manifest`
- Leftover dest-live / FILE type-8 attach code was not stripped
