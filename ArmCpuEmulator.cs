using System;
using System.Collections.Generic;

namespace ProcessorEmulator.Emulation
{
    public class ArmCpuEmulator
    {
        private const int RegisterCount = 16; // General-purpose registers: R0-R15
        private const int MemorySize = 1024 * 1024; // 1 MB of memory

        private uint[] registers;
        private byte[] memory;
        private uint programCounter;

        public ArmCpuEmulator()
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
            uint opcode = (instruction >> 21) & 0x7FF;
            switch (opcode)
            {
                case 0x458: // add
                    ExecuteAdd(instruction);
                    break;
                case 0x450: // sub
                    ExecuteSub(instruction);
                    break;
                case 0x488: // mov
                    ExecuteMov(instruction);
                    break;
                default:
                    HandleException($"Unsupported opcode: {opcode:X3}");
                    break;
            }
        }

        private void ExecuteAdd(uint instruction)
        {
            uint rn = (instruction >> 16) & 0xF;
            uint rd = (instruction >> 12) & 0xF;
            uint operand2 = instruction & 0xFFF;

            registers[rd] = registers[rn] + operand2;
        }

        private void ExecuteSub(uint instruction)
        {
            uint rn = (instruction >> 16) & 0xF;
            uint rd = (instruction >> 12) & 0xF;
            uint operand2 = instruction & 0xFFF;

            registers[rd] = registers[rn] - operand2;
        }

        private void ExecuteMov(uint instruction)
        {
            uint rd = (instruction >> 12) & 0xF;
            uint operand2 = instruction & 0xFFF;

            registers[rd] = operand2;
        }

        private void HandleException(string message)
        {
            Console.WriteLine($"Exception: {message}");
            // Implement exception handling logic here
        }
    }
}