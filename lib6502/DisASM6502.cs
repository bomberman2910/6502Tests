using System;

namespace lib6502
{
    public static class DisASM6502
    {
        public static string Disassemble(byte[] code, long pc)
        {
            switch (code[pc])
            {
                case 0x00:
                    return "BRK";
                case 0x01:
                    return IndirectXCode("ORA", code[++pc]);
                case 0x05:
                    return ZeropageCode("ORA", code[++pc]);
                case 0x06:
                    return ZeropageCode("ASL", code[++pc]);
                case 0x08:
                    return "PHP";
                case 0x09:
                    return ImmediateCode("ORA", code[++pc]);
                case 0x0A:
                    return "ASL";
                case 0x0D:
                    return AbsCode("ORA", code[++pc], code[++pc]);
                case 0x0E:
                    return AbsCode("ASL", code[++pc], code[++pc]);
                case 0x10:
                    return RelativeCode("BPL", code[++pc]);
                case 0x11:
                    return IndirectYCode("ORA", code[++pc]);
                case 0x15:
                    return ZeropageXCode("ORA", code[++pc]);
                case 0x16:
                    return ZeropageXCode("ASL", code[++pc]);
                case 0x18:
                    return "CLC";
                case 0x19:
                    return AbsYCode("ORA", code[++pc], code[++pc]);
                case 0x1D:
                    return AbsXCode("ORA", code[++pc], code[++pc]);
                case 0x1E:
                    return AbsXCode("ASL", code[++pc], code[++pc]);
                case 0x20:
                    return AbsCode("JSR", code[++pc], code[++pc]);
                case 0x21:
                    return IndirectXCode("AND", code[++pc]);
                case 0x24:
                    return ZeropageCode("BIT", code[++pc]);
                case 0x25:
                    return ZeropageCode("AND", code[++pc]);
                case 0x26:
                    return ZeropageCode("ROL", code[++pc]);
                case 0x28:
                    return "PLP";
                case 0x29:
                    return ImmediateCode("AND", code[++pc]);
                case 0x2A:
                    return "ROL";
                case 0x2C:
                    return AbsCode("BIT", code[++pc], code[++pc]);
                case 0x2D:
                    return AbsCode("AND", code[++pc], code[++pc]);
                case 0x2E:
                    return AbsCode("ROL", code[++pc], code[++pc]);
                case 0x30:
                    return RelativeCode("BMI", code[++pc]);
                case 0x31:
                    return IndirectYCode("AND", code[++pc]);
                case 0x35:
                    return ZeropageXCode("AND", code[++pc]);
                case 0x36:
                    return ZeropageXCode("ROL", code[++pc]);
                case 0x38:
                    return "SEC";
                case 0x39:
                    return AbsYCode("AND", code[++pc], code[++pc]);
                case 0x3D:
                    return AbsXCode("AND", code[++pc], code[++pc]);
                case 0x3E:
                    return AbsXCode("ROL", code[++pc], code[++pc]);
                case 0x40:
                    return "RTI";
                case 0x41:
                    return IndirectXCode("EOR", code[++pc]);
                case 0x45:
                    return ZeropageCode("EOR", code[++pc]);
                case 0x46:
                    return ZeropageCode("LSR", code[++pc]);
                case 0x48:
                    return "PHA";
                case 0x49:
                    return ImmediateCode("EOR", code[++pc]);
                case 0x4A:
                    return "LSR";
                case 0x4C:
                    return AbsCode("JMP", code[++pc], code[++pc]);
                case 0x4D:
                    return AbsCode("EOR", code[++pc], code[++pc]);
                case 0x4E:
                    return AbsCode("LSR", code[++pc], code[++pc]);
                case 0x50:
                    return RelativeCode("BVC", code[++pc]);
                case 0x51:
                    return IndirectYCode("EOR", code[++pc]);
                case 0x55:
                    return ZeropageXCode("EOR", code[++pc]);
                case 0x56:
                    return ZeropageXCode("LSR", code[++pc]);
                case 0x58:
                    return "CLI";
                case 0x59:
                    return AbsYCode("EOR", code[++pc], code[++pc]);
                case 0x5D:
                    return AbsXCode("EOR", code[++pc], code[++pc]);
                case 0x5E:
                    return AbsXCode("LSR", code[++pc], code[++pc]);
                case 0x60:
                    return "RTS";
                case 0x61:
                    return IndirectXCode("ADC", code[++pc]);
                case 0x65:
                    return ZeropageCode("ADC", code[++pc]);
                case 0x66:
                    return ZeropageCode("ROR", code[++pc]);
                case 0x68:
                    return "PLA";
                case 0x69:
                    return ImmediateCode("ADC", code[++pc]);
                case 0x6A:
                    return "ROR";
                case 0x6C:
                    return IndirectCode("JMP", code[++pc], code[++pc]);
                case 0x6D:
                    return AbsCode("ADC", code[++pc], code[++pc]);
                case 0x6E:
                    return AbsCode("ROR", code[++pc], code[++pc]);
                case 0x70:
                    return RelativeCode("BVS", code[++pc]);
                case 0x71:
                    return IndirectYCode("ADC", code[++pc]);
                case 0x75:
                    return ZeropageXCode("ADC", code[++pc]);
                case 0x76:
                    return ZeropageXCode("ROR", code[++pc]);
                case 0x78:
                    return "SEI";
                case 0x79:
                    return AbsYCode("ADC", code[++pc], code[++pc]);
                case 0x7D:
                    return AbsXCode("ADC", code[++pc], code[++pc]);
                case 0x7E:
                    return AbsXCode("ROR", code[++pc], code[++pc]);
                case 0x81:
                    return IndirectXCode("STA", code[++pc]);
                case 0x84:
                    return ZeropageCode("STY", code[++pc]);
                case 0x85:
                    return ZeropageCode("STA", code[++pc]);
                case 0x86:
                    return ZeropageCode("STX", code[++pc]);
                case 0x88:
                    return "DEY";
                case 0x8A:
                    return "TXA";
                case 0x8C:
                    return AbsCode("STY", code[++pc], code[++pc]);
                case 0x8D:
                    return AbsCode("STA", code[++pc], code[++pc]);
                case 0x8E:
                    return AbsCode("STX", code[++pc], code[++pc]);
                case 0x90:
                    return RelativeCode("BCC", code[++pc]);
                case 0x91:
                    return IndirectYCode("STA", code[++pc]);
                case 0x94:
                    return ZeropageXCode("STY", code[++pc]);
                case 0x95:
                    return ZeropageXCode("STA", code[++pc]);
                case 0x96:
                    return ZeropageYCode("STX", code[++pc]);
                case 0x98:
                    return "TYA";
                case 0x99:
                    return AbsYCode("STA", code[++pc], code[++pc]);
                case 0x9A:
                    return "TXS";
                case 0x9D:
                    return AbsXCode("STA", code[++pc], code[++pc]);
                case 0xA0:
                    return ImmediateCode("LDY", code[++pc]);
                case 0xA1:
                    return IndirectXCode("LDA", code[++pc]);
                case 0xA2:
                    return ImmediateCode("LDX", code[++pc]);
                case 0xA4:
                    return ZeropageCode("LDY", code[++pc]);
                case 0xA5:
                    return ZeropageCode("LDA", code[++pc]);
                case 0xA6:
                    return ZeropageCode("LDX", code[++pc]);
                case 0xA8:
                    return "TAY";
                case 0xA9:
                    return ImmediateCode("LDA", code[++pc]);
                case 0xAA:
                    return "TAX";
                case 0xAC:
                    return AbsCode("LDY", code[++pc], code[++pc]);
                case 0xAD:
                    return AbsCode("LDA", code[++pc], code[++pc]);
                case 0xAE:
                    return AbsCode("LDX", code[++pc], code[++pc]);
                case 0xB0:
                    return RelativeCode("BCS", code[++pc]);
                case 0xB1:
                    return IndirectYCode("LDA", code[++pc]);
                case 0xB4:
                    return ZeropageXCode("LDY", code[++pc]);
                case 0xB5:
                    return ZeropageXCode("LDA", code[++pc]);
                case 0xB6:
                    return ZeropageYCode("LDX", code[++pc]);
                case 0xB8:
                    return "CLV";
                case 0xB9:
                    return AbsYCode("LDA", code[++pc], code[++pc]);
                case 0xBA:
                    return "TSX";
                case 0xBC:
                    return AbsXCode("LDY", code[++pc], code[++pc]);
                case 0xBD:
                    return AbsXCode("LDA", code[++pc], code[++pc]);
                case 0xBE:
                    return AbsYCode("LDX", code[++pc], code[++pc]);
                case 0xC0:
                    return ImmediateCode("CPY", code[++pc]);
                case 0xC1:
                    return IndirectXCode("CMP", code[++pc]);
                case 0xC4:
                    return ZeropageCode("CPY", code[++pc]);
                case 0xC5:
                    return ZeropageCode("CMP", code[++pc]);
                case 0xC6:
                    return ZeropageCode("DEC", code[++pc]);
                case 0xC8:
                    return "INY";
                case 0xC9:
                    return ImmediateCode("CMP", code[++pc]);
                case 0xCA:
                    return "DEX";
                case 0xCC:
                    return AbsCode("CPY", code[++pc], code[++pc]);
                case 0xCD:
                    return AbsCode("CMP", code[++pc], code[++pc]);
                case 0xCE:
                    return AbsCode("DEC", code[++pc], code[++pc]);
                case 0xD0:
                    return RelativeCode("BNE", code[++pc]);
                case 0xD1:
                    return IndirectYCode("CMP", code[++pc]);
                case 0xD5:
                    return ZeropageXCode("CMP", code[++pc]);
                case 0xD6:
                    return ZeropageYCode("DEC", code[++pc]);
                case 0xD8:
                    return "CLD";
                case 0xD9:
                    return AbsYCode("CMP", code[++pc], code[++pc]);
                case 0xDD:
                    return AbsXCode("CMP", code[++pc], code[++pc]);
                case 0xDE:
                    return AbsXCode("DEC", code[++pc], code[++pc]);
                case 0xE0:
                    return ImmediateCode("CPX", code[++pc]);
                case 0xE1:
                    return IndirectXCode("SBC", code[++pc]);
                case 0xE4:
                    return ZeropageCode("CPX", code[++pc]);
                case 0xE5:
                    return ZeropageCode("SBC", code[++pc]);
                case 0xE6:
                    return ZeropageCode("INC", code[++pc]);
                case 0xE8:
                    return "INX";
                case 0xE9:
                    return ImmediateCode("SBC", code[++pc]);
                case 0xEA:
                    return "NOP";
                case 0xEC:
                    return AbsCode("CPX", code[++pc], code[++pc]);
                case 0xED:
                    return AbsCode("SBC", code[++pc], code[++pc]);
                case 0xEE:
                    return AbsCode("INC", code[++pc], code[++pc]);
                case 0xF0:
                    return RelativeCode("BEQ", code[++pc]);
                case 0xF1:
                    return IndirectYCode("SBC", code[++pc]);
                case 0xF5:
                    return ZeropageXCode("SBC", code[++pc]);
                case 0xF6:
                    return ZeropageXCode("INC", code[++pc]);
                case 0xF8:
                    return "SED";
                case 0xF9:
                    return AbsYCode("SBC", code[++pc], code[++pc]);
                case 0xFD:
                    return AbsXCode("SBC", code[++pc], code[++pc]);
                case 0xFE:
                    return AbsXCode("INC", code[++pc], code[++pc]);
                default:
                    return "Data " + code[0].ToString("X");
            }
        }

