using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using lib6502;
using NUnit.Framework;

namespace lib6502Tests;

[Parallelizable(ParallelScope.All)]
public class Test2X
{
    [Parallelizable(ParallelScope.All)]
    [TestCase("20")]
    [TestCase("21")]
    [TestCase("24")]
    [TestCase("25")]
    [TestCase("26")]
    [TestCase("28")]
    [TestCase("29")]
    [TestCase("2a")]
    [TestCase("2c")]
    [TestCase("2d")]
    [TestCase("2e")]
    public void Test(string opcode)
    {
        var testDataStream = new HttpClient()
            .GetStreamAsync(
                $"https://raw.githubusercontent.com/SingleStepTests/65x02/refs/heads/main/6502/v1/{opcode}.json")
            .GetAwaiter().GetResult();
        var testData = JsonSerializer.Deserialize<List<InstructionTestCase>>(testDataStream);
        Assert.Multiple(() =>
        {
            foreach (var testCase in testData)
            {
                var bus = new Bus();
                var cpu = new Cpu6502(bus);
                bus.Devices.Add(new RandomAccessMemory(0x8000, 0x0000));
                bus.Devices.Add(new RandomAccessMemory(0x8000, 0x8000));
                foreach (var cell in testCase.Initial.Ram)
                    bus.SetData((byte)cell[1], cell[0]);
                cpu.A = testCase.Initial.A;
                cpu.X = testCase.Initial.X;
                cpu.Y = testCase.Initial.Y;
                cpu.StatusRegister = testCase.Initial.Status;
                cpu.StackPointer = testCase.Initial.StackPointer;
                cpu.ProgramCounter = testCase.Initial.ProgramCounter;

                cpu.Step();

                Assert.Multiple(() =>
                {
                    Assert.That(cpu.A, Is.EqualTo(testCase.Final.A), () => $"Register A in {testCase.Name}");
                    Assert.That(cpu.X, Is.EqualTo(testCase.Final.X), () => $"Register X in {testCase.Name}");
                    Assert.That(cpu.Y, Is.EqualTo(testCase.Final.Y), () => $"Register Y in {testCase.Name}");
                    Assert.That(cpu.StatusRegister, Is.EqualTo(testCase.Final.Status), () => $"Status Register in {testCase.Name} is {cpu.StatusRegister:B8}, but should be {testCase.Final.Status:B8}");
                    Assert.That(cpu.StackPointer, Is.EqualTo(testCase.Final.StackPointer), () => $"Stack Pointer in {testCase.Name}");
                    Assert.That(cpu.ProgramCounter, Is.EqualTo(testCase.Final.ProgramCounter), () => $"Program Counter in {testCase.Name}");
                    foreach (var cell in testCase.Final.Ram)
                        Assert.That(bus.GetData(cell[0]), Is.EqualTo((byte)cell[1]), () => $"RAM cell {cell[0]} in {testCase.Name}");
                });
            }
        });
    }
}