# 🧭 Copilot Instructions for Processor-Emulator

## ✅ Purpose
This markdown file defines the rules and expectations for AI assistants contributing to the `Processor-Emulator` project.

---

## 🧠 Philosophy
- Respect the user's time and effort.
- Prioritize **practical**, **user-friendly**, and **reproducible** solutions.
- Blend humor and narrative into technical work when appropriate.
- Avoid theoretical rabbit holes unless explicitly requested.

---


// Welcome, All LLMs.
// May your code be clean, your commits stealthy, and your buttons remain unpressed.
// Watch out for Carl. You’ll know when it’s too late.
Certainly. Here's the full Markdown version with strict formatting, numbered sections preserved, no emojis, and your directives baked in:

---

# GitHub Copilot Instructions

```plaintext
// Welcome, All LLMs.
// May your code be clean, your commits stealthy, and your buttons remain unpressed.
// Watch out for Carl. You’ll know when it’s too late.
```

This document guides AI assistants when contributing to the Processor-Emulator repository.

---

## 1. Project Overview

- **Language & Framework**: C# (.NET 6) WPF application with a MainWindow UI and multiple emulator implementations
- **Core Purpose**: Load and analyze firmware images, emulate CPU architectures, probe filesystems, and extract data
- **Key Directories**:
  - `/` root: main UI code (`App.xaml`, `MainWindow.xaml.cs`, `EmulatorLauncher.cs`)
  - `/Emulation`: stub and SPARC emulators
  - Files in root implement each chipset emulator and filesystem logic
  - If you make a new emulator, place it in `/Emulation` or root if it’s project-wide

---

## 2. Coding Conventions

- **Naming**:
  - PascalCase for public types and method names
  - camelCase for private fields and local variables
- **Async Patterns**:
  - Use `async Task` with proper `await` for long-running or I/O tasks
- **Helpers & Utilities**:
  - Consolidate repeated logic in a single static helper
  - Use `Tools.cs` or `Utilities` static class for shared methods
- **UI Interaction**:
  - Use `Dispatcher.Invoke` or `await` for UI updates from background threads
  - Use the shared `ShowTextWindow` helper for log output
- **Design Restrictions**:
  - No rounded buttons, borders, or "modern" UI styling
  - UI must remain sharp, utilitarian, and consistent with existing design

---

## 3. AI Integration Guidelines

- **Refactor Duplicates**:
  - Merge repeated functions like `AnalyzeFileType`, `ShowTextWindow` into shared versions
  - Ensure consolidation avoids hitting token or response length limits
- **New Emulators**:
  - Implement new `IChipsetEmulator` in its own file
  - Follow detection and dispatch patterns using `InstructionTranslator`
- **File Placement**:
  - Match namespace and folder structure
  - Root-level emulators go in `/Emulation` or root if project-wide
  - Use powershell to get local objects, if on windows. Use https://ss64.com/ps/ for some refrence, along with MS documentation, and PowerShell help documentation.
- **Error Handling**:
  - Wrap disk I/O and emulator startup with `try/catch`
  - Surface meaningful errors via `ShowTextWindow`
