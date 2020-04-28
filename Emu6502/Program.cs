using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using lib6502;

namespace Emu6502
{
    class MainClass
    {
        [DllImport("libc")]
        private static extern int system(string exec);

        private static void ResizeMac(int width, int height)
        {
            system(@"printf '\e[8;" + height.ToString() + ";" + width.ToString() + "t'");
        }
        
        public static void InvertColors()
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = temp;
        }

        private static CPU6502 cpu;
        private static Bus mainbus;
        private static RAM ram;
        private static ROM rom;
        private static Screen screen;
        private static ROM charrom;
        private static TextScreen textscreen;
        private static PIA pia;
        private static SIA sia;

        private static void Reset()
        {
            mainbus = new Bus();
            ram = new RAM(4096, 0x0000);
            //var bbytes = File.ReadAllBytes("dectest.bin");
            //for (var pc = 0; pc < bbytes.Length; pc++)
            //   ram.SetData(bbytes[pc], (ushort)(0x0200 + pc));
            mainbus.Devices.Add(ram);

            rom = new ROM(4096, 0xF000);
            var initrom = new byte[4096];
            initrom[0x0FFD] = 0x02;
            for (var i = 0; i < ASMRoutines.PixelDspRoutine().Length; i++)
                initrom[0x0000 + i] = ASMRoutines.PixelDspRoutine()[i];
            for (var i = 0; i < ASMRoutines.CharDspRoutine().Length; i++)
                initrom[0x001C + i] = ASMRoutines.CharDspRoutine()[i];
            rom.SetMemory(initrom);
            mainbus.Devices.Add(rom);

            screen = new Screen(160, 120, 0xD000);
            screen.Reset();
            mainbus.Devices.Add(screen);

            textscreen = new TextScreen(40, 25, 0xD010);
            textscreen.Reset();
            mainbus.Devices.Add(textscreen);
            
            pia = new PIA(cpu, 0xD020);
            mainbus.Devices.Add(pia);
            
            sia = new SIA(0xD030);
            mainbus.Devices.Add(sia);

            cpu = new CPU6502(mainbus)
            {
                PC = 0x0200
            };
        }

        public static void Main(string[] args)
        {
            ushort currentpage = 0x0000;

            ResizeMac(140, 40);
            //Console.SetWindowSize(140, 43);
            Console.Clear();

            Reset();

            string command = "";
            List<ushort> breakpoints = new List<ushort>();

            while (!command.ToLower().Equals("q"))
            {
                Console.Clear();
                Console.WriteLine($"{cpu}\n");
                Console.WriteLine($"{pia}\n");
                for (int line = currentpage; line < ((currentpage + 0x0400) > 65536 ? 65536 : (currentpage + 0x0400)); line += 32)
                {
                    Console.Write("$" + line.ToString("X4") + ":");
                    for (var pc = line; pc < (line + 32); pc++)
                    {
                        if (pc == cpu.PC) InvertColors();
                        Console.Write(" $" + mainbus.GetData((ushort)pc).ToString("X2"));
                        if (pc == cpu.PC) InvertColors();
                    }

                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.Write(">");
                command = Console.ReadLine().ToLower();
                ushort usvalue;
                byte bvalue;
                string[] setsplit;
                var success = false;
                switch (command)
                {
                    case var cmd when cmd.Equals("q"):
                        break;
                    case var cmd when cmd.Equals("ra"):
                        Reset();
                        break;
                    case var cmd when cmd.Equals("rc"):
                        cpu.Reset();
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
                            if (success) cpu.A = bvalue;
                        }
                        else if (setsplit[1].Equals("x"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if(success) cpu.X = bvalue;
                        }
                        else if (setsplit[1].Equals("y"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if(success) cpu.Y = bvalue;
                        }
                        else if (setsplit[1].Equals("sr"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if(success) cpu.SR = bvalue;
                        }
                        else if (setsplit[1].Equals("sp"))
                        {
                            success = byte.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue);
                            if (success) cpu.SP = bvalue;
                        }
                        else if (setsplit[1].Equals("pc"))
                        {
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                            if(success) cpu.PC = usvalue;
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
                            if (success) currentpage = usvalue;
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
                                ram.SetData(bbytes[mem], (ushort)(usvalue + mem));
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
                            foreach (var brk in breakpoints)
                                Console.Write(brk.ToString("X4") + ", ");
                            _ = Console.ReadKey(true);
                            success = true;
                        }
                        else if (setsplit[1].Equals("add"))
                        {
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue);
                            if (success) breakpoints.Add(usvalue);
                        }
                        else if (setsplit[1].Equals("rm"))
                        {
                            success = ushort.TryParse(setsplit[2], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue) && breakpoints.Contains(usvalue);
                            if (success) breakpoints.Remove(usvalue);
                        }
                        if (success) break;
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                    case var cmd when cmd.Equals(""):
                        cpu.Step();
                        mainbus.PerformClockActions();
                        screen.Screenshot();
                        textscreen.Screenshot();
                        break;
                    case var cmd when cmd.Equals("r"):
                        if (breakpoints.Count == 0)
                            do
                            {
                                cpu.Step();
                                mainbus.PerformClockActions();
                            } while (mainbus.GetData(cpu.PC) != 0x00);
                        else
                            do
                            {
                                cpu.Step();
                                mainbus.PerformClockActions();
                            } while (!breakpoints.Contains(cpu.PC) && mainbus.GetData(cpu.PC) != 0x00);

                        screen?.Screenshot();
                        textscreen?.Screenshot();
                        break;
                    default:
                        Console.WriteLine("Fehlerhafte Eingabe!");
                        _ = Console.ReadKey(true);
                        break;
                }

            }
        }
    }
}
