using lib6502;
using SDL2;

namespace Apple2;

internal class AppleKeysRegister(bool[] keyboardState) : Device(0xC061, 0xC062)
{
    private byte leftKey;
    private byte rightKey;

    public override void SetData(byte data, ushort address)
    {
    }

    public override byte GetData(ushort address)
    {
        if(Request(address))
            return address == 0xC061 ? leftKey : rightKey;
        return 0;
    }

    public override void PerformClockAction(ushort lastReadAddress)
    {
        leftKey = (byte)(keyboardState[(int)SDL.SDL_Scancode.SDL_SCANCODE_LALT] ? 0xFF : 0);
        rightKey = (byte)(keyboardState[(int)SDL.SDL_Scancode.SDL_SCANCODE_RALT] ? 0xFF : 0);
    }
}