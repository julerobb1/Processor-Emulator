using System;

public class MipsCpuEmulator
{
    private uint[] registers = new uint[32];
    private byte[] memory = new byte[4096];
    private uint programCounter = 0;

    public void LoadProgram(byte[] program, uint baseAddress = 0)
    {
        Array.Copy(program, 0, memory, baseAddress, program.Length);
        programCounter = baseAddress;
    }

    public uint GetRegister(int idx) => registers[idx];
    public void SetRegister(int idx, uint val) => registers[idx] = val;
    public uint ProgramCounter => programCounter;

    private uint FetchInstruction(uint pc)
    {
        return BitConverter.ToUInt32(memory, (int)pc);
    }

    private void ExecuteInstruction(uint instr)
    {
        uint opcode = instr >> 26;
        if (opcode == 0x8) // ADDI
        {
            uint rs = (instr >> 21) & 0x1F;
            uint rt = (instr >> 16) & 0x1F;
            int imm = (short)(instr & 0xFFFF);
            registers[rt] = registers[rs] + (uint)imm;
            programCounter += 4;
        }
        else
        {
            programCounter += 4;
        }
    }

    public void Step(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            uint pc = programCounter;
            uint instr = FetchInstruction(pc);
            uint opcode = instr >> 26;
            if (opcode == 0x4) // BEQ
            {
                uint rs = (instr >> 21) & 0x1F;
                uint rt = (instr >> 16) & 0x1F;
                int imm = (short)(instr & 0xFFFF);
                int offset = imm; // sign-extended

                // Execute delay slot at PC+4
                uint delayInstr = FetchInstruction(pc + 4);
                // advance PC past delay slot
                programCounter = pc + 8;
                // Execute delay slot instruction
                ExecuteInstruction(delayInstr);

                if (registers[rs] == registers[rt])
                {
                    uint target = pc + 4 + (uint)(offset << 2);
                    programCounter = target;
                }
            }
            else
            {
                ExecuteInstruction(instr);
            }
        }
    }
}
