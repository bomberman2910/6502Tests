using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using lib6502;

namespace asm6502
{
    internal class MainClass
    {
        private static ByteCode DataToByteCode(string line)
        {
            var type = line.Trim().Split(new[] {' '}, 3)[0].Trim('.');
            var label = line.Trim().Split(new[] {' '}, 3)[1].Trim();
            var data = line.Trim().Split(new[] {' '}, 3)[2].Trim();

            switch (type.ToUpper())
            {
                case "BYTE" when data.Contains(','):
                {
                    var values = data.Split(',');
                    var bytes = new byte[values.Length];
                    if (values.Where((t, i) => !byte.TryParse(t.Trim().Trim('$'), NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture, out bytes[i])).Any())
                        throw new ArgumentException($"Ungültige Daten: {line}");
                    return new ByteCode {Code = bytes, Label = label, Position = 0x0000};
                }
                case "BYTE":
                {
                    if (!byte.TryParse(data.Trim('$'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                        throw new ArgumentException($"Ungültige Daten: {line}");
                    return new ByteCode {Code = new[] {value}, Label = label, Position = 0x0000};
                }
                case "STRING":
                {
                    var bytes = Encoding.ASCII.GetBytes(data.Trim('"'));
                    bytes = bytes.Append((byte) 0x00).ToArray();
                    return new ByteCode {Code = bytes, Label = label, Position = 0x0000};
                }
                default:
                    throw new ArgumentException($"Ungültige Daten: {line}");
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 1 && !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: asm6502.exe [code file]");
                return;
            }

            var codereader = File.OpenText(args[0]);
            var code = codereader.ReadToEnd();
            codereader.Close();

            var lines = (from line in code.Split('\n') where line.Trim().Length > 0 select new CodeLine(line)).ToList();
            lines = (from line in lines where line.Type != CodeLine.Linetype.COMMENT select line).ToList();
            foreach (var line in lines)
                line.Line = line.Clean();

            var labelindex = lines.Where(line => line.Type == CodeLine.Linetype.LABEL).ToDictionary(line => line.Clean().Split(':')[0], _ => -1);

            var datavalues = lines.Where(line => line.Type == CodeLine.Linetype.DIRECTIVE && !line.Line.ToUpper().Contains("ORG")).Select(line => DataToByteCode(line.Line)).ToList();

            var variables = lines.Where(line => line.Type == CodeLine.Linetype.VARIABLE).Select(line => line.Clean().Split('=')).ToDictionary(linesplit => linesplit[0].Trim(), linesplit => linesplit[1].Trim());

            foreach (var key in variables.Keys.ToList().Where(key => variables[key].Contains("$")))
                variables[key] = variables[key].Replace("$", "$$");
            foreach (var line in lines.Where(line => line.Type == CodeLine.Linetype.CODE))
            {
                foreach (var variable in variables.Where(variable => Regex.IsMatch(line.Clean(), $@"\b{variable.Key}\b")))
                    line.Line = Regex.Replace(line.Clean(), $@"\b{variable.Key}\b", variable.Value);
            }

            var bytes = new List<ByteCode>();
            ushort programCounter = 0x0000;

            var origin = lines.Find(line => line.Type == CodeLine.Linetype.DIRECTIVE && line.Line.ToUpper().Contains(".ORG"));
            if (origin != null)
                _ = ushort.TryParse(origin.Line.Trim().Split(' ')[1].Trim('$'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out programCounter);

            Console.WriteLine($"Origin set to ${programCounter:X4}");

            foreach (var codeline in lines)
            {
                switch (codeline.Type)
                {
                    case CodeLine.Linetype.LABEL:
                        labelindex[codeline.Clean().Trim(':')] = programCounter;
                        break;
                    case CodeLine.Linetype.CODE:
                    {
                        var line = codeline.Clean();
                        var containslabel = "";
                        var containsdatalabel = "";
                        foreach (var index in labelindex.Where(index => Regex.IsMatch(line, $@"\b{index.Key}\b")))
                        {
                            containslabel = index.Key;
                            break;
                        }

                        foreach (var data in datavalues.Where(data => Regex.IsMatch(line, $@"\b{data.Label}\b")))
                        {
                            containsdatalabel = data.Label;
                            break;
                        }

                        ByteCode bc;
                        if (!containslabel.Equals(""))
                        {
                            if (labelindex[containslabel] == -1)
                            {
                                if (line.ToUpper().StartsWith("J"))
                                {
                                    bc = new ByteCode
                                    {
                                        Code = Asm6502.Assemble(Regex.Replace(line, $@"\b{containslabel}\b", "$$0000")),
                                        Label = containslabel, Position = programCounter
                                    };
                                }
                                else if (line.ToUpper().StartsWith("B"))
                                {
                                    bc = new ByteCode
                                    {
                                        Code = Asm6502.Assemble(Regex.Replace(line, $@"\b{containslabel}\b", "$$00")),
                                        Label = containslabel, Position = programCounter
                                    };
                                }
                                else
                                    throw new ArgumentException($"Label at illegal position ({line})");
                            }
                            else
                            {
                                if (line.ToUpper().StartsWith("J"))
                                {
                                    bc = new ByteCode
                                    {
                                        Code = Asm6502.Assemble(Regex.Replace(line, $@"\b{containslabel}\b", $"$${labelindex[containslabel]:X4}")),
                                        Label = "", Position = programCounter
                                    };
                                }
                                else if (line.ToUpper().StartsWith("B"))
                                {
                                    bc = new ByteCode
                                    {
                                        Code = Asm6502.Assemble(Regex.Replace(line, $@"\b{containslabel}\b", $"$${(labelindex[containslabel] >= programCounter ? (ushort) (labelindex[containslabel] - programCounter - 2) : (ushort) (0xFE - (ushort) (programCounter - labelindex[containslabel]))):X2}")),
                                        Label = "", Position = programCounter
                                    };
                                }
                                else
                                    throw new ArgumentException($"Label at illegal position ({line})");
                            }
                        }
                        else if (!containsdatalabel.Equals(""))
                        {
                            bc = new ByteCode
                            {
                                Code = Asm6502.Assemble(Regex.Replace(line, $@"\b{containsdatalabel}\b", "$$0000")),
                                Label = containsdatalabel, Position = programCounter
                            };
                        }
                        else
                            bc = new ByteCode {Code = Asm6502.Assemble(line), Label = "", Position = programCounter};

                        programCounter += (ushort) bc.Code.Length;
                        bytes.Add(bc);
                        break;
                    }
                }
            }

            foreach (var data in datavalues)
            {
                foreach (var bc in bytes.Where(bc => bc.Label.Equals(data.Label)))
                {
                    bc.Code[1] = BitConverter.GetBytes(programCounter)[0];
                    bc.Code[2] = BitConverter.GetBytes(programCounter)[1];
                    bc.Label = "";
                    Console.WriteLine($"Data {data.Label} is used at ${programCounter:X4}");
                }

                data.Position = programCounter;
                data.Label = "";
                bytes.Add(data);
                programCounter += (ushort) data.Code.Length;
            }

            foreach (var bc in bytes.Where(bc => !bc.Label.Equals("")))
            {
                switch (bc.Code[0])
                {
                    case 0x10:
                    case 0x30:
                    case 0x50:
                    case 0x70:
                    case 0x90:
                    case 0xB0:
                    case 0xD0:
                    case 0xF0:
                        bc.Code[1] = (byte) (labelindex[bc.Label] >= bc.Position ? labelindex[bc.Label] - bc.Position - 2 : 0xFE - (bc.Position - labelindex[bc.Label]));
                        break;
                    case 0x4C:
                    case 0x6C:
                    case 0x20:
                        bc.Code[1] = BitConverter.GetBytes(labelindex[bc.Label])[0];
                        bc.Code[2] = BitConverter.GetBytes(labelindex[bc.Label])[1];
                        break;
                }
            }

            var bytecodelist = new List<byte>();
            foreach (var bc in bytes)
                bytecodelist.AddRange(bc.Code);

            File.WriteAllBytes(Path.GetFileNameWithoutExtension(args[0]) + ".bin", bytecodelist.ToArray());
            Console.WriteLine($"{code.Split('\n').Length} lines of code were converted to {bytecodelist.ToArray().Length} bytes.");
            foreach (var index in labelindex)
                Console.WriteLine($"Label {index.Key} pointing at location ${index.Value:X4}");
        }

        private class ByteCode
        {
            public byte[] Code { get; set; }
            public string Label { get; set; }
            public ushort Position { get; set; }

            public override string ToString()
            {
                var returnstring = $"{Position:X4}: ";

                Code.ToList().ForEach(b => returnstring += $"{b:X2} ");
                returnstring += DisAsm6502.Disassemble(Code, 0);

                return returnstring;
            }
        }
    }
}