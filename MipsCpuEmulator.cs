using System;
using System.IO;
using System.Windows;

namespace ProcessorEmulator.Emulation
{
    public enum MipsChipsetProfile
    {
        Generic,
        STi7101,
        STi7111,
        BCM7401,   BCM7405,      BCM7403,
        BCM7425,
        BCM7445
    }

    public class MipsCpuEmulator
    {
        private const int RegisterCount = 32;

        private uint[] registers;
        private uint programCounter;
        private float[] floatingPointRegisters;
        private CP0 cp0;
        private MipsBus _bus;

        // Hardware module stubs
        private VideoDecoderStub videoDecoder;
        private AudioDecoderStub audioDecoder;
        private SecurityModuleStub securityModule;
        private PeripheralStub peripheralModule;

        public MipsChipsetProfile ChipsetProfile { get; private set; }

        public MipsCpuEmulator(MipsChipsetProfile profile = MipsChipsetProfile.Generic)
        {
            ChipsetProfile = profile;
            registers = new uint[RegisterCount];
            floatingPointRegisters = new float[RegisterCount];
            _bus = new MipsBus(128); // 128MB RAM
            cp0 = new CP0(); // Instantiate CP0
            programCounter = 0xBFC00000; // MIPS Reset Vector

            // Clear Status Register on startup
            cp0.WriteRegister(12, 0);

            // Initialize hardware stubs
            videoDecoder = new VideoDecoderStub();
            audioDecoder = new AudioDecoderStub();
            securityModule = new SecurityModuleStub();
            peripheralModule = new PeripheralStub();
        }

        public void LoadNkBin(string filePath)
        {
            // Example: Loading the boot image into the physical ROM area
            byte[] bootImage = File.ReadAllBytes(filePath);
            uint romPhysicalBase = 0x1FC00000;
            _bus.LoadRawBinary(romPhysicalBase, bootImage);
        }

        public void LoadProgram(byte[] program, uint startAddress)
        {
            uint physicalAddress = _bus.Translate(startAddress);
            _bus.LoadRawBinary(physicalAddress, program);
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
            switch (opcode)
            {
                case 0x00: // R-type instructions
                    ExecuteRType(instruction);
                    break;
                case 0x10: // COP0 instructions
                    ExecuteCOP0(instruction);
                    break;
                case 0x08: // addi
                    ExecuteAddImmediate(instruction);
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
                    throw new NotSupportedException($"Opcode {opcode:X2} not supported.");
            }
        }

        // MTC0: Move Control to Coprocessor 0 (Write to CP0)
        // Format: mtc0 $rt, $rd
        public void Execute_MTC0(uint rt, uint rd)
        {
            uint value = registers[rt]; // Get value from general purpose register
            cp0.WriteRegister((int)rd, value);
            Console.WriteLine($"CP0 Write: Reg {rd} = 0x{value:X8}");
        }

        // MFC0: Move From Coprocessor 0 (Read from CP0)
        // Format: mfc0 $rt, $rd
        public void Execute_MFC0(uint rt, uint rd)
        {
            uint value = cp0.ReadRegister((int)rd);
            registers[rt] = value; // Put CP0 value into general purpose register
            Console.WriteLine($"CP0 Read: Reg {rd} returns 0x{value:X8}");
        }

        private void ExecuteCOP0(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
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
                default:
                    throw new NotSupportedException($"COP0 instruction with rs={rs} not supported.");
            }
        }


        private void ExecuteRType(uint instruction)
        {
            uint funct = instruction & 0x3F;
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint rd = (instruction >> 11) & 0x1F;
            uint shamt = (instruction >> 6) & 0x1F;

            registers[rd] = funct switch
            {
                // add
                0x20 => registers[rs] + registers[rt],
                // sub
                0x22 => registers[rs] - registers[rt],
                // and
                0x24 => registers[rs] & registers[rt],
                // or
                0x25 => registers[rs] | registers[rt],
                // nor
                0x27 => ~(registers[rs] | registers[rt]),
                // sll
                0x00 => registers[rt] << (int)shamt,
                // srl
                0x02 => registers[rt] >> (int)shamt,
                _ => throw new NotSupportedException($"Function {funct:X2} not supported."),
            };
        }

