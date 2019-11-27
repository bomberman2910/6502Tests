using System;
using System.Windows.Forms;
using lib6502;
using Emu6502;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace View6502_Win
{
    public partial class Form_Main : Form
    {
        public Form_Main()
        {
            InitializeComponent();
        }

        private CPU6502 cpu;
        private Bus mainbus;
        private RAM ram;
        private ROM rom;
        private TextScreen textscreen;
        private Serialport serialport;

        private DirectBitmap[] chars;

        private void Reset()
        {
            mainbus = new Bus();
            ram = new RAM(4096, 0x0000);
            byte[] bbytes = File.ReadAllBytes("stringex.bin");
            for (int pc = 0; pc < bbytes.Length; pc++)
                ram.SetData(bbytes[pc], (ushort)(0x0200 + pc));
            mainbus.Devices.Add(ram);

            rom = new ROM(4096, 0xF000);
            byte[] initrom = new byte[4096];
            initrom[0x0FFD] = 0x02;
            byte[] textrom = File.ReadAllBytes("textrom.bin");
            for (int pc = 0; pc < textrom.Length; pc++)
                initrom[pc] = textrom[pc];
            rom.SetMemory(initrom);
            mainbus.Devices.Add(rom);

            textscreen = new TextScreen(40, 25, 0xD004);
            textscreen.Reset();
            mainbus.Devices.Add(textscreen);

            serialport = new Serialport(0xD000);
            mainbus.Devices.Add(serialport);

            cpu = new CPU6502(mainbus)
            {
                PC = 0x0200
            };
        }

        private void Form_Main_Load(object sender, EventArgs e)
        {
            Reset();
            UpdateUI();

            for (int line = 0; line < 0x0400; line += 16)
            {
                textBoxMemory.Text += $"${line.ToString("X4")}:";
                for (int cell = line; cell < line + 16; cell++)
                    textBoxMemory.Text += $" ${mainbus.GetData((ushort)cell).ToString("X2")}";
                textBoxMemory.Text += "\r\n";
            }

            byte[,] charmap = new byte[128, 8];
            FileStream fis = File.OpenRead("apple1.vid");
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    charmap[i, j] = (byte)fis.ReadByte();
                }
            }
            fis.Close();
            charmap[95, 6] = 63;
            chars = new DirectBitmap[128];
            for (int i = 0; i < 128; i++)
            {
                chars[i] = new DirectBitmap(8, 8);
                for (int j = 0; j < 8; j++)
                    for (int k = 1; k < 8; k++)
                        if ((charmap[i, j] & (1 << k)) == (1 << k))
                            chars[i].SetPixel(k, j, Color.White);
            }
            pictureBoxChar.Image = chars[(int)numericUpDownPosValue.Value].Bitmap;
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            if (mainbus.GetData(cpu.PC) != 0x00)
            {
                cpu.Exec();
                mainbus.PerformClockActions();
            }
            else
            {
                timerClock.Stop();
                textscreen.Screenshot();
            }
            if (cpu.Cycles == 0)
                UpdateUI();
        }

        private void UpdateUI()
        {
            labelAValue.Text = $"${cpu.A.ToString("X2")}";
            labelXValue.Text = $"${cpu.X.ToString("X2")}";
            labelYValue.Text = $"${cpu.Y.ToString("X2")}";
            labelPCValue.Text = $"${cpu.PC.ToString("X4")}";
            labelSPValue.Text = $"${cpu.SP.ToString("X2")}";
            labelSRValue.Text = $"${cpu.SR.ToString("X2")}";

            pictureBoxScreen.Image = textscreen.bitmap_screen.Bitmap;

            labelCurInst.Text = "Current Instruction: " + DisASM6502.Disassemble(new byte[] { mainbus.GetData(cpu.PC), mainbus.GetData((ushort)(cpu.PC + 1)), mainbus.GetData((ushort)(cpu.PC + 2)) }, 0);
        }

        private void buttonCycle_Click(object sender, EventArgs e)
        {
            cpu.Exec();
            mainbus.PerformClockActions();
            UpdateUI();
        }

        private void buttonStep_Click(object sender, EventArgs e)
        {
            cpu.Step();
            mainbus.PerformClockActions();
            UpdateUI();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            Reset();
            UpdateUI();
        }

        private void buttonStart_Click(object sender, EventArgs e) => timerClock.Start();

        private void buttonStop_Click(object sender, EventArgs e) => timerClock.Stop();

        private void buttonMemStartUpdate_Click(object sender, EventArgs e)
        {
            StringBuilder memtextbuilder = new StringBuilder();
            for(int line = (int)numericUpDownMemStartValue.Value; line < numericUpDownMemStartValue.Value + 0x0400; line +=16)
            {
                memtextbuilder.Append($"${line.ToString("X4")}:");
                for (int cell = line; cell < line + 16; cell++)
                    memtextbuilder.Append($" ${mainbus.GetData((ushort)cell).ToString("X2")}");
                memtextbuilder.AppendLine();
            }
            textBoxMemory.Text = memtextbuilder.ToString();
        }

        private void numericUpDownPosValue_ValueChanged(object sender, EventArgs e) => pictureBoxChar.Image = chars[(int)numericUpDownPosValue.Value].Bitmap;

        private void pictureBoxChar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(
               chars[(int)numericUpDownPosValue.Value].Bitmap,
                new Rectangle(0, 0, pictureBoxChar.Width, pictureBoxChar.Height),
                // destination rectangle 
                0,
                0,           // upper-left corner of source rectangle
                chars[(int)numericUpDownPosValue.Value].Bitmap.Width,       // width of source rectangle
                chars[(int)numericUpDownPosValue.Value].Bitmap.Height,      // height of source rectangle
                GraphicsUnit.Pixel);
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if(!textBoxTerminal.Text.Equals(""))
            {
                serialport.Send(textBoxTerminal.Text);
            }
        }

        private void buttonMemChange_Click(object sender, EventArgs e) => mainbus.SetData((byte)numericUpDownMemChangeValue.Value, (ushort)numericUpDownMemChangeAddress.Value);

        private void buttonMemLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { CheckFileExists = true, CheckPathExists = true, Multiselect = false, Title = "Load Memory" };
            ofd.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                byte[] memfile = File.ReadAllBytes(ofd.FileName);
                for (ushort pc = (ushort)numericUpDownMemChangeAddress.Value; pc < ((numericUpDownMemChangeAddress.Value + memfile.Length) < 65536 ? (ushort)(numericUpDownMemChangeAddress.Value + memfile.Length) : 0xFFFF); pc++)
                    mainbus.SetData(memfile[pc - (int)numericUpDownMemChangeAddress.Value], pc);
                cpu.PC = (ushort)numericUpDownMemChangeAddress.Value;
            }
        }
    }
}
