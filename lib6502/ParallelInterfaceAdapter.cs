namespace lib6502;

public class ParallelInterfaceAdapter : Device
{
    private readonly Cpu6502 cpu;

    private byte portA;

    private byte portB;

    private bool readyA;

    private bool readyB;

    public ParallelInterfaceAdapter(Cpu6502 cpu, ushort position) : base(position, (ushort)(position + 4))
    {
        this.cpu = cpu;
    }

    public bool InterruptRequest { get; set; }

    public byte PortA
    {
        get
        {
            if (!OutA)
                return 0x00;
            readyA = false;
            return portA;
        }
        set
        {
            if (OutA)
                return;
            portA = value;
            ReadyA = true;
        }
    }

    public byte PortB
    {
        get
        {
            if (!OutB)
                return 0x00;
            readyB = false;
            return portB;
        }
        set
        {
            if (OutB)
                return;
            portB = value;
            ReadyB = true;
        }
    }

    public bool OutA { get; private set; }

    public bool OutB { get; private set; }

    public bool ReadyA
    {
        get { return readyA; }
        set
        {
            if (OutA)
                return;
            readyA = value;
            if (InterruptRequest)
                cpu.InterruptRequest = readyA;
        }
    }

    public bool ReadyB
    {
        get { return readyB; }
        set
        {
            if (OutB)
                return;
            readyB = value;
            if (InterruptRequest)
                cpu.InterruptRequest = readyB;
        }
    }

    public override void SetData(byte data, ushort address)
    {
        if (!Request(address))
            return;
        switch (address)
        {
            case var add when add == Start: //PORTA
                if (OutA)
                {
                    portA = data;
                    readyA = true;
                }

                break;
            case var add when add == Start + 1: //PORTB
                if (OutB)
                {
                    portB = data;
                    readyB = true;
                }

                break;
            case var add when add == Start + 2: //DDR (- - - - - - OUTB OUTA)
                OutA = Util.CheckBit(data, 0);
                OutB = Util.CheckBit(data, 1);
                break;
            case var add when add == Start + 3: //RDYR (- - - - - - RDYB RDYA)
                readyA = OutA switch
                {
                    true when Util.CheckBit(data, 0) => true,
                    true when !Util.CheckBit(data, 0) => false,
                    _ => readyA
                };
                readyB = OutB switch
                {
                    true when Util.CheckBit(data, 1) => true,
                    true when !Util.CheckBit(data, 1) => false,
                    _ => readyB
                };
                break;
        }
    }

    public override byte GetData(ushort address)
    {
        if (!Request(address))
            return 0x00;
        switch (address)
        {
            case var add when add == Start: //PORTA
                if (!OutA)
                    readyA = false;
                return portA;
            case var add when add == Start + 1: //PORTB
                if (!OutB)
                    readyB = false;
                return portB;
            case var add when add == Start + 2: //DDR (- - - - - - OUTB OUTA)
                return (byte)(((OutB ? 1 : 0) << 1) + (OutA ? 1 : 0));
            case var add when add == Start + 3: //RDYR (- - - - - - RDYB RDYA)
                return (byte)(((readyB ? 1 : 0) << 1) + (readyA ? 1 : 0));
            default:
                return 0x00;
        }
    }

    public override void PerformClockAction()
    {
    }

    public void Reset()
    {
        portA = 0;
        portB = 0;
        OutA = false;
        OutB = false;
        readyA = false;
        readyB = false;
        InterruptRequest = false;
    }

    public override string ToString() => $"PORTA: ${portA:X2}\tPORTB: ${portB:X2}\tOUTA: {OutA}\tOUTB: {OutB}\tRDYA: {readyA}\tRDYB: {readyB}\tInterrupting {(InterruptRequest ? "enabled" : "disabled")}";
}