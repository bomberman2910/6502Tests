namespace Analyze6502;

public class CodeView : BaseView
{
    public override void Draw(int x, int y)
    {
        ClearRenderArea(x, y);
        Console.SetCursorPosition(x, y);
        foreach (var line in Lines)
        {
            var cappedLine = line.Length > 80 ? line[..80] : line;
            Console.WriteLine(cappedLine);
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
        return (80, 24);
    }

    public List<string> Lines { get; set; } = [];
}

public class AsmParser
{
    
}