using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using lib6502;

namespace Emu6502
{
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

        private static List<ushort> _breakpoints;
        private static ushort _currentpage;

        [DllImport("libc")]
        private static extern int system(string exec);

        private static void ResizeMac(int width, int height)
        {
            system(@"printf '\e[8;" + height + ";" + width + "t'");
        }

        private static void InvertColors()
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = temp;
        }

        private static void Reset()
        {
            _mainbus = new Bus();
            _randomAccessMemory = new RandomAccessMemory(4096, 0x0000);
            //var bbytes = File.ReadAllBytes("dectest.bin");
            //for (var pc = 0; pc < bbytes.Length; pc++)
            //   ram.SetData(bbytes[pc], (ushort)(0x0200 + pc));
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
                    Console.Write($"${_mainbus.GetData((ushort) pc):X2}");
                    if (pc == _cpu.ProgramCounter)
                        InvertColors();
                }

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.Write(">");
        }

        private static void ExecuteCommand(string command)
        {
            ushort usvalue;
            byte bvalue;
            string[] setsplit;
            var success = false;
            switch (command)
            {
                case "q":
                    break;
                case var cmd when cmd.StartsWith("reg"):
                    setsplit = cmd.Split(' ');
                    if (setsplit.Length != 3)
                    {
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    }

                    switch (setsplit[1])
                    {
                        case "a":
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success)
                                _cpu.A = bvalue;
                            break;
                        case "x":
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success)
                                _cpu.X = bvalue;
                            break;
                        case "y":
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success)
                                _cpu.Y = bvalue;
                            break;
                        case "sr":
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success)
                                _cpu.StatusRegister = bvalue;
                            break;
                        case "sp":
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success)
                                _cpu.StackPointer = bvalue;
                            break;
                        case "pc":
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                            if (success)
                                _cpu.ProgramCounter = usvalue;
                            break;
                    }

                    if (success)
                        break;
                    Console.WriteLine("Fehlerhafte Eingabe!");
                    _ = Console.ReadKey(true);
                    break;
                case var cmd when cmd.StartsWith("mem"):
                    setsplit = cmd.Split(new[] {' '}, 4);
                    if (setsplit.Length is < 3 or > 4)
                    {
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    }

                    if (setsplit[1].Equals("disp"))
                    {
                        success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                        if (success)
                            _currentpage = usvalue;
                    }
                    else if (setsplit[1].Equals("set"))
                    {
                        success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                        if (!success)
                            break;
                        var sbytes = setsplit[3].Split(' ');
                        var bbytes = new byte[sbytes.Length];
                        for (var i = 0; i < sbytes.Length; i++)
                        {
                            success = byte.TryParse(sbytes[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bbytes[i]);
                            if (!success)
                                break;
                        }

                        for (var mem = 0; mem < bbytes.Length; mem++)
                            _randomAccessMemory.SetData(bbytes[mem], (ushort) (usvalue + mem));
                    }

                    if (success)
                        break;
                    Console.WriteLine("Fehlerhafte Eingabe!");
                    _ = Console.ReadKey(true);
                    break;
                case var cmd when cmd.StartsWith("bp"):
                    setsplit = cmd.Split(new[] {' '}, 3);
                    if (setsplit.Length is < 2 or > 3)
                    {
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    }

                    switch (setsplit[1])
                    {
                        case "list":
                            foreach (var brk in _breakpoints)
                                Console.Write(brk.ToString("X4") + ", ");
                            _ = Console.ReadKey(true);
                            success = true;
                            break;
                        case "add":
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                            if (success)
                                _breakpoints.Add(usvalue);
                            break;
                        case "rm":
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue) && _breakpoints.Contains(usvalue);
                            if (success)
                                _breakpoints.Remove(usvalue);
                            break;
                    }

                    if (success)
                        break;
                    Console.WriteLine("Fehlerhafte Eingabe!");
                    _ = Console.ReadKey(true);
                    break;
                case var cmd when cmd.StartsWith("pia"):
                    setsplit = cmd.Split(new[] {' '}, 4);
                    if (setsplit.Length is < 3 or > 4)
                    {
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    }

                    switch (setsplit[1])
                    {
                        case "get":
                            switch (setsplit[2])
                            {
                                case "a":
                                    Console.WriteLine($"Port A value: {_parallelInterfaceAdapter.PortA}");
                                    success = true;
                                    break;
                                case "b":
                                    Console.WriteLine($"Port B value: {_parallelInterfaceAdapter.PortB}");
                                    success = true;
                                    break;
                            }
                            break;
                        case "set" when setsplit[2].Equals("a"):
                            success = byte.TryParse(setsplit[3], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success)
                                _parallelInterfaceAdapter.PortA = bvalue;
                            break;
                        case "set" when setsplit[2].Equals("b"):
                            success = byte.TryParse(setsplit[3], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success)
                                _parallelInterfaceAdapter.PortB = bvalue;
                            break;
                        case "set":
                            if (setsplit[2].Equals("irq"))
                            {
                                _parallelInterfaceAdapter.InterruptRequest = !_parallelInterfaceAdapter.InterruptRequest;
                                success = true;
                            }
                            break;
                    }

                    if (success)
                        break;
                    Console.WriteLine("Fehlerhafte Eingabe!");
                    _ = Console.ReadKey(true);
                    break;
                case var cmd when cmd.StartsWith("reset"):
                    setsplit = cmd.Split(new[] {' '}, 2);
                    if (setsplit.Length != 2)
                    {
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    }

                    switch (setsplit[1])
                    {
                        case "all":
                            Reset();
                            success = true;
                            break;
                        case "cpu":
                            _cpu.Reset();
                            success = true;
                            break;
                        case "pia":
                            _parallelInterfaceAdapter.Reset();
                            success = true;
                            break;
                    }

                    if (success)
                        break;
                    Console.WriteLine("Fehlerhafte Eingabe!");
                    _ = Console.ReadKey(true);
                    break;
                case var cmd when cmd.StartsWith("asm"):
                    setsplit = cmd.Split(new[] {' '}, 3);
                    if (setsplit.Length < 3)
                    {
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    }

                    if (setsplit[1].Equals("line"))
                    {
                        try
                        {
                            var bytes = Asm6502.Assemble(setsplit[2]);
                            for (var i = 0; i < bytes.Length; i++)
                                _mainbus.SetData(bytes[i], (ushort) (_cpu.ProgramCounter + i));
                            success = true;
                        }
                        catch
                        {
                            success = false;
                        }
                    }
                    else if (setsplit[1].Equals("file"))
                    {
                        string lines;
                        try
                        {
                            lines = File.ReadAllText(setsplit[2]);
                        }
                        catch
                        {
                            Console.WriteLine("Datei nicht gefunden!");
                            _ = Console.ReadKey(true);
                            break;
                        }

                        try
                        {
                            var bytes = Asm6502.Assemble(lines);
                            for (var i = 0; i < bytes.Length; i++)
                                _mainbus.SetData(bytes[i], (ushort) (_cpu.ProgramCounter + i));
                            success = true;
                        }
                        catch
                        {
                            success = false;
                        }
                    }

                    if (success)
                        break;
                    Console.WriteLine("Fehlerhafte Eingabe!");
                    _ = Console.ReadKey(true);
                    break;
                case "":
                    _cpu.Step();
                    _mainbus.PerformClockActions();
                    _screen.Screenshot();
                    _textscreen.Screenshot();
                    break;
                case "r":
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
                    break;
                default:
                    Console.WriteLine("Fehlerhafte Eingabe!");
                    _ = Console.ReadKey(true);
                    break;
            }
        }

        public static void Main(string[] args)
        {
            _currentpage = 0x0000;

            ResizeMac(140, 40);
            //Console.SetWindowSize(140, 43);
            Console.Clear();

            Reset();

            var command = "";
            _breakpoints = new List<ushort>();

            while (command != null && !command.ToLower().Equals("q"))
            {
                PrintStatus();

                command = Console.ReadLine()?.ToLower();
                ExecuteCommand(command);
            }
        }
    }
}