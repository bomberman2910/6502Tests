using System;

namespace lib6502
{
    public class CPU6502
    {

        //will not (yet) support decimal mode

        public int Cycles { get; set; }

        public ushort PC { get; set; }
        public byte SP { get; set; }
        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte SR { get; set; } //N V - - D I Z C

        public bool NMI { get; set; }
        public bool IRQ { get; set; }

        public Bus Bus { get; }

        //Constructor
        public CPU6502(Bus bus)
        {
            Bus = bus;

            PC = (ushort)((Bus.GetData(0xFFFD) << 8) + Bus.GetData(0xFFFC));
            SP = 0xFF;
            A = 0x00;
            X = 0x00;
            Y = 0x00;
            SR = 0x20;

            Cycles = 0;
        }

        #region Helpers
        public static bool CheckBit(byte value, int bit) => ((value << (7 - bit)) >> 7) == 1;

        public override string ToString()
        {
            string output = "";
            output += $"A: ${A.ToString("X2")}\tX: ${X.ToString("X2")}\tY: ${Y.ToString("X2")}\tSP: ${SP.ToString("X2")}\tPC: ${PC.ToString("X4")}";
            output += $"\tSR: {(CheckFlag(EFlag.NEG) ? "N" : "-")}{(CheckFlag(EFlag.OVR) ? "O" : "-")}{(CheckFlag(EFlag.BRK) ? "B" : "-")}-{(CheckFlag(EFlag.DEC) ? "D" : "-")}{(CheckFlag(EFlag.IRQ) ? "I" : "-")}{(CheckFlag(EFlag.ZER) ? "Z" : "-")}{(CheckFlag(EFlag.CAR) ? "C" : "-")}\t";
            output += $"Instruction: {DisASM6502.Disassemble(new byte[] { Bus.GetData(PC), Bus.GetData((ushort)(PC + 1)), Bus.GetData((ushort)(PC + 2)) }, 0)}\n\n";
            return output;
        }

        public void Reset()
        {
            PC = (ushort)((Bus.GetData(0xFFFD) << 8) + Bus.GetData(0xFFFC));
            SP = 0xFF;
            A = 0x00;
            X = 0x00;
            Y = 0x00;
            SR = 0x20;

            Cycles = 0;
        }
        #endregion

        #region Status register methods
        private bool CheckFlag(byte flag) => ((SR & flag) >> (int)Math.Log(flag, 2)) == 1;

        private void SetFlag(byte flag, bool state)
        {
            if (state)
                SR |= flag;
            else
                SR &= (byte)~flag;
        }

        private bool TestForCarry(short value) => (value < 0) || (value > 255);

        private bool TestForOverflow(short value) => (value < -128) || (value > 127);
        #endregion

        #region Stack operations
        private void PushToStack(byte value)
        {
            Bus.SetData(value, (ushort)(0x0100 + SP));
            SP--;
        }

        private byte PullFromStack()
        {
            SP++;
            return Bus.GetData((ushort)(0x0100 + SP));
        }
        #endregion

        #region Interrupt routines
        public void HandleIRQ()
        {
            PushToStack(BitConverter.GetBytes(PC)[1]);
            PushToStack(BitConverter.GetBytes(PC)[0]);
            PushToStack(SR);
            SetFlag(EFlag.IRQ, true);
            PC = (ushort)((Bus.GetData(0xFFFF) << 8) + Bus.GetData(0xFFFE));
            Cycles = 8;
        }

        public void HandleNMI()
        {
            PushToStack(BitConverter.GetBytes(PC)[1]);
            PushToStack(BitConverter.GetBytes(PC)[0]);
            PushToStack(SR);
            SetFlag(EFlag.IRQ, true);
            NMI = false;
            PC = (ushort)((Bus.GetData(0xFFFB) << 8) + Bus.GetData(0xFFFA));
            Cycles = 8;
        }
        #endregion

        #region Addressing modes
        private ushort GetRelAddr() => (ushort)(PC + ((sbyte)Bus.GetData((ushort)(PC + 1)) + 2));

        private ushort GetAbsAddr() => (ushort)((Bus.GetData((ushort)(PC + 2)) << 8) + Bus.GetData((ushort)(PC + 1)));

        private ushort GetAbsXAddr(out bool wrap)
        {
            wrap = (ushort)(Bus.GetData((ushort)(PC + 1)) + X) > 0xFF;
            return (ushort)((Bus.GetData((ushort)(PC + 2)) << 8) + Bus.GetData((ushort)(PC + 1)) + X + ((SR & EFlag.CAR) == 1 ? 1 : 0));
        }

        private ushort GetAbsYAddr(out bool wrap)
        {
            wrap = (ushort)(Bus.GetData((ushort)(PC + 1)) + X) > 0xFF;
            return (ushort)((Bus.GetData((ushort)(PC + 2)) << 8) + Bus.GetData((ushort)(PC + 1)) + Y + ((SR & EFlag.CAR) == 1 ? 1 : 0));
        }

        private byte GetZPAddr() => Bus.GetData((ushort)(PC + 1));

        private byte GetZPXAddr() => (byte)(Bus.GetData((ushort)(PC + 1)) + X);

        private byte GetZPYAddr() => (byte)(Bus.GetData((ushort)(PC + 1)) + Y);

