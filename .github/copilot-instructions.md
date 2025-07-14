# GitHub Copilot Instructions

This document guides AI assistants when contributing to the Processor-Emulator repository.

## 1. Project Overview
- **Language & Framework**: C# (.NET 6) WPF application with a MainWindow UI and multiple emulator implementations.
- **Core Purpose**: Load and analyze firmware images, emulate CPU architectures, probe filesystems, and extract data.
- **Key Directories**:
  - `/` root: main UI code (`App.xaml`, `MainWindow.xaml.cs`, `EmulatorLauncher.cs`)
  - `/Emulation`: stub and SPARC emulators
  - Files in root implement each chipset emulator and filesystem logic.

## 2. Coding Conventions
- **Naming**: PascalCase for public types and methods, _camelCase_ for private fields.
- **Async Patterns**: Use `async Task` for I/O or longer-running operations and `await` inside.
- **Helpers & Utilities**:
  - Consolidate repeated logic in a single static helper.
  - Place utility methods in `Tools.cs` or under a `Utilities` static class.
- **UI Interaction**: Use `Dispatcher.Invoke` or `await` to update UI from background threads. Use shared `ShowTextWindow` helper for log output.

## 3. AI Integration Guidelines
- **Refactor Duplicates**: When encountering duplicate local functions (e.g., `AnalyzeFileType`, `ShowTextWindow`), merge into one shared definition without hitting request limit, length limit or creating invalid responses or getting stuck in loops.
- **New Emulators**: Implement new `IChipsetEmulator` in its own file. Follow existing patterns: detect types, dispatch via `InstructionTranslator`.
- **File Placement**: Match namespace and folder. Root-level emulators go in `/Emulation` folder or root if project-wide.
- **Error Handling**: Leverage `try/catch` around disk I/O and emulator startup. Surface errors via `ShowTextWindow`.

## 4. Build & Run Tasks
- Use the existing `tasks.json`:
  - **Build**: `dotnet build ProcessorEmulator.csproj`
  - **Run**: `dotnet run --project ProcessorEmulator.csproj`
- No unit tests present; ensure manual end-to-end validation of firmware loads.

## 5. Pull Request Guidelines
- Target `dev` branch. Keep PRs focused and small.
- Include screenshots or logs for UI changes.
- Verify compile and smoke-test emulation scenarios local to Windows/.NET 6.

> _These instructions help AI assistants contribute consistently and correctly to the Processor-Emulator codebase._
