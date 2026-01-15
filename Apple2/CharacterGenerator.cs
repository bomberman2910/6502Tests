using System.IO;

namespace Apple2;

internal class CharacterGenerator
{
    private readonly byte[][] characterData;

    public bool IsBlinkInversed { get; set; } = false;

    public CharacterGenerator()
    {
        characterData = new byte[64][];
        
        using var characterStream = File.OpenRead(Path.Combine(Program.RomLocation, Program.SystemCharRom));
        characterStream.Seek(0x20 * 8, SeekOrigin.Begin);
        var i = 0;
        while (i < 32)
        {
            var character = new byte[8];
            characterStream.ReadExactly(character, 0, 8);
            characterData[i + 0x20] = character;
            i++;
        }

        while (i < 64)
        {
            var character = new byte[8];
            characterStream.ReadExactly(character, 0, 8);
            characterData[i - 0x20] = character;
            i++;
        }
    }

    public byte[] GetCharacter(byte characterCode)
    {
        var character = characterData[characterCode % 64].Clone() as byte[];
        if(characterCode < 64 || (characterCode is >= 64 and < 128 && IsBlinkInversed))
        {
            for (var i = 0; i < 8; i++)
                character[i] = (byte)~character[i];
        }
        return character;
    }
}