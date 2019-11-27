using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace lib6502
{
    public static class ASM6502
    {
        /// <summary>
        /// Checks if a given string contains an absolute address
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckAbs(string opstring, out ushort address)
        {
            if ((opstring.Length != 5) || !opstring.StartsWith("$", StringComparison.Ordinal) || opstring.Contains(','))
            {
                address = 0;
                return false;
            }
            return ushort.TryParse(opstring.Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Checks if a given string contains an absolute address indexed by X
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckAbsX(string opstring, out ushort address)
        {
            if ((opstring.Length != 7) || !opstring.StartsWith("$", StringComparison.Ordinal) || !opstring.ToUpper().Contains(",X"))
            {
                address = 0;
                return false;
            }
            return ushort.TryParse(opstring.Substring(1, 4), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Checks if a given string contains an absolute address indexed by Y
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckAbsY(string opstring, out ushort address)
        {
            if ((opstring.Length != 7) || !opstring.StartsWith("$", StringComparison.Ordinal) || !opstring.ToUpper().Contains(",Y"))
            {
                address = 0;
                return false;
            }
            return ushort.TryParse(opstring.Substring(1, 4), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Checks if a given string contains an accumulator operand
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckAccu(string opstring) => opstring.ToUpper().Equals("A");

        /// <summary>
        /// Checks if a given string contains an immediate value
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="value">contains the value from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckImm(string opstring, out byte value)
        {
            if ((opstring.Length != 4) || !opstring.StartsWith("#$", StringComparison.Ordinal) || opstring.Contains(','))
            {
                value = 0;
                return false;
            }
            return byte.TryParse(opstring.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out value);
        }

        /// <summary>
        /// Checks if a given string contains an indirect address
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckInd(string opstring, out ushort address)
        {
            if ((opstring.Length != 7) || !opstring.StartsWith("($", StringComparison.Ordinal) || opstring.Contains(',') || opstring.EndsWith(")", StringComparison.Ordinal))
            {
                address = 0;
                return false;
            }
            return ushort.TryParse(opstring.Substring(2, 4), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Checks if a given string contains an indirect address indexed by X
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckIndX(string opstring, out byte address)
        {
            if ((opstring.Length != 7) || !opstring.StartsWith("($", StringComparison.Ordinal) || !opstring.ToUpper().EndsWith(",X)", StringComparison.OrdinalIgnoreCase))
            {
                address = 0;
                return false;
            }
            return byte.TryParse(opstring.Substring(2, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Checks if a given string contains an indirect address indexed by Y
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckIndY(string opstring, out byte address)
        {
            if ((opstring.Length != 7) || !opstring.StartsWith("($", StringComparison.Ordinal) || !opstring.ToUpper().EndsWith("),Y", StringComparison.OrdinalIgnoreCase))
            {
                address = 0;
                return false;
            }
            return byte.TryParse(opstring.Substring(2, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Checks if a given string contains a zeropage address
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckZP(string opstring, out byte address)
        {
            if ((opstring.Length != 3) || !opstring.StartsWith("$", StringComparison.Ordinal) || opstring.Contains(','))
            {
                address = 0;
                return false;
            }
            return byte.TryParse(opstring.Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Checks if a given string contains a zeropage address indexed by X
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckZPX(string opstring, out byte address)
        {
            if ((opstring.Length != 5) || !opstring.StartsWith("$", StringComparison.Ordinal) || !opstring.ToUpper().EndsWith(",X", StringComparison.OrdinalIgnoreCase))
            {
                address = 0;
                return false;
            }
            return byte.TryParse(opstring.Substring(1, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Checks if a given string contains a zeropage address indexed by Y
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private static bool CheckZPY(string opstring, out byte address)
        {
            if ((opstring.Length != 5) || !opstring.StartsWith("$", StringComparison.Ordinal) || !opstring.ToUpper().EndsWith(",Y", StringComparison.OrdinalIgnoreCase))
            {
                address = 0;
                return false;
            }
            return byte.TryParse(opstring.Substring(1, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        }

        /// <summary>
        /// Converts a code listing into a byte array
        /// </summary>
        /// <param name="code">listing</param>
        /// <returns>byte array with converted code</returns>
        public static byte[] Assemble(string code)
        {
            List<string> lines = new List<string>(code.Split('\n'));
            _ = lines.RemoveAll(line => line.StartsWith(";", StringComparison.Ordinal));
            _ = lines.RemoveAll(line => line.Equals(""));

            List<byte> bytelist = new List<byte>();
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                string[] spacesplit = trimmed.Split(' ');
                string opcode = spacesplit[0];
                ushort addr = 0;
                byte op1 = 0;
                switch (opcode.ToUpper())
                {
                    case "ADC":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x69);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x65);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x75);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x6D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x7D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x79);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckIndX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x61);
                            bytelist.Add(op1);
                        }
                        else if (CheckIndY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x71);
                            bytelist.Add(op1);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "AND":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x29);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x25);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x35);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x2D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x3D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x39);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckIndX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x21);
                            bytelist.Add(op1);
                        }
                        else if (CheckIndY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x31);
                            bytelist.Add(op1);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "ASL":
                        if (CheckAccu(spacesplit[1]))
                        {
                            bytelist.Add(0x0A);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x06);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x16);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x0E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x1E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "BCC":
                        if ((spacesplit[1].Length > 3) || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        bytelist.Add(0x90);
                        bytelist.Add(op1);
                        break;
                    case "BCS":
                        if ((spacesplit[1].Length > 3) || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        bytelist.Add(0xB0);
                        bytelist.Add(op1);
                        break;
                    case "BEQ":
                        if ((spacesplit[1].Length > 3) || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        bytelist.Add(0xF0);
                        bytelist.Add(op1);
                        break;
                    case "BIT":
                        if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x24);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x2C);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "BMI":
                        if ((spacesplit[1].Length > 3) || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                            throw new ArgumentException("Ungültiger Operand" + trimmed);
                        if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                            throw new ArgumentException("Ungültiger Operand" + trimmed);
                        bytelist.Add(0x30);
                        bytelist.Add(op1);
                        break;
                    case "BNE":
                        if ((spacesplit[1].Length > 3) || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        bytelist.Add(0xD0);
                        bytelist.Add(op1);
                        break;
                    case "BPL":
                        if ((spacesplit[1].Length > 3) || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        bytelist.Add(0x10);
                        bytelist.Add(op1);
                        break;
                    case "BRK":
                        bytelist.Add(0x00);
                        break;
                    case "BVC":
                        if ((spacesplit[1].Length > 3) || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        bytelist.Add(0x50);
                        bytelist.Add(op1);
                        break;
                    case "BVS":
                        if ((spacesplit[1].Length > 3) || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        bytelist.Add(0x70);
                        bytelist.Add(op1);
                        break;
                    case "CLC":
                        bytelist.Add(0x18);
                        break;
                    case "CLD":
                        bytelist.Add(0xD8);
                        break;
                    case "CLI":
                        bytelist.Add(0x58);
                        break;
                    case "CLV":
                        bytelist.Add(0xB8);
                        break;
                    case "CMP":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xC9);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xC5);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xD5);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xCD);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xDD);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xD9);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckIndX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xC1);
                            bytelist.Add(op1);
                        }
                        else if (CheckIndY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xD1);
                            bytelist.Add(op1);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "CPX":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xE0);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xE4);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xEC);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "CPY":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xC0);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xC4);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xCC);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "DEC":
                        if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xC6);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xD6);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xCE);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xDE);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "DEX":
                        bytelist.Add(0xCA);
                        break;
                    case "DEY":
                        bytelist.Add(0x88);
                        break;
                    case "EOR":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x49);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x45);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x55);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x4D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x5D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x59);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckIndX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x41);
                            bytelist.Add(op1);
                        }
                        else if (CheckIndY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x51);
                            bytelist.Add(op1);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "INC":
                        if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xE6);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xF6);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xEE);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xFE);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "INX":
                        bytelist.Add(0xE8);
                        break;
                    case "INY":
                        bytelist.Add(0xC8);
                        break;
                    case "JMP":
                        if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x4C);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckInd(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x6C);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "JSR":
                        if (!CheckAbs(spacesplit[1], out addr))
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        bytelist.Add(0x20);
                        bytelist.Add(BitConverter.GetBytes(addr)[0]);
                        bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        break;
                    case "LDA":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xA9);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xA5);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xB5);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xAD);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xBD);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xB9);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckIndX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xA1);
                            bytelist.Add(op1);
                        }
                        else if (CheckIndY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xB1);
                            bytelist.Add(op1);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "LDX":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xA2);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xA6);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xB6);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xAE);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xBE);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "LDY":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xA0);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xA4);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xB4);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xAC);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xBC);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "LSR":
                        if (CheckAccu(spacesplit[1]))
                        {
                            bytelist.Add(0x4A);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x46);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x56);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x4E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x5E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "NOP":
                        bytelist.Add(0xEA);
                        break;
                    case "ORA":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x09);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x05);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x15);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x0D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x1D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x19);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckIndX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x01);
                            bytelist.Add(op1);
                        }
                        else if (CheckIndY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x11);
                            bytelist.Add(op1);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "PHA":
                        bytelist.Add(0x48);
                        break;
                    case "PHP":
                        bytelist.Add(0x08);
                        break;
                    case "PLA":
                        bytelist.Add(0x68);
                        break;
                    case "PLP":
                        bytelist.Add(0x28);
                        break;
                    case "ROL":
                        if (CheckAccu(spacesplit[1]))
                        {
                            bytelist.Add(0x2A);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x26);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x36);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x2E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x3E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "ROR":
                        if (CheckAccu(spacesplit[1]))
                        {
                            bytelist.Add(0x6A);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x66);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x76);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x6E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x7E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "RTI":
                        bytelist.Add(0x40);
                        break;
                    case "RTS":
                        bytelist.Add(0x60);
                        break;
                    case "SBC":
                        if (CheckImm(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xE9);
                            bytelist.Add(op1);
                        }
                        else if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xE5);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xF5);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xED);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xFD);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0xF9);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckIndX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xE1);
                            bytelist.Add(op1);
                        }
                        else if (CheckIndY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0xF1);
                            bytelist.Add(op1);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "SEC":
                        bytelist.Add(0x38);
                        break;
                    case "SED":
                        bytelist.Add(0xF8);
                        break;
                    case "SEI":
                        bytelist.Add(0x78);
                        break;
                    case "STA":
                        if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x85);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x95);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x8D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsX(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x9D);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckAbsY(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x99);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else if (CheckIndX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x81);
                            bytelist.Add(op1);
                        }
                        else if (CheckIndY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x91);
                            bytelist.Add(op1);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "STX":
                        if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x86);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPY(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x96);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x8E);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "STY":
                        if (CheckZP(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x84);
                            bytelist.Add(op1);
                        }
                        else if (CheckZPX(spacesplit[1], out op1))
                        {
                            bytelist.Add(0x94);
                            bytelist.Add(op1);
                        }
                        else if (CheckAbs(spacesplit[1], out addr))
                        {
                            bytelist.Add(0x8C);
                            bytelist.Add(BitConverter.GetBytes(addr)[0]);
                            bytelist.Add(BitConverter.GetBytes(addr)[1]);
                        }
                        else
                            throw new ArgumentException("Ungültiger Operand " + trimmed);
                        break;
                    case "TAX":
                        bytelist.Add(0xAA);
                        break;
                    case "TAY":
                        bytelist.Add(0xA8);
                        break;
                    case "TSX":
                        bytelist.Add(0xBA);
                        break;
                    case "TXA":
                        bytelist.Add(0x8A);
                        break;
                    case "TXS":
                        bytelist.Add(0x9A);
                        break;
                    case "TYA":
                        bytelist.Add(0x98);
                        break;
                    default:
                        throw new ArgumentException("Ungültiger Opcode " + trimmed);
                }
            }
            return bytelist.ToArray();
        }
    }
}
