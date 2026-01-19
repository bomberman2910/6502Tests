using System;
using System.Linq;

namespace Apple2;

class PixelDisplay
{
    private const int Width = 640;
    private const int Height = 480;

    public uint[] FrameBuffer { get; } = new uint[Width * Height];
    public bool IsInTextMode { get; private set; } = false;
    public byte[] TextBuffer { get; private set; } = new byte[40 * 24];
    public bool IsGraphicsHighResolution { get; set; } = false;
    public bool IsPage2Active { get; set; } = false;
    public bool IsMixedScreen { get; set; } = false;
    public CharacterGenerator CharacterGenerator { get; } = new();

    public void SwitchToTextMode()
    {
        IsInTextMode = true;
        TextBuffer = Enumerable.Repeat<byte>(0x20, 40 * 24).ToArray();
        RenderTextToFrameBuffer();
    }

    public void SwitchToGraphicsMode()
    {
        IsInTextMode = false;
        if (!IsGraphicsHighResolution)
        {
            TextBuffer = Enumerable.Repeat<byte>(0x20, 40 * 24).ToArray();
        }
    }

    public void SetPixel(uint x, uint y, byte color)
    {
        if (IsInTextMode)
            return;
        if (IsGraphicsHighResolution)
            return;
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual<uint>(x, 40);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual<uint>(y, 48);
        var cellContent = TextBuffer[y / 2 * 40 + x];
        var isPixelInUpperHalf = y % 2 == 0;
        if (isPixelInUpperHalf)
        {
            var bottomHalf = cellContent & 0x0F;
            TextBuffer[y / 2 * 40 + x] = (byte)((color << 4) | bottomHalf);
        }
        else
        {
            var upperHalf = cellContent & 0xF0;
            TextBuffer[y / 2 * 40 + x] = (byte)(upperHalf | color);
        }
        RenderLowResBufferToFrameBuffer();
    }
    
    public void RenderLowResBufferToFrameBuffer()
    {
        if (IsInTextMode)
            return;

        var square = new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 };
        var x = 0;
        var y = 3;
        for (var i = 0; i < 40 * 24; i++)
        {
            if (IsMixedScreen && i >= 40 * 20)
                DrawCharacterToFrameBuffer16X16(CharacterGenerator.GetCharacter(TextBuffer[(y - 3) * 40 + x]),
                    (uint)(x * 16), (uint)(y * 16), 0x00FFFFFF);
            else
                DrawCharacterToFrameBuffer16X16WithBackground(square, (uint)(x * 16), (uint)(y * 16),
                    ColorCodeToRgb((byte)(TextBuffer[(y - 3) * 40 + x] & 0xF)),
                    ColorCodeToRgb((byte)(TextBuffer[(y - 3) * 40 + x] >> 4)));
            x++;
            if (x == 40)
            {
                x = 0;
                y++;
            }
        }
    }

    public void RenderTextToFrameBuffer()
    {
        if (!IsInTextMode)
            return;
        var x = 0;
        var y = 3;
        for (var i = 0; i < 40 * 24; i++)
        {
            DrawCharacterToFrameBuffer16X16(CharacterGenerator.GetCharacter(TextBuffer[i]), (uint)(x * 16), (uint)(y * 16), 0xFFFFFF);
            x++;
            if (x == 40)
            {
                x = 0;
                y++;
            }
        }
    }

    public void DrawCharacterToFrameBuffer16X16(byte[] character, uint x, uint y, uint foreground)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(x + 16, Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(y + 16, Height);
        for (var i = 0; i < 16; i+=2)
        {
            var exploded = ExplodeU8(character[i/2], 0, foreground);
            for (var charX = 0; charX < 8; charX++)
            {
                FrameBuffer[(y + i) * Width + x + charX * 2] = exploded[charX];
                FrameBuffer[(y + i) * Width + x + 1 + charX * 2] = exploded[charX];
                FrameBuffer[(y + i + 1) * Width + x + charX * 2] = exploded[charX];
                FrameBuffer[(y + i + 1) * Width + x + 1 + charX * 2] = exploded[charX];
            }
        }
    }
    
    public void DrawCharacterToFrameBuffer16X16WithBackground(byte[] character, uint x, uint y, uint foreground, uint background)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(x + 16, Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(y + 16, Height);
        for (var i = 0; i < 16; i+=2)
        {
            var exploded = ExplodeU8(character[i/2], background, foreground);
            for (var charX = 0; charX < 8; charX++)
            {
                FrameBuffer[(y + i) * Width + x + charX * 2] = exploded[charX];
                FrameBuffer[(y + i) * Width + x + 1 + charX * 2] = exploded[charX];
                FrameBuffer[(y + i + 1) * Width + x + charX * 2] = exploded[charX];
                FrameBuffer[(y + i + 1) * Width + x + 1 + charX * 2] = exploded[charX];
            }
        }
    }
    
    public void DrawCharacterToFrameBuffer8X16(byte[] character, uint x, uint y, uint foreground)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(x + 8, Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(y + 16, Height);
        for (var i = 0; i < 16; i+=2)
        {
            var exploded = ExplodeU8(character[i/2], 0, foreground);
            for (var charX = 0; charX < 8; charX++)
            {
                FrameBuffer[(y + i) * Width + x + charX] = exploded[charX];
                FrameBuffer[(y + i + 1) * Width + x + charX] = exploded[charX];
            }
        }
    }
    
    public void DrawCharacterToFrameBuffer8X8(byte[] character, uint x, uint y, uint foreground)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(x + 8, Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(y + 8, Height);
        for (var i = 0; i < 8; i++)
        {
            var exploded = ExplodeU8(character[i], 0, foreground);
            for (var charX = 0; charX < 8; charX++)
            {
                FrameBuffer[(y + i) * Width + x + charX] = exploded[charX];
            }
        }
    }

    private uint[] ExplodeU8(byte input, uint background, uint foreground)
    {
        var result = new uint[8];
        for (var i = 0; i < 8; i++)
        {
            if (((input >> i) & 1) == 1)
                result[i] = foreground;
            else
                result[i] = background;
        }

        return result;
    }

    private uint ColorCodeToRgb(byte code)
    {
        return code switch
        {
            0 => 0,         // black
            1 => 0xCC0033,  // magenta
            2 => 0x000099,  // dark blue
            3 => 0xCC33CC,  // purple
            4 => 0x006633,  // dark green
            5 => 0x666666,  // dark gray
            6 => 0x3333FF,  // medium blue
            7 => 0x6699FF,  // light blue
            8 => 0x996600,  // brown
            9 => 0xFF6600,  // orange
            10 => 0x999999, // light gray
            11 => 0xFF9999, // pink
            12 => 0x00CC00, // light green
            13 => 0xFFFF00, // yellow
            14 => 0x33FF99, // aqua
            15 => 0xFFFFFF, // white
            _ => 0          // fallback black
        };
    }
}