        private ushort GetIndXAddr()
        {
            byte index = (byte)(Bus.GetData((ushort)(PC + 1)) + X);
            return (ushort)((Bus.GetData((ushort)(index + 1)) << 8) + Bus.GetData((ushort)(index)));
        }

        private ushort GetIndYAddr(out bool wrap)
        {
            ushort index = Bus.GetData((ushort)(PC + 1));
            wrap = (index + Y) > 0xFF;
            return (ushort)((Bus.GetData((ushort)(index + 1)) << 8) + Bus.GetData((ushort)(index)) + Y + ((SR & EFlag.CAR) == 1 ? 1 : 0));
        }
        #endregion

        #region Operations with multiple addressing modes (except store operations)
        private void ADC(byte value)
        {
            short sum = (short)(A + value + (CheckFlag(EFlag.CAR) ? 1 : 0));
            SetFlag(EFlag.OVR, (CheckBit(A, 7) == CheckBit(value, 7)) && (CheckBit(A, 7) != CheckBit((byte)sum, 7)));
            SetFlag(EFlag.CAR, (sum > 255) || (sum < 0));
            SetFlag(EFlag.NEG, CheckBit((byte)sum, 7));
            SetFlag(EFlag.ZER, sum == 0);
            A = (byte)sum;
        }

        private void AND(byte value)
        {
            A = (byte)(A & value);
            SetFlag(EFlag.ZER, A == 0);
            SetFlag(EFlag.NEG, CheckBit(A, 7));
        }

        private byte ASL(byte value)
        {
            SetFlag(EFlag.CAR, CheckBit(value, 7));
            value <<= 1;
            SetFlag(EFlag.NEG, CheckBit(value, 7));
            SetFlag(EFlag.ZER, value == 0);
            return value;
        }

        private void BIT(byte value)
        {
            byte temp = (byte)(A & value);
            SetFlag(EFlag.ZER, temp == 0);
            SetFlag(EFlag.OVR, CheckBit(temp, 6));
            SetFlag(EFlag.NEG, CheckBit(temp, 7));
        }

        private void CMP(byte value)
        {
            if (A < value)
            {
                SetFlag(EFlag.NEG, CheckBit((byte)(A - value), 7));
                SetFlag(EFlag.ZER, false);
                SetFlag(EFlag.CAR, false);
            }
            else if (A == value)
            {
                SetFlag(EFlag.NEG, false);
                SetFlag(EFlag.ZER, true);
                SetFlag(EFlag.CAR, true);
            }
            else
            {
                SetFlag(EFlag.NEG, CheckBit((byte)(A - value), 7));
                SetFlag(EFlag.ZER, false);
                SetFlag(EFlag.CAR, true);
            }
        }

        private void CPX(byte value)
        {
            if (X < value)
            {
                SetFlag(EFlag.NEG, CheckBit((byte)(X - value), 7));
                SetFlag(EFlag.ZER, false);
                SetFlag(EFlag.CAR, false);
            }
            else if (X == value)
            {
                SetFlag(EFlag.NEG, false);
                SetFlag(EFlag.ZER, true);
                SetFlag(EFlag.CAR, true);
            }
            else
            {
                SetFlag(EFlag.NEG, CheckBit((byte)(X - value), 7));
                SetFlag(EFlag.ZER, false);
                SetFlag(EFlag.CAR, true);
            }
        }

        private void CPY(byte value)
        {
            if (Y < value)
            {
                SetFlag(EFlag.NEG, CheckBit((byte)(Y - value), 7));
                SetFlag(EFlag.ZER, false);
                SetFlag(EFlag.CAR, false);
            }
            else if (Y == value)
            {
                SetFlag(EFlag.NEG, false);
                SetFlag(EFlag.ZER, true);
                SetFlag(EFlag.CAR, true);
            }
            else
            {
                SetFlag(EFlag.NEG, CheckBit((byte)(Y - value), 7));
                SetFlag(EFlag.ZER, false);
                SetFlag(EFlag.CAR, true);
            }
        }

        private byte DEC(byte value)
        {
            value--;
            SetFlag(EFlag.NEG, CheckBit(value, 7));
            SetFlag(EFlag.ZER, value == 0);
            return value;
        }

        private void EOR(byte value)
        {
            A ^= value;
            SetFlag(EFlag.NEG, CheckBit(A, 7));
            SetFlag(EFlag.ZER, A == 0);
        }

        private byte INC(byte value)
        {
            value++;
            SetFlag(EFlag.NEG, CheckBit(value, 7));
            SetFlag(EFlag.ZER, value == 0);
            return value;
        }

        private void JMP(ushort address) => PC = address;

        private void LDA(byte value)
        {
            A = value;
            SetFlag(EFlag.NEG, CheckBit(A, 7));
            SetFlag(EFlag.ZER, A == 0);
        }

        private void LDX(byte value)
        {
            X = value;
            SetFlag(EFlag.NEG, CheckBit(X, 7));
            SetFlag(EFlag.ZER, X == 0);
        }

        private void LDY(byte value)
        {
            Y = value;
            SetFlag(EFlag.NEG, CheckBit(Y, 7));
            SetFlag(EFlag.ZER, Y == 0);
        }

