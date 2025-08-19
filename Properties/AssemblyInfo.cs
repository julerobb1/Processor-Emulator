using System;

// Suppress CLS compliance warnings for this mixed API emulator project.
// The codebase intentionally uses unsigned types (uint/ulong) for low-level
// hardware and emulation code; making the assembly CLS-compliant would
// generate a very large number of warnings. Marking the assembly as
// non-CLS-compliant reduces noise while keeping the API surface intact.
[assembly: CLSCompliant(false)]
