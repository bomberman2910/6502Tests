using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace asm6502
{
    class MainClass
    {

        struct ByteCode
        {
            public byte[] code;
            public string label;
            public ushort position;

            public override string ToString()
            {
                string returnstring = $"{position.ToString("X4")}: ";

                code.ToList().ForEach(b => returnstring += $"{b.ToString("X2")} ");
                returnstring += lib6502.DisASM6502.Disassemble(code, 0);

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

            StreamReader codereader = File.OpenText(args[0]);
            string code = codereader.ReadToEnd();
            codereader.Close();

            List<CodeLine> lines = new List<CodeLine>();
            foreach(string line in code.Split('\n'))
                if(line.Trim().Length > 0)
                    lines.Add(new CodeLine(line));
            Dictionary<string, int> labelindex = new Dictionary<string, int>();
            foreach (CodeLine line in lines)
                if (line.Type == CodeLine.Linetype.LABEL)
                    labelindex.Add(line.Line.Split(':')[0], -1);

            List<ByteCode> bytes = new List<ByteCode>();
            ushort PC = 0x0000;

            CodeLine origin = lines.Find(line => (line.Type == CodeLine.Linetype.DIRECTIVE) && line.Line.ToUpper().Contains(".ORG"));
            if(origin != null)
            {
                string[] originsplit = origin.Line.Trim().Split(' ');
                _ = ushort.TryParse(originsplit[1].Trim('$'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out PC);
            }
            Console.WriteLine($"Origin set to ${PC.ToString("X4")}");

            foreach(CodeLine codeline in lines)
            {
                if(codeline.Type == CodeLine.Linetype.LABEL)
                {
                    labelindex[codeline.Line.Split(':')[0]] = PC;
                }
                else if(codeline.Type == CodeLine.Linetype.CODE)
                {
                    string line = codeline.Line.Trim();
                    if (codeline.ContainsComment)
                        line = line.Split(';')[0];
                    string containslabel = "";
                    foreach(var index in labelindex)
                    {
                        if (line.Contains(index.Key))
                        {
                            containslabel = index.Key;
                            break;
                        }

                    }
                    ByteCode bc;
                    if(!containslabel.Equals(""))
                    {
                        if (labelindex[containslabel] == -1)
                        {
                            if (line.ToUpper().StartsWith("J"))
                                bc = new ByteCode() { code = lib6502.ASM6502.Assemble(line.Replace(containslabel, $"${0.ToString("X4")}")), label = containslabel, position = PC };
                            else if(line.ToUpper().StartsWith("B"))
                                bc = new ByteCode() { code = lib6502.ASM6502.Assemble(line.Replace(containslabel, $"${0.ToString("X2")}")), label = containslabel, position = PC };
                            else
                                throw new ArgumentException($"Label at illegal position ({line})");
                        }
                        else
                        {
                            if (line.ToUpper().StartsWith("J"))
                                bc = new ByteCode() { code = lib6502.ASM6502.Assemble(line.Replace(containslabel, $"${labelindex[containslabel].ToString("X4")}")), label = "", position = PC };
                            else if (line.ToUpper().StartsWith("B"))
                                bc = new ByteCode() { code = lib6502.ASM6502.Assemble(line.Replace(containslabel, $"${((labelindex[containslabel] >= PC) ? (ushort)(labelindex[containslabel] - PC - 2) : (ushort)(0xFE - ((ushort)(PC - labelindex[containslabel])))).ToString("X2")}")), label = "", position = PC };
                            else
                                throw new ArgumentException($"Label at illegal position ({line})");
                        }
                    }
                    else
                    {
                        bc = new ByteCode() { code = lib6502.ASM6502.Assemble(line), label = "", position = PC };
                    }
                    PC += (ushort)bc.code.Length;
                    bytes.Add(bc);
                }
            }

            foreach(ByteCode bc in bytes)
            {
                if (!bc.label.Equals(""))
                {
                    if ((bc.code[0] == 0x10) || (bc.code[0] == 0x30) || (bc.code[0] == 0x50) || (bc.code[0] == 0x70) || (bc.code[0] == 0x90) || (bc.code[0] == 0xB0) || (bc.code[0] == 0xD0) || (bc.code[0] == 0xF0))
                    {
                        bc.code[1] = (byte)((labelindex[bc.label] >= bc.position) ? labelindex[bc.label] - bc.position - 2 : 0xFE - (bc.position - labelindex[bc.label]));
                    }
                    else if ((bc.code[0] == 0x4C) || (bc.code[0] == 0x6C) || (bc.code[0] == 0x20))
                    {
                        bc.code[1] = BitConverter.GetBytes(labelindex[bc.label])[0];
                        bc.code[2] = BitConverter.GetBytes(labelindex[bc.label])[1];
                    }
                }
            }

            List<byte> bytecodelist = new List<byte>();
            foreach (ByteCode bc in bytes)
                bytecodelist.AddRange(bc.code);

            File.WriteAllBytes(Path.GetFileNameWithoutExtension(args[0]) + ".bin", bytecodelist.ToArray());
            Console.WriteLine($"{code.Split('\n').Length} lines of code were converted to {bytecodelist.ToArray().Length} bytes.");
            foreach(var index in labelindex)
            {
                Console.WriteLine($"Label {index.Key} pointing at location ${index.Value.ToString("X4")}");
            }
        }
    }
}