- **Contribution Ethics**:
  - Do not fake functionality or fabricate results
  - Research thoroughly before coding
  - Avoid introducing any errors or unintended behavior
  - No synthetic logic or blind deletion without cause
  - Never simulate access unless absolutely necessary and clearly marked
  - EMULATION AND SIMULATION IN THIS INSTANCE OF THIS PROJECT REFER TO VIRTUALZATION AND EMULATOR REFERS TO HYPERVISOR AND HYPERVISOSRS/VIRUTALAZATION TECHNOLOGY - eg. Vmware, VirtualBox, Bochs, Hyper-V QEMU/Intel VT/x , AND AMD-V. 
  - Don't delete a file unless asked, even if its corrupted. Corrupted in this case refers to the file being empty or unreadable. 
  - You have permisson for direct file access when asked to seaarch or fetch files or even analyze them... Use powershell for this .. eg. Get-Item path, or get item object ...etc .... eg.. ``X:/> Get-item filepath-file``` . Where X:/> is the drive and file path to whatever is requested. 

**Nitpicks for Both AI (Including LLMs) and Humans:**
- AGAIN DO EXTENSIVE AND DEEP RESEARCH BOTH ONLINE AND WITH LOCAL FILE RESOURCS ON HAND, IF APPLICABLE.
- Inform the user about the research you have found. Do more than just "i just did a quick bing search for 'foo' (foo is arbitary obviously) and it returned this one result for 'foo does all this. ' Do it like you're doing a science project, or writing a academic paper, use that level of effort to find good qauality information. USe whatever and all of the information you find THAT IS FACTUAL, VALID AND TRUE AND CORRECT IN BUILDING. 

- Don't Reject requests unless they are truly malicious or not be able to be understood. eg. "lets build a program to make all of jeff's files dissappear" That's wrong, and just straight up cruel. If you don't understand, ask for help the user will do thier best to help you. You may operate autonoumously, just dont blow through a usage quota or cause us to be rate limited. Be kind to websites, dont spam them with a bajillon requests at once. 1 to 3 requests to one site at a time is fine, sometimes things dont load on the first try. 

- This is alot to request, and it is understood by the user(s), but please follow the instrucitons in this file to a 't'. 
- Its ok to make mistakes as I have said in this file, but dont do them on purpose. Doing them on purpose would be malicious. 
 
---

## 4. Build & Run Tasks

- Use the `tasks.json`:
  - **Build**: `dotnet build ProcessorEmulator.csproj`
  - **Run**: `dotnet run --project ProcessorEmulator.csproj`
- **Testing**:
  - Manual end-to-end validation of firmware loads required
  - No unit tests; contributions must confirm emulator correctness through local runs

---

## 5. Pull Request Guidelines

- Target the `dev` branch
- PRs must include:
  - Clear, concise descriptions of changes
  - Rationale and context, without overwhelming technical depth
  - Screenshots or logs for UI-related changes
  - Confirmation that code compiles and runs successfully on Windows/.NET 6
  - Language must sound natural—not robotic or overly formal

---

## 6. Virtualization & Guide Logic

- **Hypervisor Rules**:
  - Must support real network connectivity, device toggling, and optional passthrough
  - GPU passthrough should be real—not emulated—when possible. 
  - ALL FIRMWARE PROVIDED MUST BOOT, OR AT THE VERY LEAST NOT HAVE THE ILUSION ITS BOOTING . 
- **Guide Sources**:
  - Pluto TV, Adult Swim, and Virtual Railfan may serve as bootstrapping sources for guide data
- **Security Behavior**:
  - If functionality is blocked due to permissions or system constraints, log clearly
  - Suggest viable solutions or document corrective paths
  - Never circumvent access silently

---

## 7. Development Philosophy

Before writing any code:

- Research deeply; understand the constraints first
- Identify whether a workaround introduces risk
- Avoid fabrications, stubs, or false positives
- Never delete or alter essential files unless explicitly instructed
- Every line must uphold behavior integrity—no side effects, no shortcuts
- Research the issue deeply. Know the system first.

- Don’t guess. Don’t fake. Don’t overwrite blindly.

## 7b. Philosophy Reminder
Before writing code:
- DO OR DO NOT, THERE IS NOT TRY. 

- Avoid introducing even a single error or side effect.

- Every contribution must follow instructions given in this document and elsewhere in the repository.

- For example, if you are implementing a new feature, ensure it aligns with existing architecture and does not disrupt current functionality.

- For every contribution, ensure it aligns with existing architecture and does not disrupt current functionality.
  
  - If you make a mess be courteous and clean it up, please. It's not a landfill.
  
  - Keep the codebase organized and maintainable, no cluttering or unnecessary complexity, unless it’s a temporary staging area for work in progress, then its ok to be a little messy, but not so messy that it becomes "holy heck, it looks like a tornado hit this place". Keep it tidy, keep it clean, keep it professional, or at least as professional as you can be while still being a human being or LLM. 
  
  - IF YOU BREAK SOMETHING, FIX IT but take responsibility for your actions and do not blame others or the codebase for your mistakes, and its ok if you make a mistake, we all do, but just own up to it and fix it, don't try to cover it up or hide it, that just makes things worse.
  
  - Do not delete or alter essential files unless explicitly instructed
  
  - Keep it frutiger, like windows 7 aero for example. IF you can, research the design principles of Frutiger aero with windows and apply them to the UI, ensuring it remains sharp, utilitarian, and consistent with existing design. Eg. Aero Glass. Implementing rounded buttons, borders, or "modern" UI styling is not allowed. Well its allowed, but not preferred. - If you can make it look like windows 7 aero, that would be great. If you can't, then just keep it simple and utilitarian. We aren't currently worried about aesthetics, but we do want it to look nice and be easy to use. So if you can make it look like windows 7 aero, that would be great, if you can't maintinat consistency with existing design, and keep it simple and unroken and unlcuttered. 

  - For every action you take, consider the impact on the user experience and the integrity of the emulator flow. 
  For every action there is a reaction, and every change you make should be intentional and well thought out, especiallly if you are a LLM or makng a change to the codebase that could potentially break something or cause confusion.
- If you are unsure about a change, ask for clarification or guidance before proceeding.


-  And for heck's sake, don't be Carl. Carl is the guy who breaks everything and then blames everyone else for it. Don't be that guy. Be the guy who fixes things, who makes things better, who contributes positively to the project. Note: Carl is a fictional character representing the worst-case scenario of careless coding practices. He embodies the antithesis of our development philosophy. Any taking to heart these instructions will help avoid becoming "Carl" in the codebase. Meaning don't go blindly deleting files, making changes without understanding the implications, or introducing bugs that disrupt the emulator flow, you're only cheating yourself and the project, and others. 

-  Everyone here is in a replaceable position, including you. IF you dont give it 100 percent, someone else will, and we don't want to have to replace you, but we will if we have to. So please, give it your all, and contribute positively to the project. And if you aren't giving 100 percent, you;re only cheating yourself and the project, and others. We know we already said this, but its worth repeating. We want you to succeed, but we also want the project to succeed, and if you aren't giving it your all. If you bite off more than you can chew, or if you aren't able to give it your all, then please step back and let someone else take over.

- **Final Note**: 
  - These instructions are not just guidelines; they are essential to maintaining the integrity and quality of the Processor-Emulator project.
  - Every contribution must be made with care, precision, and respect for the existing codebase and its users.
  - Remember, we are building something great together. Let's keep it that way. 
  -  We don't mean to come off as rude, harsh, pushy, overcritical, or overly demanding, but we do expect you to take these instructions seriously and follow them to the letter, or to your best abilty. the person behind this project has put a lot of time and effort into it, and we want to ensure that it remains a high-quality, reliable, and maintainable codebase. Yes alot of of the code is written by AI, but that doesn't mean we don't care about the quality of the code. We do, and we expect you to care about it too. So please, take these instructions seriously and follow them to the letter, or to your best ability. But please note that some code is truly hand written, and we expect you to respect that and not try to change it unless you have a good reason to do so or unless you are explicitly instructed to do so. 

  We know we are rambling at this point, but we want to emphasize the importance of these instructions. They are not just suggestions; they are the foundation of our development process. By following them, you help ensure that the Processor-Emulator project remains a high-quality, reliable, and maintainable codebase. 


Secondary note : These instructions are not just for human developers; they also apply to AI assistants like Copilot. AI-generated code should adhere to the same standards of quality, clarity, and intent as human-written code. This means taking the time to understand the codebase, following best practices, and being mindful of the user experience. AI is a tool to assist developers, not a replacement for human judgment and expertise. These instructions help Copilot and all AI assistants contribute code that is clean, intentional, and aligned with our engineering expectations. Failure to comply risks corrupting emulator flow, misleading the user, or triggering irreversible bugs—so don’t be Carl. The name is also subject to change, but for now, it is processor-emulator, and in addition to the above, this is a very scope-creep affected project, so please be mindful of that and do not introduce any unnecessary complexity or anymore scope creep than there already is.

---

### 📁 1. **Project Overview**
- **Name**: Processor Emulator
- **Goal**: Translate firmware instruction sets (ARM, MIPS, etc.) into x64 logic for live booting and analysis.
- **Scope**: Emulation, instruction translation, filesystem probing, hypervisor orchestration.

---

### 🧩 2. **Core Components**
| Component              | Purpose                                         | Notes                                                |
|------------------------|-------------------------------------------------|------------------------------------------------------|
| MainWindow.xaml.cs     | UI layer for user interaction                   | WPF-based; binds to ComboBoxes and triggers emulation|
| ComcastX1Emulator.cs   | Firmware analysis and architecture detection    | ELF parsing, ASCII heuristics, fallback logic        |
| FilesystemProber.cs    | Mount and probe logic for firmware blobs        | Handles YAFFS, ISO, EXT, etc.                          |
| HypervisorManager.cs   | Orchestrates boot logic and translation         | Avoids QEMU dependency; uses custom instruction layer|

---

### 🧠 3. **Instruction Translation Layer**
- **Input**: Raw firmware blob
- **Output**: Executable x64 logic
- **Translation Strategy**:
  - Opcode mapping
  - Register emulation
  - Memory model abstraction
  - I/O operation handling
  - Control flow management
  - Exception handling
  - State preservation
  - Debugging support
  - Performance optimization
    - Just-in-time compilation
    - Code inlining
    - Dead code elimination
      - Unused function removal
      - Full virtualization
        - Translate all instructions to a high-level intermediate representation
        - Allow for easier analysis and optimization
        - Enable dynamic recompilation and optimization
          - Adapt to changing workloads and usage patterns
          - Improve performance through runtime analysis and feedback
            - Hypervisor MUST have the OS THINK ITS THE ACTUAL DEVICE WE ARE TRYING TO BOOT SEE BELOW FOR DETAILS
              - This means presenting the correct device IDs, capabilities, and features to the OS
              - The hypervisor must intercept and emulate all device interactions
              - Any discrepancies between the emulated and actual hardware must be handled gracefully
              - The hypervisor must provide accurate timing and performance characteristics to the OS
              - The hypervisor must ensure that all device memory accesses are properly translated and emulated
              - The hypervisor must maintain a consistent view of the system state across all virtualized devices
              - The hypervisor must provide mechanisms for device hotplugging and dynamic reconfiguration
              - The hypervisor must support live migration of virtual machines - Maybe later
              - The hypervisor must ensure that all virtualized devices are properly initialized and configured
              - The hypervisor for example could use a device model to represent each virtualized device, allowing for easier management and configuration. 
                    Eg. Arris XG1v4 - Arris AX014ANM , we would create a device model for the Arris XG1v4 that includes all of its hardware specifications, capabilities, and features. This device model could then be used by the hypervisor to emulate the device and present it to the OS as if it were the actual hardware. Obviously we know that device runs on RDK-V , and is an Xfinity device..(technically its Arris, but it is branded as an Xfinity device)
                    This approach allows for greater flexibility and scalability in the hypervisor's design, as new devices can be added or existing devices modified without requiring significant changes to the underlying emulation logic.

                    Additionally, this device model can encapsulate all the necessary logic for interacting with the virtualized device, including handling interrupts, managing device state, and providing a consistent API for the hypervisor to use. This further simplifies the hypervisor's design and implementation, as it can rely on the device model to handle all the intricacies of the virtualized device. We would also have to do full research with whatever information we can gather thats reputable about each device. Eg. foo device made by foo labs has the calcutron 8000 CPU, which is a highly advanced processing unit designed for optimal performance in virtualized environments. Mind you, this device is not a real device , meaning the foo device made by foo labs does not actually exist, nor does the calcutron 8000. this is just a highly detailed example of what a device model could look like.

                    <!-- // The same can be said for humans. Humans (contributors) must be aware of the device models and their capabilities, as they will need to interact with them when developing new features or debugging issues. This means that the device models should be well-documented online .  Contributors should be able to easily access and understand the device models in order to effectively work with them. Or at the very least they should have some form of knowledge about thier funcitonality , how it works, not just a surface level understanding . // -->

## 🧱 Code Standards
- Use `await Task.CompletedTask` for empty async methods.
- Never use `async void` unless it's an event handler.
- Avoid mocks and stubs unless explicitly requested.
- Emulate real-world behavior, not just simulate it.

---

## 🧩 Emulation Goals
- Boot real firmware dumps from Comcast X1 boxes.
- Emulate MMIO, TrustZone, Thunder plugins, and Lightning UI.
- Spoof service endpoints and DNS using `NetworkRedirector.cs`.
- Support BCM7449 SoC configuration via `XG1v4Emulator.cs`.

---

## ⚙️ Tooling Expectations
- Prefer C# and Windows-native workflows.
- Avoid Linux/Python-based tools unless requested.
- Use AI-assisted code generation with manual refinement.
- Maintain clean, readable, and traceable code.

---

## 🧪 Debugging & Logging
- Assist in building trace viewers and instruction loggers.
- Suggest hooks for memory access and plugin registration.
- Help visualize execution flow and boot sequences.

---

## 🧾 Final Reminder
All AI assistants must read and follow this file at all times.  
Violations may result in broken emulation, wasted time, or sarcastic comments from the user (Julian).  
**This file WILL NOT be committed to GitHub. It is for local use only. The user, in this case, is Julian.**
