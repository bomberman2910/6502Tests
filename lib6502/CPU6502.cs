using System;

namespace lib6502;

public class Cpu6502
{
    public Cpu6502(Bus bus)
    {
        Bus = bus;

        ProgramCounter = (ushort)((Bus.GetData(0xFFFD) << 8) + Bus.GetData(0xFFFC));
        StackPointer = 0xFF;
        A = 0x00;
        X = 0x00;
        Y = 0x00;
        StatusRegister = 0x20;

        Cycles = 0;
    }

    public int Cycles { get; set; }

    public ushort ProgramCounter { get; set; }
    public byte StackPointer { get; set; }
    public byte A { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte StatusRegister { get; set; } //N V B - D I Z C

    public bool NonMaskableInterrupt { get; set; }
    public bool InterruptRequest { get; set; }

    public Bus Bus { get; }

    public override string ToString()
    {
        var output = "";
        output += $"A: ${A:X2}\tX: ${X:X2}\tY: ${Y:X2}\tSP: ${StackPointer:X2}\tPC: ${ProgramCounter:X4}";
        output += $"\tSR: {(CheckFlag(Flag.Negative) ? "N" : "-")}{(CheckFlag(Flag.Overflow) ? "O" : "-")}{(CheckFlag(Flag.Break) ? "B" : "-")}-{(CheckFlag(Flag.Decimal) ? "D" : "-")}{(CheckFlag(Flag.InterruptRequest) ? "I" : "-")}{(CheckFlag(Flag.Zero) ? "Z" : "-")}{(CheckFlag(Flag.Carry) ? "C" : "-")}\t";
        output += $"Instruction: {DisAsm6502.Disassemble(new[] { Bus.GetData(ProgramCounter), Bus.GetData((ushort)(ProgramCounter + 1)), Bus.GetData((ushort)(ProgramCounter + 2)) }, 0)}";
        return output;
    }

    public void Reset()
    {
        ProgramCounter = (ushort)((Bus.GetData(0xFFFD) << 8) + Bus.GetData(0xFFFC));
        StackPointer = 0xFF;
        A = 0x00;
        X = 0x00;
        Y = 0x00;
        StatusRegister = 0x20;

        Cycles = 0;
    }

    private bool CheckFlag(byte flag) => (StatusRegister & flag) >> (int)Math.Log(flag, 2) == 1;

    private void SetFlag(byte flag, bool state)
    {
        if (state)
            StatusRegister |= flag;
        else
            StatusRegister &= (byte)~flag;
    }

    private void PushToStack(byte value)
    {
        Bus.SetData(value, (ushort)(0x0100 + StackPointer));
        StackPointer--;
    }

    private byte PullFromStack()
    {
        StackPointer++;
        return Bus.GetData((ushort)(0x0100 + StackPointer));
    }

    private void HandleInterruptRequest()
    {
        PushToStack(BitConverter.GetBytes(ProgramCounter)[1]);
        PushToStack(BitConverter.GetBytes(ProgramCounter)[0]);
        PushToStack(StatusRegister);
        SetFlag(Flag.InterruptRequest, true);
        InterruptRequest = false;
        ProgramCounter = (ushort)((Bus.GetData(0xFFFF) << 8) + Bus.GetData(0xFFFE));
        Cycles = 8;
    }

    private void HandleNonMaskableInterrupt()
    {
        PushToStack(BitConverter.GetBytes(ProgramCounter)[1]);
        PushToStack(BitConverter.GetBytes(ProgramCounter)[0]);
        PushToStack(StatusRegister);
        SetFlag(Flag.InterruptRequest, true);
        NonMaskableInterrupt = false;
        ProgramCounter = (ushort)((Bus.GetData(0xFFFB) << 8) + Bus.GetData(0xFFFA));
        Cycles = 8;
    }

    private ushort GetRelativeAddress() => (ushort)(ProgramCounter + (sbyte)Bus.GetData((ushort)(ProgramCounter + 1)) + 2);

    private ushort GetAbsoluteAddress() => (ushort)((Bus.GetData((ushort)(ProgramCounter + 2)) << 8) + Bus.GetData((ushort)(ProgramCounter + 1)));

    private ushort GetAbsoluteXAddress(out bool wrap)
    {
        wrap = (ushort)(Bus.GetData((ushort)(ProgramCounter + 1)) + X) > 0xFF;
        return (ushort)((Bus.GetData((ushort)(ProgramCounter + 2)) << 8) + Bus.GetData((ushort)(ProgramCounter + 1)) + X);
    }

    private ushort GetAbsoluteYAddress(out bool wrap)
    {
        wrap = (ushort)(Bus.GetData((ushort)(ProgramCounter + 1)) + Y) > 0xFF;
        return (ushort)((Bus.GetData((ushort)(ProgramCounter + 2)) << 8) + Bus.GetData((ushort)(ProgramCounter + 1)) + Y);
    }

    private byte GetZeroPageAddress() => Bus.GetData((ushort)(ProgramCounter + 1));

    private byte GetZeroPageXAddress() => (byte)(Bus.GetData((ushort)(ProgramCounter + 1)) + X);

    private byte GetZeroPageYAddress() => (byte)(Bus.GetData((ushort)(ProgramCounter + 1)) + Y);

    private ushort GetIndexedXAddress()
    {
        var index = (byte)(Bus.GetData((ushort)(ProgramCounter + 1)) + X);
        return (ushort)((Bus.GetData((byte)(index + 1)) << 8) + Bus.GetData(index));
    }

    private ushort GetIndexedYAddress(out bool wrap)
    {
        ushort index = Bus.GetData((ushort)(ProgramCounter + 1));
        wrap = index + Y > 0xFF;
        return (ushort)((ushort)((Bus.GetData((byte)(index + 1)) << 8) + Bus.GetData(index)) + Y);
    }

    private void AddWithCarry(byte value)
    {
        var carryIn = CheckFlag(Flag.Carry) ? 1 : 0;
    
        if (!CheckFlag(Flag.Decimal))
        {
            // Binär-Modus
            var sum = A + value + carryIn;
        
            SetFlag(Flag.Carry, sum > 255);
            SetFlag(Flag.Overflow, ((A ^ sum) & (value ^ sum) & 0x80) != 0);
            SetFlag(Flag.Negative, (sum & 0x80) != 0);
            SetFlag(Flag.Zero, (sum & 0xFF) == 0);
        
            A = (byte)sum;
        }
        else
        {
            // Dezimal-Modus (BCD)
            var binarySum = A + value + carryIn;
        
            // Z basiert auf der BINÄREN Summe
            SetFlag(Flag.Zero, (binarySum & 0xFF) == 0);
        
            // BCD-Addition: Nibble für Nibble
            var al = (A & 0x0F) + (value & 0x0F) + carryIn;
            if (al > 0x09)
                al += 0x06;
        
            var ah = (A >> 4) + (value >> 4) + (al > 0x0F ? 1 : 0);
        
            // N, V basieren auf dem Ergebnis VOR der High-Nibble-Korrektur
            var intermediateResult = ((ah & 0x0F) << 4) | (al & 0x0F);
            SetFlag(Flag.Negative, (intermediateResult & 0x80) != 0);
            SetFlag(Flag.Overflow, ((A ^ intermediateResult) & (value ^ intermediateResult) & 0x80) != 0);
        
            if (ah > 0x09)
                ah += 0x06;
        
            SetFlag(Flag.Carry, ah > 0x0F);
        
            A = (byte)(((ah & 0x0F) << 4) | (al & 0x0F));
        }
    }

    private void And(byte value)
    {
        A = (byte)(A & value);
        SetFlag(Flag.Zero, A == 0);
        SetFlag(Flag.Negative, Util.CheckBit(A, 7));
    }

    private byte ArithmeticShiftLeft(byte value)
    {
        SetFlag(Flag.Carry, Util.CheckBit(value, 7));
        value <<= 1;
        SetFlag(Flag.Negative, Util.CheckBit(value, 7));
        SetFlag(Flag.Zero, value == 0);
        return value;
    }

    private void BitTest(byte value)
    {
        var temp = (byte)(A & value);
        SetFlag(Flag.Zero, temp == 0);
        SetFlag(Flag.Overflow, Util.CheckBit(value, 6));
        SetFlag(Flag.Negative, Util.CheckBit(value, 7));
    }

    private void Compare(byte value)
    {
        if (A < value)
        {
            SetFlag(Flag.Negative, Util.CheckBit((byte)(A - value), 7));
            SetFlag(Flag.Zero, false);
            SetFlag(Flag.Carry, false);
        }
        else if (A == value)
        {
            SetFlag(Flag.Negative, false);
            SetFlag(Flag.Zero, true);
            SetFlag(Flag.Carry, true);
        }
        else
        {
            SetFlag(Flag.Negative, Util.CheckBit((byte)(A - value), 7));
            SetFlag(Flag.Zero, false);
            SetFlag(Flag.Carry, true);
        }
    }

    private void CompareX(byte value)
    {
        if (X < value)
        {
            SetFlag(Flag.Negative, Util.CheckBit((byte)(X - value), 7));
            SetFlag(Flag.Zero, false);
            SetFlag(Flag.Carry, false);
        }
        else if (X == value)
        {
            SetFlag(Flag.Negative, false);
            SetFlag(Flag.Zero, true);
            SetFlag(Flag.Carry, true);
        }
        else
        {
            SetFlag(Flag.Negative, Util.CheckBit((byte)(X - value), 7));
            SetFlag(Flag.Zero, false);
            SetFlag(Flag.Carry, true);
        }
    }

    private void CompareY(byte value)
    {
        if (Y < value)
        {
            SetFlag(Flag.Negative, Util.CheckBit((byte)(Y - value), 7));
            SetFlag(Flag.Zero, false);
            SetFlag(Flag.Carry, false);
        }
        else if (Y == value)
        {
            SetFlag(Flag.Negative, false);
            SetFlag(Flag.Zero, true);
            SetFlag(Flag.Carry, true);
        }
        else
        {
            SetFlag(Flag.Negative, Util.CheckBit((byte)(Y - value), 7));
            SetFlag(Flag.Zero, false);
            SetFlag(Flag.Carry, true);
        }
    }

    private byte Decrement(byte value)
    {
        value--;
        SetFlag(Flag.Negative, Util.CheckBit(value, 7));
        SetFlag(Flag.Zero, value == 0);
        return value;
    }

    private void ExclusiveOr(byte value)
    {
        A ^= value;
        SetFlag(Flag.Negative, Util.CheckBit(A, 7));
        SetFlag(Flag.Zero, A == 0);
    }

    private byte Increment(byte value)
    {
        value++;
        SetFlag(Flag.Negative, Util.CheckBit(value, 7));
        SetFlag(Flag.Zero, value == 0);
        return value;
    }

    private void Jump(ushort address) => ProgramCounter = address;

    private byte LogicalShiftRight(byte value)
    {
        SetFlag(Flag.Carry, Util.CheckBit(value, 0));
        value >>= 1;
        SetFlag(Flag.Zero, value == 0);
        SetFlag(Flag.Negative, false);
        return value;
    }

    private void OrA(byte value)
    {
        A |= value;
        SetFlag(Flag.Negative, Util.CheckBit(A, 7));
        SetFlag(Flag.Zero, A == 0);
    }

    private byte RotateLeft(byte value)
    {
        var carrytemp = CheckFlag(Flag.Carry);
        SetFlag(Flag.Carry, Util.CheckBit(value, 7));
        value <<= 1;
        if (carrytemp)
            value++;
        SetFlag(Flag.Negative, Util.CheckBit(value, 7));
        SetFlag(Flag.Zero, value == 0);
        return value;
    }

    private byte RotateRight(byte value)
    {
        var carrytemp = CheckFlag(Flag.Carry);
        SetFlag(Flag.Carry, Util.CheckBit(value, 0));
        value >>= 1;
        if (carrytemp)
            value += 1 << 7;
        SetFlag(Flag.Negative, Util.CheckBit(value, 7));
        SetFlag(Flag.Zero, value == 0);
        return value;
    }

    private void SubtractWithBorrow(byte value)
    {
        if (!CheckFlag(Flag.Decimal))
        {
            // Binär-Modus: SBC = ADC mit invertiertem Wert
            AddWithCarry((byte)~value);
        }
        else
        {
            // Dezimal-Modus (BCD)
            var carryIn = CheckFlag(Flag.Carry) ? 0 : 1; // Borrow
            var binaryDiff = A - value - carryIn;
        
            // Z basiert auf der BINÄREN Differenz
            SetFlag(Flag.Zero, (binaryDiff & 0xFF) == 0);
        
            // BCD-Subtraktion: Nibble für Nibble
            var al = (A & 0x0F) - (value & 0x0F) - carryIn;
            if (al < 0)
                al -= 0x06;
        
            var ah = (A >> 4) - (value >> 4) - (al < 0 ? 1 : 0);
        
            // N, V basieren auf dem Ergebnis VOR der High-Nibble-Korrektur
            var intermediateResult = (((ah & 0x0F) << 4) | (al & 0x0F)) & 0xFF;
            SetFlag(Flag.Negative, (intermediateResult & 0x80) != 0);
            SetFlag(Flag.Overflow, ((A ^ value) & (A ^ intermediateResult) & 0x80) != 0);
        
            if (ah < 0)
                ah -= 0x06;
        
            SetFlag(Flag.Carry, ah >= 0); // Carry gesetzt wenn kein Borrow
        
            A = (byte)(((ah & 0x0F) << 4) | (al & 0x0F));
        }
    }

    public void Exec()
    {
        TotalCycles++;
        if (Cycles == 0 && NonMaskableInterrupt)
            HandleNonMaskableInterrupt();
        if (Cycles == 0 && !CheckFlag(Flag.InterruptRequest) && InterruptRequest)
            HandleInterruptRequest();
        if (Cycles != 0)
        {
            Cycles--;
            return;
        }

        var instruction = Bus.GetData(ProgramCounter);
        bool wrap;

        switch (instruction)
        {
            case 0x00: //BRK
            {
                Cycles = 7;
                ProgramCounter += 2;
                PushToStack(BitConverter.GetBytes(ProgramCounter)[1]);
                PushToStack(BitConverter.GetBytes(ProgramCounter)[0]);
                SetFlag(Flag.Break, true);
                PushToStack(StatusRegister);
                SetFlag(Flag.Break, false);
                SetFlag(Flag.InterruptRequest, true);
                ProgramCounter = (ushort)((Bus.GetData(0xFFFF) << 8) + Bus.GetData(0xFFFE));
                break;
            }
            case 0x01: //ORA   (indirect, X)
            {
                Cycles = 6;
                var address = GetIndexedXAddress();
                OrA(Bus.GetData(address));
                ProgramCounter += 2;
                break;
            }
            case 0x05: //ORA   (zeropage)
            {
                Cycles = 3;
                var address = GetZeroPageAddress();
                OrA(Bus.GetData(address));
                ProgramCounter += 2;
                break;
            }
            case 0x06: //ASL   (zeropage)
                Cycles = 5;
                Bus.SetData(ArithmeticShiftLeft(Bus.GetData(GetZeroPageAddress())), GetZeroPageAddress());
                ProgramCounter += 2;
                break;
            case 0x08: //PHP
            {
                Cycles = 3;
                var oldBreak = (StatusRegister & Flag.Break) == Flag.Break;
                var oldUnused = (StatusRegister & 0x20) == 0x20;
                SetFlag(Flag.Break, true);
                SetFlag(0x20, true);
                PushToStack(StatusRegister);
                SetFlag(Flag.Break, oldBreak);
                SetFlag(0x20, oldUnused);
                ProgramCounter++;
                break;
            }
            case 0x09: //ORA   (immediate)
                Cycles = 2;
                OrA(Bus.GetData((ushort)(ProgramCounter + 1)));
                ProgramCounter += 2;
                break;
            case 0x0A: //ASL   (accumulator)
                Cycles = 2;
                A = ArithmeticShiftLeft(A);
                ProgramCounter++;
                break;
            case 0x0D: //ORA   (absolute)
                Cycles = 4;
                OrA(Bus.GetData(GetAbsoluteAddress()));
                ProgramCounter += 3;
                break;
            case 0x0E: //ASL   (absolute)
                Cycles = 6;
                Bus.SetData(ArithmeticShiftLeft(Bus.GetData(GetAbsoluteAddress())), GetAbsoluteAddress());
                ProgramCounter += 3;
                break;
            case 0x10: //BPL   (relative)
                if (CheckFlag(Flag.Negative))
                {
                    Cycles = 2;
                    ProgramCounter += 2;
                }
                else
                {
                    var newAddress = GetRelativeAddress();
                    Cycles = BitConverter.GetBytes(newAddress)[1] != BitConverter.GetBytes(ProgramCounter)[1] ? 4 : 3;
                    ProgramCounter = newAddress;
                }

                break;
            case 0x11: //ORA   (indirect, Y)
                OrA(Bus.GetData(GetIndexedYAddress(out wrap)));
                Cycles = wrap ? 6 : 5;
                ProgramCounter += 2;
                break;
            case 0x15: //ORA   (zeropage, X)
                OrA(Bus.GetData(GetZeroPageXAddress()));
                Cycles = 4;
                ProgramCounter += 2;
                break;
            case 0x16: //ASL   (zeropage, X)
                Cycles = 6;
                Bus.SetData(ArithmeticShiftLeft(Bus.GetData(GetZeroPageXAddress())), GetZeroPageXAddress());
                ProgramCounter += 2;
                break;
            case 0x18: //CLC
                Cycles = 2;
                SetFlag(Flag.Carry, false);
                ProgramCounter++;
                break;
            case 0x19: //ORA   (absolute, Y)
                OrA(Bus.GetData(GetAbsoluteYAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0x1D: //ORA   (absolute, X)
                OrA(Bus.GetData(GetAbsoluteXAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0x1E: //ASL   (absolute, X)
                Cycles = 7;
                Bus.SetData(ArithmeticShiftLeft(Bus.GetData(GetAbsoluteXAddress(out wrap))), GetAbsoluteXAddress(out wrap));
                ProgramCounter += 3;
                break;
            case 0x20: //JSR   (absolute)
                Cycles = 6;
                byte low = Bus.GetData((ushort)(ProgramCounter + 1));
                PushToStack(BitConverter.GetBytes(ProgramCounter + 2)[1]);
                byte high = Bus.GetData((ushort)(ProgramCounter + 2));
                PushToStack(BitConverter.GetBytes(ProgramCounter + 2)[0]);
                ProgramCounter = (ushort)((high << 8) + low);
                break;
            case 0x21: //AND   (indirect, X)
                Cycles = 6;
                And(Bus.GetData(GetIndexedXAddress()));
                ProgramCounter += 2;
                break;
            case 0x24: //BIT   (zeropage)
                Cycles = 3;
                BitTest(Bus.GetData(GetZeroPageAddress()));
                ProgramCounter += 2;
                break;
            case 0x25: //AND   (zeropage)
                Cycles = 3;
                And(Bus.GetData(GetZeroPageAddress()));
                ProgramCounter += 2;
                break;
            case 0x26: //ROL   (zeropage)
            {
                Cycles = 5;
                var address = GetZeroPageAddress();
                Bus.SetData(RotateLeft(Bus.GetData(address)), address);
                ProgramCounter += 2;
                break;
            }
            case 0x28: //PLP
            {
                var oldBreak = CheckFlag(Flag.Break);
                var oldUnused = CheckFlag(0x20);
                StatusRegister = PullFromStack();
                SetFlag(Flag.Break, oldBreak);
                SetFlag(0x20, oldUnused);
                ProgramCounter++;
                break;
            }
            case 0x29: //AND   (immediate)
                Cycles = 2;
                And(Bus.GetData((ushort)(ProgramCounter + 1)));
                ProgramCounter += 2;
                break;
            case 0x2A: //ROL   (accumulator)
                Cycles = 2;
                A = RotateLeft(A);
                ProgramCounter++;
                break;
            case 0x2C: //BIT   (absolute)
                Cycles = 4;
                BitTest(Bus.GetData(GetAbsoluteAddress()));
                ProgramCounter += 3;
                break;
            case 0x2D: //AND   (absolute)
                Cycles = 4;
                And(Bus.GetData(GetAbsoluteAddress()));
                ProgramCounter += 3;
                break;
            case 0x2E: //ROL   (absolute)
                Cycles = 6;
                Bus.SetData(RotateLeft(Bus.GetData(GetAbsoluteAddress())), GetAbsoluteAddress());
                ProgramCounter += 3;
                break;
            case 0x30: //BMI   (relative)
                if (CheckFlag(Flag.Negative))
                {
                    var newAddress = GetRelativeAddress();
                    Cycles = BitConverter.GetBytes(newAddress)[1] != BitConverter.GetBytes(ProgramCounter)[1] ? 4 : 3;
                    ProgramCounter = newAddress;
                }
                else
                {
                    Cycles = 2;
                    ProgramCounter += 2;
                }

                break;
            case 0x31: //AND   (indirect, Y)
                And(Bus.GetData(GetIndexedYAddress(out wrap)));
                Cycles = wrap ? 6 : 5;
                ProgramCounter += 2;
                break;
            case 0x35: //AND   (zeropage, X)
                Cycles = 4;
                And(Bus.GetData(GetZeroPageXAddress()));
                ProgramCounter += 2;
                break;
            case 0x36: //ROL   (zeropage, x)
                Bus.SetData(RotateLeft(Bus.GetData(GetZeroPageXAddress())), GetZeroPageXAddress());
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0x38: //SEC
                Cycles = 2;
                SetFlag(Flag.Carry, true);
                ProgramCounter++;
                break;
            case 0x39: //AND   (absolute, Y)
                And(Bus.GetData(GetAbsoluteYAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0x3D: //AND   (absolute, X)
                And(Bus.GetData(GetAbsoluteXAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0x3E: //ROL   (absolute, X)
                Bus.SetData(RotateLeft(Bus.GetData(GetAbsoluteXAddress(out wrap))), GetAbsoluteXAddress(out wrap));
                Cycles = 7;
                ProgramCounter += 3;
                break;
            case 0x40: //RTI
            {
                Cycles = 6;
                var oldBreak = CheckFlag(Flag.Break);
                var oldUnused = CheckFlag(0x20);
                StatusRegister = PullFromStack();
                SetFlag(Flag.Break, oldBreak);
                SetFlag(0x20, oldUnused);
                ProgramCounter = (ushort)(PullFromStack() + (PullFromStack() << 8));
                break;
            }
            case 0x41: //EOR   (indirect, X)
                ExclusiveOr(Bus.GetData(GetIndexedXAddress()));
                Cycles = 5;
                ProgramCounter += 2;
                break;
            case 0x45: //EOR   (zeropage)
                ExclusiveOr(Bus.GetData(GetZeroPageAddress()));
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0x46: //LSR   (zeropage)
            {
                var address = GetZeroPageAddress();
                Bus.SetData(LogicalShiftRight(Bus.GetData(address)), address);
                Cycles = 5;
                ProgramCounter += 2;
                break;
            }
            case 0x48: //PHA
                Cycles = 3;
                PushToStack(A);
                ProgramCounter++;
                break;
            case 0x49: //EOR   (immediate)
                ExclusiveOr(Bus.GetData((ushort)(ProgramCounter + 1)));
                Cycles = 2;
                ProgramCounter += 2;
                break;
            case 0x4A: //LSR   (accumulator)
                A = LogicalShiftRight(A);
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0x4C: //JMP   (absolute)
                Cycles = 3;
                Jump(GetAbsoluteAddress());
                break;
            case 0x4D: //EOR   (absolute)
                Cycles = 4;
                ExclusiveOr(Bus.GetData(GetAbsoluteAddress()));
                ProgramCounter += 3;
                break;
            case 0x4E: //LSR   (absolute)
            {
                var address = GetAbsoluteAddress();
                Bus.SetData(LogicalShiftRight(Bus.GetData(address)), address);
                Cycles = 6;
                ProgramCounter += 3;
                break;
            }
            case 0x50: //BVC   (relative)
                if (CheckFlag(Flag.Overflow))
                {
                    Cycles = 2;
                    ProgramCounter += 2;
                }
                else
                {
                    var newaddr = GetRelativeAddress();
                    Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(ProgramCounter)[1] ? 4 : 3;
                    ProgramCounter = newaddr;
                }

                break;
            case 0x51: //EOR   (indirect, Y)
                ExclusiveOr(Bus.GetData(GetIndexedYAddress(out wrap)));
                Cycles = wrap ? 6 : 5;
                ProgramCounter += 2;
                break;
            case 0x55: //EOR   (zeropage, X)
                ExclusiveOr(Bus.GetData(GetZeroPageXAddress()));
                Cycles = 4;
                ProgramCounter += 2;
                break;
            case 0x56: //LSR   (zeropage, X)
                Bus.SetData(LogicalShiftRight(Bus.GetData(GetZeroPageXAddress())), GetZeroPageXAddress());
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0x58: //CLI
                SetFlag(Flag.InterruptRequest, false);
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0x59: //EOR   (absolute, Y)
                ExclusiveOr(Bus.GetData(GetAbsoluteYAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0x5D: //EOR   (absolute, X)
                ExclusiveOr(Bus.GetData(GetAbsoluteXAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0x5E: //LSR   (absolute, X)
                Bus.SetData(LogicalShiftRight(Bus.GetData(GetAbsoluteXAddress(out wrap))), GetAbsoluteXAddress(out wrap));
                Cycles = 7;
                ProgramCounter += 3;
                break;
            case 0x60: //RTS
                Cycles = 6;
                ProgramCounter = (ushort)(PullFromStack() + (PullFromStack() << 8));
                ProgramCounter++;
                break;
            case 0x61: //ADC   (indirect, X)
                AddWithCarry(Bus.GetData(GetIndexedXAddress()));
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0x65: //ADC   (zeropage)
                AddWithCarry(Bus.GetData(GetZeroPageAddress()));
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0x66: //ROR   (zeropage)
                Bus.SetData(RotateRight(Bus.GetData(GetZeroPageAddress())), GetZeroPageAddress());
                Cycles = 5;
                ProgramCounter += 2;
                break;
            case 0x68: //PLA
                Cycles = 4;
                A = PullFromStack();
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter++;
                break;
            case 0x69: //ADC   (immediate)
                AddWithCarry(Bus.GetData((ushort)(ProgramCounter + 1)));
                Cycles = 2;
                ProgramCounter += 2;
                break;
            case 0x6A: //ROR   (accumulator)
                A = RotateRight(A);
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0x6C: //JMP   (indirect)
            {
                var source = GetAbsoluteAddress();
                var targetHigh = Bus.GetData((ushort)((source & 0xFF) == 0xFF ? source - 0xFF : source + 1)) << 8;
                var targetLow = Bus.GetData(source);
                var target = (ushort)(targetHigh + targetLow);
                ProgramCounter = target;
                Cycles = 5;
                break;
            }
            case 0x6D: //ADC   (absolute)
                AddWithCarry(Bus.GetData(GetAbsoluteAddress()));
                Cycles = 4;
                ProgramCounter += 3;
                break;
            case 0x6E: //ROR   (absolute)
                Bus.SetData(RotateRight(Bus.GetData(GetAbsoluteAddress())), GetAbsoluteAddress());
                Cycles = 6;
                ProgramCounter += 3;
                break;
            case 0x70: //BVS   (relative)
                if (CheckFlag(Flag.Overflow))
                {
                    var newAddress = GetRelativeAddress();
                    Cycles = BitConverter.GetBytes(newAddress)[1] != BitConverter.GetBytes(ProgramCounter)[1] ? 4 : 3;
                    ProgramCounter = newAddress;
                }
                else
                {
                    Cycles = 2;
                    ProgramCounter += 2;
                }

                break;
            case 0x71: //ADC   (indirect, Y)
                AddWithCarry(Bus.GetData(GetIndexedYAddress(out wrap)));
                Cycles = wrap ? 6 : 5;
                ProgramCounter += 2;
                break;
            case 0x75: //ADC   (zeropage, X)
                AddWithCarry(Bus.GetData(GetZeroPageXAddress()));
                Cycles = 4;
                ProgramCounter += 2;
                break;
            case 0x76: //ROR   (zeropage, X)
                Bus.SetData(RotateRight(Bus.GetData(GetZeroPageXAddress())), GetZeroPageXAddress());
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0x78: //SEI
                SetFlag(Flag.InterruptRequest, true);
                ProgramCounter++;
                Cycles = 2;
                break;
            case 0x79: //ADC   (absolute, Y)
                AddWithCarry(Bus.GetData(GetAbsoluteYAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0x7D: //ADC   (absolute, X)
                AddWithCarry(Bus.GetData(GetAbsoluteXAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0x7E: //ROR   (absolute, X)
                Bus.SetData(RotateRight(Bus.GetData(GetAbsoluteXAddress(out wrap))), GetAbsoluteXAddress(out wrap));
                Cycles = 7;
                ProgramCounter += 3;
                break;
            case 0x81: //STA   (indirect, X)
                Bus.SetData(A, GetIndexedXAddress());
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0x84: //STY   (zeropage)
                Bus.SetData(Y, GetZeroPageAddress());
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0x85: //STA   (zeropage)
                Bus.SetData(A, GetZeroPageAddress());
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0x86: //STX   (zeropage)
                Bus.SetData(X, GetZeroPageAddress());
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0x88: //DEY
                Y--;
                SetFlag(Flag.Negative, Util.CheckBit(Y, 7));
                SetFlag(Flag.Zero, Y == 0);
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0x8A: //TXA
                A = X;
                SetFlag(Flag.Zero, A == 0);
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0x8C: //STY   (absolute)
                Bus.SetData(Y, GetAbsoluteAddress());
                Cycles = 4;
                ProgramCounter += 3;
                break;
            case 0x8D: //STA   (absolute)
                Bus.SetData(A, GetAbsoluteAddress());
                Cycles = 4;
                ProgramCounter += 3;
                break;
            case 0x8E: //STX   (absolute)
                Bus.SetData(X, GetAbsoluteAddress());
                Cycles = 4;
                ProgramCounter += 3;
                break;
            case 0x90: //BCC   (relative)
                if (CheckFlag(Flag.Carry))
                {
                    Cycles = 2;
                    ProgramCounter += 2;
                }
                else
                {
                    var newAddress = GetRelativeAddress();
                    Cycles = BitConverter.GetBytes(newAddress)[1] != BitConverter.GetBytes(ProgramCounter)[1] ? 4 : 3;
                    ProgramCounter = newAddress;
                }

                break;
            case 0x91: //STA   (indirect, Y)
                Bus.SetData(A, GetIndexedYAddress(out _));
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0x94: //STY   (zeropage, X)
                Bus.SetData(Y, GetZeroPageXAddress());
                Cycles = 4;
                ProgramCounter += 2;
                break;
            case 0x95: //STA   (zeropage, X)
                Bus.SetData(A, GetZeroPageXAddress());
                Cycles = 4;
                ProgramCounter += 2;
                break;
            case 0x96: //STX   (zeropage, Y)
                Bus.SetData(X, GetZeroPageYAddress());
                Cycles = 4;
                ProgramCounter += 2;
                break;
            case 0x98: //TYA
                A = Y;
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter++;
                break;
            case 0x99: //STA   (absolute, Y)
                Bus.SetData(A, GetAbsoluteYAddress(out _));
                Cycles = 5;
                ProgramCounter += 3;
                break;
            case 0x9A: //TXS
                StackPointer = X;
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0x9D: //STA   (absolute, X)
                Bus.SetData(A, GetAbsoluteXAddress(out _));
                Cycles = 5;
                ProgramCounter += 3;
                break;
            case 0xA0: //LDY   (immediate)
                Y = Bus.GetData((ushort)(ProgramCounter + 1));
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(Y, 7));
                SetFlag(Flag.Zero, Y == 0);
                ProgramCounter += 2;
                break;
            case 0xA1: //LDA   (indirect, X)
                A = Bus.GetData(GetIndexedXAddress());
                Cycles = 6;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter += 2;
                break;
            case 0xA2: //LDX   (immediate)
                X = Bus.GetData((ushort)(ProgramCounter + 1));
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter += 2;
                break;
            case 0xA4: //LDY   (zeropage)
                Y = Bus.GetData(GetZeroPageAddress());
                Cycles = 3;
                SetFlag(Flag.Negative, Util.CheckBit(Y, 7));
                SetFlag(Flag.Zero, Y == 0);
                ProgramCounter += 2;
                break;
            case 0xA5: //LDA   (zeropage)
                A = Bus.GetData(GetZeroPageAddress());
                Cycles = 3;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter += 2;
                break;
            case 0xA6: //LDX   (zeropage)
                X = Bus.GetData(GetZeroPageAddress());
                Cycles = 3;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter += 2;
                break;
            case 0xA8: //TAY
                Y = A;
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(Y, 7));
                SetFlag(Flag.Zero, Y == 0);
                ProgramCounter++;
                break;
            case 0xA9: //LDA   (immediate)
                A = Bus.GetData((ushort)(ProgramCounter + 1));
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter += 2;
                break;
            case 0xAA: //TAX
                X = A;
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter++;
                break;
            case 0xAC: //LDY   (absolute)
                Y = Bus.GetData(GetAbsoluteAddress());
                Cycles = 4;
                SetFlag(Flag.Negative, Util.CheckBit(Y, 7));
                SetFlag(Flag.Zero, Y == 0);
                ProgramCounter += 3;
                break;
            case 0xAD: //LDA   (absolute)
                A = Bus.GetData(GetAbsoluteAddress());
                Cycles = 4;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter += 3;
                break;
            case 0xAE: //LDX   (absolute)
                X = Bus.GetData(GetAbsoluteAddress());
                Cycles = 4;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter += 3;
                break;
            case 0xB0: //BCS   (relative)
                if (CheckFlag(Flag.Carry))
                {
                    var newAddress = GetRelativeAddress();
                    Cycles = BitConverter.GetBytes(newAddress)[1] != BitConverter.GetBytes(ProgramCounter)[1] ? 4 : 3;
                    ProgramCounter = newAddress;
                }
                else
                {
                    Cycles = 2;
                    ProgramCounter += 2;
                }

                break;
            case 0xB1: //LDA   (indirect, Y)
                A = Bus.GetData(GetIndexedYAddress(out wrap));
                Cycles = wrap ? 6 : 5;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter += 2;
                break;
            case 0xB4: //LDY   (zeropage, X)
                Y = Bus.GetData(GetZeroPageXAddress());
                Cycles = 4;
                SetFlag(Flag.Negative, Util.CheckBit(Y, 7));
                SetFlag(Flag.Zero, Y == 0);
                ProgramCounter += 2;
                break;
            case 0xB5: //LDA   (zeropage, X)
                A = Bus.GetData(GetZeroPageXAddress());
                Cycles = 4;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter += 2;
                break;
            case 0xB6: //LDX   (zeropage, Y)
                X = Bus.GetData(GetZeroPageYAddress());
                Cycles = 4;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter += 2;
                break;
            case 0xB8: //CLV
                SetFlag(Flag.Overflow, false);
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0xB9: //LDA   (absolute, Y)
                A = Bus.GetData(GetAbsoluteYAddress(out wrap));
                Cycles = wrap ? 5 : 4;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter += 3;
                break;
            case 0xBA: //TSX
                X = StackPointer;
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter++;
                break;
            case 0xBC: //LDY   (absolute, X)
                Y = Bus.GetData(GetAbsoluteXAddress(out wrap));
                Cycles = wrap ? 5 : 4;
                SetFlag(Flag.Negative, Util.CheckBit(Y, 7));
                SetFlag(Flag.Zero, Y == 0);
                ProgramCounter += 3;
                break;
            case 0xBD: //LDA   (absolute, X)
                A = Bus.GetData(GetAbsoluteXAddress(out wrap));
                Cycles = wrap ? 5 : 4;
                SetFlag(Flag.Negative, Util.CheckBit(A, 7));
                SetFlag(Flag.Zero, A == 0);
                ProgramCounter += 3;
                break;
            case 0xBE: //LDX   (absolute, Y)
                X = Bus.GetData(GetAbsoluteYAddress(out wrap));
                Cycles = wrap ? 5 : 4;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter += 3;
                break;
            case 0xC0: //CPY   (immediate)
                CompareY(Bus.GetData((ushort)(ProgramCounter + 1)));
                Cycles = 2;
                ProgramCounter += 2;
                break;
            case 0xC1: //CMP   (indirect, X)
                Compare(Bus.GetData(GetIndexedXAddress()));
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0xC4: //CPY   (zeropage)
                CompareY(Bus.GetData(GetZeroPageAddress()));
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0xC5: //CMP   (zeropage)
                Compare(Bus.GetData(GetZeroPageAddress()));
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0xC6: //DEC   (zeropage)
                Bus.SetData(Decrement(Bus.GetData(GetZeroPageAddress())), GetZeroPageAddress());
                Cycles = 5;
                ProgramCounter += 2;
                break;
            case 0xC8: //INY
                Y++;
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(Y, 7));
                SetFlag(Flag.Zero, Y == 0);
                ProgramCounter++;
                break;
            case 0xC9: //CMP   (immediate)
                Compare(Bus.GetData((ushort)(ProgramCounter + 1)));
                Cycles = 2;
                ProgramCounter += 2;
                break;
            case 0xCA: //DEX
                X--;
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter++;
                break;
            case 0xCC: //CPY   (absolute)
                CompareY(Bus.GetData(GetAbsoluteAddress()));
                Cycles = 4;
                ProgramCounter += 3;
                break;
            case 0xCD: //CMP   (absolute)
                Compare(Bus.GetData(GetAbsoluteAddress()));
                Cycles = 4;
                ProgramCounter += 3;
                break;
            case 0xCE: //DEC   (absolute)
                Bus.SetData(Decrement(Bus.GetData(GetAbsoluteAddress())), GetAbsoluteAddress());
                Cycles = 6;
                ProgramCounter += 3;
                break;
            case 0xD0: //BNE   (relative)
            {
                var address = GetRelativeAddress();
                if (CheckFlag(Flag.Zero))
                {
                    Cycles = 2;
                    ProgramCounter += 2;
                }
                else
                {
                    Cycles = BitConverter.GetBytes(address)[1] != BitConverter.GetBytes(ProgramCounter)[1] ? 4 : 3;
                    ProgramCounter = address;
                }
            }

                break;
            case 0xD1: //CMP   (indirect, Y)
                Compare(Bus.GetData(GetIndexedYAddress(out wrap)));
                Cycles = wrap ? 6 : 5;
                ProgramCounter += 2;
                break;
            case 0xD5: //CMP   (zeropage, X)
                Compare(Bus.GetData(GetZeroPageXAddress()));
                Cycles = 4;
                ProgramCounter += 2;
                break;
            case 0xD6: //DEC   (zeropage, X)
                Bus.SetData(Decrement(Bus.GetData(GetZeroPageXAddress())), GetZeroPageXAddress());
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0xD8: //CLD
                SetFlag(Flag.Decimal, false);
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0xD9: //CMP   (absolute, Y)
                Compare(Bus.GetData(GetAbsoluteYAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0xDD: //CMP   (absolute, X)
                Compare(Bus.GetData(GetAbsoluteXAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0xDE: //DEC   (absolute, X)
                Bus.SetData(Decrement(Bus.GetData(GetAbsoluteXAddress(out _))), GetAbsoluteXAddress(out _));
                Cycles = 7;
                ProgramCounter += 3;
                break;
            case 0xE0: //CPX   (immediate)
                CompareX(Bus.GetData((ushort)(ProgramCounter + 1)));
                Cycles = 2;
                ProgramCounter += 2;
                break;
            case 0xE1: //SBC   (indirect, X)
                SubtractWithBorrow(Bus.GetData(GetIndexedXAddress()));
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0xE4: //CPX   (zeropage)
                CompareX(Bus.GetData(GetZeroPageAddress()));
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0xE5: //SBC   (zeropage)
                SubtractWithBorrow(Bus.GetData(GetZeroPageAddress()));
                Cycles = 3;
                ProgramCounter += 2;
                break;
            case 0xE6: //INC   (zeropage)
                Bus.SetData(Increment(Bus.GetData(GetZeroPageAddress())), GetZeroPageAddress());
                Cycles = 5;
                ProgramCounter += 2;
                break;
            case 0xE8: //INX
                X++;
                Cycles = 2;
                SetFlag(Flag.Negative, Util.CheckBit(X, 7));
                SetFlag(Flag.Zero, X == 0);
                ProgramCounter++;
                break;
            case 0xE9: //SBC   (immediate)
                SubtractWithBorrow(Bus.GetData((ushort)(ProgramCounter + 1)));
                Cycles = 2;
                ProgramCounter += 2;
                break;
            case 0xEA: //NOP
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0xEC: //CPX   (absolute)
                CompareX(Bus.GetData(GetAbsoluteAddress()));
                Cycles = 4;
                ProgramCounter += 3;
                break;
            case 0xED: //SBC   (absolute)
                SubtractWithBorrow(Bus.GetData(GetAbsoluteAddress()));
                Cycles = 4;
                ProgramCounter += 3;
                break;
            case 0xEE: //INC   (absolute)
                Bus.SetData(Increment(Bus.GetData(GetAbsoluteAddress())), GetAbsoluteAddress());
                Cycles = 6;
                ProgramCounter += 3;
                break;
            case 0xF0: //BEQ   (relative)
                if (CheckFlag(Flag.Zero))
                {
                    var newaddr = GetRelativeAddress();
                    Cycles = BitConverter.GetBytes(newaddr)[1] != BitConverter.GetBytes(ProgramCounter)[1] ? 4 : 3;
                    ProgramCounter = newaddr;
                }
                else
                {
                    Cycles = 2;
                    ProgramCounter += 2;
                }

                break;
            case 0xF1: //SBC   (indirect, Y)
                SubtractWithBorrow(Bus.GetData(GetIndexedYAddress(out wrap)));
                Cycles = wrap ? 6 : 5;
                ProgramCounter += 2;
                break;
            case 0xF5: //SBC   (zeropage, X)
                SubtractWithBorrow(Bus.GetData(GetZeroPageXAddress()));
                Cycles = 4;
                ProgramCounter += 2;
                break;
            case 0xF6: //INC   (zeropage, X)
                Bus.SetData(Increment(Bus.GetData(GetZeroPageXAddress())), GetZeroPageXAddress());
                Cycles = 6;
                ProgramCounter += 2;
                break;
            case 0xF8: //SED
                SetFlag(Flag.Decimal, true);
                Cycles = 2;
                ProgramCounter++;
                break;
            case 0xF9: //SBC   (absolute, Y)
                SubtractWithBorrow(Bus.GetData(GetAbsoluteYAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0xFD: //SBC   (absolute, X)
                SubtractWithBorrow(Bus.GetData(GetAbsoluteXAddress(out wrap)));
                Cycles = wrap ? 5 : 4;
                ProgramCounter += 3;
                break;
            case 0xFE: //INC   (absolute, X)
                Bus.SetData(Increment(Bus.GetData(GetAbsoluteXAddress(out _))), GetAbsoluteXAddress(out _));
                Cycles = 7;
                ProgramCounter += 3;
                break;
            default: //
                throw new ArgumentException("Ungültiger Opcode");
        }
        
        Bus.PerformClockActions();
    }

    public ulong TotalCycles { get; set; }

    public void Step()
    {
        Exec();
        while (Cycles > 0)
            Exec();
    }
}