using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        private static CPU6502 cpu;
        private static Bus mainbus;
        private static RAM ram;
        private static ROM rom;
        private static Screen screen;
        private static ROM charrom;
        private static TextScreen textscreen;

        private static void Reset()
        {
            mainbus = new Bus();
            ram = new RAM(4096, 0x0000);
            byte[] bbytes = File.ReadAllBytes("textscreen01.bin");
            for (int pc = 0; pc < bbytes.Length; pc++)
                ram.SetData(bbytes[pc], (ushort)(0x0200 + pc));
            mainbus.Devices.Add(ram);

            rom = new ROM(4096, 0xF000);
            byte[] initrom = new byte[4096];
            initrom[0x0FFD] = 0x02;
            for (int i = 0; i < ASMRoutines.PixelDspRoutine().Length; i++)
                initrom[0x0000 + i] = ASMRoutines.PixelDspRoutine()[i];
            for (int i = 0; i < ASMRoutines.CharDspRoutine().Length; i++)
                initrom[0x001C + i] = ASMRoutines.CharDspRoutine()[i];
            rom.SetMemory(initrom);
            mainbus.Devices.Add(rom);

            charrom = new ROM(1024, 0xEC00);
            charrom.SetMemory(File.ReadAllBytes("apple1.vid"));

            screen = new Screen(160, 120, 0xD000);
            screen.Reset();
            mainbus.Devices.Add(screen);

            textscreen = new TextScreen(40, 25, 0xD004);
            textscreen.Reset();
            mainbus.Devices.Add(textscreen);

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
                Console.WriteLine(cpu);
                for (int line = currentpage; line < ((currentpage + 0x0400) > 65536 ? 65536 : (currentpage + 0x0400)); line += 32)
                {
                    Console.Write("$" + line.ToString("X4") + ":");
                    for (int pc = line; pc < (line + 32); pc++)
                        Console.Write(" $" + mainbus.GetData((ushort)pc).ToString("X2"));
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.Write(">");
                command = Console.ReadLine().ToLower();
                ushort usvalue;
                byte bvalue;
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
                    case var cmd when cmd.StartsWith("a ", StringComparison.Ordinal):
                        if (byte.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue))
                            cpu.A = bvalue;
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.StartsWith("x ", StringComparison.Ordinal):
                        if (byte.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue))
                            cpu.X = bvalue;
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.StartsWith("y ", StringComparison.Ordinal):
                        if (byte.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue))
                            cpu.Y = bvalue;
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.StartsWith("sr ", StringComparison.Ordinal):
                        if (byte.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue))
                            cpu.SR = bvalue;
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.StartsWith("sp ", StringComparison.Ordinal):
                        if (byte.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out bvalue))
                            cpu.SP = bvalue;
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.StartsWith("pc ", StringComparison.Ordinal):
                        if (ushort.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue))
                            cpu.PC = usvalue;
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.StartsWith("d ", StringComparison.Ordinal):
                        if (ushort.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue))
                            currentpage = usvalue;
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.StartsWith("m ", StringComparison.Ordinal):
                        if (ushort.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue))
                        {
                            string[] sbytes = Console.ReadLine().Split(' ');
                            byte[] bbytes = new byte[sbytes.Length];
                            byte tmp = 0;
                            foreach (string s in sbytes)
                            {
                                if (!byte.TryParse(s, out tmp))
                                {
                                    Console.WriteLine("Fehlerhafte Eingabe!");
                                    _ = Console.ReadKey(true);
                                    break;
                                }
                            }
                            for (int i = 0; i < sbytes.Length; i++)
                                bbytes[i] = DisASM6502.HexStringToByte(sbytes[i]);
                            for (int mem = 0; mem < bbytes.Length; mem++)
                                ram.SetData(bbytes[mem], (ushort)(usvalue + mem));
                        }
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.Equals(""):
                        cpu.Step();
                        mainbus.PerformClockActions();
                        screen.Screenshot();
                        textscreen.Screenshot();
                        break;
                    case var cmd when cmd.Equals("bl"):
                        foreach (ushort brk in breakpoints)
                            Console.Write(brk.ToString("X4") + ", ");
                        _ = Console.ReadKey(true);
                        break;
                    case var cmd when cmd.StartsWith("ba ", StringComparison.Ordinal):
                        if (ushort.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue))
                            breakpoints.Add(usvalue);
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.StartsWith("br", StringComparison.Ordinal):
                        if (ushort.TryParse(cmd.Split(' ')[1], NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out usvalue) && breakpoints.Contains(usvalue))
                            breakpoints.Remove(usvalue);
                        else
                        {
                            Console.WriteLine("Fehlerhafte Eingabe!");
                            _ = Console.ReadKey(true);
                        }
                        break;
                    case var cmd when cmd.Equals("r"):
                        if (breakpoints.Count == 0)
                        {
                            do
                            {
                                cpu.Step();
                                mainbus.PerformClockActions();
                            } while (!(mainbus.GetData(cpu.PC) == 0x00));
                        }
                        else
                        {
                            do
                            {
                                cpu.Step();
                                mainbus.PerformClockActions();
                            } while (!breakpoints.Contains(cpu.PC) && !(mainbus.GetData(cpu.PC) == 0x00));
                        }
                        screen.Screenshot();
                        textscreen.Screenshot();
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
