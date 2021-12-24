using System;

namespace asm6502v2
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var number = 1;
            ushort address = 0x0000;

            while (true)
            {
                Console.WriteLine("Enter a line of assembler code:");
                var line = new CodeLine(Console.ReadLine(), 1, 0x0000);
                Console.WriteLine($"\n{line.ToString(CodeLine.StringType.LinkerMode)}");
                Console.WriteLine("========================================");
                Console.WriteLine($".Content:\t\t{line.Content}");
                Console.WriteLine($".CleanContent:\t\t{line.CleanContent}");
                Console.WriteLine($".ContainsComment:\t{line.ContainsComment}");
                Console.WriteLine($".IsCommentLine:\t\t{line.IsCommentLine}");
                Console.WriteLine($".ContainsLabel:\t\t{line.ContainsLabel}");
                Console.WriteLine(line.Label == null ? ".Label:\t\t\tNo Label available" : $".Label:\t\t\t{line.Label?.Name}");
                Console.WriteLine($".IsDataLine:\t\t{line.IsDataLine}");
                if (line.Data == null)
                    Console.WriteLine(".Data:\t\t\tNo Data available");
                else
                {
                    Console.Write($".Data:\t\t\t[ {line.Data[0]:X2}");
                    for (var i = 1; i < line.Data.Length; i++)
                        Console.Write($", {line.Data[i]:X2}");
                }

                Console.WriteLine(" ]");

                Console.ReadKey(true);
                number++;
                address += (ushort) line.Length;
            }
        }
    }
}