namespace lib6502
{
    public static class EFlag
    {
        public static byte NEG => 0b10000000;
        public static byte OVR => 0b01000000;
        public static byte BRK => 0b00010000;
        public static byte DEC => 0b00001000;
        public static byte IRQ => 0b00000100;
        public static byte ZER => 0b00000010;
        public static byte CAR => 0b00000001;
    }
}
