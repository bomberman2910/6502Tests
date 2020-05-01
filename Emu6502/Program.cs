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
        [DllImport("libc")]
        private static extern int system(string exec);

        private static void ResizeMac(int width, int height)
        {
            system(@"printf '\e[8;" + height.ToString() + ";" + width.ToString() + "t'");
        }

        private static void InvertColors()
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = temp;
        }

        private static CPU6502 _cpu;
        private static Bus _mainbus;
        private static RAM _ram;
        private static ROM _rom;
        private static Screen _screen;
        private static TextScreen _textscreen;
        private static PIA _pia;
        private static SIA _sia;
        
        private static List<ushort> _breakpoints;
        private static ushort _currentpage;

        private static void Reset()
        {
            _mainbus = new Bus();
            _ram = new RAM(4096, 0x0000);
            //var bbytes = File.ReadAllBytes("dectest.bin");
            //for (var pc = 0; pc < bbytes.Length; pc++)
            //   ram.SetData(bbytes[pc], (ushort)(0x0200 + pc));
            _mainbus.Devices.Add(_ram);

            _rom = new ROM(4096, 0xF000);
            var initrom = new byte[4096];
            initrom[0x0FFD] = 0x02;
            for (var i = 0; i < ASMRoutines.PixelDspRoutine().Length; i++)
                initrom[0x0000 + i] = ASMRoutines.PixelDspRoutine()[i];
            for (var i = 0; i < ASMRoutines.CharDspRoutine().Length; i++)
                initrom[0x001C + i] = ASMRoutines.CharDspRoutine()[i];
            _rom.SetMemory(initrom);
            _mainbus.Devices.Add(_rom);

            _screen = new Screen(160, 120, 0xD000);
            _screen.Reset();
            _mainbus.Devices.Add(_screen);

            _textscreen = new TextScreen(40, 25, 0xD010);
            _textscreen.Reset();
            _mainbus.Devices.Add(_textscreen);
            
            _pia = new PIA(_cpu, 0xD020);
            _mainbus.Devices.Add(_pia);
            
            _sia = new SIA(0xD030);
            _mainbus.Devices.Add(_sia);

            _cpu = new CPU6502(_mainbus)
            {
                PC = 0x0200
            };
        }

        private static void PrintStatus()
        {
            Console.Clear();
            Console.WriteLine($"{_cpu}\n");
            Console.WriteLine($"{_pia}\n");
            for (int line = _currentpage; line < ((_currentpage + 0x0400) > 65536 ? 65536 : (_currentpage + 0x0400)); line += 32)
            {
                Console.Write("$" + line.ToString("X4") + ":");
                for (var pc = line; pc < (line + 32); pc++)
                {
                    Console.Write(" ");
                    if (pc == _cpu.PC) InvertColors();
                    Console.Write($"${_mainbus.GetData((ushort) pc):X2}");
                    if (pc == _cpu.PC) InvertColors();
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
                    case var cmd when cmd.Equals("q"):
                        break;
                    case var cmd when cmd.StartsWith("reg"):
                        setsplit = cmd.Split(' ');
                        if (setsplit.Length != 3)
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                            break;
                        }
                        if (setsplit[1].Equals("a"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success) _cpu.A = bvalue;
                        }
                        else if (setsplit[1].Equals("x"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if(success) _cpu.X = bvalue;
                        }
                        else if (setsplit[1].Equals("y"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if(success) _cpu.Y = bvalue;
                        }
                        else if (setsplit[1].Equals("sr"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if(success) _cpu.SR = bvalue;
                        }
                        else if (setsplit[1].Equals("sp"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success) _cpu.SP = bvalue;
                        }
                        else if (setsplit[1].Equals("pc"))
                        {
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                            if(success) _cpu.PC = usvalue;
                        }

                        if (success) break;
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    case var cmd when cmd.StartsWith("mem"):
                        setsplit = cmd.Split(new[] {' '}, 4);
                        if (setsplit.Length < 3 || setsplit.Length > 4)
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                            break;
                        }
                        if (setsplit[1].Equals("disp"))
                        {
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                            if (success) _currentpage = usvalue;
                        }
                        else if (setsplit[1].Equals("set"))
                        {
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                            if (!success) break;
                            var sbytes = setsplit[3].Split(' ');
                            var bbytes = new byte[sbytes.Length];
                            for (var i = 0; i < sbytes.Length; i++)
                            {
                                success = byte.TryParse(sbytes[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bbytes[i]);
                                if (!success) break;
                            }
                            for (var mem = 0; mem < bbytes.Length; mem++)
                                _ram.SetData(bbytes[mem], (ushort)(usvalue + mem));
                        }
                        if (success) break;
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    case var cmd when cmd.StartsWith("bp"):
                        setsplit = cmd.Split(new[] {' '}, 3);
                        if (setsplit.Length < 2 || setsplit.Length > 3)
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                            break;
                        }

                        if (setsplit[1].Equals("list"))
                        {
                            foreach (var brk in _breakpoints)
                                Console.Write(brk.ToString("X4") + ", ");
                            _ = Console.ReadKey(true);
                            success = true;
                        }
                        else if (setsplit[1].Equals("add"))
                        {
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                            if (success) _breakpoints.Add(usvalue);
                        }
                        else if (setsplit[1].Equals("rm"))
                        {
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue) && _breakpoints.Contains(usvalue);
                            if (success) _breakpoints.Remove(usvalue);
                        }
                        if (success) break;
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    case var cmd when cmd.StartsWith("pia"):
                        setsplit = cmd.Split(new[] {' '}, 4);
                        if (setsplit.Length < 3 || setsplit.Length > 4)
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                            break;
                        }

                        if (setsplit[1].Equals("get"))
                        {
                            if (setsplit[2].Equals("a"))
                            {
                                Console.WriteLine($"Port A value: {_pia.PORTA}");
                                success = true;
                            }
                            else if (setsplit[2].Equals("b"))
                            {
                                Console.WriteLine($"Port B value: {_pia.PORTB}");
                                success = true;
                            }
                        } 
                        else if (setsplit[1].Equals("set"))
                        {
                            if (setsplit[2].Equals("a"))
                            {
                                success = byte.TryParse(setsplit[3], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                                if (success) _pia.PORTA = bvalue;
                            }
                            else if (setsplit[2].Equals("b"))
                            {
                                success = byte.TryParse(setsplit[3], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                                if (success) _pia.PORTB = bvalue;
                            }
                            else if (setsplit[2].Equals("irq"))
                            {
                                _pia.IRQ = !_pia.IRQ;
                                success = true;
                            }
                        }
                        if (success) break;
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

                        if (setsplit[1].Equals("all"))
                        {
                            Reset();
                            success = true;
                        }
                        else if (setsplit[1].Equals("cpu"))
                        {
                            _cpu.Reset();
                            success = true;
                        }
                        else if (setsplit[1].Equals("pia"))
                        {
                            _pia.Reset();
                            success = true;
                        }

                        if (success) break;
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
                                var bytes = ASM6502.Assemble(setsplit[2]);
                                for (var i = 0; i < bytes.Length; i++)
                                    _mainbus.SetData(bytes[i], (ushort) (_cpu.PC + i));
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
                                var bytes = ASM6502.Assemble(lines);
                                for (var i = 0; i < bytes.Length; i++)
                                    _mainbus.SetData(bytes[i], (ushort) (_cpu.PC + i));
                                success = true;
                            }
                            catch
                            {
                                success = false;
                            }
                        }
                        if (success) break;
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    case var cmd when cmd.Equals(""):
                        _cpu.Step();
                        _mainbus.PerformClockActions();
                        _screen.Screenshot();
                        _textscreen.Screenshot();
                        break;
                    case var cmd when cmd.Equals("r"):
                        if (_breakpoints.Count == 0)
                            do
                            {
                                _cpu.Step();
                                _mainbus.PerformClockActions();
                            } while (_mainbus.GetData(_cpu.PC) != 0x00);
                        else
                            do
                            {
                                _cpu.Step();
                                _mainbus.PerformClockActions();
                            } while (!_breakpoints.Contains(_cpu.PC) && _mainbus.GetData(_cpu.PC) != 0x00);

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