        private void ExecuteLoadWord(uint instruction)
        {
            uint baseReg = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            uint address = registers[baseReg] + (uint)offset;
            registers[rt] = _bus.Read32(address);
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
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            // MIPS has a branch delay slot: the instruction immediately after
            // the branch is executed before the branch target is taken.
            uint pcAfterFetch = programCounter; // address of delay-slot instruction

            if (registers[rs] == registers[rt])
            {
                // Fetch and execute the delay-slot instruction explicitly
                uint delayInstr = FetchInstructionAt(pcAfterFetch);
                // Advance PC past the delay-slot instruction as if it was fetched normally
                programCounter = pcAfterFetch + 4;
                DecodeAndExecute(delayInstr);

                // Now take the branch target (target = pcAfterFetch + (offset<<2))
                programCounter = pcAfterFetch + (uint)(offset << 2);
            }
        }

        private void ExecuteBranchNotEqual(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int offset = (short)(instruction & 0xFFFF);

            // Handle branch delay slot as with other conditional branches
            uint pcAfterFetch = programCounter; // address of delay-slot instruction

            if (registers[rs] != registers[rt])
            {
                uint delayInstr = FetchInstructionAt(pcAfterFetch);
                programCounter = pcAfterFetch + 4;
                DecodeAndExecute(delayInstr);
                programCounter = pcAfterFetch + (uint)(offset << 2);
            }
        }

