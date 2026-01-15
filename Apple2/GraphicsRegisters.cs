using System;
using lib6502;

namespace Apple2;

internal class GraphicsRegisters(PixelDisplay Display) : Device(0xC050, 0xC057)
{
    private byte[] oldState = new byte[8];
    private byte[] registers = new byte[8];
    
    public override void SetData(byte data, ushort address)
    {
        if(Request(address))
            registers[address - 0xC050] =  data;
    }

    public override byte GetData(ushort address)
    {
        if(Request(address))
            return oldState[address - 0xC050];
        return 0x00;
    }

    public override void PerformClockAction(ushort lastReadAddress)
    {
        var changedRegisters = new bool[8];
        for (var i = 0; i < 8; i++)
        {
            if (registers[i] != oldState[i])
            {
                changedRegisters[i] = true;
                oldState[i] = registers[i];
            }
        }

        if (changedRegisters[0] || lastReadAddress == 0xC050) // switch to graphics mode
        {
            Display.SwitchToGraphicsMode();
            Console.WriteLine("Switched to graphics mode");
        }
        else if (changedRegisters[1] || lastReadAddress == 0xC051) // switch to text mode
        {
            Display.SwitchToTextMode();
            Console.WriteLine("Switched to text mode");
        }
        else if (changedRegisters[2] || lastReadAddress == 0xC052) // fullscreen graphics
        {
            Display.IsMixedScreen = false;
        }
        else if (changedRegisters[3] || lastReadAddress == 0xC053) // mixed screen
        {
            Display.IsMixedScreen = true;
        }
        else if (changedRegisters[4] || lastReadAddress == 0xC054) // switch to page 1
        {
            // TODO
        }
        else if (changedRegisters[5] || lastReadAddress == 0xC055) // switch to page 2
        {
            // TODO
        }
        else if (changedRegisters[6] || lastReadAddress == 0xC056) // switch to low res graphics
        {
            // TODO
        }
        else if (changedRegisters[7] || lastReadAddress == 0xC057) // switch to high res graphics
        {
            // TODO
        }
    }
}