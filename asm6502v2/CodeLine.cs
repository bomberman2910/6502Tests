using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace asm6502v2
{
    public class CodeLine
    {
        // Content
        public string Content { get; }
        public string CleanContent => Content.TrimEnd(';');
        // Comment
        public bool ContainsComment => Content.Contains(";");
        public bool IsCommentLine => Content.StartsWith(";");
        // Label
        public bool ContainsLabel => Label != null;
        public Label? Label { get; }
        // Data
        public bool IsDataLine => Data != null;
        public byte[] Data { get; }
        // Other
        public int Number { get; }
        public ushort Address { get; }

        public CodeLine(string line, int number, ushort address)
        {
            Content = line.Trim();
            Number = number;
            Address = address;
            if ((!(ContainsComment || IsCommentLine) && Content.Contains(":")) ||
                ((ContainsComment && !IsCommentLine) && Content.Split(';')[0].Contains(":")))
                Label = new Label {Name = Content.Split(':')[0], Location = address};
            else
                Label = null;
            if (Label != null)
            {
                if (Content.Split(':')[1].Trim().StartsWith("."))
                {
                    var dataPart = Content.Split(':')[1].Trim();
                    var dataType = dataPart.Split('\t')[0];
                    
                }
            }
        }

        public byte[] GetBytes()
        {
            if (IsCommentLine) return null;
            if (ContainsLabel)
            {
                if (Content.Split(':')[1].Contains(";"))
                {
                    if (Content.Split(':', ';')[1].Trim().Equals(""))
                        return null;
                }
                else
                {
                    if (Content.Split(':')[1].Trim().Equals(""))
                        return null;
                }
            }

            var bytes = new List<byte>();
            var splittedline = Content.Split(new[] {' '}, 2);
            var opcode = splittedline[0];
            byte op1;
            ushort addr;
            switch (opcode.ToUpper())
            {
                case "ADC":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x69, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x65, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x75, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x6D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x7D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x79, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckIndX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x61, op1});
                    else if (CheckIndY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x71, op1});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "AND":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x29, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x25, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x35, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x2D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x3D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x39, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckIndX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x21, op1});
                    else if (CheckIndY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x31, op1});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "ASL":
                    if (CheckAccu(splittedline[1]))
                        bytes.Add(0x0A);
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x06, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x16, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x0E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x1E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "BCC":
                    if ((splittedline[1].Length > 3) || !splittedline[1].StartsWith("$", StringComparison.Ordinal))
                        throw new InvalidOperandException(CleanContent, Number);
                    if (!byte.TryParse(splittedline[1].Substring(1), NumberStyles.HexNumber,
                        NumberFormatInfo.CurrentInfo,
                        out op1))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[] {0x90, op1});
                    break;
                case "BCS":
                    if ((splittedline[1].Length > 3) || !splittedline[1].StartsWith("$", StringComparison.Ordinal))
                        throw new InvalidOperandException(CleanContent, Number);
                    if (!byte.TryParse(splittedline[1].Substring(1), NumberStyles.HexNumber,
                        NumberFormatInfo.CurrentInfo,
                        out op1))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[] {0xB0, op1});
                    break;
                case "BEQ":
                    if ((splittedline[1].Length > 3) || !splittedline[1].StartsWith("$", StringComparison.Ordinal))
                        throw new InvalidOperandException(CleanContent, Number);
                    if (!byte.TryParse(splittedline[1].Substring(1), NumberStyles.HexNumber,
                        NumberFormatInfo.CurrentInfo,
                        out op1))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[] {0xF0, op1});
                    break;
                case "BIT":
                    if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x24, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x2C, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "BMI":
                    if ((splittedline[1].Length > 3) || !splittedline[1].StartsWith("$", StringComparison.Ordinal))
                        throw new InvalidOperandException(CleanContent, Number);
                    if (!byte.TryParse(splittedline[1].Substring(1), NumberStyles.HexNumber,
                        NumberFormatInfo.CurrentInfo,
                        out op1))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[] {0x30, op1});
                    break;
                case "BNE":
                    if ((splittedline[1].Length > 3) || !splittedline[1].StartsWith("$", StringComparison.Ordinal))
                        throw new InvalidOperandException(CleanContent, Number);
                    if (!byte.TryParse(splittedline[1].Substring(1), NumberStyles.HexNumber,
                        NumberFormatInfo.CurrentInfo,
                        out op1))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[] {0xD0, op1});
                    break;
                case "BPL":
                    if ((splittedline[1].Length > 3) || !splittedline[1].StartsWith("$", StringComparison.Ordinal))
                        throw new InvalidOperandException(CleanContent, Number);
                    if (!byte.TryParse(splittedline[1].Substring(1), NumberStyles.HexNumber,
                        NumberFormatInfo.CurrentInfo,
                        out op1))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[] {0x10, op1});
                    break;
                case "BRK":
                    bytes.Add(0x00);
                    break;
                case "BVC":
                    if ((splittedline[1].Length > 3) || !splittedline[1].StartsWith("$", StringComparison.Ordinal))
                        throw new InvalidOperandException(CleanContent, Number);
                    if (!byte.TryParse(splittedline[1].Substring(1), NumberStyles.HexNumber,
                        NumberFormatInfo.CurrentInfo,
                        out op1))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[] {0x50, op1});
                    break;
                case "BVS":
                    if ((splittedline[1].Length > 3) || !splittedline[1].StartsWith("$", StringComparison.Ordinal))
                        throw new InvalidOperandException(CleanContent, Number);
                    if (!byte.TryParse(splittedline[1].Substring(1), NumberStyles.HexNumber,
                        NumberFormatInfo.CurrentInfo,
                        out op1))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[] {0x70, op1});
                    break;
                case "CLC":
                    bytes.Add(0x18);
                    break;
                case "CLD":
                    bytes.Add(0xD8);
                    break;
                case "CLI":
                    bytes.Add(0x58);
                    break;
                case "CLV":
                    bytes.Add(0xB8);
                    break;
                case "CMP":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xC9, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xC5, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xD5, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xCD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xDD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xD9, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckIndX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xC1, op1});
                    else if (CheckIndY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xD1, op1});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "CPX":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xE0, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xE4, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xEC, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "CPY":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xC0, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xC4, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xCC, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "DEC":
                    if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xC6, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xD6, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xCE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xDE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "DEX":
                    bytes.Add(0xCA);
                    break;
                case "DEY":
                    bytes.Add(0x88);
                    break;
                case "EOR":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x49, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x45, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x55, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x4D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x5D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x59, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckIndX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x41, op1});
                    else if (CheckIndY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x51, op1});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "INC":
                    if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xE6, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xF6, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xEE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xFE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "INX":
                    bytes.Add(0xE8);
                    break;
                case "INY":
                    bytes.Add(0xC8);
                    break;
                case "JMP":
                    if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x4C, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckInd(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x6C, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "JSR":
                    if (!CheckAbs(splittedline[1], out addr))
                        throw new InvalidOperandException(CleanContent, Number);
                    bytes.AddRange(new byte[]
                        {0x20, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    break;
                case "LDA":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xA9, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xA5, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xB5, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xAD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xBD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xB9, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckIndX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xA1, op1});
                    else if (CheckIndY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xB1, op1});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "LDX":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xA2, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xA6, op1});
                    else if (CheckZPY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xB6, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xAE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xBE, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "LDY":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xA0, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xA4, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xB4, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xAC, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xBC, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "LSR":
                    if (CheckAccu(splittedline[1]))
                        bytes.Add(0x4A);
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x46, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x56, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x4E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x5E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "NOP":
                    bytes.Add(0xEA);
                    break;
                case "ORA":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x09, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x05, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x15, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x0D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x1D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x19, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckIndX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x01, op1});
                    else if (CheckIndY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x11, op1});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "PHA":
                    bytes.Add(0x48);
                    break;
                case "PHP":
                    bytes.Add(0x08);
                    break;
                case "PLA":
                    bytes.Add(0x68);
                    break;
                case "PLP":
                    bytes.Add(0x28);
                    break;
                case "ROL":
                    if (CheckAccu(splittedline[1]))
                        bytes.Add(0x2A);
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x66, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x36, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x2E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x3E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "ROR":
                    if (CheckAccu(splittedline[1]))
                        bytes.Add(0x6A);
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x66, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x76, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x6E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x7E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "RTI":
                    bytes.Add(0x40);
                    break;
                case "RTS":
                    bytes.Add(0x60);
                    break;
                case "SBC":
                    if (CheckImm(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xE9, op1});
                    else if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xE5, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xF5, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xED, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xFD, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0xF9, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckIndX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xE1, op1});
                    else if (CheckIndY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0xF1, op1});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "SEC":
                    bytes.Add(0x38);
                    break;
                case "SED":
                    bytes.Add(0xF8);
                    break;
                case "SEI":
                    bytes.Add(0x78);
                    break;
                case "STA":
                    if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x85, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x95, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x8D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsX(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x9D, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckAbsY(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x99, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else if (CheckIndX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x81, op1});
                    else if (CheckIndY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x91, op1});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "STX":
                    if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x86, op1});
                    else if (CheckZPY(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x96, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x8E, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "STY":
                    if (CheckZP(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x84, op1});
                    else if (CheckZPX(splittedline[1], out op1))
                        bytes.AddRange(new byte[] {0x94, op1});
                    else if (CheckAbs(splittedline[1], out addr))
                        bytes.AddRange(new byte[]
                            {0x8C, BitConverter.GetBytes(addr)[0], BitConverter.GetBytes(addr)[1]});
                    else
                        throw new InvalidOperandException(CleanContent, Number);
                    break;
                case "TAX":
                    bytes.Add(0xAA);
                    break;
                case "TAY":
                    bytes.Add(0xA8);
                    break;
                case "TSX":
                    bytes.Add(0xBA);
                    break;
                case "TXA":
                    bytes.Add(0x8A);
                    break;
                case "TXS":
                    bytes.Add(0x9A);
                    break;
                case "TYA":
                    bytes.Add(0x98);
                    break;
                default:
                    throw new ArgumentException($"Invalid code in line {Number}: {CleanContent}");
            }
            return bytes.ToArray();
        }

        /// <summary>
        /// Checks if a given string contains an absolute address
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private bool CheckAbs(string opstring, out ushort address)
        {
            ushort adding = 0;
            if (opstring.Contains("+"))
            {
                var split = opstring.Split('+');
                if (!ushort.TryParse(split[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out adding))
                    throw new InvalidOperandException(CleanContent, Number);
                opstring = split[0].Trim();
            }
            if (!Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{4}$"))
            { 
                address = 0;
                return false;
            }
            if(!ushort.TryParse(opstring.Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address))
                throw new InvalidOperandException(CleanContent, Number);
            address += adding;
            return true;
        }

        /// <summary>
        /// Checks if a given string contains an absolute address indexed by X
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private bool CheckAbsX(string opstring, out ushort address)
        {
            if (Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{4},\s*X"))
                return ushort.TryParse(opstring.Substring(1, 4), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
            address = 0;
            return false;
        }

        /// <summary>
        /// Checks if a given string contains an absolute address indexed by Y
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private bool CheckAbsY(string opstring, out ushort address)
        {
            if (!Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{4},\s*Y"))
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
        private bool CheckAccu(string opstring) => opstring.ToUpper().Equals("A");

        /// <summary>
        /// Checks if a given string contains an immediate value
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="value">contains the value from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private bool CheckImm(string opstring, out byte value)
        {
            if (!Regex.IsMatch(opstring.ToUpper(), @"\#\$[0-9A-F]{2}"))
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
        private bool CheckInd(string opstring, out ushort address)
        {
            if (Regex.IsMatch(opstring.ToUpper(), @"\(\$[0-9A-F]{4}\)"))
                return ushort.TryParse(opstring.Substring(2, 4), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
            address = 0;
            return false;
        }

        /// <summary>
        /// Checks if a given string contains an indirect address indexed by X
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private bool CheckIndX(string opstring, out byte address)
        {
            if (Regex.IsMatch(opstring.ToUpper(), @"\(\$[0-9A-F]{2},\s*X\)"))
                return byte.TryParse(opstring.Substring(2, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
            address = 0;
            return false;
        }

        /// <summary>
        /// Checks if a given string contains an indirect address indexed by Y
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        private bool CheckIndY(string opstring, out byte address)
        {
            if (Regex.IsMatch(opstring.ToUpper(), @"\(\$[0-9A-F]{2}\),\s*Y"))
                return byte.TryParse(opstring.Substring(2, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
            address = 0;
            return false;
        }

        /// <summary>
        /// Checks if a given string contains a zeropage address
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        // ReSharper disable once InconsistentNaming
        private bool CheckZP(string opstring, out byte address)
        {
            byte adding = 0;
            if (opstring.Contains("+"))
            {
                var split = opstring.Split('+');
                if (!byte.TryParse(split[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out adding))
                    throw new InvalidOperandException(CleanContent, Number);
                opstring = split[0].Trim();
            }
            if (!Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{2}$"))
            {
                address = 0;
                return false;
            }
            if(!byte.TryParse(opstring.Substring(1), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address))
                throw new InvalidOperandException(CleanContent, Number);
            address += adding;
            return true;
        }

        /// <summary>
        /// Checks if a given string contains a zeropage address indexed by X
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        // ReSharper disable once InconsistentNaming
        private bool CheckZPX(string opstring, out byte address)
        {
            if (Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{2},\s*X"))
                return byte.TryParse(opstring.Substring(1, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
            address = 0;
            return false;
        }

        /// <summary>
        /// Checks if a given string contains a zeropage address indexed by Y
        /// </summary>
        /// <param name="opstring">string containing operand</param>
        /// <param name="address">contains the address from the string</param>
        /// <returns>true if check is successful, false if not</returns>
        // ReSharper disable once InconsistentNaming
        private bool CheckZPY(string opstring, out byte address)
        {
            if (Regex.IsMatch(opstring.ToUpper(), @"\$[0-9A-F]{2},\s*Y"))
                return byte.TryParse(opstring.Substring(1, 2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out address);
            address = 0;
            return false;
        }

        public string ToString(StringType stringType)
        {
            switch (stringType)
            {
                case StringType.Standard:
                    return Content;
                case StringType.LinkerMode:
                    var bytesString = "";
                    if (GetBytes() != null)
                    {
                        if (GetBytes().Length == 1)
                            bytesString = $"{GetBytes()[0]:X2}      ";
                        else if (GetBytes().Length == 2)
                            bytesString = $"{GetBytes()[0]:X2} {GetBytes()[1]:X2}   ";
                        else if (GetBytes().Length == 3)
                            bytesString = $"{GetBytes()[0]:X2} {GetBytes()[1]:X2} {GetBytes()[2]:X2}";
                    }
                    else
                        bytesString = "        ";

                    return $"{Number}  {bytesString}\t{Content}";
                case StringType.WithLineNumber:
                    return $"{Number}  {Content}";
                case StringType.Clean:
                    return CleanContent;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stringType), stringType, null);
            }
        }

        public enum StringType
        {
            Standard,
            LinkerMode,
            WithLineNumber,
            Clean
        }
    }
}