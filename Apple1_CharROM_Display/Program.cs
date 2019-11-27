using System;
using System.IO;
using System.Drawing;

namespace Apple1_CharROM_Display
{
    class MainClass
    {
        private static byte[,] charmap;

        private static void LoadROM()
        {
            FileStream fis = File.OpenRead("apple1.vid");
            charmap = new byte[128, 8];
            for(int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    charmap[i, j] = (byte)fis.ReadByte();
                }
            }
            fis.Close();
            charmap[95, 6] = 63;
        }

        private static bool CheckBit(byte value, int bit) => ((value << (7 - bit)) >> 7) == 1;

        public static void Main(string[] args)
        {
            LoadROM();
            DirectBitmap charbitmap = new DirectBitmap(8, 128 * 8);
            for (int i = 0; i < charmap.GetLength(0); i++)
                for (int j = 0; j < 8; j++)
                    for (int k = 1; k < 8; k++)
                        if ((charmap[i, j] & (1 << k)) == (1 << k))
                            charbitmap.SetPixel(k, i * 8 + j, Color.White);
            charbitmap.Bitmap.Save("charset.bmp");
            charbitmap.Dispose();
        }
    }
}
