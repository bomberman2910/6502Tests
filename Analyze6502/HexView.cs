using System.Text;

namespace Analyze6502;

public class HexView : BaseView
{
    // complete file buffer
    public byte[] Buffer { get; set; } = [];
    // base offset for all addresses
    public uint Offset { get; set; }
    // display window offset starts with zero and is referring to the buffer 
    // start offset for display window
    public uint CurrentDisplayOffset { get; set; }
    // maximum number of displayed lines
    public int MaxLines { get; set; } = 20;
    // displayed bytes per line
    public ushort BytesPerLine { get; set; } = 16;
    // determines if lines should have ASCII representations of each byte displayed after them
    public bool DisplayAscii { get; set; } = true;

    public override void Draw(int x, int y)
    {
        ClearRenderArea(x, y);
        Console.SetCursorPosition(x, y);
        if(Buffer.Length == 0)
        {
            Console.WriteLine($"{ConvertAddressToString(Offset)}: ");
            return;
        }
        var lastDisplayableOffset = (uint)(CurrentDisplayOffset + MaxLines * BytesPerLine);
        if(lastDisplayableOffset >= Buffer.Length)
            lastDisplayableOffset = (uint)(Buffer.Length - 1);
        for (var i = CurrentDisplayOffset; i <= lastDisplayableOffset; i += BytesPerLine)
        {
            var offsetString = ConvertAddressToString(Offset + i);
            Console.Write($"{offsetString}: ");
            var lastOffsetForLine = i + BytesPerLine - 1;
            var byteCountForLine = BytesPerLine;
            if(lastOffsetForLine > lastDisplayableOffset)
            {
                byteCountForLine = (ushort)(lastDisplayableOffset - i + 1);
                lastOffsetForLine = lastDisplayableOffset;
            }
            var lineBytes = new byte[byteCountForLine];
            for (var j = i; j < lastOffsetForLine; j++) 
                lineBytes[j - i] = Buffer[j];
            Console.Write(string.Join(" ", lineBytes.Select(b => b.ToString("X2"))));
            if (DisplayAscii)
            {
                Console.Write(new string(' ', 3 + (BytesPerLine - byteCountForLine) * 3));
                foreach (var b in lineBytes)
                {
                    var character = Encoding.UTF8.GetString([b])[0];
                    if (char.IsControl(character))
                    {
                        var oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write('.');
                        Console.ForegroundColor = oldColor;
                    }
                    else
                    {
                        Console.Write(character);
                    }
                }
            }
            Console.WriteLine();
        }
    }

    public override void ClearRenderArea(int x, int y)
    {
        var size = GetSize();
        Console.SetCursorPosition(x, y);
        for (var line = 0; line < size.height; line++)
            Console.WriteLine(new string(' ', size.width));
    }

    private (int width, int height) GetSize()
    {
        return (9 + (BytesPerLine * 3) + 4 + BytesPerLine, MaxLines);
    }

    private string ConvertAddressToString(uint address)
    {
        var offsetBytes = BitConverter.GetBytes(address);
        Array.Reverse(offsetBytes);
        var offsetString = Convert.ToHexString(offsetBytes).Replace("-", "").PadLeft(8, '0');
        return offsetString;
    }
}