        public static string Disassemble(string code)
        {
            string[] sbytes = code.Split(' ');
            byte[] bbytes = new byte[sbytes.Length];
            for (int i = 0; i < sbytes.Length; i++)
            {
                bbytes[i] = HexStringToByte(sbytes[i]);
            }
            return Disassemble(bbytes, 0);
        }

        public static byte HexStringToByte(string stringbyte) => byte.Parse(stringbyte, System.Globalization.NumberStyles.HexNumber);

        #region Code Strings
        private static string ZeropageCode(string mnemonic, byte address) => $"{mnemonic} ${address.ToString("X2")}";
        private static string ZeropageXCode(string mnemonic, byte address) => $"{ZeropageCode(mnemonic, address)}, X";
        private static string ZeropageYCode(string mnemonic, byte address) => $"{ZeropageCode(mnemonic, address)}, Y";
        private static string AbsCode(string mnemonic, byte low, byte high) => $"{mnemonic} ${high.ToString("X2")}{low.ToString("X2")}";
        private static string AbsXCode(string mnemonic, byte low, byte high) => $"{AbsCode(mnemonic, low, high)}, X";
        private static string AbsYCode(string mnemonic, byte low, byte high) => $"{AbsCode(mnemonic, low, high)}, Y";
        private static string ImmediateCode(string mnemonic, byte value) => $"{mnemonic} #${value.ToString("X2")}";
        private static string IndirectCode(string mnemonic, byte low, byte high) => $"{mnemonic} (${high.ToString("X2")}{low.ToString("X2")})";
        private static string IndirectXCode(string mnemonic, byte address) => $"{mnemonic} (${address.ToString("X2")}, X)";
        private static string IndirectYCode(string mnemonic, byte address) => $"{mnemonic} (${address.ToString("X2")}), Y";
        private static string RelativeCode(string mnemonic, byte value) => $"{mnemonic} ${value.ToString("X2")}";
        #endregion
    }
}
