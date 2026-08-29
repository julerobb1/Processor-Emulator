using System;
using System.IO;
using System.Windows;
using System.Diagnostics; // Added for Debug.WriteLine
using ProcessorEmulator.Core;

namespace ProcessorEmulator.Emulation
{
    public class MipsCpuEmulator
    {
        public enum Register
        {
            PC,
            SP,
            RA = 31,
            V0 = 2,
            V1 = 3,
            A0 = 4,
            A1 = 5,
            A2 = 6,
            A3 = 7
        }

        private const int RegisterCount = 32;

        private uint[] registers;
        private uint programCounter;
        private float[] floatingPointRegisters;
        private readonly CP0 _cp0;
        private readonly MipsBus _bus;
        private VirtualRegistry _virtualRegistry; // Declared VirtualRegistry
        private readonly string _logFilePath;
        private bool _inDelaySlot;
        private uint _currentPc;
        private uint _hi;
        private uint _lo;
        // Same HI/LO pair as _hi/_lo (main-side names).
        private uint hi { get => _hi; set => _hi = value; }
        private uint lo { get => _lo; set => _lo = value; }

        public event Action<string> OnLogMessage; // Event for logging to UI

        // Event for console-like output (guest prints)
        public event Action<string>? OnConsoleOutput;

        public MipsCpuEmulator(MipsBus bus, CP0 cp0)
        {
            _bus = bus;
            _cp0 = cp0;
            registers = new uint[RegisterCount];
            floatingPointRegisters = new float[RegisterCount];
            programCounter = 0xBFC00000; // MIPS Reset Vector
            hi = 0;
            lo = 0;
            _virtualRegistry = new VirtualRegistry(); // Initialized VirtualRegistry
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulator_log.txt"); // Log file in exe directory

            // Clear log file on startup for fresh session
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }

        // Parameterless constructor for legacy callers (InstructionDispatcher, tests, etc.)
        public MipsCpuEmulator()
        {
            var cp0 = new CP0();
            var bus = new MipsBus(cp0);

            _bus = bus;
            _cp0 = cp0;
            registers = new uint[RegisterCount];
            floatingPointRegisters = new float[RegisterCount];
            programCounter = 0xBFC00000; // MIPS Reset Vector
            hi = 0;
            lo = 0;
            _virtualRegistry = new VirtualRegistry();
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulator_log.txt");

            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }

