using System;

namespace lib6502;

public class ReadOnlyMemory : Device
{
    public ReadOnlyMemory(ushort size, ushort start) : base(start, (ushort)(start + size - 1))
    {
    }

    public void SetMemory(byte[] mem)
    {
        if (mem.Length != Memory.Length)
            throw new ArgumentException("Wrong size", nameof(mem));
        for (var i = 0; i < mem.Length; i++)
            Memory[i] = mem[i];
    }

    public override byte GetData(ushort address) => Request(address) ? Memory[address - Start] : (byte)0x00;

    public override void PerformClockAction()
    {
    }

    public override void SetData(byte data, ushort address)
    {
    }
}