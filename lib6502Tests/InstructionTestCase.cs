using System.Text.Json.Serialization;

namespace lib6502Tests;

public class InstructionTestCase
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("initial")] public ProcessorState Initial { get; set; }

    [JsonPropertyName("final")] public ProcessorState Final { get; set; }
}