        private void ExecuteFloatingPoint(uint instruction)
        {
            uint fmt = (instruction >> 21) & 0x1F;
            uint ft = (instruction >> 16) & 0x1F;
            uint fs = (instruction >> 11) & 0x1F;
            uint fd = (instruction >> 6) & 0x1F;
            uint funct = instruction & 0x3F;

            floatingPointRegisters[fd] = funct switch
            {
                // add.s
                0x00 => floatingPointRegisters[fs] + floatingPointRegisters[ft],
                // sub.s
                0x01 => floatingPointRegisters[fs] - floatingPointRegisters[ft],
                // mul.s
                0x02 => floatingPointRegisters[fs] * floatingPointRegisters[ft],
                // div.s
                0x03 => floatingPointRegisters[fs] / floatingPointRegisters[ft],
                _ => throw new NotSupportedException($"Floating-point function {funct:X2} not supported."),
            };
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
                    // jr also honors the branch delay slot: execute next instruction
                    // before jumping to the register target.
                    uint pcAfterFetch = programCounter;
                    uint delayInstr = FetchInstructionAt(pcAfterFetch);
                    // Advance PC past delay slot
                    programCounter = pcAfterFetch + 4;
                    DecodeAndExecute(delayInstr);

                    // Now jump to register target
                    programCounter = registers[rs];
                    break;
                default:
                    throw new NotSupportedException($"System function {funct:X2} not supported.");
            }
        }

        private void HandleSyscall()
        {
            uint syscallCode = registers[2]; // v0 register
            switch (syscallCode)
            {
                case 1: // Print integer
                    Console.WriteLine(registers[4]); // a0 register
                    break;
                case 4: // Print string
                    uint address = registers[4];
                    while (_bus.Read8(address) != 0)
                    {
                        Console.Write((char)_bus.Read8(address));
                        address++;
                    }
                    Console.WriteLine();
                    break;
                default:
                    throw new NotSupportedException($"Syscall {syscallCode} not supported.");
            }
        }

        private static void HandleException(string message)
        {
            Console.WriteLine($"Exception: {message}");
            // Implement exception handling logic here
        }

        private void ExecuteAddImmediate(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            int imm = (short)(instruction & 0xFFFF);
            registers[rt] = registers[rs] + (uint)imm;
        }

        private void ExecuteAndImmediate(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint imm = instruction & 0xFFFF;
            registers[rt] = registers[rs] & imm;
        }

        private void ExecuteOrImmediate(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint imm = instruction & 0xFFFF;
            registers[rt] = registers[rs] | imm;
        }

        private void ExecuteXorImmediate(uint instruction)
        {
            uint rs = (instruction >> 21) & 0x1F;
            uint rt = (instruction >> 16) & 0x1F;
            uint imm = instruction & 0xFFFF;
            registers[rt] = registers[rs] ^ imm;
        }

        // Dispatcher interface for unified translation
        public void DispatchInstruction(uint instruction, string targetArch)
        {
            if (targetArch == "MIPS")
            {
                DecodeAndExecute(instruction);
            }
            else
            {
                // Translate to target architecture (e.g., x64) and execute
                // Placeholder: Implement translation logic here
            }
        }

        // Backwards-compatible bridge: allow callers to dispatch without specifying target arch
        public void DispatchInstruction(uint instruction)
        {
            DecodeAndExecute(instruction);
        }

        // Testing helpers: expose register access and stepping for unit tests
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

        // Expose minimal hardware stubs publicly so UI code can access them
        public VideoDecoderStub VideoDecoder => videoDecoder;
        public AudioDecoderStub AudioDecoder => audioDecoder;

        // Execute a single fetch/decode/execute cycle (or multiple cycles)
        public void Step(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                uint instruction = FetchInstruction();
                DecodeAndExecute(instruction);
            }
        }

        // Connect UI input (example for WPF)
        public void ConnectUIInput(Window window)
        {
            window.KeyDown += (s, e) => peripheralModule.HandleKeyboardInput((ConsoleKey)Enum.Parse(typeof(ConsoleKey), e.Key.ToString(), true));
            // Mouse click mapping can be added as needed
        }
    }

    // Hardware module stubs
    public class VideoDecoderStub
    {
        // Minimal state to avoid null reference usage by UI
        public int Width { get; private set; } = 720;
        public int Height { get; private set; } = 480;

        public void Initialize(int width = 720, int height = 480)
        {
            Width = width;
            Height = height;
        }

        public void RenderFrame(byte[] frameData)
        {
            // No-op for headless testing; keep method to satisfy callers
        }

        public void Reset()
        {
            // Reset internal state if later extended
        }
    }

    public class AudioDecoderStub
    {
        // Minimal audio buffer simulation
        private readonly System.Collections.Generic.Queue<byte[]> buffer = new System.Collections.Generic.Queue<byte[]>();

        public void Initialize(int sampleRate = 48000, int channels = 2)
        {
            // store or ignore for now
        }

        public void EnqueueAudio(byte[] pcmData)
        {
            if (pcmData != null && pcmData.Length > 0) buffer.Enqueue(pcmData);
        }

        public byte[] DequeueAudio()
        {
            return buffer.Count > 0 ? buffer.Dequeue() : Array.Empty<byte>();
        }

        public void Reset()
        {
            buffer.Clear();
        }
    }
    public class SecurityModuleStub { /* Emulate smartcard, encryption, etc. */ }
    public class PeripheralStub
    {
        public event Action<string> RemoteButtonPressed;

        public void PressButton(string button)
        {
            RemoteButtonPressed?.Invoke(button);
        }

        // Map keyboard keys to remote buttons (full mapping)
        public void HandleKeyboardInput(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow: PressButton("UP"); break;
                case ConsoleKey.DownArrow: PressButton("DOWN"); break;
                case ConsoleKey.LeftArrow: PressButton("LEFT"); break;
                case ConsoleKey.RightArrow: PressButton("RIGHT"); break;
                case ConsoleKey.Enter: PressButton("OK"); break;
                case ConsoleKey.Escape: PressButton("EXIT"); break;
                case ConsoleKey.M: PressButton("MENU"); break;
                case ConsoleKey.G: PressButton("GUIDE"); break;
                case ConsoleKey.I: PressButton("INFO"); break;
                case ConsoleKey.D1: PressButton("1"); break;
                case ConsoleKey.D2: PressButton("2"); break;
                case ConsoleKey.D3: PressButton("3"); break;
                case ConsoleKey.D4: PressButton("4"); break;
                case ConsoleKey.D5: PressButton("5"); break;
                case ConsoleKey.D6: PressButton("6"); break;
                case ConsoleKey.D7: PressButton("7"); break;
                case ConsoleKey.D8: PressButton("8"); break;
                case ConsoleKey.D9: PressButton("9"); break;
                case ConsoleKey.D0: PressButton("0"); break;
                case ConsoleKey.P: PressButton("PAUSE"); break;
                case ConsoleKey.Spacebar: PressButton("PLAY"); break;
                case ConsoleKey.F: PressButton("FF"); break;
                case ConsoleKey.R: PressButton("REW"); break;
                case ConsoleKey.S: PressButton("STOP"); break;
                // ...add more as needed...
            }
        }

        public void HandleMouseClick()
        {
            PressButton("OK");
        }

        // Connect UI input (example for WPF)
        // Uncomment the following method and add 'using System.Windows;' if using WPF.
        /*
        public void ConnectUIInput(System.Windows.Window window)
        {
            window.KeyDown += (s, e) => HandleKeyboardInput((ConsoleKey)Enum.Parse(typeof(ConsoleKey), e.Key.ToString(), true));
            // Mouse click mapping can be added as needed
        }
        */
    }
}