using System;
namespace lib6502
{
    public class ROM : Device
    {
        public ROM(ushort size, ushort start) : base(start, (ushort)(start + size - 1))
        {
        }

        public void SetMemory(byte[] mem)
        {
            if (mem.Length != memory.Length)
                throw new ArgumentException("Wrong size", nameof(mem));
            for (int i = 0; i < mem.Length; i++)
                memory[i] = mem[i];
        }

        public override byte GetData(ushort address) => Request(address) ? memory[address - start] : (byte)0x00;

        public override void PerformClockAction()
        {
            return;
        }

        public override void SetData(byte data, ushort address)
        {
            return;
        }
    }
}
