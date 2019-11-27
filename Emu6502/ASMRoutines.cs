using System;
using lib6502;

namespace Emu6502
{
    public static class ASMRoutines
    {

        public static byte[] CharDspRoutine() => ASM6502.Assemble("LDA $D007\nCMP #$01\nBNE $F9\nLDA $00\nSTA $D004\nLDA $01\nSTA $D005\nLDA $02\nSTA $D006\nLDA #$02\nSTA $D007\nRTS");

        public static byte[] PixelDspRoutine() => ASM6502.Assemble("LDA $D003\nCMP #$01\nBNE $F9\nLDA $00\nSTA $D000\nLDA $01\nSTA $D001\nLDA $02\nSTA $D002\nLDA #$02\nSTA $D003\nRTS");

    }
}
