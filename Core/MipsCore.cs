
namespace ProcessorEmulator.Core
{
    /// <summary>
    /// A MIPS I compliant CPU core. Fetches, decodes, and executes instructions.
    /// </summary>
    public class MipsCore
    {
        private readonly MipsCpuState _state;
        private readonly MipsBus _bus;
        private readonly CP0 _cp0;

        public MipsCore(MipsBus bus, CP0 cp0, uint startPC = 0)
        {
            _bus = bus;
            _cp0 = cp0;
            _state = new MipsCpuState(startPC);
        }

        /// <summary>
        /// Executes a single CPU instruction cycle.
        /// </summary>
        public void Step()
        {
            // Fetch instruction from the address pointed to by the Program Counter
            uint instructionWord = _bus.Read32(_state.PC);
            
            // Advance the PC to the next instruction.
            // Note: Branch instructions will modify this further.
            _state.PC += 4;

            // Decode and execute the instruction
            Execute(MipsDecoder.Decode(instructionWord));
        }

        private void Execute(MipsInstruction instr)
        {
            switch (instr.Opcode)
            {
                case 0x00: // R-Type instructions
                    ExecuteRType(instr);
                    break;
                case 0x02: // J
                    _state.PC = (_state.PC & 0xF0000000) | (instr.Addr << 2);
                    break;
                case 0x03: // JAL
                    _state.SetRegister(31, _state.PC); // Store return address in $ra
                    _state.PC = (_state.PC & 0xF0000000) | (instr.Addr << 2);
                    break;
                case 0x04: // BEQ
                    if (_state.GetRegister((int)instr.Rs) == _state.GetRegister((int)instr.Rt))
                        _state.PC += (uint)(instr.ImmSigned << 2);
                    break;
                case 0x05: // BNE
                    if (_state.GetRegister((int)instr.Rs) != _state.GetRegister((int)instr.Rt))
                        _state.PC += (uint)(instr.ImmSigned << 2);
                    break;
                case 0x09: // ADDIU
                    _state.SetRegister((int)instr.Rt, _state.GetRegister((int)instr.Rs) + (uint)instr.ImmSigned);
                    break;
                case 0x0D: // ORI
                    _state.SetRegister((int)instr.Rt, _state.GetRegister((int)instr.Rs) | instr.Imm);
                    break;
                case 0x0F: // LUI
                    _state.SetRegister((int)instr.Rt, instr.Imm << 16);
                    break;
                case 0x23: // LW
                    {
                        uint addr = _state.GetRegister((int)instr.Rs) + (uint)instr.ImmSigned;
                        uint value = _bus.Read32(addr);
                        _state.SetRegister((int)instr.Rt, value);
                    }
                    break;
                case 0x2B: // SW
                    {
                        uint addr = _state.GetRegister((int)instr.Rs) + (uint)instr.ImmSigned;
                        _bus.Write32(addr, _state.GetRegister((int)instr.Rt));
                    }
                    break;
                default:
                    throw new IllegalInstructionException($"Unimplemented opcode 0x{instr.Opcode:X2}");
            }
        }

        private void ExecuteRType(MipsInstruction instr)
        {
            switch (instr.Funct)
            {
                case 0x00: // SLL
                    _state.SetRegister((int)instr.Rd, _state.GetRegister((int)instr.Rt) << (int)instr.Shamt);
                    break;
                case 0x08: // JR
                    _state.PC = _state.GetRegister((int)instr.Rs);
                    break;
                case 0x21: // ADDU
                    _state.SetRegister((int)instr.Rd, _state.GetRegister((int)instr.Rs) + _state.GetRegister((int)instr.Rt));
                    break;
                case 0x25: // OR
                    _state.SetRegister((int)instr.Rd, _state.GetRegister((int)instr.Rs) | _state.GetRegister((int)instr.Rt));
                    break;
                default:
                    throw new IllegalInstructionException($"Unimplemented R-Type function 0x{instr.Funct:X2}");
            }
        }
    }
}