        private byte LSR(byte value)
        {
            SetFlag(EFlag.CAR, CheckBit(value, 0));
            value >>= 1;
            SetFlag(EFlag.ZER, value == 0);
            return value;
        }

        private void ORA(byte value)
        {
            A |= value;
            SetFlag(EFlag.NEG, CheckBit(A, 7));
            SetFlag(EFlag.ZER, A == 0);
        }

        private byte ROL(byte value)
        {
            bool carrytemp = CheckFlag(EFlag.CAR);
            SetFlag(EFlag.CAR, CheckBit(value, 7));
            value <<= 1;
            if (carrytemp)
                value++;
            SetFlag(EFlag.NEG, CheckBit(value, 7));
            SetFlag(EFlag.ZER, value == 0);
            return value;
        }

        private byte ROR(byte value)
        {
            bool carrytemp = CheckFlag(EFlag.CAR);
            SetFlag(EFlag.CAR, CheckBit(value, 0));
            value >>= 1;
            if (carrytemp)
                value += 1 << 7;
            SetFlag(EFlag.NEG, CheckBit(value, 7));
            SetFlag(EFlag.ZER, value == 0);
            return value;
        }

        private void SBC(byte value) => ADC((byte)~value);
        #endregion

        #region Execution
        public void Exec()
        {
            if (!CheckFlag(EFlag.IRQ) && IRQ)
                HandleIRQ();
            if (NMI)
                HandleNMI();
            if (Cycles == 0)
            {
                byte instruction = Bus.GetData(PC);
                bool wrap;

                switch (instruction)
                {
                    case 0x00:  //BRK
                        Cycles = 7;
                        PC += 2;
                        PushToStack(BitConverter.GetBytes(PC)[1]);
                        PushToStack(BitConverter.GetBytes(PC)[0]);
                        PushToStack(SR);
                        SetFlag(EFlag.BRK, true);
                        SetFlag(EFlag.IRQ, true);
                        PC = (ushort)((Bus.GetData(0xFFFF) << 8) + Bus.GetData(0xFFFE));
                        break;
                    case 0x01:  //ORA   (indirect, X)
                        Cycles = 6;
                        ORA(Bus.GetData(GetIndXAddr()));
                        PC += 2;
                        break;
                    case 0x02:  //
                    case 0x03:  //
                    case 0x04:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x05:  //ORA   (zeropage)
                        Cycles = 3;
                        ORA(Bus.GetData(GetZPAddr()));
                        PC += 2;
                        break;
                    case 0x06:  //ASL   (zeropage)
                        Cycles = 5;
                        Bus.SetData(ASL(Bus.GetData(GetZPAddr())), GetZPAddr());
                        PC += 2;
                        break;
                    case 0x07:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x08:  //PHP
                        Cycles = 3;
                        PushToStack(SR);
                        PC++;
                        break;
                    case 0x09:  //ORA   (immediate)
                        Cycles = 2;
                        ORA(Bus.GetData((ushort)(PC + 1)));
                        PC += 2;
                        break;
                    case 0x0A:  //ASL   (accumulator)
                        Cycles = 2;
                        A = ASL(A);
                        PC++;
                        break;
                    case 0x0B:  //
                    case 0x0C:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x0D:  //ORA   (absolute)
                        Cycles = 4;
                        ORA(Bus.GetData(GetAbsAddr()));
                        PC += 3;
                        break;
                    case 0x0E:  //ASL   (absolute)
                        Cycles = 6;
                        Bus.SetData(ASL(Bus.GetData(GetAbsAddr())), GetAbsAddr());
                        PC += 3;
                        break;
                    case 0x0F:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x10:  //BPL   (relative)
                        if (CheckFlag(EFlag.NEG))
                        {
                            Cycles = 2;
                            PC += 2;
                        }
                        else
                        {
                            ushort newaddr = GetRelAddr();
                            Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(PC)[1] ? 4 : 3;
                            PC = newaddr;
                        }
                        break;
                    case 0x11:  //ORA   (indirect, Y)
                        ORA(Bus.GetData(GetIndYAddr(out wrap)));
                        Cycles = wrap ? 6 : 5;
                        PC += 2;
                        break;
                    case 0x12:  //
                    case 0x13:  //
                    case 0x14:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x15:  //ORA   (zeropage, X)
                        ORA(Bus.GetData(GetZPXAddr()));
                        Cycles = 4;
                        PC += 2;
                        break;
                    case 0x16:  //ASL   (zeropage, X)
                        Cycles = 6;
                        Bus.SetData(ASL(Bus.GetData(GetZPXAddr())), GetZPXAddr());
                        PC += 2;
                        break;
                    case 0x17:
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x18:  //CLC
                        Cycles = 2;
                        SetFlag(EFlag.CAR, false);
                        PC++;
                        break;
                    case 0x19:  //ORA   (absolute, Y)
                        ORA(Bus.GetData(GetAbsYAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0x1A:  //
                    case 0x1B:  //
                    case 0x1C:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x1D:  //ORA   (absolute, X)
                        ORA(Bus.GetData(GetAbsXAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0x1E:  //ASL   (absolute, X)
                        Cycles = 7;
                        Bus.SetData(ASL(Bus.GetData(GetAbsXAddr(out wrap))), GetAbsXAddr(out wrap));
                        PC += 3;
                        break;
                    case 0x1F:
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x20:  //JSR   (absolute)
                        Cycles = 6;
                        PushToStack(BitConverter.GetBytes(PC + 2)[1]);
                        PushToStack(BitConverter.GetBytes(PC + 2)[0]);
                        PC = GetAbsAddr();
                        break;
                    case 0x21:  //AND   (indirect, X)
                        Cycles = 6;
                        AND(Bus.GetData(GetIndXAddr()));
                        PC += 2;
                        break;
                    case 0x22:  //
                    case 0x23:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x24:  //BIT   (zeropage)
                        Cycles = 3;
                        BIT(Bus.GetData(GetZPAddr()));
                        PC += 2;
                        break;
                    case 0x25:  //AND   (zeropage)
                        Cycles = 3;
                        AND(Bus.GetData(GetZPAddr()));
                        PC += 2;
                        break;
                    case 0x26:  //ROL   (zeropage)
                        Cycles = 5;
                        ROL(Bus.GetData(GetZPAddr()));
                        PC += 2;
                        break;
                    case 0x27:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x28:  //PLP
                        SR = PullFromStack();
                        PC++;
                        break;
                    case 0x29:  //AND   (immediate)
                        Cycles = 2;
                        AND(Bus.GetData((ushort)(PC + 1)));
                        PC += 2;
                        break;
                    case 0x2A:  //ROL   (accumulator)
                        Cycles = 2;
                        A = ROL(A);
                        PC++;
                        break;
                    case 0x2B:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x2C:  //BIT   (absolute)
                        Cycles = 4;
                        BIT(Bus.GetData(GetAbsAddr()));
                        PC += 3;
                        break;
                    case 0x2D:  //AND   (absolute)
                        Cycles = 4;
                        AND(Bus.GetData(GetAbsAddr()));
                        PC += 3;
                        break;
                    case 0x2E:  //ROL   (absolute)
                        Cycles = 6;
                        Bus.SetData(ROL(Bus.GetData(GetAbsAddr())), GetAbsAddr());
                        PC += 3;
                        break;
                    case 0x2F:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x30:  //BMI   (relative)
                        if (CheckFlag(EFlag.NEG))
                        {
                            ushort newaddr = GetRelAddr();
                            Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(PC)[1] ? 4 : 3;
                            PC = newaddr;
                        }
                        else
                        {
                            Cycles = 2;
                            PC += 2;
                        }
                        break;
                    case 0x31:  //AND   (indirect, Y)
                        AND(Bus.GetData(GetIndYAddr(out wrap)));
                        Cycles = wrap ? 6 : 5;
                        PC += 2;
                        break;
                    case 0x32:  //
                    case 0x33:  //
                    case 0x34:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x35:  //AND   (zeropage, X)
                        Cycles = 4;
                        AND(Bus.GetData(GetZPXAddr()));
                        PC += 2;
                        break;
                    case 0x36:  //ROL   (zeropage, x)
                        Bus.SetData(ROL(Bus.GetData(GetZPXAddr())), GetZPXAddr());
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0x37:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x38:  //SEC
                        Cycles = 2;
                        SetFlag(EFlag.CAR, true);
                        PC++;
                        break;
                    case 0x39:  //AND   (absolute, Y)
                        AND(Bus.GetData(GetAbsYAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0x3A:  //
                    case 0x3B:  //
                    case 0x3C:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x3D:  //AND   (absolute, X)
                        AND(Bus.GetData(GetAbsXAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0x3E:  //ROL   (absolute, X)
                        Bus.SetData(ROL(Bus.GetData(GetAbsXAddr(out wrap))), GetAbsXAddr(out wrap));
                        Cycles = 7;
                        PC += 3;
                        break;
                    case 0x3F:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x40:  //RTI
                        Cycles = 6;
                        SR = PullFromStack();
                        PC = (ushort)(PullFromStack() + (PullFromStack() << 8));
                        break;
                    case 0x41:  //EOR   (indirect, Y)
                        EOR(Bus.GetData(GetIndYAddr(out wrap)));
                        Cycles = wrap ? 6 : 5;
                        PC += 2;
                        break;
                    case 0x42:  //
                    case 0x43:  //
                    case 0x44:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x45:  //EOR   (zeropage)
                        EOR(Bus.GetData(GetZPAddr()));
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0x46:  //LSR   (zeropage)
                        Bus.SetData(LSR(Bus.GetData(GetZPAddr())), GetZPAddr());
                        Cycles = 5;
                        PC += 2;
                        break;
                    case 0x47:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x48:  //PHA
                        Cycles = 3;
                        PushToStack(A);
                        PC++;
                        break;
                    case 0x49:  //EOR   (immediate)
                        EOR(Bus.GetData((ushort)(PC + 1)));
                        Cycles = 2;
                        PC += 2;
                        break;
                    case 0x4A:  //LSR   (accumulator)
                        A = LSR(A);
                        Cycles = 2;
                        PC++;
                        break;
                    case 0x4B:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x4C:  //JMP   (absolute)
                        Cycles = 3;
                        JMP(GetAbsAddr());
                        break;
                    case 0x4D:  //EOR   (absolute)
                        Cycles = 4;
                        EOR(Bus.GetData(GetAbsAddr()));
                        PC += 3;
                        break;
                    case 0x4E:  //LSR   (absolute)
                        Bus.SetData(LSR(Bus.GetData(GetAbsAddr())), GetAbsAddr());
                        Cycles = 6;
                        PC += 3;
                        break;
                    case 0x4F:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x50:  //BVC   (relative)
                        if (CheckFlag(EFlag.OVR))
                        {
                            Cycles = 2;
                            PC += 2;
                        }
                        else
                        {
                            ushort newaddr = GetRelAddr();
                            Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(PC)[1] ? 4 : 3;
                            PC = newaddr;
                        }
                        break;
                    case 0x51:  //EOR   (indirect, Y)
                        EOR(Bus.GetData(GetIndYAddr(out wrap)));
                        Cycles = wrap ? 6 : 5;
                        PC += 2;
                        break;
                    case 0x52:  //
                    case 0x53:  //
                    case 0x54:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x55:  //EOR   (zeropage, X)
                        EOR(Bus.GetData(GetZPXAddr()));
                        Cycles = 4;
                        PC += 2;
                        break;
                    case 0x56:  //LSR   (zeropage, X)
                        Bus.SetData(LSR(Bus.GetData(GetZPXAddr())), GetZPXAddr());
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0x57:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x58:  //CLI
                        SetFlag(EFlag.IRQ, false);
                        Cycles = 2;
                        PC++;
                        break;
                    case 0x59:  //EOR   (absolute, Y)
                        EOR(Bus.GetData(GetAbsYAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0x5A:  //
                    case 0x5B:  //
                    case 0x5C:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x5D:  //EOR   (absolute, X)
                        EOR(Bus.GetData(GetAbsXAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0x5E:  //LSR   (absolute, X)
                        Bus.SetData(LSR(Bus.GetData(GetAbsXAddr(out wrap))), GetAbsXAddr(out wrap));
                        Cycles = 7;
                        PC += 3;
                        break;
                    case 0x5F:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x60:  //RTS
                        Cycles = 6;
                        PC = (ushort)(PullFromStack() + (PullFromStack() << 8));
                        PC++;
                        break;
                    case 0x61:  //ADC   (indirect, X)
                        ADC(Bus.GetData(GetIndXAddr()));
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0x62:  //
                    case 0x63:  //
                    case 0x64:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x65:  //ADC   (zeropage)
                        ADC(Bus.GetData(GetZPAddr()));
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0x66:  //ROR   (zeropage)
                        Bus.SetData(ROR(Bus.GetData(GetZPAddr())), GetZPAddr());
                        Cycles = 5;
                        PC += 2;
                        break;
                    case 0x67:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x68:  //PLA
                        Cycles = 4;
                        A = PullFromStack();
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC++;
                        break;
                    case 0x69:  //ADC   (immediate)
                        ADC(Bus.GetData((ushort)(PC + 1)));
                        Cycles = 2;
                        PC += 2;
                        break;
                    case 0x6A:  //ROR   (accumulator)
                        A = ROR(A);
                        Cycles = 2;
                        PC++;
                        break;
                    case 0x6B:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x6C:  //JMP   (indirect)
                        PC = (ushort)((Bus.GetData(GetAbsAddr()) << 8) + Bus.GetData((ushort)(GetAbsAddr() + 1)));
                        Cycles = 5;
                        break;
                    case 0x6D:  //ADC   (absolute)
                        ADC(Bus.GetData(GetAbsAddr()));
                        Cycles = 4;
                        PC += 3;
                        break;
                    case 0x6E:  //ROR   (absolute)
                        Bus.SetData(ROR(Bus.GetData(GetAbsAddr())), GetAbsAddr());
                        Cycles = 6;
                        PC += 3;
                        break;
                    case 0x6F:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x70:  //BVS   (relative)
                        if (CheckFlag(EFlag.OVR))
                        {
                            ushort newaddr = GetRelAddr();
                            Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(PC)[1] ? 4 : 3;
                            PC = newaddr;
                        }
                        else
                        {
                            Cycles = 2;
                            PC += 2;
                        }
                        break;
                    case 0x71:  //ADC   (indirect, Y)
                        ADC(Bus.GetData(GetIndYAddr(out wrap)));
                        Cycles = wrap ? 6 : 5;
                        PC += 2;
                        break;
                    case 0x72:  //
                    case 0x73:  //
                    case 0x74:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x75:  //ADC   (zeropage, X)
                        ADC(Bus.GetData(GetZPXAddr()));
                        Cycles = 4;
                        PC += 2;
                        break;
                    case 0x76:  //ROR   (zeropage, X)
                        Bus.SetData(ROR(Bus.GetData(GetZPXAddr())), GetZPXAddr());
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0x77:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x78:  //SEI
                        SetFlag(EFlag.IRQ, true);
                        PC++;
                        Cycles = 2;
                        break;
                    case 0x79:  //ADC   (absolute, Y)
                        ADC(Bus.GetData(GetAbsYAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0x7A:  //
                    case 0x7B:  //
                    case 0x7C:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x7D:  //ADC   (absolute, X)
                        ADC(Bus.GetData(GetAbsXAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0x7E:  //ROR   (absolute, X)
                        Bus.SetData(ROR(Bus.GetData(GetAbsXAddr(out wrap))), GetAbsXAddr(out wrap));
                        Cycles = 7;
                        PC += 3;
                        break;
                    case 0x7F:  //
                    case 0x80:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x81:  //STA   (indirect, X)
                        Bus.SetData(A, GetIndXAddr());
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0x82:  //
                    case 0x83:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x84:  //STY   (zeropage)
                        Bus.SetData(Y, GetZPAddr());
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0x85:  //STA   (zeropage)
                        Bus.SetData(A, GetZPAddr());
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0x86:  //STX   (zeropage)
                        Bus.SetData(X, GetZPAddr());
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0x87:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x88:  //DEY
                        Y--;
                        SetFlag(EFlag.NEG, CheckBit(Y, 7));
                        SetFlag(EFlag.ZER, Y == 0);
                        Cycles = 2;
                        PC++;
                        break;
                    case 0x89:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x8A:  //TXA
                        A = X;
                        SetFlag(EFlag.ZER, A == 0);
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        Cycles = 2;
                        PC++;
                        break;
                    case 0x8B:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x8C:  //STY   (absolute)
                        Bus.SetData(Y, GetAbsAddr());
                        Cycles = 4;
                        PC += 3;
                        break;
                    case 0x8D:  //STA   (absolute)
                        Bus.SetData(A, GetAbsAddr());
                        Cycles = 4;
                        PC += 3;
                        break;
                    case 0x8E:  //STX   (absolute)
                        Bus.SetData(X, GetAbsAddr());
                        Cycles = 4;
                        PC += 3;
                        break;
                    case 0x8F:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x90:  //BCC   (relative)
                        if (CheckFlag(EFlag.CAR))
                        {
                            Cycles = 2;
                            PC += 2;
                        }
                        else
                        {
                            ushort newaddr = GetRelAddr();
                            Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(PC)[1] ? 4 : 3;
                            PC = newaddr;
                        }
                        break;
                    case 0x91:  //STA   (indirect, Y)
                        Bus.SetData(A, GetIndYAddr(out wrap));
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0x92:  //
                    case 0x93:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x94:  //STY   (zeropage, X)
                        Bus.SetData(Y, GetZPXAddr());
                        Cycles = 4;
                        PC += 2;
                        break;
                    case 0x95:  //STA   (zeropage, X)
                        Bus.SetData(A, GetZPXAddr());
                        Cycles = 4;
                        PC += 2;
                        break;
                    case 0x96:  //STX   (zeropage, Y)
                        Bus.SetData(X, GetZPYAddr());
                        Cycles = 4;
                        PC += 2;
                        break;
                    case 0x97:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x98:  //TYA
                        A = Y;
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC++;
                        break;
                    case 0x99:  //STA   (absolute, Y)
                        Bus.SetData(A, GetAbsYAddr(out wrap));
                        Cycles = 5;
                        PC += 3;
                        break;
                    case 0x9A:  //TXS
                        SP = X;
                        Cycles = 2;
                        PC++;
                        break;
                    case 0x9B:  //
                    case 0x9C:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0x9D:  //STA   (absolute, X)
                        Bus.SetData(A, GetAbsXAddr(out wrap));
                        Cycles = 5;
                        PC += 3;
                        break;
                    case 0x9E:  //
                    case 0x9F:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xA0:  //LDY   (immediate)
                        Y = Bus.GetData((ushort)(PC + 1));
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(Y, 7));
                        SetFlag(EFlag.ZER, Y == 0);
                        PC += 2;
                        break;
                    case 0xA1:  //LDA   (indirect, X)
                        A = Bus.GetData(GetIndXAddr());
                        Cycles = 6;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC += 2;
                        break;
                    case 0xA2:  //LDX   (immediate)
                        X = Bus.GetData((ushort)(PC + 1));
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC += 2;
                        break;
                    case 0xA3:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xA4:  //LDY   (zeropage)
                        Y = Bus.GetData(GetZPAddr());
                        Cycles = 3;
                        SetFlag(EFlag.NEG, CheckBit(Y, 7));
                        SetFlag(EFlag.ZER, Y == 0);
                        PC += 2;
                        break;
                    case 0xA5:  //LDA   (zeropage)
                        A = Bus.GetData(GetZPAddr());
                        Cycles = 3;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC += 2;
                        break;
                    case 0xA6:  //LDX   (zeropage)
                        X = Bus.GetData(GetZPAddr());
                        Cycles = 3;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC += 2;
                        break;
                    case 0xA7:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xA8:  //TAY
                        Y = A;
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(Y, 7));
                        SetFlag(EFlag.ZER, Y == 0);
                        PC++;
                        break;
                    case 0xA9:  //LDA   (immediate)
                        A = Bus.GetData((ushort)(PC + 1));
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC += 2;
                        break;
                    case 0xAA:  //TAX
                        X = A;
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC++;
                        break;
                    case 0xAB:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xAC:  //LDY   (absolute)
                        Y = Bus.GetData(GetAbsAddr());
                        Cycles = 4;
                        SetFlag(EFlag.NEG, CheckBit(Y, 7));
                        SetFlag(EFlag.ZER, Y == 0);
                        PC += 3;
                        break;
                    case 0xAD:  //LDA   (absolute)
                        A = Bus.GetData(GetAbsAddr());
                        Cycles = 4;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC += 3;
                        break;
                    case 0xAE:  //LDX   (absolute)
                        X = Bus.GetData(GetAbsAddr());
                        Cycles = 4;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC += 3;
                        break;
                    case 0xAF:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xB0:  //BCS   (relative)
                        if (CheckFlag(EFlag.CAR))
                        {
                            ushort newaddr = GetRelAddr();
                            Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(PC)[1] ? 4 : 3;
                            PC = newaddr;
                        }
                        else
                        {
                            Cycles = 2;
                            PC += 2;
                        }
                        break;
                    case 0xB1:  //LDA   (indirect, Y)
                        A = Bus.GetData(GetIndYAddr(out wrap));
                        Cycles = wrap ? 6 : 5;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC += 2;
                        break;
                    case 0xB2:  //
                    case 0xB3:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xB4:  //LDY   (zeropage, X)
                        Y = Bus.GetData(GetZPXAddr());
                        Cycles = 4;
                        SetFlag(EFlag.NEG, CheckBit(Y, 7));
                        SetFlag(EFlag.ZER, Y == 0);
                        PC += 2;
                        break;
                    case 0xB5:  //LDA   (zeropage, X)
                        A = Bus.GetData(GetZPXAddr());
                        Cycles = 4;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC += 2;
                        break;
                    case 0xB6:  //LDX   (zeropage, Y)
                        X = Bus.GetData(GetZPYAddr());
                        Cycles = 4;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC += 2;
                        break;
                    case 0xB7:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xB8:  //CLV
                        SetFlag(EFlag.OVR, false);
                        Cycles = 2;
                        PC++;
                        break;
                    case 0xB9:  //LDA   (absolute, Y)
                        A = Bus.GetData(GetAbsYAddr(out wrap));
                        Cycles = wrap ? 5 : 4;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC += 3;
                        break;
                    case 0xBA:  //TSX
                        X = SP;
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC++;
                        break;
                    case 0xBB:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xBC:  //LDY   (absolute, X)
                        Y = Bus.GetData(GetAbsXAddr(out wrap));
                        Cycles = wrap ? 5 : 4;
                        SetFlag(EFlag.NEG, CheckBit(Y, 7));
                        SetFlag(EFlag.ZER, Y == 0);
                        PC += 3;
                        break;
                    case 0xBD:  //LDA   (absolute, X)
                        A = Bus.GetData(GetAbsXAddr(out wrap));
                        Cycles = wrap ? 5 : 4;
                        SetFlag(EFlag.NEG, CheckBit(A, 7));
                        SetFlag(EFlag.ZER, A == 0);
                        PC += 3;
                        break;
                    case 0xBE:  //LDX   (absolute, Y)
                        X = Bus.GetData(GetAbsYAddr(out wrap));
                        Cycles = wrap ? 5 : 4;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC += 3;
                        break;
                    case 0xBF:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xC0:  //CPY   (immediate)
                        CPY(Bus.GetData((ushort)(PC + 1)));
                        Cycles = 2;
                        PC += 2;
                        break;
                    case 0xC1:  //CMP   (indirect, X)
                        CMP(Bus.GetData(GetIndXAddr()));
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0xC2:  //
                    case 0xC3:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xC4:  //CPY   (zeropage)
                        CPY(Bus.GetData(GetZPAddr()));
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0xC5:  //CMP   (zeropage)
                        CMP(Bus.GetData(GetZPAddr()));
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0xC6:  //DEC   (zeropage)
                        Bus.SetData(DEC(Bus.GetData(GetZPAddr())), GetZPAddr());
                        Cycles = 5;
                        PC += 2;
                        break;
                    case 0xC7:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xC8:  //INY
                        Y++;
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(Y, 7));
                        SetFlag(EFlag.ZER, Y == 0);
                        PC++;
                        break;
                    case 0xC9:  //CMP   (immediate)
                        CMP(Bus.GetData((ushort)(PC + 1)));
                        Cycles = 2;
                        PC += 2;
                        break;
                    case 0xCA:  //DEX
                        X--;
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC++;
                        break;
                    case 0xCB:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xCC:  //CPY   (absolute)
                        CPY(Bus.GetData(GetAbsAddr()));
                        Cycles = 4;
                        PC += 3;
                        break;
                    case 0xCD:  //CMP   (absolute)
                        CMP(Bus.GetData(GetAbsAddr()));
                        Cycles = 4;
                        PC += 3;
                        break;
                    case 0xCE:  //DEC   (absolute)
                        Bus.SetData(DEC(Bus.GetData(GetAbsAddr())), GetAbsAddr());
                        Cycles = 6;
                        PC += 3;
                        break;
                    case 0xCF:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xD0:  //BNE   (relative)
                        if (CheckFlag(EFlag.ZER))
                        {
                            Cycles = 2;
                            PC += 2;
                        }
                        else
                        {
                            ushort newaddr = GetRelAddr();
                            Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(PC)[1] ? 4 : 3;
                            PC = newaddr;
                        }
                        break;
                    case 0xD1:  //CMP   (indirect, Y)
                        CMP(Bus.GetData(GetIndYAddr(out wrap)));
                        Cycles = wrap ? 6 : 5;
                        PC += 2;
                        break;
                    case 0xD2:  //
                    case 0xD3:  //
                    case 0xD4:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xD5:  //CMP   (zeropage, X)
                        CMP(Bus.GetData(GetZPXAddr()));
                        Cycles = 4;
                        PC += 2;
                        break;
                    case 0xD6:  //DEC   (zeropage, X)
                        Bus.SetData(DEC(Bus.GetData(GetZPXAddr())), GetZPXAddr());
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0xD7:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xD8:  //CLD
                        SetFlag(EFlag.DEC, false);
                        Cycles = 2;
                        PC++;
                        break;
                    case 0xD9:  //CMP   (absolute, Y)
                        CMP(Bus.GetData(GetAbsYAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0xDA:  //
                    case 0xDB:  //
                    case 0xDC:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xDD:  //CMP   (absolute, X)
                        CMP(Bus.GetData(GetAbsXAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0xDE:  //DEC   (absolute, X)
                        Bus.SetData(DEC(Bus.GetData(GetAbsXAddr(out wrap))), GetAbsXAddr(out wrap));
                        Cycles = 7;
                        PC += 3;
                        break;
                    case 0xDF:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xE0:  //CPX   (immediate)
                        CPX(Bus.GetData((ushort)(PC + 1)));
                        Cycles = 2;
                        PC += 2;
                        break;
                    case 0xE1:  //SBC   (indirect, X)
                        SBC(Bus.GetData(GetIndXAddr()));
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0xE2:  //
                    case 0xE3:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xE4:  //CPX   (zeropage)
                        CPX(Bus.GetData(GetZPAddr()));
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0xE5:  //SBC   (zeropage)
                        SBC(Bus.GetData(GetZPAddr()));
                        Cycles = 3;
                        PC += 2;
                        break;
                    case 0xE6:  //INC   (zeropage)
                        Bus.SetData(INC(Bus.GetData(GetZPAddr())), GetZPAddr());
                        Cycles = 5;
                        PC += 2;
                        break;
                    case 0xE7:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xE8:  //INX
                        X++;
                        Cycles = 2;
                        SetFlag(EFlag.NEG, CheckBit(X, 7));
                        SetFlag(EFlag.ZER, X == 0);
                        PC++;
                        break;
                    case 0xE9:  //SBC   (immediate)
                        SBC(Bus.GetData((ushort)(PC + 1)));
                        Cycles = 2;
                        PC += 2;
                        break;
                    case 0xEA:  //NOP
                        Cycles = 2;
                        PC++;
                        break;
                    case 0xEB:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xEC:  //CPX   (absolute)
                        CPX(Bus.GetData(GetAbsAddr()));
                        Cycles = 4;
                        PC += 3;
                        break;
                    case 0xED:  //SBC   (absolute)
                        SBC(Bus.GetData(GetAbsAddr()));
                        Cycles = 4;
                        PC += 3;
                        break;
                    case 0xEE:  //INC   (absolute)
                        Bus.SetData(INC(Bus.GetData(GetAbsAddr())), GetAbsAddr());
                        Cycles = 6;
                        PC += 3;
                        break;
                    case 0xEF:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xF0:  //BEQ   (relative)
                        if (CheckFlag(EFlag.ZER))
                        {
                            ushort newaddr = GetRelAddr();
                            Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(PC)[1] ? 4 : 3;
                            PC = newaddr;
                        }
                        else
                        {
                            Cycles = 2;
                            PC += 2;
                        }
                        break;
                    case 0xF1:  //SBC   (indirect, Y)
                        SBC(Bus.GetData(GetIndYAddr(out wrap)));
                        Cycles = wrap ? 6 : 5;
                        PC += 2;
                        break;
                    case 0xF2:  //
                    case 0xF3:  //
                    case 0xF4:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xF5:  //SBC   (zeropage, X)
                        SBC(Bus.GetData(GetZPXAddr()));
                        Cycles = 4;
                        PC += 2;
                        break;
                    case 0xF6:  //INC   (zeropage, X)
                        Bus.SetData(INC(Bus.GetData(GetZPXAddr())), GetZPXAddr());
                        Cycles = 6;
                        PC += 2;
                        break;
                    case 0xF7:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xF8:  //SED
                        SetFlag(EFlag.DEC, true);
                        Cycles = 2;
                        PC++;
                        break;
                    case 0xF9:  //SBC   (absolute, Y)
                        SBC(Bus.GetData(GetAbsYAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0xFA:  //
                    case 0xFB:  //
                    case 0xFC:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    case 0xFD:  //SBC   (absolute, X)
                        SBC(Bus.GetData(GetAbsXAddr(out wrap)));
                        Cycles = wrap ? 5 : 4;
                        PC += 3;
                        break;
                    case 0xFE:  //INC   (absolute, X)
                        Bus.SetData(INC(Bus.GetData(GetAbsXAddr(out wrap))), GetAbsXAddr(out wrap));
                        Cycles = 7;
                        PC += 3;
                        break;
                    case 0xFF:  //
                        throw new ArgumentException("Ungültiger Opcode");
                    default:
                        break;
                }
            }
            else { Cycles--; }
        }

        public void Step()
        {
            Exec();
            while (Cycles > 0)
                Exec();
        }
        #endregion
    }
}
