using lib6502;

namespace Terminal6502
{
    public class Terminal : Device
    {
        private byte recv;
        private byte send;
        private byte status;

        public Terminal(ushort rangeStart) : base(rangeStart, (ushort) (rangeStart + 4))
        {
            RDY = false;
        }

        public byte SEND
        {
            get
            {
                SetStatus(StatusBit.SRV_DATA, false);
                return send;
            }
            private set
            {
                send = value;
                SetStatus(StatusBit.SRV_DATA, true);
            }
        }

        public byte RECV
        {
            get
            {
                SetStatus(StatusBit.CLI_DATA, false);
                return recv;
            }
            set
            {
                SetStatus(StatusBit.CLI_DATA, true);
                recv = value;
            }
        }

        public bool RDY
        {
            get { return Cpu6502.CheckBit(status, 3); } //Check SRV_RDY
            set { SetStatus(StatusBit.CLI_RDY, value); } //Set CLI_RDY
        }

        public bool DATA
        {
            get { return Cpu6502.CheckBit(status, 2); }
        }

        public override void SetData(byte data, ushort address)
        {
            if (!Request(address))
                return;
            switch (address)
            {
                case var adr when adr - Start == 0: //SEND
                    SEND = data;
                    break;
                case var adr when adr - Start == 1: //RECV (readonly for 6502)
                    break;
                case var adr when adr - Start == 2:
                    SetStatus(StatusBit.SRV_RDY, data > 0x00); //Set SRV_RDY
                    break;
                case var adr when adr - Start == 3:
                    SetStatus(StatusBit.SRV_DATA, data > 0x00); //Set SRV_DATA
                    break;
            }
        }

        public override byte GetData(ushort address)
        {
            if (!Request(address))
                return 0x00;
            switch (address)
            {
                case var adr when adr - Start == 0: //SEND
                    return 0x00;
                case var adr when adr - Start == 1: //RECV (readonly)
                    return RECV;
                case var adr when adr - Start == 2:
                    return (byte) (Cpu6502.CheckBit(status, 1) ? 1 : 0); //Check CLI_RDY
                case var adr when adr - Start == 3:
                    return (byte) (Cpu6502.CheckBit(status, 0) ? 1 : 0); //Check CLI_DATA
            }

            return 0x00;
        }

        public override void PerformClockAction()
        {
        }

        public void SetStatus(byte bit, bool state)
        {
            if (state)
                status |= bit;
            else
                status &= (byte) ~bit;
        }

        public static class StatusBit
        {
            public static byte SRV_RDY
            {
                get { return 0b00001000; }
            }

            public static byte SRV_DATA
            {
                get { return 0b00000100; }
            }

            public static byte CLI_RDY
            {
                get { return 0b00000010; }
            }

            public static byte CLI_DATA
            {
                get { return 0b00000001; }
            }
        }
    }
}