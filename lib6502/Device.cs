using System;
namespace lib6502
{
    public abstract class Device
    {
        public Device(ushort range_start, ushort range_end)
        {
            start = range_start;
            end = range_end;
            memory = new byte[end - start + 1];
        }

        protected readonly byte[] memory;
        protected readonly ushort start;
        protected readonly ushort end;

        public bool Request(ushort address) => (address >= start) && (address <= end);

        public abstract void SetData(byte data, ushort address);

        public abstract byte GetData(ushort address);

        public abstract void PerformClockAction();
    }
}