        // Execute a single fetch/decode/execute cycle (or multiple cycles)
        public void Step(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                // Check for and handle pending hardware interrupts before executing an instruction.
                if (_cp0.ShouldTriggerInterrupt())
                {
                    TriggerException(0); // 0 is the code for Interrupt
                    // Vector this step; fetch 0x80000180 on the next iteration.
                    continue;
                }

                if (BinBlkMedia.TryStep(registers, _bus, ref programCounter))
                {
                    _cp0.UpdateTimer(1);
                    _bus.Tick(1);
                    continue;
                }

                _currentPc = programCounter;
                try
                {
                    if (HostHardDisk.TryStep(registers, _bus, ref programCounter))
                    {
                        _cp0.UpdateTimer(1);
                        _bus.Tick(1);
                        continue;
                    }
                }
                catch (TlbMissException ex)
                {
                    // HD DISK_READ writes a kuseg dest. Do not invent a PTE;
                    // the firmware refill/demand-zero path already owns that VA.
                    TriggerTlbException(ex);
                    _cp0.UpdateTimer(1);
                    _bus.Tick(1);
                    continue;
                }

                if (programCounter == CeRomTocFiles.CreateFileWin32Chk)
                    CeRomTocFiles.TryRejectMscoreeFileHandle(_bus, registers);

                if (programCounter == CeRomTocFiles.CreateFileFail)
                {
                    try
                    {
                        uint path = registers[23];
                        if (!CeRomTocFiles.TryContinueRomModule(_bus, path, out uint attr, out uint tocEntry, out byte attachType))
                            CeRomTocFiles.TryContinueRomModule(_bus, registers[4], out attr, out tocEntry, out attachType);
                        if (tocEntry != 0)
                        {
                            // Type 7: TOC module, e32 at entry+0x14.
                            // Type 8: ExtraROM FILE (wait55). object+5=1
                            // skips name copy (s7 may be unmapped).
                            // Do not set ROMMODULE. Do not invent e32.
                            uint obj = registers[30];
                            _bus.Write32(obj, tocEntry);
                            _bus.Write8(obj + 4, attachType);
                            if (attachType == CeRomTocFiles.FileAttachType)
                                _bus.Write8(obj + 5, 1);
                            _bus.Write32(registers[29] + 40, attr);
                            registers[3] = attr;
                            if (attachType == CeRomTocFiles.FileAttachType
                                && CeRomTocFiles.TryStartTv2FileDecompress(
                                    _bus, registers, ref programCounter))
                            {
                                _cp0.UpdateTimer(1);
                                _bus.Tick(1);
                                continue;
                            }
                            // Type 7: NameCopyContinue CreateFileMappings
                            // the TOCentry (wait61 126). Return v0=0 with
                            // object+0=entry +4=7 so 0x800196E4 LoadE32s.
                            programCounter = attachType == CeRomTocFiles.TocAttachType
                                ? CeRomTocFiles.CreateFileOk
                                : CeRomTocFiles.NameCopyContinue;
                            _cp0.UpdateTimer(1);
                            _bus.Tick(1);
                            continue;
                        }
                    }
                    catch (TlbMissException ex)
                    {
                        // wait52: hook Read32(s7=0x040851E8) aborted
                        // the probe. That VA is filesys slot-2, not a
                        // dump ExtraROM/gwes/coredll/BINFS/tv2 page.
                        // Do not invent a map. Firmware refill owns it.
                        TriggerTlbException(ex);
                        _cp0.UpdateTimer(1);
                        _bus.Tick(1);
                        continue;
                    }
                }

                // 0x80016AFC miss (v0=2). s3=UTF16 name, s4=object.
                // ExtraROM TOC[33] ddi_nop / TOC[46] mscoree /
                // TOC[34] ole32 are not on *(0x80342B10).
                if (programCounter == CeRomTocFiles.TocWalkMiss)
                {
                    if (CeRomTocFiles.TryAttachExtraRomTocWalk(_bus, registers[19], registers[20]))
                    {
                        registers[2] = 0;
                        programCounter = CeRomTocFiles.TocWalkMissContinue;
                        _cp0.UpdateTimer(1);
                        _bus.Tick(1);
                        continue;
                    }
                }

                if (programCounter == CeRomTocFiles.CallDllStartip)
                    CeRomTocFiles.TryFillTocStartip(_bus, registers[23], true);

                if (programCounter == CeRomTocFiles.XipExeCallDllSkip)
                {
                    if (CeRomTocFiles.TryForceXipExeCallDll(_bus, registers, ref programCounter))
                    {
                        _cp0.UpdateTimer(1);
                        _bus.Tick(1);
                        continue;
                    }
                }

                if (programCounter == CeRomTocFiles.ThreadStartTrampoline
                    || programCounter == CeRomTocFiles.LoadExeE32Ret
                    || programCounter == CeRomTocFiles.LoadExeStartipRet)
                    CeRomTocFiles.TryFillProcExeStartip(_bus);

                if (programCounter == CeRomTocFiles.ProcessAttachGate)
                    CeRomTocFiles.TryEnableFilterProcessAttach(_bus, registers);

                if (programCounter == CeRomTocFiles.Win32CreateFile)
                {
                    if (CeRomTocFiles.TryMissMissingDevice(_bus, registers[4], registers, ref programCounter)
                        || CeRomTocFiles.TryMissMscoreeWin32(_bus, registers[4], registers, ref programCounter))
                    {
                        _cp0.UpdateTimer(1);
                        _bus.Tick(1);
                        continue;
                    }
                }

                if (programCounter == CeRomTocFiles.HeapCreateStore)
                    registers[2] = CeRomTocFiles.KeepProcessHeapIfCreateFailed(_bus, registers[2], registers[3]);

                if (programCounter == CeRomTocFiles.FsGetProc)
                {
                    if (CeRomTocFiles.TryResolveFilterExport(_bus, registers[4], registers[5], registers, ref programCounter))
                    {
                        _cp0.UpdateTimer(1);
                        _bus.Tick(1);
                        continue;
                    }
                }

                if (programCounter == CeRomTocFiles.BindImpMiss)
                {
                    uint e32Lite = registers[23];
                    uint fp = registers[30];
                    uint s4 = registers[20];
                    try
                    {
                        uint o32List = _bus.Read32(fp + 180);
                        uint lookup = _bus.Read32(s4 + 0x10);
                        if (CeRomTocFiles.TryFillEmptyO32Lite(_bus, e32Lite, o32List, lookup))
                        {
                            programCounter = CeRomTocFiles.BindImpWalk;
                            _cp0.UpdateTimer(1);
                            _bus.Tick(1);
                            continue;
                        }
                    }
                    catch
                    {
                    }
                }

                _currentPc = programCounter;
                try
                {
                    uint instruction = FetchInstruction();
                    DecodeAndExecute(instruction);
                }
                catch (TlbMissException ex)
                {
                    TriggerTlbException(ex);
                }
                catch (CpuAlignmentException)
                {
                    TriggerAddressError(_currentPc);
                }

                // Advance the internal timer by one cycle per instruction.
                _cp0.UpdateTimer(1);
                _bus.Tick(1);
            }
        }

        private void TriggerException(uint exceptionCode)
        {
            Console.WriteLine($"--- EXCEPTION: Code {exceptionCode} ---");
        
            // 1. Save current PC to CP0 EPC (Reg 14)
            // If in a branch delay slot, EPC should point to the branch instruction, not the delay slot.
            // (For simplicity, we are not handling branch delay slot exceptions perfectly here)
            _cp0.EPC = programCounter;

            // 2. Set Cause register with the exception code
            // Clear existing code, then set new one.
            _cp0.Cause = (_cp0.Cause & 0xFFFFFF83) | (exceptionCode << 2);

            // 3. Set Status.EXL (Exception Level) bit to 1 to prevent nested interrupts
            _cp0.Status |= (1 << 1);
            
            // 4. Jump to the General Exception Vector
            // If BEV is set, use 0xBFC00380, otherwise use 0x80000180.
            if ((_cp0.Status & (1 << 22)) != 0) // Check BEV bit
            {
                programCounter = 0xBFC00380;
            }
            else
            {
                programCounter = 0x80000180;
            }
            HostHardDisk.NoteCpuException(exceptionCode, _cp0.EPC, 0, programCounter, registers, _bus);
        }

