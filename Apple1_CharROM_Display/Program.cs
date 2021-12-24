using System.Drawing;
using System.IO;

namespace Apple1_CharROM_Display
{
    internal class MainClass
    {
        private static byte[,] _charmap;

        private static void LoadRom()
        {
            var fis = File.OpenRead("apple1.vid");
            _charmap = new byte[128, 8];
            for (var i = 0; i < 128; i++)
            {
                for (var j = 0; j < 8; j++)
                    _charmap[i, j] = (byte) fis.ReadByte();
            }

            fis.Close();
            _charmap[95, 6] = 63;
        }

        public static void Main(string[] args)
        {
            LoadRom();
            var charbitmap = new DirectBitmap(8, 128 * 8);
            for (var i = 0; i < _charmap.GetLength(0); i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    for (var k = 1; k < 8; k++)
                    {
                        if ((_charmap[i, j] & (1 << k)) == 1 << k)
                            charbitmap.SetPixel(k, i * 8 + j, Color.White);
                    }
                }
            }

            charbitmap.Bitmap.Save("charset.bmp");
            charbitmap.Dispose();
        }
    }
}