using System;
using System.Collections.Generic;

namespace lib6502
{
    public class Bus
    {
        public Bus()
        {
            Devices = new List<Device>();
        }

        public List<Device> Devices { get; }

        public byte GetData(ushort address)
        {
            foreach (Device d in Devices)
                if (d.Request(address))
                    return d.GetData(address);
            return 0x00;
        }

        public void SetData(byte data, ushort address)
        {
            foreach (Device d in Devices)
                if (d.Request(address))
                {
                    d.SetData(data, address);
                    return;
                }
        }

        public void PerformClockActions()
        {
            foreach (Device d in Devices)
                d.PerformClockAction();
        }
    }
}
