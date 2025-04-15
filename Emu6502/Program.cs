using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Commander;
using lib6502;

namespace Emu6502;

internal static class MainClass
{
    private static Cpu6502 _cpu;
    private static Bus _mainbus;
    private static RandomAccessMemory _randomAccessMemory;
    private static ReadOnlyMemory _readOnlyMemory;
    private static Screen _screen;
    private static TextScreen _textscreen;
    private static ParallelInterfaceAdapter _parallelInterfaceAdapter;
    private static SerialInterfaceAdapter _serialInterfaceAdapter;

    private static DirectBitmap _frameBuffer;
    public static readonly object FrameBufferLock = new();

    private static List<ushort> _breakpoints;
    private static ushort _currentpage;
    private static bool _showSdl;

    private static Commander.Commander _commander;

    [DllImport("libc", CharSet = CharSet.Unicode)]
    private static extern int system(string exec);

    private static void ResizeMac(int width, int height)
    {
        system(@"printf '\e[8;" + height + ";" + width + "t'");
    }

    private static void InvertColors()
    {
        (Console.ForegroundColor, Console.BackgroundColor) = (Console.BackgroundColor, Console.ForegroundColor);
    }

    private static void Reset()
    {
        _mainbus = new Bus();
        _randomAccessMemory = new RandomAccessMemory(4096, 0x0000);
        _mainbus.Devices.Add(_randomAccessMemory);

        _readOnlyMemory = new ReadOnlyMemory(4096, 0xF000);
        var initrom = new byte[4096];
        initrom[0x0FFD] = 0x02;
        for (var i = 0; i < AsmRoutines.PixelDspRoutine().Length; i++)
            initrom[0x0000 + i] = AsmRoutines.PixelDspRoutine()[i];
        for (var i = 0; i < AsmRoutines.CharDspRoutine().Length; i++)
            initrom[0x001C + i] = AsmRoutines.CharDspRoutine()[i];
        _readOnlyMemory.SetMemory(initrom);
        _mainbus.Devices.Add(_readOnlyMemory);

        _screen = new Screen(160, 120, 0xD000);
        _screen.Reset();
        _frameBuffer = _screen.BitmapScreen;
        _mainbus.Devices.Add(_screen);

        _textscreen = new TextScreen(40, 25, 0xD010);
        _textscreen.Reset();
        _mainbus.Devices.Add(_textscreen);

        _parallelInterfaceAdapter = new ParallelInterfaceAdapter(_cpu, 0xD020);
        _mainbus.Devices.Add(_parallelInterfaceAdapter);

        _serialInterfaceAdapter = new SerialInterfaceAdapter(0xD030);
        _mainbus.Devices.Add(_serialInterfaceAdapter);

        _cpu = new Cpu6502(_mainbus)
        {
            ProgramCounter = 0x0200
        };
    }

    private static void PrintStatus()
    {
        Console.Clear();
        Console.WriteLine($"{_cpu}\n");
        Console.WriteLine($"{_parallelInterfaceAdapter}\n");
        for (int line = _currentpage; line < (_currentpage + 0x0400 > 65536 ? 65536 : _currentpage + 0x0400); line += 32)
        {
            Console.Write("$" + line.ToString("X4") + ":");
            for (var pc = line; pc < line + 32; pc++)
            {
                Console.Write(" ");
                if (pc == _cpu.ProgramCounter)
                    InvertColors();
                Console.Write($"${_mainbus.GetData((ushort)pc):X2}");
                if (pc == _cpu.ProgramCounter)
                    InvertColors();
            }

            Console.WriteLine();
        }

        Console.WriteLine();
        Console.Write(">");
    }

    [Command("reg a", "Sets the value of register A")]
    [Argument(typeof(byte), "value", "New value for register A")]
    private static void ModifyRegisterA(byte value)
    {
        _cpu.A = value;
    }

    [Command("reg x", "Sets the value of register X")]
    [Argument(typeof(byte), "value", "New value for register X")]
    private static void ModifyRegisterX(byte value)
    {
        _cpu.X = value;
    }

    [Command("reg y", "Sets the value of register Y")]
    [Argument(typeof(byte), "value", "New value for register Y")]
    private static void ModifyRegisterY(byte value)
    {
        _cpu.Y = value;
    }

    [Command("reg sr", "Sets the value of the status register")]
    [Argument(typeof(byte), "value", "New value for the status register")]
    private static void ModifyRegisterSR(byte value)
    {
        _cpu.StatusRegister = value;
    }

    [Command("reg sp", "Sets the value of the stack pointer")]
    [Argument(typeof(byte), "value", "New value for the stack pointer")]
    private static void ModifyRegisterSP(byte value)
    {
        _cpu.StackPointer = value;
    }

    [Command("reg pc", "Sets the value of the program counter")]
    [Argument(typeof(ushort), "value", "New value for the program counter")]
    private static void ModifyPC(byte value)
    {
        _cpu.ProgramCounter = value;
    }

    [Command("mem disp", "Moves the start of the displayed memory page")]
    [Argument(typeof(ushort), "page_start", "New page start for displayed memory")]
    private static void DisplayMem(ushort pageStart)
    {
        _currentpage = pageStart;
    }

    [Command("mem set", "Sets memory")]
    [Argument(typeof(ushort), "address", "The address to where the values are to be written")]
    [Argument(typeof(byte[]), "values", "The values to be written to the address")]
    private static void SetMemory(ushort address, byte[] values)
    {
        for (var mem = 0; mem < values.Length; mem++)
            _randomAccessMemory.SetData(values[mem], (ushort)(address + mem));
    }

