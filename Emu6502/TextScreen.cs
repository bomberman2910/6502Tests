using System.Drawing;
using System.IO;

namespace Emu6502
{
    public class TextScreen : Screen
    {

        private byte[,] charmap;

        public TextScreen(byte width, byte height, ushort start_address) : base((width * 8), (height * 8), start_address)
        {

            FileStream fis = File.OpenRead("apple1.vid");
            charmap = new byte[128, 8];
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    charmap[i, j] = (byte)fis.ReadByte();
                }
            }
            fis.Close();
            charmap[95, 6] = 63;
        }

        public override void PerformClockAction()
        {
            if (memory[3] == 0x02)
            {
                for (int j = 0; j < 8; j++)
                    for (int k = 1; k < 8; k++)
                        if ((charmap[memory[2], j] & (1 << k)) == (1 << k))
                            bitmap_screen.SetPixel(memory[0] * 8 + k, memory[1] * 8 + j, Color.White);
                memory[3] = 0x01;
            }
        }

        public new void Screenshot()
        {
            bitmap_screen.Bitmap.Save("char_screen.bmp");
        }
    }
}
