using System;
using System.IO;
using System.Windows;

namespace ProcessorEmulator.Emulation
{
    public class MipsCpuEmulator
    {
        private const int RegisterCount = 32;

        private uint[] registers;
        private uint programCounter;
        private float[] floatingPointRegisters;
        private readonly CP0 _cp0;
        private readonly MipsBus _bus;

        public MipsCpuEmulator(MipsBus bus, CP0 cp0)
        {
            _bus = bus;
            _cp0 = cp0;
            registers = new uint[RegisterCount];
            floatingPointRegisters = new float[RegisterCount];
            programCounter = 0xBFC00000; // MIPS Reset Vector
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
                    // The exception has changed the PC, so we continue to the next loop iteration
                    // to fetch from the new interrupt handler address.
                }

                uint instruction = FetchInstruction();
                DecodeAndExecute(instruction);

                // Advance the internal timer by one cycle per instruction.
                _cp0.UpdateTimer(1);
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
        }


        private uint FetchInstruction()
        {
            uint instruction = _bus.Read32(programCounter);
            Console.WriteLine($"PC: 0x{programCounter:X8} -> PADDR: 0x{_bus.Translate(programCounter):X8}, INSTR: 0x{instruction:X8}");
            programCounter += 4;
            return instruction;
        }
        
        private uint FetchInstructionAt(uint vaddr)
        {
            return _bus.Read32(vaddr);
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
                        ExecuteJ(instruction);
                        break;
                    case 0x03: // jal
                        ExecuteJal(instruction);
                        break;
                    case 0x10: // COP0 instructions
                        ExecuteCOP0(instruction);
                        break;
                    case 0x08: // addi
                        ExecuteAddImmediate(instruction);
                        break;
                    case 0x09: // addiu
                        ExecuteAddImmediate(instruction); // ADDIU is handled by ADDI logic, no overflow trapping
                        break;
                    case 0x0A: // slti
                        ExecuteSlti(instruction);
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
                        ExecuteLui(instruction);
                        break;
                    case 0x23: // lw
                        ExecuteLoadWord(instruction);
                        break;
                    case 0x2B: // sw
                        ExecuteStoreWord(instruction);
                        break;
                    case 0x04: // beq
                        ExecuteBranchEqual(instruction);
                        break;
                    case 0x05: // bne
                        ExecuteBranchNotEqual(instruction);
                        break;
                    // ...add more opcodes as needed...
                    default:
                        TriggerException(10); // 10 is Reserved Instruction exception
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catching emulator-level errors, not guest exceptions.
                HandleEmulatorError(ex.Message);
            }
        }

        private void ExecuteJ(uint instruction)
        {
            uint target = instruction & 0x3FFFFFF;
            programCounter = (programCounter & 0xF0000000) | (target << 2);
        }

        private void ExecuteJal(uint instruction)
        {
            registers[31] = programCounter + 4; // Return address is the instruction after the delay slot
            ExecuteJ(instruction);
        }

        // MTC0: Move Control to Coprocessor 0 (Write to CP0)
        // Format: mtc0 $rt, $rd
        public void Execute_MTC0(uint rt, uint rd)
        {
            uint value = registers[rt]; // Get value from general purpose register
            _cp0.WriteRegister((int)rd, value);
        }

        // MFC0: Move From Coprocessor 0 (Read from CP0)
        // Format: mfc0 $rt, $rd
        public void Execute_MFC0(uint rt, uint rd)
        {
            uint value = _cp0.ReadRegister((int)rd);
            registers[rt] = value; // Put CP0 value into general purpose register
        }

        private void ExecuteCOP0(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            
            // Check for ERET instruction
            if (rs == 0x10 && (instruction & 0x3F) == 0x18)
            {
                // ERET: Exception Return
                // 1. Clear Status.EXL bit
                _cp0.Status &= ~(1u << 1); 
                // 2. Jump back to where the exception occurred
                programCounter = _cp0.EPC;
                return;
            }
            
            uint rt = (instruction >> 16) & 0x1F;
            uint rd = (instruction >> 11) & 0x1F;

            switch (rs)
            {
                case 0x00: // MFC0
                    Execute_MFC0(rt, rd);
                    break;
                case 0x04: // MTC0
                    Execute_MTC0(rt, rd);
                    break;
                case 0x10: // TLB operations (and ERET)
                    uint funct = instruction & 0x3F;
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
                            // Already handled above, but good to have here for completeness/future refactor
                            _cp0.Status &= ~(1u << 1); 
                            programCounter = _cp0.EPC;
                            break;
                        default:
                            TriggerException(10); // Reserved Instruction
                            break;
                    }
                    break;
                default:
                    TriggerException(10); // Reserved Instruction
                    break;
            }
        }


        private void ExecuteRType(uint instruction)
        {
            uint funct = instruction & 0x3F;
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint rd = (instruction >> 11) & 0x1F;
            uint shamt = (instruction >> 6) & 0x1F;

            // Register 0 is hardwired to zero
            if (rd == 0) return;

            switch (funct)
            {
                case 0x20: // add
                    registers[rd] = registers[rs] + registers[rt];
                    break;
                case 0x22: // sub
                    registers[rd] = registers[rs] - registers[rt];
                    break;
                case 0x24: // and
                    registers[rd] = registers[rs] & registers[rt];
                    break;
                case 0x25: // or
                    registers[rd] = registers[rs] | registers[rt];
                    break;
                case 0x27: // nor
                    registers[rd] = ~(registers[rs] | registers[rt]);
                    break;
                case 0x00: // sll
                    registers[rd] = registers[rt] << (int)shamt;
                    break;
                case 0x02: // srl
                    registers[rd] = registers[rt] >> (int)shamt;
                    break;
                case 0x04: // sllv
                    registers[rd] = registers[rt] << (int)registers[rs];
                    break;
                case 0x06: // srlv
                    registers[rd] = registers[rt] >> (int)registers[rs];
                    break;
                case 0x08: // jr
                    ExecuteJumpRegister(instruction);
                    break;
                case 0x0C: // syscall
                     TriggerException(8); // Syscall exception
                     break;
                default:
                    TriggerException(10); // Reserved Instruction
                    break;
            };
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

        private void ExecuteLoadWord(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            uint address = registers[baseReg] + (uint)offset;
            if (rt != 0) // writes to R0 are discarded
            {
                registers[rt] = _bus.Read32(address);
            }
        }

        private void ExecuteStoreWord(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            uint address = registers[baseReg] + (uint)offset;
            _bus.Write32(address, registers[rt]);
        }

        private void ExecuteBranchEqual(uint instruction)
        {
            // Note: This is a simplified implementation that does NOT handle the branch delay slot correctly
            // for exceptions. A full implementation would require more complex pipeline management.
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);
            
            uint branchPC = programCounter; // PC is already advanced to next instruction
            if (registers[rs] == registers[rt])
            {
                programCounter = (branchPC) + (uint)(offset << 2);
            }
        }

        private void ExecuteBranchNotEqual(uint instruction)
        {
            // Note: Simplified implementation without correct delay slot exception handling.
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            uint branchPC = programCounter;
            if (registers[rs] != registers[rt])
            {
                programCounter = (branchPC) + (uint)(offset << 2);
            }
        }
        
        private void ExecuteJumpRegister(uint instruction)
        {
            // Note: Simplified implementation without correct delay slot exception handling.
            uint rs = (instruction >> 21) & 0x1F;
            programCounter = registers[rs];
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

        public uint ProgramCounter => programCounter;
    }
}