    [Command("mem load", "Asks for path to binary file and loads contents to given address")]
    [Argument(typeof(ushort), "address", "The address to where the values are to be written")]
    private static void LoadMemory(ushort address)
    {
        var path = Console.ReadLine();
        if (!File.Exists(path))
        {
            Console.WriteLine("File not found");
            return;
        }

        var bytes = File.ReadAllBytes(path);
        for (var mem = 0; mem < bytes.Length; mem++)
            _randomAccessMemory.SetData(bytes[mem], (ushort)(address + mem));
    }

    [Command("bp list", "Lists all currently set breakpoints")]
    private static void ListBreakpoints()
    {
        foreach (var brk in _breakpoints)
            Console.Write(brk.ToString("X4") + ", ");
    }

    [Command("bp add", "Adds a breakpoint")]
    [Argument(typeof(ushort), "address", "address of the breakpoint")]
    private static void AddBreakpoint(ushort address)
    {
        _breakpoints.Add(address);
    }

    [Command("bp rm", "Removes a breakpoint")]
    [Argument(typeof(ushort), "address", "address of the breakpoint")]
    private static void RemoveBreakpoint(ushort address)
    {
        _breakpoints.Remove(address);
    }

    [Command("pia get a", "Displays the current value of port A of the PIA")]
    private static void GetPiaPortA()
    {
        Console.WriteLine($"Port A value: {_parallelInterfaceAdapter.PortA}");
    }

    [Command("pia get b", "Displays the current value of port B of the PIA")]
    private static void GetPiaPortB()
    {
        Console.WriteLine($"Port B value: {_parallelInterfaceAdapter.PortB}");
    }

    [Command("pia set a", "Writes a value to port A of the PIA")]
    [Argument(typeof(byte), "value", "Value to write to Port A")]
    private static void SetPiaPortA(byte value)
    {
        _parallelInterfaceAdapter.PortA = value;
    }

    [Command("pia set b", "Writes a value to port B of the PIA")]
    [Argument(typeof(byte), "value", "Value to write to Port B")]
    private static void SetPiaPortB(byte value)
    {
        _parallelInterfaceAdapter.PortB = value;
    }

    [Command("pia set irq", "Flips the state of the IRQ flag of the PIA")]
    private static void SetPiaIrq()
    {
        _parallelInterfaceAdapter.InterruptRequest = !_parallelInterfaceAdapter.InterruptRequest;
    }

    [Command("reset all", "Resets the entire system")]
    private static void ResetAll()
    {
        Reset();
    }

    [Command("reset cpu", "Resets the CPU")]
    private static void ResetCpu()
    {
        _cpu.Reset();
    }

    [Command("reset pia", "Resets the PIA")]
    private static void ResetPia()
    {
        _parallelInterfaceAdapter.Reset();
    }

    [Command("asm line", "Takes a line of assembly, tries to assemble it and writes it to memory starting at the program counter")]
    private static void AssembleLine()
    {
        var line = Console.ReadLine();
        byte[] bytes;
        try
        {
            bytes = Asm6502.Assemble(line);
        }
        catch
        {
            Console.WriteLine("Error during assembly");
            return;
        }

        for (var i = 0; i < bytes.Length; i++)
            _mainbus.SetData(bytes[i], (ushort)(_cpu.ProgramCounter + i));
    }

    [Command("asm file", "Takes a path to a file and reads the contents, then tries to assemble it and writes it to memory starting at the program counter")]
    private static void AssembleFile()
    {
        var path = Console.ReadLine();
        if (!File.Exists(path))
        {
            Console.WriteLine("File not found");
            return;
        }

        var lines = File.ReadAllText(path);
        byte[] bytes;
        try
        {
            bytes = Asm6502.Assemble(lines);
        }
        catch
        {
            Console.WriteLine("Error during assembly");
            return;
        }

        for (var i = 0; i < bytes.Length; i++)
            _mainbus.SetData(bytes[i], (ushort)(_cpu.ProgramCounter + i));
    }

    [Command("run", "Runs the system either until 0x00 is read as the next instruction or a breakpoint is encountered")]
    private static void Run()
    {
        if (_breakpoints.Count == 0)
        {
            do
            {
                _cpu.Step();
                _mainbus.PerformClockActions();
            } while (_mainbus.GetData(_cpu.ProgramCounter) != 0x00);
        }
        else
        {
            do
            {
                _cpu.Step();
                _mainbus.PerformClockActions();
            } while (!_breakpoints.Contains(_cpu.ProgramCounter) && _mainbus.GetData(_cpu.ProgramCounter) != 0x00);
        }

        _screen?.Screenshot();
        _textscreen?.Screenshot();
    }

    [Command("h", "Displays this help message")]
    private static void PrintHelp()
    {
        Console.WriteLine(_commander.GenerateHelpLines());
    }

    public static void Main(string[] args)
    {
        _commander = new Commander.Commander();
        _commander.RegisterCommandsInType(typeof(MainClass));

        _currentpage = 0x0000;

        //ResizeMac(140, 40);
        Console.Clear();

        Reset();

        var command = "";
        _breakpoints = new List<ushort>();

        while (command != null && !command.ToLower().Equals("q"))
        {
            PrintStatus();

            command = Console.ReadLine()?.ToLower();
            switch (command)
            {
                case "q":
                    _showSdl = false;
                    return;
                case "":
                    _cpu.Step();
                    _mainbus.PerformClockActions();
                    // _screen.Render(renderer, 640, 480);
                    _screen.Screenshot();
                    _textscreen.Screenshot();
                    break;
                default:
                    try
                    {
                        _commander.ExecuteCommand(command);
                    }
                    catch
                    {
                        Console.WriteLine("Input error");
                    }

                    _ = Console.ReadKey(true);
                    break;
            }
        }
    }
}