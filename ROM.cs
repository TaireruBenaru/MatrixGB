using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixGB
{
    public class ROM
    {
        public byte[] CurrentROM;
        public byte CurrentROMBank = 1;

        bool ROMOnly;
        bool IsMBC1;
        bool IsMBC2;
        bool IsMBC3;

        public RAM LoadROM(RAM ram, string Path)
        {
            CurrentROM = File.ReadAllBytes(Path);
            switch(CurrentROM[0x147])
            {
                default:
                    throw new NotImplementedException("Cartridge Type: " + CurrentROM[0x147] + " is not implemented!!!");
                case 0:
                    ROMOnly = true;
                    break;
                case 1:
                    IsMBC1 = true;
                    break;
                case 2:
                    IsMBC1 = true;
                    break;
                case 3:
                    IsMBC1 = true;
                    break;
                case 5:
                    IsMBC2 = true;
                    break;
                case 6:
                    IsMBC2 = true;
                    break;
            }

            Array.Copy(CurrentROM, 0, ram.Memory, 0, 0x7FFF);
            return ram;
        }

    }
}