        private void TriggerTlbException(TlbMissException ex)
        {
            uint code = ex.IsStore ? 3u : 2u;
            Console.WriteLine($"--- EXCEPTION: Code {code} TLB {(ex.IsStore ? "Store" : "Load")} {(ex.IsInvalid ? "Invalid" : "Refill")} vaddr=0x{ex.FaultingAddress:X8} ---");

            bool alreadyExl = (_cp0.Status & (1 << 1)) != 0;
            _cp0.PrepareTlbException(ex.FaultingAddress);

            uint cause = (_cp0.Cause & 0xFFFFFF83) | (code << 2);
            if (_inDelaySlot)
                cause |= 1u << 31;
            else
                cause &= 0x7FFFFFFF;
            _cp0.Cause = cause;

            _cp0.EPC = _currentPc;
            _cp0.Status |= (1 << 1);

            bool bev = (_cp0.Status & (1 << 22)) != 0;
            bool refill = !ex.IsInvalid && !alreadyExl;
            if (refill)
                programCounter = bev ? 0xBFC00200u : 0x80000000u;
            else
                programCounter = bev ? 0xBFC00380u : 0x80000180u;
            HostHardDisk.NoteCpuException(code, _cp0.EPC, ex.FaultingAddress, programCounter, registers, _bus);
        }

        private void TriggerAddressError(uint vaddr)
        {
            Console.WriteLine($"--- EXCEPTION: Code 4 AdEL vaddr=0x{vaddr:X8} ---");
            _cp0.BadVAddr = vaddr;
            _cp0.EPC = vaddr;
            uint cause = (_cp0.Cause & 0xFFFFFF83) | (4u << 2);
            cause &= 0x7FFFFFFF;
            _cp0.Cause = cause;
            _cp0.Status |= (1 << 1);
            bool bev = (_cp0.Status & (1 << 22)) != 0;
            programCounter = bev ? 0xBFC00380u : 0x80000180u;
            HostHardDisk.NoteCpuException(4, _cp0.EPC, vaddr, programCounter, registers, _bus);
        }


        private uint FetchInstruction()
        {
            if ((programCounter & 3) != 0)
                throw new CpuAlignmentException($"Unaligned fetch PC=0x{programCounter:X8}");
            uint instruction = ReadMemory32(programCounter);
            programCounter += 4;
            return instruction;
        }
        
        private uint FetchInstructionAt(uint vaddr)
        {
            return ReadMemory32(vaddr); // Use new ReadMemory32
        }

        // MMIO Interceptor
        private uint ReadMemory32(uint address)
        {
            // Handle Hardware-Specific Registers (Broadcom/Mediaroom)
            if (address >= 0x1F000000 && address <= 0x1F000FFF)
            {
                return HandlePeripheralRead(address);
            }

            // Standard RAM access
            return _bus.Read32(address);
        }

        private uint ReadMemory16(uint address)
        {
            if (address >= 0x1F000000 && address <= 0x1F000FFF)
                return HandlePeripheralRead(address) & 0xFFFF;

            byte a = _bus.Read8(address);
            byte b = _bus.Read8(address + 1);
            return _bus.IsBigEndian ? (uint)((a << 8) | b) : (uint)(a | (b << 8));
        }

        private byte ReadMemory8(uint address)
        {
            if (address >= 0x1F000000 && address <= 0x1F000FFF)
                return (byte)HandlePeripheralRead(address);
            return _bus.Read8(address);
        }

        private void WriteMemory32(uint address, uint value)
        {
            // Handle Hardware-Specific Registers (Broadcom/Mediaroom)
            if (address >= 0x1F000000 && address <= 0x1F000FFF)
            {
                HandlePeripheralWrite(address, value);
                return;
            }

            // Standard RAM access
            _bus.Write32(address, value);
        }

        private void WriteMemory16(uint address, uint value)
        {
            if (address >= 0x1F000000 && address <= 0x1F000FFF)
            {
                HandlePeripheralWrite(address, value & 0xFFFF);
                return;
            }

            byte lo = (byte)(value & 0xFF);
            byte hi = (byte)((value >> 8) & 0xFF);
            if (_bus.IsBigEndian)
            {
                _bus.Write8(address, hi);
                _bus.Write8(address + 1, lo);
            }
            else
            {
                _bus.Write8(address, lo);
                _bus.Write8(address + 1, hi);
            }
        }

        private void WriteMemory8(uint address, byte value)
        {
            if (address >= 0x1F000000 && address <= 0x1F000FFF)
            {
                HandlePeripheralWrite(address, value);
                return;
            }
            _bus.Write8(address, value);
        }

        private uint HandlePeripheralRead(uint address)
        {
            switch(address)
            {
                case 0x1F000020: // Example: Chip ID Register
                    Debug.WriteLine($"[MMIO] Reading Chip ID Register at 0x{address:X8}. Returning 0x7405.");
                    return 0x7405; // Return a Broadcom BCM7405 ID
                default:
                    Debug.WriteLine($"[MMIO] Unhandled peripheral read at 0x{address:X8}. Returning 0.");
                    return 0;
            }
        }

