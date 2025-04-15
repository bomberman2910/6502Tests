using Commander;
using CommandLine;

namespace Analyze6502;

internal class Program
{
    private static byte[] currentBuffer = [];
    private static HexView hexView = new();
    private static CodeView codeView = new();
    private static BaseView activeView = hexView;
    
    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(RunApplication).WithNotParsed(HandleParserError);
    }

    private static void HandleParserError(IEnumerable<Error> errors)
    {
        foreach (var error in errors) 
            Console.WriteLine(error.Tag);
    }

    private static void RunApplication(Options obj)
    {
        var commander = new Commander.Commander();
        commander.RegisterCommandsInType(typeof(Program));
        var command = string.Empty;
        while (command != null && !command.ToLower().Equals("q"))
        {
            activeView.Draw(0, 0);
            
            Console.SetCursorPosition(0, 24);
            Console.Write(new string(' ', command.Length + 2));
            Console.SetCursorPosition(0, 24);
            Console.Write("> ");
            command = Console.ReadLine()?.ToLower();
            switch (command)
            {
                case "q":
                    return;
                case "":
                    break;
                default:
                    try
                    {
                        commander.ExecuteCommand(command ?? string.Empty);
                    }
                    catch
                    {
                        Console.WriteLine("Input error");
                    }
                    break;
            }
        }
    }

    [Command("load hex", "Loads a Hex file")]
    [Argument(typeof(string), "path",  "The path to the file")]
    public static void LoadHexFile(string path)
    {
        if(!File.Exists(path))
            throw new FileNotFoundException("File not found", path);
        var lines = File.ReadAllLines(path);
        currentBuffer = lines.SelectMany(line => line.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1].Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(byteString => Convert.ToByte(byteString, 16))).ToArray();
        hexView.Buffer = currentBuffer;

        var offset = BitConverter.ToUInt32(Convert.FromHexString(lines[0].Split(':')[0].PadLeft(8, '0')).Reverse().ToArray());
        hexView.Offset = offset;
    }

    [Command("view hex", "Moves the display offset of the hexadecimal view of the file")]
    [Argument(typeof(uint), "offset", "The new offset at which the view starts at")]
    public static void MoveHexViewOffset(uint offset)
    {
        hexView.CurrentDisplayOffset = offset - hexView.Offset;
    }

    [Command("disasm", "Dissassembles the file")]
    [Argument(typeof(uint), "start", "Start of the disassembly")]
    [Argument(typeof(uint), "length", "Length of the disassembly")]
    public static void DisassembleFile(uint start, uint length)
    {
        var actualStart = start - hexView.Offset;
        var end = actualStart + length;
        var chunk = currentBuffer[(int)actualStart..(int)end];

        long pc = 0;
        var result = new List<string>();
        while (pc < end)
        {
            var nextInstructionPc = pc;
            try
            {
                var instruction = DisassemblyHelpers.Disassemble(chunk, ref pc);
                result.Add(instruction);
            }
            catch
            {
                result.Add($"Instruction cut off -> {chunk[nextInstructionPc]:X2}");
            }

            if (result.Count >= 23)
                break;
            pc++;
        }

        codeView.Lines = result;
    }

    [Command("switch", "Switches between hex view and code view")]
    public static void SwitchView()
    {
        if (activeView == hexView)
            activeView = codeView;
        else
            activeView = hexView;
    }
}