using lib6502;

namespace Emu6502
{
    public static class AsmRoutines
    {
        public static byte[] CharDspRoutine() => Asm6502.Assemble("LDA $D013\nCMP #$01\nBNE $F9\nLDA $00\nSTA $D010\nLDA $01\nSTA $D011\nLDA $02\nSTA $D012\nLDA #$02\nSTA $D013\nRTS");

        public static byte[] PixelDspRoutine() => Asm6502.Assemble("LDA $D003\nCMP #$01\nBNE $F9\nLDA $00\nSTA $D000\nLDA $01\nSTA $D001\nLDA $02\nSTA $D002\nLDA #$02\nSTA $D003\nRTS");

        public static byte[] TestRoutine() => Asm6502.Assemble("LDA #$50\nSTA $00\nLDA #$3C\nSTA $01\nLDA #$FF\nSTA $02\nJSR $F000");
    }
}