        private void HandlePeripheralWrite(uint address, uint value)
        {
            // For now, just log writes to unhandled peripheral registers.
            Debug.WriteLine($"[MMIO] Unhandled peripheral write to 0x{address:X8} with value 0x{value:X8}.");
        }

        private void DecodeAndExecute(uint instruction)
        {
            uint opcode = (instruction >> 26) & 0x3F;
            try
            {
                switch (opcode)
                {
                    case 0x00: // R-type instructions
                        ExecuteRType(instruction);
                        break;
                    case 0x02: // j
                        ExecuteJump(instruction, false);
                        break;
                    case 0x03: // jal
                        ExecuteJump(instruction, true);
                        break;
                    case 0x10: // COP0 instructions
                        ExecuteCOP0(instruction);
                        break;
                    case 0x08: // addi
                        ExecuteAddImmediate(instruction);
                        break;
                    case 0x09: // addiu
                        ExecuteAddImmediateUnsigned(instruction);
                        break;
                    case 0x0A: // slti
                        ExecuteSetLessThanImmediate(instruction);
                        break;
                    case 0x0B: // sltiu
                        ExecuteSetLessThanImmediateUnsigned(instruction);
                        break;
                    case 0x0C: // andi
                        ExecuteAndImmediate(instruction);
                        break;
                    case 0x0D: // ori
                        ExecuteOrImmediate(instruction);
                        break;
                    case 0x0E: // xori
                        ExecuteXorImmediate(instruction);
                        break;
                    case 0x0F: // lui
                        ExecuteLoadUpperImmediate(instruction);
                        break;
                    case 0x20: // lb
                        ExecuteLoadByte(instruction, unsigned: false);
                        break;
                    case 0x21: // lh
                        ExecuteLoadHalf(instruction, unsigned: false);
                        break;
                    case 0x22: // lwl
                        ExecuteLoadWordLeft(instruction);
                        break;
                    case 0x23: // lw
                        ExecuteLoadWord(instruction);
                        break;
                    case 0x24: // lbu
                        ExecuteLoadByte(instruction, unsigned: true);
                        break;
                    case 0x26: // lwr
                        ExecuteLoadWordRight(instruction);
                        break;
                    case 0x25: // lhu
                        ExecuteLoadHalf(instruction, unsigned: true);
                        break;
                    case 0x28: // sb
                        ExecuteStoreByte(instruction);
                        break;
                    case 0x29: // sh
                        ExecuteStoreHalf(instruction);
                        break;
                    case 0x2A: // swl
                        ExecuteStoreWordLeft(instruction);
                        break;
                    case 0x2B: // sw
                        ExecuteStoreWord(instruction);
                        break;
                    case 0x2E: // swr
                        ExecuteStoreWordRight(instruction);
                        break;
                    case 0x04: // beq
                        ExecuteBranchEqual(instruction);
                        break;
                    case 0x05: // bne
                        ExecuteBranchNotEqual(instruction);
                        break;
                    case 0x01: // REGIMM (bltz / bgez / bltzal / bgezal)
                        ExecuteRegimm(instruction);
                        break;
                    case 0x06: // blez
                        ExecuteBranchVsZero(instruction, greaterThan: false);
                        break;
                    case 0x07: // bgtz
                        ExecuteBranchVsZero(instruction, greaterThan: true);
                        break;
                    case 0x2F: // cache — no data cache in this interpreter
                        break;
                    default:
                        TriggerException(10); // 10 is Reserved Instruction exception
                        break;
                }
            }
            catch (TlbMissException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Catching emulator-level errors, not guest exceptions.
                HandleEmulatorError(ex.Message);
            }
        }

        private void ExecuteJ(uint instruction)
        {
            uint oldPc = programCounter;
            uint target = (programCounter & 0xF0000000) | ((instruction & 0x3FFFFFF) << 2);
            ExecuteDelaySlotThenJump(target);
            LogBranch(oldPc, programCounter, "J");
        }

        private void ExecuteJal(uint instruction)
        {
            uint oldPc = programCounter;
            registers[31] = programCounter + 4; // Return address is the instruction after the delay slot
            ExecuteJ(instruction);
            LogBranch(oldPc, programCounter, "JAL");
        }

        // main-side names: same J/JAL, through the delay-slot path.
        private void ExecuteJump(uint instruction, bool link)
        {
            if (link)
                ExecuteJal(instruction);
            else
                ExecuteJ(instruction);
        }

        private void ExecuteDelaySlotThenJump(uint target)
        {
            if (_inDelaySlot)
            {
                programCounter = target;
                return;
            }

            _inDelaySlot = true;
            try
            {
                uint delayInstr = FetchInstruction();
                DecodeAndExecute(delayInstr);
                programCounter = target;
            }
            catch (TlbMissException ex)
            {
                TriggerTlbException(ex);
            }
            finally
            {
                _inDelaySlot = false;
            }
        }



