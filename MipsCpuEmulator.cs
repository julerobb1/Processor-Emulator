using System;
using System.Collections.Generic;

namespace ProcessorEmulator.Emulation
{
    public class MipsCpuEmulator
    {
        private const int RegisterCount = 32;
        private const int MemorySize = 1024 * 1024; // 1 MB of memory

        private uint[] registers;
        private byte[] memory;
        private uint programCounter;
        private float[] floatingPointRegisters;

        public MipsCpuEmulator()
        {
            registers = new uint[RegisterCount];
            floatingPointRegisters = new float[RegisterCount];
            memory = new byte[MemorySize];
            programCounter = 0x0;
        }

        public void LoadProgram(byte[] program, uint startAddress)
        {
            Array.Copy(program, 0, memory, startAddress, program.Length);
            programCounter = startAddress;
        }

        public void Run()
        {
            while (true)
            {
                uint instruction = FetchInstruction();
                DecodeAndExecute(instruction);
            }
        }

        private uint FetchInstruction()
        {
            uint instruction = BitConverter.ToUInt32(memory, (int)programCounter);
            programCounter += 4;
            return instruction;
        }

        private void DecodeAndExecute(uint instruction)
        {
            uint opcode = (instruction >> 26) & 0x3F;
            switch (opcode)
            {
                case 0x00: // R-type instructions
                    ExecuteRType(instruction);
                    break;
                case 0x11: // COP1 (Floating-point instructions)
                    ExecuteFloatingPoint(instruction);
                    break;
                case 0x1C: // DSP instructions
                    ExecuteDSPInstruction(instruction);
                    break;
                case 0x10: // System instructions
                    ExecuteSystemInstruction(instruction);
                    break;
                default:
                    HandleException($"Unsupported opcode: {opcode:X2}");
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
                default:
                    throw new NotSupportedException($"Function {funct:X2} not supported.");
            }
        }

        private void ExecuteLoadWord(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            uint address = registers[baseReg] + (uint)offset;
            registers[rt] = BitConverter.ToUInt32(memory, (int)address);
        }

        private void ExecuteStoreWord(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            uint address = registers[baseReg] + (uint)offset;
            byte[] data = BitConverter.GetBytes(registers[rt]);
            Array.Copy(data, 0, memory, (int)address, 4);
        }

        private void ExecuteBranchEqual(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            if (registers[rs] == registers[rt])
            {
                programCounter += (uint)(offset << 2);
            }
        }

        private void ExecuteBranchNotEqual(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            if (registers[rs] != registers[rt])
            {
                programCounter += (uint)(offset << 2);
            }
        }

        private void ExecuteFloatingPoint(uint instruction)
        {
            uint fmt = (instruction >> 21) & 0x1F;
            uint ft = (instruction >> 16) & 0x1F;
            uint fs = (instruction >> 11) & 0x1F;
            uint fd = (instruction >> 6) & 0x1F;
            uint funct = instruction & 0x3F;

            switch (funct)
            {
                case 0x00: // add.s
                    floatingPointRegisters[fd] = floatingPointRegisters[fs] + floatingPointRegisters[ft];
                    break;
                case 0x01: // sub.s
                    floatingPointRegisters[fd] = floatingPointRegisters[fs] - floatingPointRegisters[ft];
                    break;
                case 0x02: // mul.s
                    floatingPointRegisters[fd] = floatingPointRegisters[fs] * floatingPointRegisters[ft];
                    break;
                case 0x03: // div.s
                    floatingPointRegisters[fd] = floatingPointRegisters[fs] / floatingPointRegisters[ft];
                    break;
                default:
                    throw new NotSupportedException($"Floating-point function {funct:X2} not supported.");
            }
        }

        private void ExecuteDSPInstruction(uint instruction)
        {
            uint funct = instruction & 0x3F;
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint rd = (instruction >> 11) & 0x1F;

            switch (funct)
            {
                case 0x20: // madd (Multiply-Add)
                    registers[rd] += (uint)((int)registers[rs] * (int)registers[rt]);
                    break;
                case 0x21: // msub (Multiply-Subtract)
                    registers[rd] -= (uint)((int)registers[rs] * (int)registers[rt]);
                    break;
                default:
                    HandleException($"Unsupported DSP function: {funct:X2}");
                    break;
            }
        }

        private void ExecuteSystemInstruction(uint instruction)
        {
            uint funct = instruction & 0x3F;

            switch (funct)
            {
                case 0x0C: // syscall
                    HandleSyscall();
                    break;
                case 0x08: // jr (jump register)
                    uint rs = (instruction >> 21) & 0x1F;
                    programCounter = registers[rs];
                    break;
                default:
                    throw new NotSupportedException($"System function {funct:X2} not supported.");
            }
        }

        private void HandleSyscall()
        {
            uint syscallCode = registers[2]; // $v0 register
            switch (syscallCode)
            {
                case 1: // Print integer
                    Console.WriteLine(registers[4]); // $a0 register
                    break;
                case 4: // Print string
                    uint address = registers[4];
                    while (memory[address] != 0)
                    {
                        Console.Write((char)memory[address]);
                        address++;
                    }
                    Console.WriteLine();
                    break;
                default:
                    throw new NotSupportedException($"Syscall {syscallCode} not supported.");
            }
        }

        private void HandleException(string message)
        {
            Console.WriteLine($"Exception: {message}");
            // Implement exception handling logic here
        }
    }
}