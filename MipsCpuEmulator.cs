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

        public MipsCpuEmulator()
        {
            registers = new uint[RegisterCount];
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
                default:
                    throw new NotSupportedException($"Opcode {opcode:X2} not supported.");
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
    }
}