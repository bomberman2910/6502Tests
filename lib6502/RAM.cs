using System;
namespace lib6502
{
    public class RAM : Device
    {
        public RAM(ushort size, ushort start):base(start, (ushort)(start + size - 1))
        {
        }

        public override byte GetData(ushort address) => Request(address) ? memory[address - start] : (byte)0x00;

        public override void PerformClockAction()
        {
            return;
        }

        public override void SetData(byte data, ushort address)
        {
            if (Request(address))
                memory[address - start] = data;
        }
    }
}
