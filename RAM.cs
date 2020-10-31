using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixGB
{
    public class RAM
    {
        public byte[] Memory = new byte[0xFFFF+1];
        public byte TIMA
        {
            get { return Memory[0xFF05]; }
            set { Memory[0xFF05] = value; }
        }
        public byte TMA
        {
            get { return Memory[0xFF06]; }
            set { Memory[0xFF06] = value; }
        }
        public byte TMC
        {
            get { return Memory[0xFF07]; }
            set { Memory[0xFF07] = value; }
        }
        string SerialOutput;

        public byte GetByte(int Position)
        {
            //Gets byte at position
            return Memory[Position];
        }

        public UInt16 GetByte16(UInt16 Position)
        {
            //Gets byte at position
            byte[] Value = { Memory[Position], Memory[Position + 1] };
            UInt16 ByteValue = BitConverter.ToUInt16(Value, 0);
            return ByteValue;
        }

        public void SetByte(int Position, byte Value)
        {
            if (Memory[0xff02] != 0x00)
            {
                char c = (char)Memory[0xff01];
                Debug.WriteLine(Memory[0xff01]);
                SerialOutput += c;
                Debug.WriteLine(c);
                Memory[0xff02] = 0x00;
            }

            //Sets byte at position
            Memory[Position] = Value;
            return;
        }


    }
}
