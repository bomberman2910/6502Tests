using System.Collections.Generic;
using System.Linq;
using lib6502;

namespace View6502_Win
{
    class Serialport : Device
    {

        private List<char> sendstring;

        public Serialport(ushort start) : base(start, (ushort)(start + 2))
        {
            memory[0] = 0x00;   //char available
            memory[1] = 0x00;   //client ready
            sendstring = new List<char>();
        }

        public override byte GetData(ushort address)
        {
            if (((address - start) == 0) || ((address - start) == 2))
                return memory[address - start];
            else
                return 0;
        }

        public override void PerformClockAction()
        {
            if((sendstring.Count != 0) && (memory[1] == 0x01) && (memory[0] != 0x01))
            {
                memory[2] = (byte)sendstring.ElementAt(0);
                sendstring.RemoveAt(0);
                memory[0] = 0x01;
            }
        }

        public override void SetData(byte data, ushort address)
        {
            if (((address - start) == 0) || ((address - start) == 1))
                memory[address - start] = data;
        }

        public void Send(string str)
        {
            sendstring = str.ToList();
            sendstring.Add('\0');
        }
    }
}
