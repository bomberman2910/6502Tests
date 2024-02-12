using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lib6502;

public class SerialInterfaceAdapter : Device
{
    private List<byte> buffer;

    public SerialInterfaceAdapter(ushort start) : base(start, (ushort)(start + 1))
    {
        Memory[0] = 0x01; //status (Bit 1: char available; Bit 0: client ready)
        buffer = new List<byte>();
    }

    public override byte GetData(ushort address) => Request(address) ? Memory[address - Start] : (byte)0x00;

    public override void PerformClockAction()
    {
        if (buffer.Count == 0 || (Memory[0] & 0x01) == 0x00 || (Memory[0] & 0x02) == 0x01)
            return;
        Memory[1] = buffer.ElementAt(0);
        buffer.RemoveAt(0);
        Memory[0] = (byte)(Memory[0] | 0x02);
    }

    public override void SetData(byte data, ushort address)
    {
        if (address - Start == 0)
            Memory[address - Start] = data;
    }

    public void Send(IEnumerable<byte> inbuf) => buffer = inbuf.ToList();

    public void SendString(string str)
    {
        buffer = Encoding.ASCII.GetBytes(str).ToList();
        buffer.Add(0x00);
    }
}