using System;

namespace asm6502v2
{
    [Serializable]
    internal class InvalidOperandException : Exception
    {
        public InvalidOperandException()
        {
        }

        public InvalidOperandException(string message, Exception inner) : base(message, inner)
        {
        }

        public InvalidOperandException(string line, int number) : base($"Invalid operand in line {number.ToString()}: {line}")
        {
        }
    }
}