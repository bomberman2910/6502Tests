using System;
using System.IO;
using System.Text;
using lib6502;

namespace Terminal6502
{
    internal static class Program
    {
        private static CPU6502 _cpu;
        private static Bus _mainbus;
        private static RAM _ram;
        private static ROM _rom;
        private static Terminal _terminal;
        
        public static void Main(string[] args)
        {

            var initrom = new byte[4096];
            initrom[0x0FFC] = 0x00;
            initrom[0x0FFD] = 0xF0;
            var bytes = File.ReadAllBytes("termtest.bin");
            for (var i = 0; i < bytes.Length; i++)
                initrom[i] = bytes[i];
            
            _ram = new RAM(4096, 0x0000);    //0x0000-0x0FFF
            _rom = new ROM(4096, 0xF000);    //0xF000-0xFFFF
            _rom.SetMemory(initrom);
            
            _terminal = new Terminal(0xD000);

            _mainbus = new Bus();
            _mainbus.Devices.Add(_ram);
            _mainbus.Devices.Add(_rom);
            _mainbus.Devices.Add(_terminal);
            
            _cpu = new CPU6502(_mainbus);
            
            var key = new ConsoleKeyInfo();

            while (true)
            {
                if (_terminal.RDY)
                {
                    var newline = false;
                    if (Console.KeyAvailable)
                    {
                        key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Q && key.Modifiers == ConsoleModifiers.Control)
                            Environment.Exit(0);
                        if (char.IsControl(key.KeyChar))
                        {
                            switch (key.Key)
                            {
                                case ConsoleKey.Enter:
                                    newline = true;
                                    Send(0x0A);
                                    break;
                            }
                        }
                        else if (char.IsSymbol(key.KeyChar) || char.IsLetterOrDigit(key.KeyChar))
                            Send(Encoding.ASCII.GetBytes(new[] {key.KeyChar})[0]);
                    }
                    else
                    {
                        Send(0x00);
                    }
                    if (!newline) continue;
                    Send(0x0D);
                }
                _cpu.Step();
                _mainbus.PerformClockActions();
            }
        }

        private static void Send(byte chr)
        {
            while (!_terminal.RDY) { }
            _terminal.RECV = chr;
        }

        private static string Receive()
        {
            while(!_terminal.DATA) { }

            return Encoding.ASCII.GetString(new []{ _terminal.SEND });
        }
    }
}