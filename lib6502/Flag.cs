namespace lib6502
{
    public static class Flag
    {
        public static byte Negative
        {
            get { return 0b10000000; }
        }

        public static byte Overflow
        {
            get { return 0b01000000; }
        }

        public static byte Break
        {
            get { return 0b00010000; }
        }

        public static byte Decimal
        {
            get { return 0b00001000; }
        }

        public static byte InterruptRequest
        {
            get { return 0b00000100; }
        }

        public static byte Zero
        {
            get { return 0b00000010; }
        }

        public static byte Carry
        {
            get { return 0b00000001; }
        }
    }
}