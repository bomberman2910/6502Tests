using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace lib6502Tests;

public class ProcessorState
{
    [JsonPropertyName("pc")] public ushort ProgramCounter { get; set; }

    [JsonPropertyName("s")] public byte StackPointer { get; set; }

    [JsonPropertyName("a")] public byte A { get; set; }

    [JsonPropertyName("x")] public byte X { get; set; }

    [JsonPropertyName("y")] public byte Y { get; set; }

    [JsonPropertyName("p")] public byte Status { get; set; }

    [JsonPropertyName("ram")] public List<ushort[]> Ram { get; set; }
}