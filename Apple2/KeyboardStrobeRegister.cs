using lib6502;

namespace Apple2;

internal class KeyboardStrobeRegister() : Device(0xC000, 0xC000)
{
    private byte content = 0;
    
    public override void SetData(byte data, ushort address)
    {
        if (Request(address))
            content = data;
    }

    public override byte GetData(ushort address)
    {
        if (Request(address))
            return content;
        return 0;
    }

    public override void PerformClockAction(ushort lastReadAddress)
    {
        if (lastReadAddress == 0xC010)
            content = (byte)(content & 0x7F);
    }
}