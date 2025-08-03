using System;

namespace ProcessorEmulator.Emulation
{
    public class ArmCpuEmulator : IEmulator
    {
        private const int RegisterCount = 16; // R0-R15
        private const int MemorySize = 1024 * 1024; // 1 MB
        private uint[] registers;
        private byte[] memory;
        private uint programCounter;

        public uint ProgramCounter { get => programCounter; set => programCounter = value; }
        public uint StackPointer { get; set; }
        public int InstructionCount { get; private set; }
        public uint CurrentInstruction { get; private set; }
        public uint[] RegisterState => registers;
        public byte[] MemoryState => memory;

        public ArmCpuEmulator()
        {
            registers = new uint[RegisterCount];
            memory = new byte[MemorySize];
            programCounter = 0x0;
        }

        public void LoadBinary(byte[] binary, uint loadAddress)
        {
            LoadProgram(binary, loadAddress);
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
                Step();
            }
        }

        public void Step()
        {
            uint instruction = FetchInstruction();
            DecodeAndExecute(instruction);
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
                case 0x418: // and
                    ExecuteAnd(instruction);
                    break;
                case 0x430: // orr
                    ExecuteOrr(instruction);
                    break;
                case 0x438: // eor
                    ExecuteEor(instruction);
                    break;
                // ...add more opcodes as needed...
                default:
                    throw new NotSupportedException($"Opcode {opcode:X3} not supported.");
            }
        }

        public void MapMemory(uint address, byte[] data)
        {
            if (address + data.Length > memory.Length)
            {
                throw new ArgumentOutOfRangeException("Memory map exceeds available memory.");
            }
            Array.Copy(data, 0, memory, address, data.Length);
        }

        public void RegisterDevice(IDeviceEmulator device)
        {
            // TODO: Implement device registration
            Console.WriteLine($"Device {device.GetType().Name} registered.");
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
        private void ExecuteAnd(uint instruction)
        {
            uint rn = (instruction >> 16) & 0xF;
            uint rd = (instruction >> 12) & 0xF;
            uint operand2 = instruction & 0xFFF;
            registers[rd] = registers[rn] & operand2;
        }
        private void ExecuteOrr(uint instruction)
        {
            uint rn = (instruction >> 16) & 0xF;
            uint rd = (instruction >> 12) & 0xF;
            uint operand2 = instruction & 0xFFF;
            registers[rd] = registers[rn] | operand2;
        }
        private void ExecuteEor(uint instruction)
        {
            uint rn = (instruction >> 16) & 0xF;
            uint rd = (instruction >> 12) & 0xF;
            uint operand2 = instruction & 0xFFF;
            registers[rd] = registers[rn] ^ operand2;
        }

        // Dispatcher interface for unified translation
        public void DispatchInstruction(uint instruction, string targetArch)
        {
            if (targetArch == "ARM" || targetArch == "ARM64")
            {
                DecodeAndExecute(instruction);
            }
            else
            {
                // Translate to target architecture (e.g., x64) and execute
                // Placeholder: Implement translation logic here
            }
        }
    }
}
