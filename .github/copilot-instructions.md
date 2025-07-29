
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
- **Error Handling**:
  - Wrap disk I/O and emulator startup with `try/catch`
  - Surface meaningful errors via `ShowTextWindow`
- **Contribution Ethics**:
  - Do not fake functionality or fabricate results
  - Research thoroughly before coding
  - Avoid introducing any errors or unintended behavior
  - No synthetic logic or blind deletion without cause
  - Never simulate access unless absolutely necessary and clearly marked

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
  - GPU passthrough should be real—not emulated—when possible
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

## 7b. Philosophy Reminder
Before writing code:
- Research the issue deeply. Know the system first.

- Don’t guess. Don’t fake. Don’t overwrite blindly.

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