using System;
using lib6502;

namespace Apple2;

internal class LowResGraphicsRam(PixelDisplay Display) : Device(0x0400, 0x0BFF)
{
    private byte[] graphicsMemory = new byte[0x0800];
    
    public override void SetData(byte data, ushort address)
    {
        if (Request(address))
            graphicsMemory[address - 0x0400] = data;
    }

    public override byte GetData(ushort address)
    {
        if (Request(address))
            return graphicsMemory[address - 0x0400];
        return 0;
    }

    public override void PerformClockAction(ushort lastReadAddress)
    {
        var i = 0;
        var lineStartAddress = 0x0000;
        while (i < 8)
        {
            Array.Copy(graphicsMemory, lineStartAddress + i * 0x80, Display.TextBuffer, i * 40, 40);
            i++;
        }

        lineStartAddress = 0x0028;
        while (i < 16)
        {
            Array.Copy(graphicsMemory, lineStartAddress + i % 8 * 0x80, Display.TextBuffer, i * 40, 40);
            i++;
        }
        
        lineStartAddress = 0x0050;
        while (i < 24)
        {
            Array.Copy(graphicsMemory, lineStartAddress + i % 8 * 0x80, Display.TextBuffer, i * 40, 40);
            i++;
        }
    }
}