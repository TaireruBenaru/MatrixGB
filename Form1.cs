using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace MatrixGB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void LaunchBootROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CPU cpu = new CPU();
            RAM ram = new RAM();
            ROM rom = new ROM();
            //ram = rom.LoadROM(ram, @"C:\Users\Taireru\source\repos\MatrixGB\DMG_ROM.bin");
            ram = rom.LoadROM(ram, @"C:\Users\Taireru\source\repos\MatrixGB\06-ld r,r.gb");
            cpu.ProgramCounter = 0x100;
            //byte[] ROMBytes = File.ReadAllBytes(@"C:\Users\Taireru\source\repos\MatrixGB\06-ld r,r.gb");
            cpu.RunROM(ram);
        }
    }
}
