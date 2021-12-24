using System.Drawing;
using System.IO;

namespace Emu6502
{
    public class TextScreen : Screen
    {
        private readonly byte[,] charmap;

        public TextScreen(byte width, byte height, ushort start_address) : base(width * 8, height * 8, start_address)
        {
            var fis = File.OpenRead("apple1.vid");
            charmap = new byte[128, 8];
            for (var i = 0; i < 128; i++)
            {
                for (var j = 0; j < 8; j++)
                    charmap[i, j] = (byte) fis.ReadByte();
            }

            fis.Close();
            charmap[95, 6] = 63;
        }

        public override void PerformClockAction()
        {
            if (Memory[3] != 0x02)
                return;
            for (var j = 0; j < 8; j++)
            {
                for (var k = 1; k < 8; k++)
                {
                    if ((charmap[Memory[2], j] & (1 << k)) == 1 << k)
                        BitmapScreen.SetPixel(Memory[0] * 8 + k, Memory[1] * 8 + j, Color.White);
                }
            }

            Memory[3] = 0x01;
        }

        public new void Screenshot()
        {
            BitmapScreen.Bitmap.Save("char_screen.bmp");
        }
    }
}