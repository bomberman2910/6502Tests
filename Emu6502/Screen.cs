using lib6502;
using System.Drawing;

namespace Emu6502
{
    public class Screen : Device
    {
        public DirectBitmap bitmap_screen;

        public Screen(int width, int height, ushort start_address) : base(start_address, (ushort)(start_address + 3))
        {
            bitmap_screen = new DirectBitmap(width, height);
            memory[3] = 0x01;
        }

        public override byte GetData(ushort address) => Request(address) && (address == end) ? memory[3] : (byte)0x00;

        public override void PerformClockAction()
        {
            if (memory[3] == 0x02)
            {
                bitmap_screen.SetPixel(memory[0], memory[1], Color.FromArgb(0xFF + (0xFF << 8) + (0xFF << 16) + (memory[2] << 24)));
                memory[3] = 0x01;
            }
        }

        public override void SetData(byte data, ushort address)
        {
            if (Request(address))
                memory[address - start] = data;
        }

        public void Screenshot()
        {
            bitmap_screen.Bitmap.Save("screen.bmp");
        }

        public void Reset()
        {
            for (int y = 0; y < bitmap_screen.Height; y++)
                for (int x = 0; x < bitmap_screen.Width; x++)
                    bitmap_screen.SetPixel(x, y, Color.Black);
            Screenshot();
        }
    }
}
