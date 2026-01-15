namespace lib6502;

public abstract class Device
{
    protected readonly ushort End;

    protected readonly byte[] Memory;
    protected readonly ushort Start;

    protected Device(ushort rangeStart, ushort rangeEnd)
    {
        Start = rangeStart;
        End = rangeEnd;
        Memory = new byte[End - Start + 1];
    }

    public bool Request(ushort address) => address >= Start && address <= End;

    public abstract void SetData(byte data, ushort address);

    public abstract byte GetData(ushort address);

    public abstract void PerformClockAction(ushort lastReadAddress);
}