        private void ExecuteCOP0(uint instruction)
        {
            // MIPS COP0 instructions: bits 25-21 determine the sub-operation (rs field)
            uint rs = (instruction >> 21) & 0x1F; 
            uint rt = (instruction >> 16) & 0x1F; // General purpose register for data transfer
            uint rd = (instruction >> 11) & 0x1F; // COP0 register index

            // The 'funct' field is only used for R-type COP0 instructions (opcode 0x10, rs == 0x10)
            uint funct = instruction & 0x3F;

            switch (rs) // The 'rs' field (bits 25-21) defines the major COP0 operation
            {
                case 0x00: // MFC0 (Move From Coprocessor 0)
                    Execute_MFC0(rt, rd);
                    break;
                    
                case 0x04: // MTC0 (Move To Coprocessor 0)
                    Execute_MTC0(rt, rd);
                    break;

                case 0x10: // COP0 functions with 'funct' field (e.g., TLB operations, ERET)
                    switch (funct)
                    {
                        case 0x01: // TLBR
                            _cp0.ReadTLBEntry();
                            break;
                        case 0x02: // TLBWI
                            _cp0.WriteTLBEntryIndexed();
                            break;
                        case 0x06: // TLBWR
                            _cp0.WriteTLBEntryRandom();
                            break;
                        case 0x08: // TLBP
                            _cp0.ProbeTLB();
                            break;
                        case 0x18: // ERET
                            // ERET: Exception Return
                            // 1. Clear Status.EXL bit
                            _cp0.Status &= ~(1u << 1); 
                            // 2. Jump back to where the exception occurred
                            programCounter = _cp0.EPC;
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine($"[MIPS] Unhandled COP0 funct: 0x{funct:X}");
                            TriggerException(10); // Reserved Instruction
                            break;
                    }
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"[MIPS] Unhandled COP0 sub-op (rs field): 0x{rs:X}");
                    TriggerException(10); // Reserved Instruction
                    break;
            }
        }

        public void Execute_MTC0(uint rt, uint rd)
        {
            _cp0.WriteRegister((int)rd, registers[rt]);
        }

        public void Execute_MFC0(uint rt, uint rd)
        {
            if (rt != 0)
                registers[rt] = _cp0.ReadRegister((int)rd);
        }


        private void ExecuteRType(uint instruction)
        {
            uint funct = instruction & 0x3F;
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint rd = (instruction >> 11) & 0x1F;
            uint shamt = (instruction >> 6) & 0x1F;

            // jr / jalr / syscall: $zero discard must not skip the transfer.
            if (funct == 0x08)
            {
                ExecuteJumpRegister(instruction);
                return;
            }
            if (funct == 0x09)
            {
                ExecuteJumpAndLinkRegister(instruction);
                return;
            }
            if (funct == 0x0C)
            {
                ExecuteSyscall(instruction);
                return;
            }
            if (funct == 0x11) // mthi
            {
                _hi = registers[rs];
                return;
            }
            if (funct == 0x13) // mtlo
            {
                _lo = registers[rs];
                return;
            }
            if (funct == 0x18) // mult — HI/LO write; rd is always $zero
            {
                long result = (long)(int)registers[rs] * (long)(int)registers[rt];
                _lo = (uint)result;
                _hi = (uint)(result >> 32);
                return;
            }
            if (funct == 0x19) // multu
            {
                ulong result = (ulong)registers[rs] * (ulong)registers[rt];
                _lo = (uint)result;
                _hi = (uint)(result >> 32);
                return;
            }
            if (funct == 0x1A) // div
            {
                if (registers[rt] != 0)
                {
                    _lo = (uint)((int)registers[rs] / (int)registers[rt]);
                    _hi = (uint)((int)registers[rs] % (int)registers[rt]);
                }
                return;
            }
            if (funct == 0x1B) // divu
            {
                if (registers[rt] != 0)
                {
                    _lo = registers[rs] / registers[rt];
                    _hi = registers[rs] % registers[rt];
                }
                return;
            }

            if (rd == 0) return;

            switch (funct)
            {
                case 0x10: // mfhi
                    registers[rd] = _hi;
                    break;
                case 0x12: // mflo
                    registers[rd] = _lo;
                    break;
                case 0x20: // add
                case 0x21: // addu
                    registers[rd] = registers[rs] + registers[rt];
                    break;
                case 0x22: // sub
                case 0x23: // subu
                    registers[rd] = registers[rs] - registers[rt];
                    break;
                case 0x24: // and
                    registers[rd] = registers[rs] & registers[rt];
                    break;
                case 0x25: // or
                    registers[rd] = registers[rs] | registers[rt];
                    break;
                case 0x26: // xor
                    registers[rd] = registers[rs] ^ registers[rt];
                    break;
                case 0x27: // nor
                    registers[rd] = ~(registers[rs] | registers[rt]);
                    break;
                case 0x2A: // slt
                    registers[rd] = (int)registers[rs] < (int)registers[rt] ? 1u : 0u;
                    break;
                case 0x2B: // sltu
                    registers[rd] = registers[rs] < registers[rt] ? 1u : 0u;
                    break;
                case 0x00: // sll
                    registers[rd] = registers[rt] << (int)shamt;
                    break;
                case 0x02: // srl
                    registers[rd] = registers[rt] >> (int)shamt;
                    break;
                case 0x03: // sra
                    registers[rd] = (uint)((int)registers[rt] >> (int)shamt);
                    break;
                case 0x04: // sllv
                    registers[rd] = registers[rt] << (int)registers[rs];
                    break;
                case 0x06: // srlv
                    registers[rd] = registers[rt] >> (int)registers[rs];
                    break;
                case 0x07: // srav
                    registers[rd] = (uint)((int)registers[rt] >> (int)registers[rs]);
                    break;
                default:
                    TriggerException(10); // Reserved Instruction
                    break;
            };
        }

        private void ExecuteSyscall(uint instruction)
        {
            // MIPS convention: $v0 (register 2) holds the syscall ID.
            uint syscallCode = registers[2]; 
            LogSyscall(syscallCode, instruction); // Log the syscall

            switch (syscallCode)
            {
                case 0x1D: // Hypothetical WinCE RegQueryValueExW
                    // Assuming $a0 (register 4) holds the address of the path string.
                    // This is a simplification; a real implementation would parse the string from memory.
                    // For now, we'll hardcode to the "IsAuthorized" key for demonstration.
                    registers[2] = _virtualRegistry.ReadValue("Software\\Microsoft\\Mediaroom\\Client\\IsAuthorized");
                    Debug.WriteLine($"[AUTH] App checked 'IsAuthorized' registry key via syscall 0x{syscallCode:X}. Returning: {registers[2]}");
                    break;
                case 0x1001: // Hypothetical: Mediaroom Auth Check
                    SimulateAuthSuccess();
                    break;
                case 0x2002: // Hypothetical: Graphics Draw (Placeholder)
                    RenderToWindowsForm();
                    // Emit a console-visible message for now so UI can show progress
                    OnLogMessage?.Invoke($"[SYSCALL] Graphics/Print invoked at PC=0x{programCounter:X8} (code 0x{syscallCode:X})\n");
                    OnConsoleOutput?.Invoke($"[GUEST_PRINT] PC=0x{programCounter:X8} syscall=0x{syscallCode:X}\n");
                    break;
                default:
                    Debug.WriteLine($"[MIPS] Unhandled Syscall: 0x{syscallCode:X}. Instruction: 0x{instruction:X8}");
                    // Optionally, trigger an exception or just return.
                    // For now, we'll just log and continue to avoid halting emulation.
                    break;
            }
        }

        private void SimulateAuthSuccess()
        {
            // Tell the MIPS app: "Yes, this device is authorized"
            registers[2] = 1; // Return true/success in $v0
            registers[3] = 0; // Clear error codes in $v1
            Debug.WriteLine("[AUTH] Spoofed Authorization Handshake: SUCCESS");
        }

        private void RenderToWindowsForm()
        {
            // Placeholder for future graphics rendering logic.
            Debug.WriteLine("[GRAPHICS] RenderToWindowsForm: (Not yet implemented)");
        }
        
        private void ExecuteLui(uint instruction)
        {
            uint rt = (instruction >> 16) & 0x1F;
            uint imm = instruction & 0xFFFF;
            if (rt != 0)
            {
                registers[rt] = imm << 16;
            }
        }

        private void ExecuteSlti(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int imm = (short)(instruction & 0xFFFF);
            if (rt != 0)
            {
                registers[rt] = (int)registers[rs] < imm ? 1u : 0u;
            }
        }

        private void ExecuteSltiu(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int imm = (short)(instruction & 0xFFFF);
            if (rt != 0)
            {
                registers[rt] = registers[rs] < (uint)imm ? 1u : 0u;
            }
        }

        private void ExecuteLoadWord(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            uint address = registers[baseReg] + (uint)offset;
            if (rt != 0) // writes to R0 are discarded
            {
                registers[rt] = ReadMemory32(address); // Use new ReadMemory32
            }
        }

        private void ExecuteLoadWordLeft(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            uint address = registers[baseReg] + (uint)offset;
            if (rt == 0)
                return;

            uint word = ReadMemory32(address & ~3u);
            int align = (int)(address & 3);
            uint dest = registers[rt];
            if (_bus.IsBigEndian)
            {
                switch (align)
                {
                    case 0: dest = word; break;
                    case 1: dest = (word << 8) | (dest & 0x000000FFu); break;
                    case 2: dest = (word << 16) | (dest & 0x0000FFFFu); break;
                    default: dest = (word << 24) | (dest & 0x00FFFFFFu); break;
                }
            }
            else
            {
                switch (align)
                {
                    case 0: dest = (word << 24) | (dest & 0x00FFFFFFu); break;
                    case 1: dest = (word << 16) | (dest & 0x0000FFFFu); break;
                    case 2: dest = (word << 8) | (dest & 0x000000FFu); break;
                    default: dest = word; break;
                }
            }
            registers[rt] = dest;
        }

        private void ExecuteLoadWordRight(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            uint address = registers[baseReg] + (uint)offset;
            if (rt == 0)
                return;

            uint word = ReadMemory32(address & ~3u);
            int align = (int)(address & 3);
            uint dest = registers[rt];
            if (_bus.IsBigEndian)
            {
                switch (align)
                {
                    case 0: dest = (word >> 24) | (dest & 0xFFFFFF00u); break;
                    case 1: dest = (word >> 16) | (dest & 0xFFFF0000u); break;
                    case 2: dest = (word >> 8) | (dest & 0xFF000000u); break;
                    default: dest = word; break;
                }
            }
            else
            {
                switch (align)
                {
                    case 0: dest = word; break;
                    case 1: dest = (word >> 8) | (dest & 0xFF000000u); break;
                    case 2: dest = (word >> 16) | (dest & 0xFFFF0000u); break;
                    default: dest = (word >> 24) | (dest & 0xFFFFFF00u); break;
                }
            }
            registers[rt] = dest;
        }

        private void ExecuteLoadHalf(uint instruction, bool unsigned)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            uint address = registers[baseReg] + (uint)offset;
            if (rt != 0)
            {
                ushort half = (ushort)ReadMemory16(address);
                registers[rt] = unsigned ? half : (uint)(short)half;
            }
        }

        private void ExecuteLoadByte(uint instruction, bool unsigned)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            uint address = registers[baseReg] + (uint)offset;
            if (rt != 0)
            {
                byte value = ReadMemory8(address);
                registers[rt] = unsigned ? value : (uint)(sbyte)value;
            }
        }

        private void ExecuteStoreWord(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            uint address = registers[baseReg] + (uint)offset;
            WriteMemory32(address, registers[rt]); // Use new WriteMemory32
        }

        private void ExecuteStoreWordLeft(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            uint address = registers[baseReg] + (uint)offset;
            uint aligned = address & ~3u;
            uint word = ReadMemory32(aligned);
            int align = (int)(address & 3);
            uint src = registers[rt];
            if (_bus.IsBigEndian)
            {
                switch (align)
                {
                    case 0: word = src; break;
                    case 1: word = (src >> 8) | (word & 0xFF000000u); break;
                    case 2: word = (src >> 16) | (word & 0xFFFF0000u); break;
                    default: word = (src >> 24) | (word & 0xFFFFFF00u); break;
                }
            }
            else
            {
                switch (align)
                {
                    case 0: word = (src >> 24) | (word & 0xFFFFFF00u); break;
                    case 1: word = (src >> 16) | (word & 0xFFFF0000u); break;
                    case 2: word = (src >> 8) | (word & 0xFF000000u); break;
                    default: word = src; break;
                }
            }
            WriteMemory32(aligned, word);
        }

        private void ExecuteStoreWordRight(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            uint address = registers[baseReg] + (uint)offset;
            uint aligned = address & ~3u;
            uint word = ReadMemory32(aligned);
            int align = (int)(address & 3);
            uint src = registers[rt];
            if (_bus.IsBigEndian)
            {
                switch (align)
                {
                    case 0: word = (src << 24) | (word & 0x00FFFFFFu); break;
                    case 1: word = (src << 16) | (word & 0x0000FFFFu); break;
                    case 2: word = (src << 8) | (word & 0x000000FFu); break;
                    default: word = src; break;
                }
            }
            else
            {
                switch (align)
                {
                    case 0: word = src; break;
                    case 1: word = (src << 8) | (word & 0x000000FFu); break;
                    case 2: word = (src << 16) | (word & 0x0000FFFFu); break;
                    default: word = (src << 24) | (word & 0x00FFFFFFu); break;
                }
            }
            WriteMemory32(aligned, word);
        }

        private void ExecuteStoreHalf(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            uint address = registers[baseReg] + (uint)offset;
            WriteMemory16(address, registers[rt]);
        }

        private void ExecuteStoreByte(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            uint address = registers[baseReg] + (uint)offset;
            WriteMemory8(address, (byte)registers[rt]);
        }

        private void ExecuteBranchEqual(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            short offset = (short)(instruction & 0xFFFF); // Sign-extended offset

            if (registers[rs] == registers[rt])
                TakeBranch(offset, "BEQ");
        }

        private void ExecuteBranchNotEqual(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            short offset = (short)(instruction & 0xFFFF);

            if (registers[rs] != registers[rt])
                TakeBranch(offset, "BNE");
        }

        private void ExecuteRegimm(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            short offset = (short)(instruction & 0xFFFF);
            bool negative = (int)registers[rs] < 0;
            bool taken;
            string name;

            switch (rt)
            {
                case 0x00:
                    taken = negative;
                    name = "BLTZ";
                    break;
                case 0x01:
                    taken = !negative;
                    name = "BGEZ";
                    break;
                case 0x10:
                    registers[31] = programCounter + 4;
                    taken = negative;
                    name = "BLTZAL";
                    break;
                case 0x11:
                    registers[31] = programCounter + 4;
                    taken = !negative;
                    name = "BGEZAL";
                    break;
                default:
                    TriggerException(10);
                    return;
            }

            if (taken)
                TakeBranch(offset, name);
        }

        private void ExecuteBranchVsZero(uint instruction, bool greaterThan)
        {
            uint rs = (instruction >> 21) & 0x1F;
            short offset = (short)(instruction & 0xFFFF);
            int value = (int)registers[rs];
            bool taken = greaterThan ? value > 0 : value <= 0;
            if (taken)
                TakeBranch(offset, greaterThan ? "BGTZ" : "BLEZ");
        }

        private void TakeBranch(short offset, string name)
        {
            uint oldPc = programCounter;
            uint target = programCounter + (uint)(offset << 2);
            ExecuteDelaySlotThenJump(target);
            LogBranch(oldPc, programCounter, name);
        }
        
        private void ExecuteJumpRegister(uint instruction)
        {
            uint oldPc = programCounter;
            uint rs = (instruction >> 21) & 0x1F;
            uint target = registers[rs];
            ExecuteDelaySlotThenJump(target);
            LogBranch(oldPc, programCounter, "JR");
        }

        private void ExecuteJumpAndLinkRegister(uint instruction)
        {
            uint oldPc = programCounter;
            uint rs = (instruction >> 21) & 0x1F;
            uint rd = (instruction >> 11) & 0x1F;
            uint target = registers[rs];
            if (rd != 0)
                registers[rd] = programCounter + 4;
            if (target == CeRomTocFiles.Win32SetFilePointer
                && CeRomTocFiles.IsTv2FileHandle(registers[4]))
            {
                if (_inDelaySlot)
                {
                    programCounter = target;
                    return;
                }
                _inDelaySlot = true;
                try
                {
                    uint delayInstr = FetchInstruction();
                    DecodeAndExecute(delayInstr);
                    if (CeRomTocFiles.TryServeTv2SetFilePointer(registers, target, ref target))
                    {
                        programCounter = target;
                        return;
                    }
                    programCounter = target;
                }
                catch (TlbMissException ex)
                {
                    TriggerTlbException(ex);
                }
                finally
                {
                    _inDelaySlot = false;
                }
                LogBranch(oldPc, programCounter, "JALR");
                return;
            }
            ExecuteDelaySlotThenJump(target);
            LogBranch(oldPc, programCounter, "JALR");
        }

        private static void HandleEmulatorError(string message)
        {
            Console.WriteLine($"Emulator Error: {message}");
            // In a real app, this might show a dialog or stop the emulation.
        }

        private void ExecuteAddImmediate(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int imm = (short)(instruction & 0xFFFF);
            if (rt != 0)
            {
                registers[rt] = registers[rs] + (uint)imm;
            }
        }

        private void ExecuteAddImmediateUnsigned(uint instruction)
        {
            ExecuteAddImmediate(instruction);
        }

        private void ExecuteSetLessThanImmediate(uint instruction)
        {
            ExecuteSlti(instruction);
        }

        private void ExecuteSetLessThanImmediateUnsigned(uint instruction)
        {
            ExecuteSltiu(instruction);
        }

        private void ExecuteLoadUpperImmediate(uint instruction)
        {
            ExecuteLui(instruction);
        }

        private void ExecuteAndImmediate(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint imm = instruction & 0xFFFF;
            if (rt != 0)
            {
                registers[rt] = registers[rs] & imm;
            }
        }

        private void ExecuteOrImmediate(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint imm = instruction & 0xFFFF;
            if (rt != 0)
            {
                registers[rt] = registers[rs] | imm;
            }
        }

        private void ExecuteXorImmediate(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint imm = instruction & 0xFFFF;
            if (rt != 0)
            {
                registers[rt] = registers[rs] ^ imm;
            }
        }

        public uint GetRegister(int index)
        {
            if (index < 0 || index >= registers.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return registers[index];
        }

        public void SetRegister(int index, uint value)
        {
            if (index < 0 || index >= registers.Length) throw new ArgumentOutOfRangeException(nameof(index));
            registers[index] = value;
        }

        // Convenience helpers for callers that reference named registers (used by boot manager code)
        public uint GetRegister(Register reg)
        {
            switch (reg)
            {
                case Register.PC: return programCounter;
                case Register.SP: return registers[29];
                case Register.RA: return registers[31];
                case Register.V0: return registers[2];
                case Register.V1: return registers[3];
                default:
                    // For argument registers and others, map where possible
                    return reg switch
                    {
                        Register.A0 => registers[4],
                        Register.A1 => registers[5],
                        Register.A2 => registers[6],
                        Register.A3 => registers[7],
                        _ => 0u
                    };
            }
        }

        public void SetRegister(Register reg, uint value)
        {
            switch (reg)
            {
                case Register.PC:
                    programCounter = value;
                    break;
                case Register.SP:
                    registers[29] = value;
                    break;
                case Register.RA:
                    registers[31] = value;
                    break;
                case Register.V0:
                    registers[2] = value;
                    break;
                case Register.V1:
                    registers[3] = value;
                    break;
                case Register.A0:
                    registers[4] = value; break;
                case Register.A1:
                    registers[5] = value; break;
                case Register.A2:
                    registers[6] = value; break;
                case Register.A3:
                    registers[7] = value; break;
                default:
                    // no-op for unknown/unused named registers
                    break;
            }
        }

        public uint ProgramCounter => programCounter;

        public uint Hi => hi;
        public uint Lo => lo;

        private void LogSyscall(uint syscallCode, uint instruction)
        {
            try
            {
                string logEntry = $"[SYSCALL] PC: 0x{programCounter:X8}, Syscall ID: 0x{syscallCode:X}, Instruction: 0x{instruction:X8}, R2($v0): 0x{registers[2]:X8}, R3($v1): 0x{registers[3]:X8}\n";
                File.AppendAllText(_logFilePath, logEntry);
                OnLogMessage?.Invoke(logEntry); // Invoke the event for UI
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing to syscall log: {ex.Message}");
            }
        }

        private static void LogBranch(uint oldPc, uint newPc, string branchType)
        {
        }

        // Public wrapper used by other components to dispatch a single instruction
        // This allows InstructionDispatcher to forward instructions to this emulator.
        public void DispatchInstruction(uint instruction, string sourceArch)
        {
            DecodeAndExecute(instruction);
        }

        // Simple run loop used by legacy callers that expect a continuous emulation.
        // Runs until the hosting thread is aborted or an exception occurs.
        public void Run()
        {
            while (true)
            {
                Step(1);
            }
        }
    }
}