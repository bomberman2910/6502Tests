using System.Globalization;

namespace lib6502;

public static class Util
{
    public static bool CheckBit(byte value, int bit) => (value & (1 << bit)) == 1 << bit;
    public static byte BcdToDec(byte bcd) => (byte)(10 * (bcd >> 4) + (bcd & 0xF));

    public static byte DecToBcd(byte dec)
    {
        if (dec > 99)
            dec = (byte)(dec % 100);
        return (byte)(((dec / 10) << 4) | (dec % 10));
    }

    public static bool TestForOverflow(short value) => value is < -128 or > 127;
    public static bool TestForCarry(short value) => value is < 0 or > 255;
    public static byte HexStringToByte(string stringbyte) => byte.Parse(stringbyte, NumberStyles.HexNumber);
}