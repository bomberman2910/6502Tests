namespace lib6502;

public class RandomAccessMemory : Device
{
    public RandomAccessMemory(ushort size, ushort start) : base(start, (ushort)(start + size - 1))
    {
    }

    public override byte GetData(ushort address) => Request(address) ? Memory[address - Start] : (byte)0x00;

    public override void PerformClockAction()
    {
    }

    public override void SetData(byte data, ushort address)
    {
        if (Request(address))
            Memory[address - Start] = data;
    }
}