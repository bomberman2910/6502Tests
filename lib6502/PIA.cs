using System.Net.NetworkInformation;

namespace lib6502
{
    public class PIA : Device
    {
        private CPU6502 _cpu;
        
        public bool IRQ { get; set; }

        private byte _porta;
        public byte PORTA
        {
            get
            {
                if (!_outa) return 0x00;
                _rdya = false;
                return _porta;
            }
            set
            {
                if (_outa) return;
                _porta = value;
                _rdya = true;
            }
        }

        private byte _portb;
        public byte PORTB
        {
            get
            {
                if (!_outb) return 0x00;
                _rdyb = false;
                return _portb;
            }
            set
            {
                if (_outb) return;
                _portb = value;
                _rdyb = true;
            }
        }

        private bool _outa;
        public bool OUTA
        {
            get => _outa;
            private set => _outa = value;
        }

        private bool _outb;
        public bool OUTB
        {
            get => _outb;
            private set => _outb = value;
        }

        private bool _rdya;

        public bool RDYA
        {
            get => _rdya;
            set
            {
                if (_outa) return;
                _rdya = value;
                if (IRQ) _cpu.IRQ = true;
            }
        }

        private bool _rdyb;

        public bool RDYB
        {
            get => _rdyb;
            set
            {
                if (_outb) return;
                _rdyb = value;
                if (IRQ) _cpu.IRQ = true;
            }
        }

        public PIA(CPU6502 cpu, ushort position) : base(position, (ushort)(position + 4))
        {
            _cpu = cpu;
        }

        public override void SetData(byte data, ushort address)
        {
            if (!Request(address)) return;
            switch (address)
            {
                case var add when add == start:        //PORTA
                    if (OUTA)
                    {
                        PORTA = data;
                        RDYA = true;
                    }
                    break;
                case var add when add == start + 1:    //PORTB
                    if (OUTB)
                    {
                        PORTB = data;
                        RDYB = true;
                    }
                    break;
                case var add when add == start + 2:    //DDR (- - - - - - OUTB OUTA)
                    OUTA = CPU6502.CheckBit(data, 0);
                    OUTB = CPU6502.CheckBit(data, 1);
                    break;
                case var add when add == start + 3:    //RDYR (- - - - - - RDYB RDYA)
                    if (OUTA && CPU6502.CheckBit(data, 0))
                        RDYA = true;
                    else if (OUTA && !CPU6502.CheckBit(data, 0))
                        RDYA = false;
                    if (OUTB && CPU6502.CheckBit(data, 1))
                        RDYB = true;
                    else if (OUTB && !CPU6502.CheckBit(data, 1))
                        RDYB = false;
                    break;
            }
        }

        public override byte GetData(ushort address)
        {
            if (!Request(address)) return 0x00;
            switch (address)
            {
                case var add when add == start:        //PORTA
                    if (!_outa) _rdya = false;
                    return _porta;
                case var add when add == start + 1:    //PORTB
                    if (!_outb) _rdyb = false;
                    return _portb;
                case var add when add == start + 2:    //DDR (- - - - - - OUTB OUTA)
                    return (byte) (((_outb ? 1 : 0) << 1) + (_outa ? 1 : 0));
                case var add when add == start + 3:    //RDYR (- - - - - - RDYB RDYA)
                    return (byte) (((_rdyb ? 1 : 0) << 1) + (_rdya ? 1 : 0));
                default:
                    return 0x00;
            }
        }

        public override void PerformClockAction()
        {
            return;
        }
    }
}