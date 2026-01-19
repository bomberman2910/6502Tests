using System;
using System.IO;
using lib6502;

namespace Apple2;

internal class DiskDrive(ushort romAddress, byte[] nibFileContent) : Device((ushort)(0xC080 + (byte)((romAddress << 4) >> 8)), (ushort)(0xC080 + (byte)((romAddress << 4) >> 8) + 0x0F))
{
    private const double OutermostTrack = 34;
    
    private bool[] phaseStates = [false, false, false, false];
    private byte lastActiveState = 0;
    private uint currentPosition = 0;
    private byte dataLatch = 0;
    
    public double CurrentTrack { get; private set; } = OutermostTrack;
    public byte[] NibFileContent { get; set; } = nibFileContent;

    public override void SetData(byte data, ushort address)
    {
        
    }

    public override byte GetData(ushort address)
    {
        if (!Request(address))
            return 0;
        switch(address-Start)
        {
            case 0x000C:
            {
                var imageAddress = (uint)Math.Floor(CurrentTrack * 0x1A00 + currentPosition);
                var readByte = NibFileContent[imageAddress];
                currentPosition++;
                if (currentPosition == 0x1A00)
                    currentPosition = 0;
                //Console.WriteLine("Reading byte {0:X2} from floppy. Current position: T={1} B={2:X4}", readByte, currentTrack, currentPosition % 0x1A00);
                return readByte;
            }
        }

        //Console.WriteLine("Current Track: {0} {1}", currentTrack, string.Join(';', phaseStates));
        
        return 0;
    }

    public override void PerformClockAction(ushort lastReadAddress)
    {
        switch (lastReadAddress - Start)
        {
            case 0x0000:
                phaseStates[0] = false;
                if (phaseStates[1])
                    CurrentTrack += 0.25;
                else if (phaseStates[3])
                    CurrentTrack -= 0.25;
                lastActiveState = 0;
                break;
            case 0x0001:
                phaseStates[0] = true;
                if (phaseStates[1])
                    CurrentTrack -= 0.25;
                else if (phaseStates[3])
                    CurrentTrack += 0.25;
                else if (lastActiveState == 1)
                    CurrentTrack -= 0.5;
                else if (lastActiveState == 3)
                    CurrentTrack += 0.5;
                break;
            case 0x0002:
                phaseStates[1] = false;
                if (phaseStates[0])
                    CurrentTrack -= 0.25;
                else if (phaseStates[2])
                    CurrentTrack += 0.25;
                lastActiveState = 1;
                break;
            case 0x0003:
                phaseStates[1] = true;
                if (phaseStates[0])
                    CurrentTrack += 0.25;
                else if (phaseStates[2])
                    CurrentTrack -= 0.25;
                else if (lastActiveState == 2)
                    CurrentTrack -= 0.5;
                else if (lastActiveState == 0)
                    CurrentTrack += 0.5;
                break;
            case 0x0004:
                phaseStates[2] = false;
                if (phaseStates[1])
                    CurrentTrack -= 0.25;
                else if (phaseStates[3])
                    CurrentTrack += 0.25;
                lastActiveState = 2;
                break;
            case 0x0005:
                phaseStates[2] = true;
                if (phaseStates[1])
                    CurrentTrack += 0.25;
                else if (phaseStates[3])
                    CurrentTrack -= 0.25;
                else if (lastActiveState == 3)
                    CurrentTrack -= 0.5;
                else if (lastActiveState == 1)
                    CurrentTrack += 0.5;
                break;
            case 0x0006:
                phaseStates[3] = false;
                if (phaseStates[2])
                    CurrentTrack -= 0.25;
                else if (phaseStates[0])
                    CurrentTrack += 0.25;
                lastActiveState = 3;
                break;
            case 0x0007:
                phaseStates[3] = true;
                if (phaseStates[2])
                    CurrentTrack += 0.25;
                else if (phaseStates[0])
                    CurrentTrack -= 0.25;
                else if (lastActiveState == 0)
                    CurrentTrack -= 0.5;
                else if (lastActiveState == 2)
                    CurrentTrack += 0.5;
                break;
        }
        
        if (CurrentTrack < 0)
            CurrentTrack = 0;
        if (CurrentTrack > OutermostTrack)
            CurrentTrack = OutermostTrack;
    }
}