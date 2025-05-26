using System;
using System.Collections.Generic;

namespace ProcessorEmulator.Emulation
{
    public class X86CpuEmulator
    {
        private const int RegisterCount = 8; // General-purpose registers: EAX, EBX, etc.
        private const int MemorySize = 1024 * 1024; // 1 MB of memory

        private uint[] registers;
        private byte[] memory;
        private uint instructionPointer;

        public X86CpuEmulator()
        {
            registers = new uint[RegisterCount];
            memory = new byte[MemorySize];
            instructionPointer = 0x0;
        }

        public void LoadProgram(byte[] program, uint startAddress)
        {
            Array.Copy(program, 0, memory, startAddress, program.Length);
            instructionPointer = startAddress;
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
            uint instruction = BitConverter.ToUInt32(memory, (int)instructionPointer);
            instructionPointer += 4;
            return instruction;
        }

        private void DecodeAndExecute(uint instruction)
        {
            byte opcode = (byte)(instruction & 0xFF);
            switch (opcode)
            {
                case 0x01: // add
                    ExecuteAdd(instruction);
                    break;
                case 0x29: // sub
                    ExecuteSub(instruction);
                    break;
                case 0x89: // mov
                    ExecuteMov(instruction);
                    break;
                default:
                    HandleException($"Unsupported opcode: {opcode:X2}");
                    break;
            }
        }

        private void ExecuteAdd(uint instruction)
        {
            // Example: add eax, ebx
            registers[0] += registers[1];
        }

        private void ExecuteSub(uint instruction)
        {
            // Example: sub eax, ebx
            registers[0] -= registers[1];
        }

        private void ExecuteMov(uint instruction)
        {
            // Example: mov eax, ebx
            registers[0] = registers[1];
        }

        private void HandleException(string message)
        {
            Console.WriteLine($"Exception: {message}");
            // Implement exception handling logic here
        }
    }
}