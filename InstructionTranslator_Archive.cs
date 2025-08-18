using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessorEmulator
{
    /// <summary>
    /// ARCHIVED: Old stub version of InstructionTranslator for reference/spare
    /// </summary>
    public class InstructionTranslator_Archive
    {
        private Dictionary<string, Action> instructionHandlers = new Dictionary<string, Action>();
        public InstructionTranslator_Archive() { InitializeHandlers(); }
        private void InitializeHandlers()
        {
            instructionHandlers["ARM_DataProcessing"] = HandleArmDataProcessing;
            instructionHandlers["ARM_Mov"] = HandleArmMov;
            instructionHandlers["ARM_Ldr"] = HandleArmLdr;
            instructionHandlers["ARM_Ldm"] = HandleArmLdm;
            instructionHandlers["ARM_Bx"] = HandleArmBx;
            instructionHandlers["ARM_Bl"] = HandleArmBl;
            instructionHandlers["ARM_Svc"] = HandleArmSvc;
            instructionHandlers["MIPS_Lui"] = HandleMipsLui;
            instructionHandlers["MIPS_Lw"] = HandleMipsLw;
            instructionHandlers["MIPS_Sw"] = HandleMipsSw;
            instructionHandlers["MIPS_Jal"] = HandleMipsJal;
            instructionHandlers["MIPS_Jr"] = HandleMipsJr;
            instructionHandlers["MIPS_Syscall"] = HandleMipsSyscall;
        }
        private void HandleArmDataProcessing() { }
        private void HandleArmMov() { }
        private void HandleArmLdr() { }
        private void HandleArmLdm() { }
        private void HandleArmBx() { }
        private void HandleArmBl() { }
        private void HandleArmSvc() { }
        private void HandleMipsLui() { }
        private void HandleMipsLw() { }
        private void HandleMipsSw() { }
        private void HandleMipsJal() { }
        private void HandleMipsJr() { }
        private void HandleMipsSyscall() { }
        private Action FindInstructionHandler(string name) => instructionHandlers.ContainsKey(name) ? instructionHandlers[name] : null;
        private Action FindArmHandler(string name) => instructionHandlers.ContainsKey(name) ? instructionHandlers[name] : null;
        private Action FindMipsHandler(string name) => instructionHandlers.ContainsKey(name) ? instructionHandlers[name] : null;
        private Action FindMipsSpecialHandler(string name) => instructionHandlers.ContainsKey(name) ? instructionHandlers[name] : null;
        private uint RotateRight(uint value, int count) => (value >> count) | (value << (32 - count));
    }
}
