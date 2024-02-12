using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace lib6502;

public static class Asm6502
{
    /// <summary>
    ///     Checks if a given string contains an absolute address
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckAbs(string opstring, out ushort address)
    {
        ushort adding = 0;
        if (opstring.Contains("+"))
        {
            var split = opstring.Split('+');
            if (!ushort.TryParse(split[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out adding))
                throw new ArgumentException($"Ungültiger Operand: {opstring}");
            opstring = split[0].Trim();
        }

        if (!Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{4}$"))
        {
            address = 0;
            return false;
        }

        if (!ushort.TryParse(opstring.Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address))
            throw new ArgumentException($"Ungültiger Operand: {opstring}");
        address += adding;
        return true;
    }

    /// <summary>
    ///     Checks if a given string contains an absolute address indexed by X
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckAbsX(string opstring, out ushort address)
    {
        if (Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{4},\s*X"))
            return ushort.TryParse(opstring.Substring(1, 4), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        address = 0;
        return false;
    }

    /// <summary>
    ///     Checks if a given string contains an absolute address indexed by Y
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckAbsY(string opstring, out ushort address)
    {
        if (Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{4},\s*Y"))
            return ushort.TryParse(opstring.Substring(1, 4), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        address = 0;
        return false;
    }

    /// <summary>
    ///     Checks if a given string contains an accumulator operand
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckAccu(string opstring) => opstring.ToUpper().Equals("A");

    /// <summary>
    ///     Checks if a given string contains an immediate value
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="value">contains the value from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckImm(string opstring, out byte value)
    {
        if (Regex.IsMatch(opstring.ToUpper(), @"\#\$[0-9A-F]{2}"))
            return byte.TryParse(opstring.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out value);
        value = 0;
        return false;
    }

    /// <summary>
    ///     Checks if a given string contains an indirect address
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckInd(string opstring, out ushort address)
    {
        if (Regex.IsMatch(opstring.ToUpper(), @"\(\$[0-9A-F]{4}\)"))
            return ushort.TryParse(opstring.Substring(2, 4), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        address = 0;
        return false;
    }

    /// <summary>
    ///     Checks if a given string contains an indirect address indexed by X
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckIndX(string opstring, out byte address)
    {
        if (Regex.IsMatch(opstring.ToUpper(), @"\(\$[0-9A-F]{2},\s*X\)"))
            return byte.TryParse(opstring.Substring(2, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        address = 0;
        return false;
    }

    /// <summary>
    ///     Checks if a given string contains an indirect address indexed by Y
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckIndY(string opstring, out byte address)
    {
        if (Regex.IsMatch(opstring.ToUpper(), @"\(\$[0-9A-F]{2}\),\s*Y"))
            return byte.TryParse(opstring.Substring(2, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        address = 0;
        return false;
    }

    /// <summary>
    ///     Checks if a given string contains a zeropage address
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckZp(string opstring, out byte address)
    {
        byte adding = 0;
        if (opstring.Contains("+"))
        {
            var split = opstring.Split('+');
            if (!byte.TryParse(split[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out adding))
                throw new ArgumentException($"Ungültiger Operand: {opstring}");
            opstring = split[0].Trim();
        }

        if (!Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{2}$"))
        {
            address = 0;
            return false;
        }

        if (!byte.TryParse(opstring.Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address))
            throw new ArgumentException($"Ungültiger Operand: {opstring}");
        address += adding;
        return true;
    }

    /// <summary>
    ///     Checks if a given string contains a zeropage address indexed by X
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckZpx(string opstring, out byte address)
    {
        if (Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{2},\s*X"))
            return byte.TryParse(opstring.Substring(1, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        address = 0;
        return false;
    }

    /// <summary>
    ///     Checks if a given string contains a zeropage address indexed by Y
    /// </summary>
    /// <param name="opstring">string containing operand</param>
    /// <param name="address">contains the address from the string</param>
    /// <returns>true if check is successful, false if not</returns>
    private static bool CheckZpy(string opstring, out byte address)
    {
        if (Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{2},\s*Y"))
            return byte.TryParse(opstring.Substring(1, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
        address = 0;
        return false;
    }

    /// <summary>
    ///     Converts a code listing into a byte array
    /// </summary>
    /// <param name="code">listing</param>
    /// <returns>byte array with converted code</returns>
    public static byte[] Assemble(string code)
    {
        var lines = new List<string>(code.Split('\n'));
        _ = lines.RemoveAll(line => line.StartsWith(";", StringComparison.Ordinal));
        _ = lines.RemoveAll(line => line.Equals(""));

        var bytelist = new List<byte>();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var spacesplit = trimmed.Split(new[] { ' ' }, 2);
            var opcode = spacesplit[0];
            ushort addr;
            byte op1;
            switch (opcode.ToUpper())
            {
                case "ADC":
                    if (CheckImm(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x69, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x65, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x75, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x6D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x7D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x79, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckIndX(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x61, op1 });
                    else if (CheckIndY(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x71, op1 });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "AND":
                    if (CheckImm(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x29, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x25, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x35, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x2D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x3D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x39, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckIndX(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x21, op1 });
                    else if (CheckIndY(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x31, op1 });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "ASL":
                    if (CheckAccu(spacesplit[1]))
                        bytelist.Add(0x0A);
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x06, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x16, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x0E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x1E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "BCC":
                    if (spacesplit[1].Length > 3 || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    bytelist.AddRange(new byte[] { 0x90, op1 });
                    break;
                case "BCS":
                    if (spacesplit[1].Length > 3 || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    bytelist.AddRange(new byte[] { 0xB0, op1 });
                    break;
                case "BEQ":
                    if (spacesplit[1].Length > 3 || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    bytelist.AddRange(new byte[] { 0xF0, op1 });
                    break;
                case "BIT":
                    if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x24, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x2C, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "BMI":
                    if (spacesplit[1].Length > 3 || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                        throw new ArgumentException("Ungültiger Operand" + trimmed);
                    if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                        throw new ArgumentException("Ungültiger Operand" + trimmed);
                    bytelist.AddRange(new byte[] { 0x30, op1 });
                    break;
                case "BNE":
                    if (spacesplit[1].Length > 3 || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    bytelist.AddRange(new byte[] { 0xD0, op1 });
                    break;
                case "BPL":
                    if (spacesplit[1].Length > 3 || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    bytelist.AddRange(new byte[] { 0x10, op1 });
                    break;
                case "BRK":
                    bytelist.Add(0x00);
                    break;
                case "BVC":
                    if (spacesplit[1].Length > 3 || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    bytelist.AddRange(new byte[] { 0x50, op1 });
                    break;
                case "BVS":
                    if (spacesplit[1].Length > 3 || !spacesplit[1].StartsWith("$", StringComparison.Ordinal))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    if (!byte.TryParse(spacesplit[1].Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out op1))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    bytelist.AddRange(new byte[] { 0x70, op1 });
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
                        bytelist.AddRange(new byte[] { 0xC9, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xC5, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xD5, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xCD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xDD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xD9, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckIndX(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xC1, op1 });
                    else if (CheckIndY(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xD1, op1 });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "CPX":
                    if (CheckImm(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xE0, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xE4, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xEC, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "CPY":
                    if (CheckImm(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xC0, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xC4, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xCC, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "DEC":
                    if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xC6, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xD6, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xCE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xDE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
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
                        bytelist.AddRange(new byte[] { 0x49, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x45, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x55, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x4D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x5D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x59, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckIndX(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x41, op1 });
                    else if (CheckIndY(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x51, op1 });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "INC":
                    if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xE6, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xF6, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xEE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xFE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
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
                        bytelist.AddRange(new byte[] { 0x4C, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckInd(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x6C, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "JSR":
                    if (!CheckAbs(spacesplit[1], out addr))
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    bytelist.AddRange(new byte[] { 0x20, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    break;
                case "LDA":
                    if (CheckImm(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xA9, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xA5, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xB5, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xAD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xBD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xB9, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckIndX(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xA1, op1 });
                    else if (CheckIndY(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xB1, op1 });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "LDX":
                    if (CheckImm(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xA2, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xA6, op1 });
                    else if (CheckZpy(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xB6, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xAE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xBE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "LDY":
                    if (CheckImm(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xA0, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xA4, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xB4, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xAC, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xBC, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "LSR":
                    if (CheckAccu(spacesplit[1]))
                        bytelist.Add(0x4A);
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x46, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x56, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x4E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x5E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "NOP":
                    bytelist.Add(0xEA);
                    break;
                case "ORA":
                    if (CheckImm(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x09, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x05, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x15, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x0D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x1D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x19, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckIndX(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x01, op1 });
                    else if (CheckIndY(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x11, op1 });
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
                        bytelist.Add(0x2A);
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x66, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x36, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x2E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x3E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "ROR":
                    if (CheckAccu(spacesplit[1]))
                        bytelist.Add(0x6A);
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x66, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x76, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x6E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x7E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
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
                        bytelist.AddRange(new byte[] { 0xE9, op1 });
                    else if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xE5, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xF5, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xED, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xFD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0xF9, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckIndX(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xE1, op1 });
                    else if (CheckIndY(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0xF1, op1 });
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
                    if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x85, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x95, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x8D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsX(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x9D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckAbsY(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x99, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else if (CheckIndX(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x81, op1 });
                    else if (CheckIndY(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x91, op1 });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "STX":
                    if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x86, op1 });
                    else if (CheckZpy(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x96, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x8E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
                    else
                        throw new ArgumentException("Ungültiger Operand " + trimmed);
                    break;
                case "STY":
                    if (CheckZp(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x84, op1 });
                    else if (CheckZpx(spacesplit[1], out op1))
                        bytelist.AddRange(new byte[] { 0x94, op1 });
                    else if (CheckAbs(spacesplit[1], out addr))
                        bytelist.AddRange(new byte[] { 0x8C, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1] });
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