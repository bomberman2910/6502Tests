using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace asm6502
{
    class MainClass
    {
        private class ByteCode
        {
            public byte[] Code;
            public string Label;
            public ushort Position;

            public override string ToString()
            {
                var returnstring = $"{Position:X4}: ";

                Code.ToList().ForEach(b => returnstring += $"{b:X2} ");
                returnstring += lib6502.DisASM6502.Disassemble(Code, 0);

                return returnstring;
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0 || ((args.Length > 1) && !File.Exists(args[0])))
            {
                Console.WriteLine("Usage: asm6502.exe [code file]");
                return;
            }

            var codereader = File.OpenText(args[0]);
            var code = codereader.ReadToEnd();
            codereader.Close();

            List<CodeLine> lines = (from line in code.Split('\n') where line.Trim().Length > 0 select new CodeLine(line)).ToList();
            var labelindex = lines.Where(line => line.Type == CodeLine.Linetype.LABEL).ToDictionary(line => line.Line.Split(':')[0], line => -1);
            var datavalues = (from line in lines where line.Type == CodeLine.Linetype.DIRECTIVE && line.Line.ToUpper().Contains("BYTE") select line.Line.Trim().Split(' ')).ToDictionary(linesplit => linesplit[1], linesplit => int.Parse(linesplit[2].Trim('$'), NumberStyles.HexNumber));

            var bytes = new List<ByteCode>();
            ushort PC = 0x0000;

            var origin = lines.Find(line => (line.Type == CodeLine.Linetype.DIRECTIVE) && line.Line.ToUpper().Contains(".ORG"));
            if(origin != null)
            {
                var originsplit = origin.Line.Trim().Split(' ');
                _ = ushort.TryParse(originsplit[1].Trim('$'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out PC);
            }
            Console.WriteLine($"Origin set to ${PC:X4}");

            foreach(var codeline in lines)
            {
                if(codeline.Type == CodeLine.Linetype.LABEL)
                {
                    labelindex[codeline.Line.Split(':')[0]] = PC;
                }
                else if(codeline.Type == CodeLine.Linetype.CODE)
                {
                    var line = codeline.Line.Trim();
                    if (codeline.ContainsComment)
                        line = line.Split(';')[0];
                    var containslabel = "";
                    var containsdatalabel = "";
                    foreach (var index in labelindex.Where(index => line.Contains(index.Key)))
                    {
                        containslabel = index.Key;
                        break;
                    }
                    foreach (var data in datavalues.Where(data => line.Contains(data.Key)))
                    {
                        containsdatalabel = data.Key;
                        break;
                    }
                    ByteCode bc;
                    if(!containslabel.Equals(""))
                    {
                        if (labelindex[containslabel] == -1)
                        {
                            if (line.ToUpper().StartsWith("J"))
                                bc = new ByteCode() { Code = lib6502.ASM6502.Assemble(line.Replace(containslabel, $"$0000")), Label = containslabel, Position = PC };
                            else if(line.ToUpper().StartsWith("B"))
                                bc = new ByteCode() { Code = lib6502.ASM6502.Assemble(line.Replace(containslabel, $"$00")), Label = containslabel, Position = PC };
                            else
                                throw new ArgumentException($"Label at illegal position ({line})");
                        }
                        else
                        {
                            if (line.ToUpper().StartsWith("J"))
                                bc = new ByteCode() { Code = lib6502.ASM6502.Assemble(line.Replace(containslabel, $"${labelindex[containslabel]:X4}")), Label = "", Position = PC };
                            else if (line.ToUpper().StartsWith("B"))
                                bc = new ByteCode() { Code = lib6502.ASM6502.Assemble(line.Replace(containslabel, $"${((labelindex[containslabel] >= PC) ? (ushort)(labelindex[containslabel] - PC - 2) : (ushort)(0xFE - ((ushort)(PC - labelindex[containslabel])))):X2}")), Label = "", Position = PC };
                            else
                                throw new ArgumentException($"Label at illegal position ({line})");
                        }
                    }
                    else if(!containsdatalabel.Equals(""))
                    {
                        bc = new ByteCode() { Code = lib6502.ASM6502.Assemble(line.Replace(containsdatalabel, $"$0000")), Label = containsdatalabel, Position = PC };
                    }
                    else
                    {
                        bc = new ByteCode() { Code = lib6502.ASM6502.Assemble(line), Label = "", Position = PC };
                    }
                    PC += (ushort)bc.Code.Length;
                    bytes.Add(bc);
                }
            }

            foreach(var data in datavalues)
            {
                bytes.Add(new ByteCode() { Code = new[] { (byte)data.Value }, Label = "", Position = PC });
                foreach (var bc in bytes.Where(bc => bc.Label.Equals(data.Key)))
                {
                    bc.Code[1] = BitConverter.GetBytes(PC)[0];
                    bc.Code[2] = BitConverter.GetBytes(PC)[1];
                    bc.Label = "";
                    Console.WriteLine($"Data {data.Key} (${data.Value:X2}) is used at ${PC:X4}");
                }
                PC++;
            }

            foreach (var bc in bytes.Where(bc => !bc.Label.Equals("")))
            {
                if ((bc.Code[0] == 0x10) || (bc.Code[0] == 0x30) || (bc.Code[0] == 0x50) || (bc.Code[0] == 0x70) || (bc.Code[0] == 0x90) || (bc.Code[0] == 0xB0) || (bc.Code[0] == 0xD0) || (bc.Code[0] == 0xF0))
                {
                    bc.Code[1] = (byte)((labelindex[bc.Label] >= bc.Position) ? labelindex[bc.Label] - bc.Position - 2 : 0xFE - (bc.Position - labelindex[bc.Label]));
                }
                else if ((bc.Code[0] == 0x4C) || (bc.Code[0] == 0x6C) || (bc.Code[0] == 0x20))
                {
                    bc.Code[1] = BitConverter.GetBytes(labelindex[bc.Label])[0];
                    bc.Code[2] = BitConverter.GetBytes(labelindex[bc.Label])[1];
                }
            }

            var bytecodelist = new List<byte>();
            foreach (var bc in bytes)
                bytecodelist.AddRange(bc.Code);

            File.WriteAllBytes(Path.GetFileNameWithoutExtension(args[0]) + ".bin", bytecodelist.ToArray());
            Console.WriteLine($"{code.Split('\n').Length} lines of code were converted to {bytecodelist.ToArray().Length} bytes.");
            foreach(var index in labelindex)
            {
                Console.WriteLine($"Label {index.Key} pointing at location ${index.Value:X4}");
            }
        }
    }
}
