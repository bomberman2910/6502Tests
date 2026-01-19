using System;
using lib6502;
using SDL2;

namespace Apple2;

internal class SoundRegister() : Device(0xC030, 0xC030)
{
    public override void SetData(byte data, ushort address)
    {
        if(Request(address))
        {
            Click();
        }
    }

    private static unsafe void Click()
    {
        const int sampleCount = 1;
        var samples = new short[sampleCount];
        for (var i = 0; i < samples.Length; i++) 
            samples[i] = 30000;
        fixed (short* buffer = samples)
        {
            var bufferPointer = new IntPtr(buffer);
            var sdlQueueAudio = SDL.SDL_QueueAudio(Program.AudioDeviceId, bufferPointer, sampleCount * sizeof(short));
            if (sdlQueueAudio != 0)
                Console.WriteLine($"Could not queue audio: {SDL.SDL_GetError()}");
        }
    }

    public override byte GetData(ushort address)
    {
        if (Request(address))
            Click();
        return 0;
    }

    public override void PerformClockAction(ushort lastReadAddress)
    {
    }
}