using System;
using System.Collections.Generic;
using System.Numerics;

namespace ProcessorEmulator.Core
{
    /// <summary>
    /// Minimal IR value representation used for immediates inside the IR.
    /// This is a lightweight wrapper to allow the rest of the IR code to compile.
    /// </summary>
    public readonly struct IrValue
    {
        public readonly long SignedValue;
        public readonly ulong UnsignedValue;
        public IrValue(long v) { SignedValue = v; UnsignedValue = (ulong)v; }
        public IrValue(ulong v) { UnsignedValue = v; SignedValue = (long)v; }
        public override string ToString() => SignedValue.ToString();
    }

    /// <summary>
    /// Defines the type of an IR operand.
    /// </summary>
    public enum IrOperandType
    {
        /// <summary>
        /// A temporary variable, internal to the IR, used to hold intermediate values.
        /// </summary>
        Temporary,
        /// <summary>
        /// A named architectural register (e.g., "EAX", "R1").
        /// </summary>
        Register,
        /// <summary>
        /// An immediate value embedded in the instruction.
        /// </summary>
        Immediate
    }

    /// <summary>
    /// Represents an operand for an IR statement. It can be a temporary variable,
    /// a CPU register, or an immediate value.
    /// </summary>
    public readonly struct IrOperand
    {
        public readonly IrOperandType Type;
        
        // Value is stored in one of these fields depending on Type
        private readonly object _value;

        public IrOperand(int temporaryIndex)
        {
            Type = IrOperandType.Temporary;
            _value = temporaryIndex;
        }

        public IrOperand(string registerName)
        {
            Type = IrOperandType.Register;
            _value = registerName ?? throw new ArgumentNullException(nameof(registerName));
        }

        public IrOperand(IrValue immediateValue)
        {
            Type = IrOperandType.Immediate;
            _value = immediateValue;
        }

        public int TemporaryIndex => Type == IrOperandType.Temporary ? (int)_value : throw new InvalidOperationException("Operand is not a temporary.");
        public string RegisterName => Type == IrOperandType.Register ? (string)_value : throw new InvalidOperationException("Operand is not a register.");
        public IrValue ImmediateValue => Type == IrOperandType.Immediate ? (IrValue)_value : throw new InvalidOperationException("Operand is not an immediate.");
    }

    /// <summary>
    /// Defines the fundamental operations of the intermediate representation.
    /// These are simple, explicit, and platform-agnostic.
    /// </summary>
    public enum IrOpcode
    {
        // Data Transfer
        COPY,      // Copy data from a source operand to a destination operand.

        // Memory Access
        LOAD,      // result_temp = LOAD(address_operand)
        STORE,     // STORE(address_operand, value_operand)

        // Arithmetic
        ADD, SUB, MUL, 
        DIV_U,     // Unsigned Division
        DIV_S,     // Signed Division
        REM_U,     // Unsigned Remainder
        REM_S,     // Signed Remainder

        // Bitwise/Logic
        AND, OR, XOR, NOT,
        SHL,       // Shift Left
        SHR_L,     // Shift Right (Logical)
        SHR_A,     // Shift Right (Arithmetic)

        // Comparison (produces a 1-bit result)
        CMP_EQ,    // Equal
        CMP_NE,    // Not Equal
        CMP_LT_U,  // Unsigned Less Than
        CMP_LT_S,  // Signed Less Than
        CMP_LE_U,  // Unsigned Less Than or Equal
        CMP_LE_S,  // Signed Less Than or Equal

        // Control Flow
        JUMP,      // Unconditional jump to a single address operand.
        BRANCH,    // Conditional branch: BRANCH(condition_operand, true_address_operand, false_address_operand)

        // System
        TRAP,      // Raise a trap/exception to the host system with a code.
        HINT_NOP   // No operation. Can be used for padding or hints.
    }

    /// <summary>
    /// A single statement in the IR, representing a fundamental operation like "t1 = ADD(t0, imm(5))".
    /// </summary>
    public class IrStatement
    {
        public IrOpcode Opcode { get; }
        public IrOperand Destination { get; } // Optional, can be null
        public IReadOnlyList<IrOperand> Sources { get; }
        public IReadOnlyDictionary<string, object> Metadata { get; }

        public IrStatement(IrOpcode opcode, IrOperand destination, IrOperand[] sources, IReadOnlyDictionary<string, object> metadata = null)
        {
            Opcode = opcode;
            Destination = destination;
            Sources = sources;
            Metadata = metadata ?? new Dictionary<string, object>();
        }

        // Overload for constructor without metadata for convenience
        public IrStatement(IrOpcode opcode, IrOperand destination, params IrOperand[] sources)
            : this(opcode, destination, sources, null)
        {
        }
    }

    /// <summary>
    /// A complete, decoded instruction, translated into a sequence of IR statements.
    /// This is the output of an IInstructionDecoder and the input to an IInstructionExecutor.
    /// </summary>
    public class CanonicalInstruction
    {
        public ulong Address { get; }
        public int Length { get; }
        public IReadOnlyList<IrStatement> Statements { get; }

        public CanonicalInstruction(ulong address, int length, IReadOnlyList<IrStatement> statements)
        {
            Address = address;
            Length = length;
            Statements = statements;
        }
    }
}
