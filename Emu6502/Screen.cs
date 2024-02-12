﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;
using lib6502;
using SDL2;
using static SDL2.SDL;

namespace Emu6502
{
    public class Screen : Device
    {
        public DirectBitmap BitmapScreen;

        public Screen(int width, int height, ushort startAddress) : base(startAddress, (ushort)(startAddress + 3))
        {
            BitmapScreen = new DirectBitmap(width, height);
            Memory[3] = 0x01;
        }

        public override byte GetData(ushort address) => Request(address) && address == End ? Memory[3] : (byte)0x00;

        public override void PerformClockAction()
        {
            if (Memory[3] != 0x02)
                return;
            lock(MainClass.frameBufferLock)
                BitmapScreen.SetPixel(Memory[0], Memory[1], Color.FromArgb(0xFF + (0xFF << 8) + (0xFF << 16) + (Memory[2] << 24)));
            Memory[3] = 0x01;
        }

        public override void SetData(byte data, ushort address)
        {
            if (Request(address))
                Memory[address - Start] = data;
        }

        public void Screenshot()
        {
            lock(MainClass.frameBufferLock)
                BitmapScreen.Bitmap.Save("screen.bmp");
        }

        public void Reset()
        {
            lock (MainClass.frameBufferLock)
            {
                for (var y = 0; y < BitmapScreen.Height; y++)
                {
                    for (var x = 0; x < BitmapScreen.Width; x++)
                        BitmapScreen.SetPixel(x, y, Color.Black);
                }
            }

            Screenshot();
        }
    }
}