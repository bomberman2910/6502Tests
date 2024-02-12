using System.Collections.Generic;
using System.Linq;

namespace lib6502;

public class Bus
{
    public Bus()
    {
        Devices = new List<Device>();
    }

    public List<Device> Devices { get; }

    public byte GetData(ushort address) => Devices.Where(d => d.Request(address)).Select(d => d.GetData(address)).FirstOrDefault();

    public void SetData(byte data, ushort address)
    {
        foreach (var d in Devices)
        {
            if (!d.Request(address))
                continue;
            d.SetData(data, address);
            return;
        }
    }

    public void PerformClockActions()
    {
        foreach (var d in Devices)
            d.PerformClockAction();
    }
}