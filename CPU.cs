using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixGB
{
    public class CPU
    {
        //Registers
        public byte A;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte F;
        public byte H;
        public byte L;
        public UInt16 AF
        {
            get { return (UInt16)(A << 8 | F); }
            set { A = (byte)(value >> 8); F = (byte)(value & 0xFF); statusFlags = (StatusFlags)F; }
        }
        public UInt16 BC
        {
            get { return (UInt16)(B << 8 | C); }
            set { B = (byte)(value >> 8); C = (byte)(value & 0xFF); }
        }
        public UInt16 DE
        {
            get { return (UInt16)(D << 8 | E); }
            set { D = (byte)(value >> 8); E = (byte)(value & 0xFF); }
        }
        public UInt16 HL
        {
            get { return (UInt16)(H << 8 | L); }
            set { H = (byte)(value >> 8); L = (byte)(value & 0xFF); }
        }

        public UInt16 ProgramCounter;
        public UInt16 StackPointer;

        public StatusFlags statusFlags;

        bool IME;
        bool HALT;


        public byte InstructionLength;

        public bool IsRunning;
        public const int MaxCycles = 69905;
        public int CyclesPassed;
        public int Cycles;
        public byte CurrentByte;

        RAM ram;

        [Flags]
        public enum StatusFlags
        {
            Carry = (1 << 4),
            HalfCarry = (1 << 5),
            Negative = (1 << 6),
            Zero = (1 << 7)
        };

        public void RunROM(RAM rAM)
        {
            ram = rAM;
            IsRunning = true;
            PowerSequence();
            while (IsRunning)
            {
                //Evaluate byte as instruction
                while (CyclesPassed < MaxCycles)
                {
                    //Get byte at Program Counter
                    CurrentByte = ram.GetByte(ProgramCounter);
                    string RegisterText = String.Format("A: {2} F: {7} B: {3} C: {4} D: {5} E: {6} H: {8} L: {9} SP: {1} PC: 00:{0} ({10} {11} {12} {13})", ProgramCounter.ToString("X4"), StackPointer.ToString("X4"), A.ToString("X2"), B.ToString("X2"), C.ToString("X2"), D.ToString("X2"), E.ToString("X2"), F.ToString("X2"), H.ToString("X2"), L.ToString("X2"), ram.Memory[ProgramCounter].ToString("X2"), ram.Memory[ProgramCounter + 1].ToString("X2"), ram.Memory[ProgramCounter + 2].ToString("X2"), ram.Memory[ProgramCounter + 3].ToString("X2"));
                    Debug.WriteLine(RegisterText);
                    Opcode();
                    ProgramCounter += InstructionLength;
                    CyclesPassed += Cycles;
                    //UpdateTimer();
                }
                CyclesPassed = 0;  
                //RenderScreen();
            }
        }

        public void PowerSequence()
        {
            A = 0x01;
            F = 0xB0;
            statusFlags = (StatusFlags)F;  
            B = 0x00;
            C = 0x13;
            D = 0x00;
            E = 0xD8;
            H = 0x01;
            L = 0x4D;
            StackPointer = 0xFFFE;

            ram.SetByte(0xFF10, 0x80); //NR10
            ram.SetByte(0xFF11, 0xBF); //NR11
            ram.SetByte(0xFF12, 0xF3); //NR12
            ram.SetByte(0xFF14, 0xBF); //NR14
            ram.SetByte(0xFF16, 0x3F); //NR21
            ram.SetByte(0xFF19, 0xBF); //NR24
            ram.SetByte(0xFF1A, 0x7F); //NR30
            ram.SetByte(0xFF1B, 0xFF); //NR31
            ram.SetByte(0xFF1C, 0x9F); //NR32
            ram.SetByte(0xFF1E, 0xBF); //NR33
            ram.SetByte(0xFF20, 0xFF); //NR41
            ram.SetByte(0xFF23, 0xBF); //NR30
            ram.SetByte(0xFF24, 0x77); //NR50
            ram.SetByte(0xFF25, 0xF3); //NR51
            ram.SetByte(0xFF26, 0xF1); //NR42
            ram.SetByte(0xFF40, 0x91); //LCDC
            ram.SetByte(0xFF47, 0xFC); //BGP
            ram.SetByte(0xFF48, 0xFF); //OBP0
            ram.SetByte(0xFF49, 0xFF); //OBP1

        }


        void Opcode()
        {
            switch(CurrentByte)
            {
                default:
                    //Debug.WriteLine("Opcode not implemented...");
                      throw new NotImplementedException("Opcode not " + CurrentByte + " is not implemented!!!");

                case 0x00:
                    Cycles = 4;
                    NOPInstruction();
                    break;
                case 0x01:
                    //Debug.WriteLine("LD BC,u16 - Identifier byte: " + CurrentByte);
                    Cycles = 12;
                    BC = LD_r16u16();
                    break;
                case 0x02:
                    Cycles = 8;
                    LD_r16r8(BC, A);
                    break;
                case 0x03:
                    Cycles = 8;
                    BC = INC_r16(BC);
                    break;
                case 0x04:
                    Cycles = 4;
                    //Debug.WriteLine("INC B - Identifier byte: " + CurrentByte);
                    B = INC_r8(B);
                    break;
                case 0x05:
                    Cycles = 4;
                    //Debug.WriteLine("DEC B - Identifier byte: " + CurrentByte);
                    B = DEC_r8(B);
                    break;
                case 0x06:
                    //Debug.WriteLine("LD B,u8 - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    B = LD_u8();
                    break;
                case 0x08:
                    Cycles = 20;
                    InstructionLength = 3;
                    StackPointer = ram.GetByte16((ushort)(ProgramCounter + 1));
                    break;
                case 0x09:
                    Cycles = 8;
                    ADDr16(BC);
                    break;
                case 0x0A:
                    //Debug.WriteLine("LD A,(BC) - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    A = LD_r8r16(BC);
                    break;
                case 0x0B:
                    Cycles = 8;
                    BC = DEC_r16(BC);
                    break;
                case 0x0C:
                    //Debug.WriteLine("INC C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = INC_r8(C);
                    break;
                case 0x0D:
                    //Debug.WriteLine("DEC C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = DEC_r8(C);
                    break;
                case 0x0E:
                    //Debug.WriteLine("LD C,u8 - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    C = LD_u8();
                    break;
                case 0x11:
                    //Debug.WriteLine("LD DE,u16 - Identifier byte: " + CurrentByte);
                    Cycles = 12;
                    DE = LD_r16u16();
                    break;
                case 0x12:
                    Cycles = 8;
                    LD_r16r8(DE, A);
                    break;
                case 0x13:
                    Cycles = 8;
                    DE = INC_r16(DE);
                    break;
                case 0x14:
                    //Debug.WriteLine("INC D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = INC_r8(D);
                    break;
                case 0x15:
                    //Debug.WriteLine("DEC D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = DEC_r8(D);
                    break;
                case 0x16:
                    //Debug.WriteLine("LD D,u8 - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    D = LD_u8();
                    break;
                case 0x17:
                    Cycles = 4;
                    RLA();
                    break;
                case 0x18:
                    Cycles = 12;
                    JR_u8Instruction();
                    break;
                case 0x19:
                    Cycles = 8;
                    ADDr16(DE);
                    break;
                case 0x1A:
                    //Debug.WriteLine("LD A,(DE) - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    A = LD_r8r16(DE);
                    break;
                case 0x1B:
                    Cycles = 8;
                    DE = DEC_r16(DE);
                    break;
                case 0x1C:
                    //Debug.WriteLine("INC E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = INC_r8(E);
                    break;
                case 0x1D:
                    //Debug.WriteLine("DEC E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = DEC_r8(E);
                    break;
                case 0x1E:
                    //Debug.WriteLine("LD E,u8 - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    E = LD_u8();
                    break;
                case 0x1F:
                    Cycles = 4;
                    RRA();
                    break;
                case 0x20:
                    JR_NFu8(StatusFlags.Zero);
                    break;
                case 0x21:
                    //Debug.WriteLine("LD HL,u16 - Identifier byte: " + CurrentByte);
                    Cycles = 12;
                    HL = LD_r16u16();
                    break;
                case 0x22:
                    InstructionLength = 1;
                    Cycles = 8;
                    ram.SetByte(HL++, A);
                    break;
                case 0x23:
                    Cycles = 8;
                    HL = INC_r16(HL);
                    break;
                case 0x24:
                    //Debug.WriteLine("INC H - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = INC_r8(H);
                    break;
                case 0x25:
                    //Debug.WriteLine("DEC H - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = DEC_r8(H);
                    break;
                case 0x26:
                    //Debug.WriteLine("LD H,u8 - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    H = LD_u8();
                    break;
                case 0x27:
                    Cycles = 4;
                    InstructionLength = 1;
                    DAA();
                    break;
                case 0x28:
                    JR_Fu8(StatusFlags.Zero);
                    break;
                case 0x29:
                    Cycles = 8;
                    ADDr16(HL);
                    break;
                case 0x2A:
                    Cycles = 8;
                    A = LD_r8r16(HL);
                    HL++;
                    break;
                case 0x2B:
                    Cycles = 8;
                    HL = DEC_r16(HL);
                    break;
                case 0x2C:
                    //Debug.WriteLine("INC L - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = INC_r8(L);
                    break;
                case 0x2D:
                    //Debug.WriteLine("DEC L - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = DEC_r8(L);
                    break;
                case 0x2E:
                    //Debug.WriteLine("LD L,u8 - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    L = LD_u8();
                    break;
                case 0x30:
                    JR_NFu8(StatusFlags.Carry);
                    break;
                case 0x31:
                    //Debug.WriteLine("LD SP,u16 - Identifier byte: " + CurrentByte);
                    Cycles = 12;
                    StackPointer = LD_r16u16Instruction();
                    break;
                case 0x32:
                    Cycles = 8;
                    InstructionLength = 1; 
                    ram.SetByte(HL--, A);
                    break;
                case 0x34:
                    Cycles = 12;
                    ram.SetByte(HL, INC_r8(ram.GetByte(HL)));
                    break;
                case 0x35:
                    Cycles = 12;
                    ram.SetByte(HL, DEC_r8(ram.GetByte(HL)));
                    break;
                case 0x37:
                    Cycles = 4;
                    InstructionLength = 1;
                    UnsetBitFlag(StatusFlags.Negative);
                    UnsetBitFlag(StatusFlags.HalfCarry);
                    SetBitFlag(StatusFlags.Carry);
                    break;
                case 0x38:
                    JR_Fu8(StatusFlags.Carry);
                    break;
                case 0x39:
                    Cycles = 8;
                    ADDr16(StackPointer);
                    break;
                case 0x3A:
                    //TODO: OVERHAUL THIS
                    Cycles = 8;
                    LD_AHLMInstruction();
                    break;
                case 0x3C:
                    //Debug.WriteLine("INC A - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    A = INC_r8(A);
                    break;
                case 0x3D:
                    //Debug.WriteLine("DEC A - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    A = DEC_r8(A);
                    break;
                case 0x3E:
                    //Debug.WriteLine("LD A,u8 - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    A = LD_u8();
                    break;
                case 0x40:
                    //Debug.WriteLine("LD B,B - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    B = LD_r8(B);
                    break;
                case 0x41:
                    //Debug.WriteLine("LD B,C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    B = LD_r8(C);
                    break;
                case 0x42:
                    //Debug.WriteLine("LD B,D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    B = LD_r8(D);
                    break;
                case 0x43:
                    //Debug.WriteLine("LD B,E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    B = LD_r8(E);
                    break;
                case 0x44:
                    Cycles = 4;
                    //Debug.WriteLine("LD B,H - Identifier byte: " + CurrentByte);
                    B = LD_r8(H);
                    break;
                case 0x45:
                    Cycles = 4;
                    //Debug.WriteLine("LD B,L - Identifier byte: " + CurrentByte);
                    B = LD_r8(L);
                    break;
                case 0x46:
                    //Debug.WriteLine("LD B,(HL) - Identifier byte: " + CurrentByte);
                    Cycles = 12;
                    B = LD_r8r16(HL);
                    break;
                case 0x47:
                    //Debug.WriteLine("LD B,A - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    B = LD_r8(A);
                    break;
                case 0x48:
                    //Debug.WriteLine("LD C,B - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = LD_r8(B);
                    break;
                case 0x49:
                    //Debug.WriteLine("LD C,C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = LD_r8(C);
                    break;
                case 0x4A:
                    //Debug.WriteLine("LD C,D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = LD_r8(D);
                    break;
                case 0x4B:
                    //Debug.WriteLine("LD C,E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = LD_r8(E);
                    break;
                case 0x4C:
                    //Debug.WriteLine("LD C,H - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = LD_r8(H);
                    break;
                case 0x4D:
                    //Debug.WriteLine("LD C,L - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = LD_r8(L);
                    break;
                case 0x4E:
                    //Debug.WriteLine("LD C,(HL) - Identifier byte: " + CurrentByte);
                    C = LD_r8r16(HL);
                    break;
                case 0x4F:
                    //Debug.WriteLine("LD C,A - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    C = LD_r8(A);
                    break;
                case 0x50:
                    //Debug.WriteLine("LD D,B - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = LD_r8(B);
                    break;
                case 0x51:
                    //Debug.WriteLine("LD D,C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = LD_r8(C);
                    break;
                case 0x52:
                    //Debug.WriteLine("LD D,D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = LD_r8(D);
                    break;
                case 0x53:
                    //Debug.WriteLine("LD D,E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = LD_r8(E);
                    break;
                case 0x54:
                    //Debug.WriteLine("LD D,H - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = LD_r8(H);
                    break;
                case 0x55:
                    //Debug.WriteLine("LD D,L - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = LD_r8(L);
                    break;
                case 0x56:
                    //Debug.WriteLine("LD D,(HL) - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    D = LD_r8r16(HL);
                    break;
                case 0x57:
                    //Debug.WriteLine("LD D,A - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    D = LD_r8(A);
                    break;
                case 0x58:
                    //Debug.WriteLine("LD E,B - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = LD_r8(B);
                    break;
                case 0x59:
                    //Debug.WriteLine("LD E,C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = LD_r8(C);
                    break;
                case 0x5A:
                    //Debug.WriteLine("LD E,D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = LD_r8(D);
                    break;
                case 0x5B:
                    //Debug.WriteLine("LD E,E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = LD_r8(E);
                    break;
                case 0x5C:
                    //Debug.WriteLine("LD E,H - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = LD_r8(H);
                    break;
                case 0x5D:
                    //Debug.WriteLine("LD E,L - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = LD_r8(L);
                    break;
                case 0x5E:
                    Cycles = 8;
                    //Debug.WriteLine("LD E,(HL) - Identifier byte: " + CurrentByte);
                    E = LD_r8r16(HL);
                    break;
                case 0x5F:
                    //Debug.WriteLine("LD E,A - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    E = LD_r8(A);
                    break;
                case 0x60:
                    //Debug.WriteLine("LD H,B - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = LD_r8(B);
                    break;
                case 0x61:
                    //Debug.WriteLine("LD H,C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = LD_r8(C);
                    break;
                case 0x62:
                    //Debug.WriteLine("LD H,D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = LD_r8(D);
                    break;
                case 0x63:
                    //Debug.WriteLine("LD H,E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = LD_r8(E);
                    break;
                case 0x64:
                    //Debug.WriteLine("LD H,H - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = LD_r8(H);
                    break;
                case 0x65:
                    //Debug.WriteLine("LD H,L - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = LD_r8(L);
                    break;
                case 0x66:
                    //Debug.WriteLine("LD H,(HL) - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    H = LD_r8r16(HL);
                    break;
                case 0x67:
                    //Debug.WriteLine("LD H,A - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    H = LD_r8(A);
                    break;
                case 0x68:
                    //Debug.WriteLine("LD L,B - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = LD_r8(B);
                    break;
                case 0x69:
                    //Debug.WriteLine("LD L,C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = LD_r8(C);
                    break;
                case 0x6A:
                    //Debug.WriteLine("LD L,D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = LD_r8(D);
                    break;
                case 0x6B:
                    //Debug.WriteLine("LD L,E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = LD_r8(E);
                    break;
                case 0x6C:
                    //Debug.WriteLine("LD L,H - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = LD_r8(H);
                    break;
                case 0x6D:
                    //Debug.WriteLine("LD L,L - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = LD_r8(L);
                    break;
                case 0x6E:
                    //Debug.WriteLine("LD L,(HL) - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    L = LD_r8r16(HL);
                    break;
                case 0x6F:
                    //Debug.WriteLine("LD L,A - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    L = LD_r8(A);
                    break;
                case 0x70:
                    Cycles = 8;
                    LD_r16r8(HL, B);
                    break;
                case 0x71:
                    Cycles = 8;
                    LD_r16r8(HL, C);
                    break;
                case 0x72:
                    Cycles = 8;
                    LD_r16r8(HL, D);
                    break;
                case 0x73:
                    Cycles = 8;
                    LD_r16r8(HL, E);
                    break;
                case 0x74:
                    Cycles = 8;
                    LD_r16r8(HL, H);
                    break;
                case 0x75:
                    Cycles = 8;
                    LD_r16r8(HL, L);
                    break;
                case 0x77:
                    Cycles = 8;
                    LD_r16r8(HL, A);
                    break;
                case 0x78:
                    //Debug.WriteLine("LD A,B - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    A = LD_r8(B);
                    break;
                case 0x79:
                    //Debug.WriteLine("LD A,C - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    A = LD_r8(C);
                    break;
                case 0x7A:
                    //Debug.WriteLine("LD A,D - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    A = LD_r8(D);
                    break;
                case 0x7B:
                    //Debug.WriteLine("LD A,E - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    A = LD_r8(E);
                    break;
                case 0x7C:
                    //Debug.WriteLine("LD A,H - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    A = LD_r8(H);
                    break;
                case 0x7D:
                    //Debug.WriteLine("LD A,L - Identifier byte: " + CurrentByte);
                    Cycles = 4;
                    A = LD_r8(L);
                    break;
                case 0x7E:
                    //Debug.WriteLine("LD A,(HL) - Identifier byte: " + CurrentByte);
                    Cycles = 8;
                    A = LD_r8r16(HL);
                    break;
                case 0x80:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADD(B);
                    break;
                case 0x81:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADD(C);
                    break;
                case 0x82:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADD(D);
                    break;
                case 0x83:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADD(E);
                    break;
                case 0x84:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADD(H);
                    break;
                case 0x85:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADD(L);
                    break;
                case 0x86:
                    InstructionLength = 1;
                    Cycles = 8;
                    ADD(ram.GetByte(HL));
                    break;
                case 0x87:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADD(A);
                    break;
                case 0x88:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADC(B);
                    break;
                case 0x89:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADC(C);
                    break;
                case 0x8A:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADC(D);
                    break;
                case 0x8B:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADC(E);
                    break;
                case 0x8C:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADC(H);
                    break;
                case 0x8D:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADC(L);
                    break;
                case 0x8E:
                    InstructionLength = 1;
                    Cycles = 8;
                    ADC(ram.GetByte(HL));
                    break;
                case 0x8F:
                    InstructionLength = 1;
                    Cycles = 4;
                    ADC(A);
                    break;
                case 0x90:
                    InstructionLength = 1;
                    Cycles = 4;
                    SUB(B);
                    break;
                case 0x91:
                    InstructionLength = 1;
                    Cycles = 4;
                    SUB(C);
                    break;
                case 0x92:
                    InstructionLength = 1;
                    Cycles = 4;
                    SUB(D);
                    break;
                case 0x93:
                    InstructionLength = 1;
                    Cycles = 4;
                    SUB(E);
                    break;
                case 0x94:
                    InstructionLength = 1;
                    Cycles = 4;
                    SUB(H);
                    break;
                case 0x95:
                    InstructionLength = 1;
                    Cycles = 4;
                    SUB(L);
                    break;
                case 0x96:
                    InstructionLength = 1;
                    Cycles = 8;
                    SUB(ram.GetByte(HL));
                    break;
                case 0x97:
                    InstructionLength = 1;
                    Cycles = 4;
                    SUB(A);
                    break;
                case 0xA0:
                    Cycles = 4;
                    AND(B);
                    break;
                case 0xA1:
                    Cycles = 4;
                    AND(C);
                    break;
                case 0xA2:
                    Cycles = 4;
                    AND(D);
                    break;
                case 0xA3:
                    Cycles = 4;
                    AND(E);
                    break;
                case 0xA4:
                    Cycles = 4;
                    AND(H);
                    break;
                case 0xA5:
                    Cycles = 4;
                    AND(L);
                    break;
                case 0xA6:
                    Cycles = 8;
                    AND(ram.GetByte(HL));
                    break;
                case 0xA7:
                    Cycles = 4;
                    AND(A);
                    break;
                case 0xA8:
                    Cycles = 4;
                    InstructionLength = 1;
                    XOR(B);
                    break;
                case 0xA9:
                    Cycles = 4;
                    InstructionLength = 1;
                    XOR(C);
                    break;
                case 0xAA:
                    Cycles = 4;
                    InstructionLength = 1;
                    XOR(D);
                    break;
                case 0xAB:
                    Cycles = 4;
                    InstructionLength = 1;
                    XOR(E);
                    break;
                case 0xAC:
                    Cycles = 4;
                    InstructionLength = 1;
                    XOR(H);
                    break;
                case 0xAD:
                    Cycles = 4;
                    InstructionLength = 1;
                    XOR(L);
                    break;
                case 0xAE:
                    Cycles = 8;
                    InstructionLength = 1;
                    XOR(ram.GetByte(HL));
                    break;
                case 0xAF:
                    Cycles = 4;
                    InstructionLength = 1;
                    XOR(A);
                    break;
                case 0xB0:
                    Cycles = 4;
                    OR(B);
                    break;
                case 0xB1:
                    Cycles = 4;
                    OR(C);
                    break;
                case 0xB2:
                    Cycles = 4;
                    OR(D);
                    break;
                case 0xB3:
                    Cycles = 4;
                    OR(E);
                    break;
                case 0xB4:
                    Cycles = 4;
                    OR(H);
                    break;
                case 0xB5:
                    Cycles = 4;
                    OR(L);
                    break;
                case 0xB6:
                    Cycles = 8;
                    OR(ram.GetByte(HL));
                    break;
                case 0xB7:
                    Cycles = 4;
                    OR(A);
                    break;
                case 0xB8:
                    InstructionLength = 1;
                    Cycles = 4;
                    CP(B);
                    break;
                case 0xB9:
                    InstructionLength = 1;
                    Cycles = 4;
                    CP(C);
                    break;
                case 0xBA:
                    InstructionLength = 1;
                    Cycles = 4;
                    CP(D);
                    break;
                case 0xBB:
                    InstructionLength = 1;
                    Cycles = 4;
                    CP(E);
                    break;
                case 0xBC:
                    InstructionLength = 1;
                    Cycles = 4;
                    CP(H);
                    break;
                case 0xBD:
                    InstructionLength = 1;
                    Cycles = 4;
                    CP(L);
                    break;
                case 0xBE:
                    InstructionLength = 1;
                    Cycles = 8;
                    CP(ram.GetByte(HL));
                    break;
                case 0xBF:
                    InstructionLength = 1;
                    Cycles = 4;
                    CP(A);
                    break;
                case 0xC0:
                    RET_NotCondition(StatusFlags.Zero);
                    break;
                case 0xC1:
                    InstructionLength = 1;
                    Cycles = 12;
                    BC = StackRead16();
                    break;
                case 0xC2:
                    JR_NFu16(StatusFlags.Zero);
                    break;
                case 0xC3:
                    JP_16Instruction();
                    break;
                case 0xC4:
                    CallNFu16(StatusFlags.Zero);
                    break;
                case 0xC5:
                    InstructionLength = 1;
                    Cycles = 16;
                    StackWrite(B);
                    StackWrite(C);
                    break;
                case 0xC6:
                    InstructionLength = 2;
                    Cycles = 8;
                    ADD(ram.GetByte(ProgramCounter+1));
                    break;
                case 0xC8:
                    RET_Condition(StatusFlags.Zero);
                    break;
                case 0xC9:
                    RET();
                    break;
                case 0xCA:
                    JR_Fu16(StatusFlags.Zero);
                    break;
                case 0xCB:
                    CBOpcode();
                    break;
                case 0xCC:
                    CallFu16(StatusFlags.Zero);
                    break;
                case 0xCD:
                    Callu16();
                    break;
                case 0xCE:
                    InstructionLength = 2;
                    Cycles = 8;
                    ADC(ram.GetByte(ProgramCounter + 1));
                    break;
                case 0xD0:
                    RET_NotCondition(StatusFlags.Carry);
                    break;
                case 0xD1:
                    InstructionLength = 1;
                    Cycles = 12;
                    DE = StackRead16();
                    break;
                case 0xD2:
                    JR_NFu16(StatusFlags.Carry);
                    break;
                case 0xD4:
                    CallNFu16(StatusFlags.Carry);
                    break;
                case 0xD5:
                    InstructionLength = 1;
                    Cycles = 16;
                    StackWrite(D);
                    StackWrite(E);
                    break;
                case 0xD6:
                    InstructionLength = 2;
                    Cycles = 8;
                    SUB(ram.GetByte(ProgramCounter + 1));
                    break;
                case 0xD8:
                    RET_Condition(StatusFlags.Carry);
                    break;
                case 0xDA:
                    JR_Fu16(StatusFlags.Carry);
                    break;
                case 0xDC:
                    CallFu16(StatusFlags.Carry);
                    break;
                case 0xE0:
                    Cycles = 12;
                    InstructionLength = 2;
                    LD_FF00u8(ram.GetByte(ProgramCounter + 1));
                    break;
                case 0xE1:
                    InstructionLength = 1;
                    Cycles = 12;
                    HL = StackRead16();
                    break;
                case 0xE2:
                    InstructionLength = 1;
                    Cycles = 8;
                    LD_r8FF00u8(C);
                    break;
                case 0xE5:
                    InstructionLength = 1;
                    Cycles = 16;
                    StackWrite(H);
                    StackWrite(L);
                    break;
                case 0xE6:
                    Cycles = 8;
                    AND(ram.GetByte(ProgramCounter+1));
                    InstructionLength = 2;
                    break;
                case 0xE9:
                    InstructionLength = 0;
                    Cycles = 4;
                    ProgramCounter = HL;
                    break;
                case 0xEA:
                    Cycles = 16;
                    LD_u16r8(A);
                    break;
                case 0xEE:
                    Cycles = 8;
                    InstructionLength = 2;
                    XOR(ram.GetByte(ProgramCounter + 1));
                    break;
                case 0xF0:
                    InstructionLength = 2;
                    Cycles = 12;
                    LD_r8FF00u8(ram.GetByte(ProgramCounter + 1));
                    break;
                case 0xF1:
                    InstructionLength = 1;
                    Cycles = 12;
                    AF = StackRead16();
                    F &= byte.MaxValue ^ (1 << 0);
                    F &= byte.MaxValue ^ (1 << 1);
                    F &= byte.MaxValue ^ (1 << 2);
                    F &= byte.MaxValue ^ (1 << 3);
                    break;
                case 0xF2:
                    InstructionLength = 1;
                    Cycles = 8;
                    LD_r8FF00u8(C);
                    break;
                case 0xF3:
                    InstructionLength = 1;
                    Cycles = 4;
                    IME = false;
                    break;
                case 0xF5:
                    InstructionLength = 1;
                    Cycles = 16;
                    StackWrite(A);
                    StackWrite(F);
                    break;
                case 0xFA:
                    Cycles = 16;
                    A = LD_r8r16(ram.GetByte(ProgramCounter + 1));
                    InstructionLength = 3;
                    break;
                case 0xFE:
                    InstructionLength = 2;
                    Cycles = 8;
                    CP(ram.GetByte(ProgramCounter+1));
                    break;

            }
        }

        void CBOpcode()
        {
            //Debug.WriteLine("CB Prefixed Opcode - Identifier byte: " + CurrentByte);
            byte PrefixOpcode = ram.GetByte(ProgramCounter + 1);
            InstructionLength = 2;
            //Debug.WriteLine(ram.Memory[PrefixOpcode]);
            switch(PrefixOpcode)
            {
                default:
                    throw new NotImplementedException("Prefixed Opcode not " + PrefixOpcode + " is not implemented!!!");
                case 0x10:
                    Cycles = 8;
                    B = RL(B);
                    break;
                case 0x11:
                    Cycles = 8;
                    C = RL(C);
                    break;
                case 0x12:
                    Cycles = 8;
                    D = RL(D);
                    break;
                case 0x13:
                    Cycles = 8;
                    E = RL(E);
                    break;
                case 0x14:
                    Cycles = 8;
                    H = RL(H);
                    break;
                case 0x15:
                    Cycles = 8;
                    L = RL(L);
                    break;
                case 0x17:
                    Cycles = 8;
                    A = RL(A);
                    break;
                case 0x18:
                    Cycles = 8;
                    B = RR(B);
                    break;
                case 0x19:
                    Cycles = 8;
                    C = RR(C);
                    break;
                case 0x1A:
                    Cycles = 8;
                    D = RR(D);
                    break;
                case 0x1B:
                    Cycles = 8;
                    E = RR(E);
                    break;
                case 0x1C:
                    Cycles = 8;
                    H = RR(H);
                    break;
                case 0x1D:
                    Cycles = 8;
                    L = RR(L);
                    break;
                case 0x1F:
                    Cycles = 8;
                    A = RR(A);
                    break;
                case 0x30:
                    Cycles = 8;
                    B = Swapu8(B);
                    break;
                case 0x31:
                    Cycles = 8;
                    C = Swapu8(C);
                    break;
                case 0x32:
                    Cycles = 8;
                    D = Swapu8(D);
                    break;
                case 0x33:
                    Cycles = 8;
                    B = Swapu8(E);
                    break;
                case 0x34:
                    Cycles = 8;
                    B = Swapu8(H);
                    break;
                case 0x35:
                    Cycles = 8;
                    B = Swapu8(L);
                    break;
                case 0x37:
                    Cycles = 8;
                    A = Swapu8(A);
                    break;
                case 0x38:
                    Cycles = 8;
                    B = SRL(B);
                    break;
                case 0x39:
                    Cycles = 8;
                    C = SRL(C);
                    break;
                case 0x3A:
                    Cycles = 8;
                    D = SRL(D);
                    break;
                case 0x3B:
                    Cycles = 8;
                    E = SRL(E);
                    break;
                case 0x3C:
                    Cycles = 8;
                    H = SRL(H);
                    break;
                case 0x3D:
                    Cycles = 8;
                    L = SRL(L);
                    break;
                case 0x3F:
                    Cycles = 8;
                    A = SRL(A);
                    break;
                case 0x40:
                    Cycles = 8;
                    TestBitu8(0, B);
                    break;
                case 0x41:
                    Cycles = 8;
                    TestBitu8(0, C);
                    break;
                case 0x42:
                    Cycles = 8;
                    TestBitu8(0, D);
                    break;
                case 0x43:
                    Cycles = 8;
                    TestBitu8(0, E);
                    break;
                case 0x44:
                    Cycles = 8;
                    TestBitu8(0, H);
                    break;
                case 0x45:
                    Cycles = 8;
                    TestBitu8(0, L);
                    break;
                case 0x47:
                    Cycles = 8;
                    TestBitu8(0, A);
                    break;
                case 0x48:
                    Cycles = 8;
                    TestBitu8(1, B);
                    break;
                case 0x49:
                    Cycles = 8;
                    TestBitu8(1, C);
                    break;
                case 0x4A:
                    Cycles = 8;
                    TestBitu8(1, D);
                    break;
                case 0x4B:
                    Cycles = 8;
                    TestBitu8(1, E);
                    break;
                case 0x4C:
                    Cycles = 8;
                    TestBitu8(1, H);
                    break;
                case 0x4D:
                    Cycles = 8;
                    TestBitu8(1, L);
                    break;
                case 0x4F:
                    Cycles = 8;
                    TestBitu8(1, A);
                    break;
                case 0x50:
                    Cycles = 8;
                    TestBitu8(2,B);
                    break;
                case 0x51:
                    Cycles = 8;
                    TestBitu8(2, C);
                    break;
                case 0x52:
                    Cycles = 8;
                    TestBitu8(2, D);
                    break;
                case 0x53:
                    Cycles = 8;
                    TestBitu8(2, E);
                    break;
                case 0x54:
                    Cycles = 8;
                    TestBitu8(2, H);
                    break;
                case 0x55:
                    Cycles = 8;
                    TestBitu8(2, L);
                    break;
                case 0x57:
                    Cycles = 8;
                    TestBitu8(2, A);
                    break;
                case 0x58:
                    Cycles = 8;
                    TestBitu8(3, B);
                    break;
                case 0x59:
                    Cycles = 8;
                    TestBitu8(3, C);
                    break;
                case 0x5A:
                    Cycles = 8;
                    TestBitu8(3, D);
                    break;
                case 0x5B:
                    Cycles = 8;
                    TestBitu8(3, E);
                    break;
                case 0x5C:
                    Cycles = 8;
                    TestBitu8(3, H);
                    break;
                case 0x5D:
                    Cycles = 8;
                    TestBitu8(3, L);
                    break;
                case 0x5F:
                    Cycles = 8;
                    TestBitu8(3, A);
                    break;
                case 0x60:
                    Cycles = 8;
                    TestBitu8(4, B);
                    break;
                case 0x61:
                    Cycles = 8;
                    TestBitu8(4, C);
                    break;
                case 0x62:
                    Cycles = 8;
                    TestBitu8(4, D);
                    break;
                case 0x63:
                    Cycles = 8;
                    TestBitu8(4, E);
                    break;
                case 0x64:
                    Cycles = 8;
                    TestBitu8(4, H);
                    break;
                case 0x65:
                    Cycles = 8;
                    TestBitu8(4, L);
                    break;
                case 0x67:
                    Cycles = 8;
                    TestBitu8(4, A);
                    break;
                case 0x68:
                    Cycles = 8;
                    TestBitu8(5, B);
                    break;
                case 0x69:
                    Cycles = 8;
                    TestBitu8(5, C);
                    break;
                case 0x6A:
                    Cycles = 8;
                    TestBitu8(5, D);
                    break;
                case 0x6B:
                    Cycles = 8;
                    TestBitu8(5, E);
                    break;
                case 0x6C:
                    Cycles = 8;
                    TestBitu8(5, H);
                    break;
                case 0x6D:
                    Cycles = 8;
                    TestBitu8(5, L);
                    break;
                case 0x6F:
                    Cycles = 8;
                    TestBitu8(5, A);
                    break;
                case 0x70:
                    Cycles = 8;
                    TestBitu8(6, B);
                    break;
                case 0x71:
                    Cycles = 8;
                    TestBitu8(6, C);
                    break;
                case 0x72:
                    Cycles = 8;
                    TestBitu8(6, D);
                    break;
                case 0x73:
                    Cycles = 8;
                    TestBitu8(6, E);
                    break;
                case 0x74:
                    Cycles = 8;
                    TestBitu8(6, H);
                    break;
                case 0x75:
                    Cycles = 8;
                    TestBitu8(6, L);
                    break;
                case 0x77:
                    Cycles = 8;
                    TestBitu8(6, A);
                    break;
                case 0x78:
                    Cycles = 8;
                    TestBitu8(7, B);
                    break;
                case 0x79:
                    Cycles = 8;
                    TestBitu8(7, C);
                    break;
                case 0x7A:
                    Cycles = 8;
                    TestBitu8(7, D);
                    break;
                case 0x7B:
                    Cycles = 8;
                    TestBitu8(7, E);
                    break;
                case 0x7C:
                    Cycles = 8;
                    TestBitu8(7, H);
                    break;
                case 0x7D:
                    Cycles = 8;
                    TestBitu8(7, L);
                    break;
                case 0x7F:
                    Cycles = 8;
                    TestBitu8(7, A);
                    break;
            }
        }

        void SetBitFlag(StatusFlags status)
        {
            statusFlags |= status;
            F = (byte)statusFlags;
        }

        void UnsetBitFlag(StatusFlags status)
        {
            statusFlags &= ~status;
            F = (byte)statusFlags;
        }

        void StackWrite(byte value)
        {
            StackPointer--;
            ram.SetByte(StackPointer, value);
        }
        UInt16 StackRead16()
        {

            UInt16 value = ram.GetByte16(StackPointer);
            StackPointer += 2;
            return value;
        }


        #region Unprefixed Opcodes

        void NOPInstruction()
        {
            InstructionLength = 1;
            //Debug.WriteLine("NOP - Identifier byte: " + CurrentByte);
        }

        byte LD_u8()
        {
            InstructionLength = 2;
            return ram.GetByte(ProgramCounter + 1);
        }
        byte LD_r8r16(UInt16 reg)
        {
            InstructionLength = 1;
            return ram.GetByte(reg);
        }

        void LD_r16r8(UInt16 reg, byte reg3)
        {
            InstructionLength = 1;
            ram.SetByte(reg, reg3);
            //Debug.WriteLine("LD (DE),A - Identifier byte: " + CurrentByte);
        }


        ushort LD_r16u16Instruction()
        {
            InstructionLength = 3;
            return ram.GetByte16((ushort)(ProgramCounter + 1));
        }

        void LD_u16r8(byte reg)
        {
            InstructionLength = 3;
            UInt16 Address = ram.GetByte16((ushort)(ProgramCounter + 1));
            ram.SetByte(Address, reg);
        }

        void LD_r8FF00u8(byte u8)
        {
            UInt16 Address = (UInt16)(0xFF00 + u8);
            A = ram.GetByte(Address);
            //Debug.WriteLine("LD (FF00+C),A - Identifier byte: " + CurrentByte);
        }

        void LD_FF00u8(byte u8)
        {
            UInt16 Address = (UInt16)(0xFF00 + u8);
            ram.SetByte(Address, A);
            //Debug.WriteLine("LD (FF00+C),A - Identifier byte: " + CurrentByte);
        }


        UInt16 LD_r16u16()
        {
            InstructionLength = 3;
            return ram.GetByte16((ushort)(ProgramCounter + 1));
        }

        byte LD_r8(byte register)
        {
            InstructionLength = 1;
            return register;
        }

        byte INC_r8(byte reg)
        {
            InstructionLength = 1;
            if(reg == 0xFF)
            {
                SetBitFlag(StatusFlags.HalfCarry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.HalfCarry);
            }
            reg++;
            if (reg == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }

            UnsetBitFlag(StatusFlags.Negative);

            return reg;
        }

        UInt16 INC_r16(UInt16 reg)
        {
            InstructionLength = 1;
            reg++;
            if (reg == 0x00)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            UnsetBitFlag(StatusFlags.Negative);
            return reg;
        }

        void ADD(byte Byte)
        {
            byte Result = (byte)(A + Byte);
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            if (A + Byte > 255)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }
            UnsetBitFlag(StatusFlags.Negative);
        }

        void ADDr16(UInt16 reg)
        {
            InstructionLength = 1;
            byte Result = (byte)(HL + reg);
            if (A + reg > 0xFFFF)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }
            UnsetBitFlag(StatusFlags.Negative);
            HL = Result;
        }

        void ADC(byte Byte)
        {
            int Carry = statusFlags.HasFlag(StatusFlags.Carry) ? 1 : 0;
            byte Result = (byte)(A + Byte + Carry);
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            if(A + Byte + Carry > 255)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }

        }



        byte DEC_r8(byte reg)
        {
            InstructionLength = 1;
            if (reg == 0)
            {
                SetBitFlag(StatusFlags.HalfCarry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.HalfCarry);
            }
            reg--;
            if (reg == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            SetBitFlag(StatusFlags.Negative);
            return reg;
        }
        UInt16 DEC_r16(UInt16 reg)
        {
            InstructionLength = 1;
            reg--;
            if (reg == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            SetBitFlag(StatusFlags.Negative);
            return reg;
        }

        void SUB(byte Input)
        {
            if (A == 0)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }
            if(Input > A)
            {
                SetBitFlag(StatusFlags.HalfCarry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.HalfCarry);
            }
            byte Result = (byte)(A - Input);
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            SetBitFlag(StatusFlags.Negative);
            A = Result;
        }

        void OR(byte Input)
        {
            InstructionLength = 1;
            byte Result = (byte)(A | Input);
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            A = Result;
        }

        void XOR(byte Value)
        {
            A ^= Value;
            if (A == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            UnsetBitFlag(StatusFlags.Negative);
            UnsetBitFlag(StatusFlags.HalfCarry);
            UnsetBitFlag(StatusFlags.Carry);
            //Debug.WriteLine("XOR A,A - Identifier byte: " + CurrentByte);
        }

        void AND(byte Input)
        {
            InstructionLength = 1;
            byte Result = (byte)(A & Input);
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            SetBitFlag(StatusFlags.HalfCarry);
            UnsetBitFlag(StatusFlags.Negative);
            UnsetBitFlag(StatusFlags.Carry);
            A = Result;
        }

        void CP(byte Input)
        {
            if (A == 0)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }
            byte Result = (byte)(A - Input);
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            SetBitFlag(StatusFlags.Negative);
            //TODO: fix this...?
        }

        void Callu16()
        {
            InstructionLength = 0;
            Cycles = 24;
            UInt16 newPC = ram.GetByte16((UInt16)(ProgramCounter+1));
            byte lowbyte = (byte)(ProgramCounter + 2 & 0xFF);
            byte highbyte = (byte)((ProgramCounter + 2 >> 8) & 0xFF);
            StackWrite(highbyte);
            StackWrite(lowbyte);
            ProgramCounter = newPC;
        }

        void CallFu16(StatusFlags Flag)
        {
            Cycles = 12;
            if (statusFlags.HasFlag(Flag))
            {
                Cycles = 24;
                InstructionLength = 0;
                UInt16 newPC = ram.GetByte16((UInt16)(ProgramCounter + 1));
                byte lowbyte = (byte)(ProgramCounter + 2 & 0xFF);
                byte highbyte = (byte)((ProgramCounter + 2 >> 8) & 0xFF);
                StackWrite(highbyte);
                StackWrite(lowbyte);
                ProgramCounter = newPC;
            }
            else
            {
                InstructionLength = 3;
            }
            
        }

        void CallNFu16(StatusFlags Flag)
        {
            Cycles = 12;
            if (!statusFlags.HasFlag(Flag))
            {
                InstructionLength = 0;
                Cycles = 24;
                UInt16 newPC = ram.GetByte16((UInt16)(ProgramCounter + 1));
                byte lowbyte = (byte)(ProgramCounter + 2 & 0xFF);
                byte highbyte = (byte)((ProgramCounter + 2 >> 8) & 0xFF);
                StackWrite(highbyte);
                StackWrite(lowbyte);
                ProgramCounter = newPC;
            }
            else
            {
                InstructionLength = 3;
            }

        }


        void RET_Condition(StatusFlags Condition)
        {
            InstructionLength = 1;
            Cycles = 8;
            if (statusFlags.HasFlag(Condition))
            {
                Cycles = 20;
                ProgramCounter = StackRead16();
            }
        }
        void RET_NotCondition(StatusFlags Condition)
        {
            InstructionLength = 1;
            Cycles = 8;
            if (!statusFlags.HasFlag(Condition))
            {
                Cycles = 20;
                ProgramCounter = StackRead16();
            }
        }
        void RET()
        {
            InstructionLength = 1;
            Cycles = 16;
            ProgramCounter = StackRead16();
        }

        void RLA()
        {
            InstructionLength = 1;
            byte Result = (byte)(A << 1);
            if ((int)(A << 1) > 255)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }
            UnsetBitFlag(StatusFlags.Negative);
            UnsetBitFlag(StatusFlags.Zero);
            UnsetBitFlag(StatusFlags.HalfCarry);
            A = Result;
        }

        void LD_AHLPInstruction()
        {
            InstructionLength = 1;
            byte[] HLByte = { H, L };
            UInt16 HL = (UInt16)(BitConverter.ToUInt16(HLByte, 0));
            A = ram.GetByte(HL++);
            //Debug.WriteLine("LD A,(HL+) - Identifier byte: " + CurrentByte);
        }

        void LD_AHLMInstruction()
        {
            InstructionLength = 1;
            byte[] HLByte = { H, L };
            UInt16 HL = (UInt16)(BitConverter.ToUInt16(HLByte, 0) - 1);
            A = ram.GetByte(HL);
            //Debug.WriteLine("LD A,(HL-) - Identifier byte: " + CurrentByte);
        }




        void LD_HLMAInstruction()
        {
            InstructionLength = 1;
            byte[] HLByte = { H, L};
            UInt16 HL = BitConverter.ToUInt16(HLByte, 0);
            ram.SetByte(HL - 1, A);
            //Debug.WriteLine("LD (HL-),A - Identifier byte: " + CurrentByte);
        }


        void JP_16Instruction()
        {
            Cycles = 16;
            InstructionLength = 0;
            ProgramCounter = ram.GetByte16((ushort)(ProgramCounter + 1));
            //Debug.WriteLine("JP u16 - Identifier byte: " + CurrentByte);
        }

        void JR_NFu8(StatusFlags statusFlag)
        {
            InstructionLength = 2;
            Cycles = 8;
            if (!statusFlags.HasFlag(statusFlag))
            {
                Cycles = 12;
                sbyte Offset = (sbyte)ram.GetByte(ProgramCounter + 1);
                ProgramCounter = (ushort)(ProgramCounter + Offset);
            }
            //Debug.WriteLine("JR NZ,i8 - Identifier byte: " + CurrentByte);
        }

        void JR_NFu16(StatusFlags statusFlag)
        {
            InstructionLength = 2;
            Cycles = 12;
            if (!statusFlags.HasFlag(statusFlag))
            {
                Cycles = 16;
                UInt16 Offset = ram.GetByte16((ushort)(ProgramCounter + 1));
                ProgramCounter = (ushort)(ProgramCounter + Offset);
            }
        }

        void JR_Fu8(StatusFlags statusFlag)
        {
            InstructionLength = 2;
            Cycles = 8;
            if (statusFlags.HasFlag(statusFlag))
            {
                Cycles = 12;
                sbyte Offset = (sbyte)ram.GetByte(ProgramCounter + 1);
                ProgramCounter = (ushort)(ProgramCounter + Offset);
            }
            //Debug.WriteLine("JR NZ,i8 - Identifier byte: " + CurrentByte);
        }

        void JR_Fu16(StatusFlags statusFlag)
        {
            InstructionLength = 2;
            Cycles = 12;
            if (statusFlags.HasFlag(statusFlag))
            {
                Cycles = 16;
                UInt16 Offset = ram.GetByte16((ushort)(ProgramCounter + 1));
                ProgramCounter = (ushort)(ProgramCounter + Offset);
            }
        }

        void JR_u8Instruction()
        {
            InstructionLength = 2;
            sbyte Offset = (sbyte)ram.GetByte(ProgramCounter + 1);
            ProgramCounter = (ushort)(ProgramCounter + Offset);
        }

        void RETInstruction()
        {
            InstructionLength = 1;
            ProgramCounter = ram.GetByte16((ushort)(StackPointer + 1));
            //Debug.WriteLine("NOP - Identifier byte: " + CurrentByte);
        }

        

        void RRA()
        {
            InstructionLength = 1;
            A = (byte)(A >> 1 | (statusFlags.HasFlag(StatusFlags.Carry) ? 0x80 : 0));
            F = 0;
            statusFlags = (StatusFlags)F;
        }


        void DAA()
        {
            if(statusFlags.HasFlag(StatusFlags.Negative))
            {
                if (statusFlags.HasFlag(StatusFlags.Carry))
                {
                    A -= 0x60;
                }
                if (statusFlags.HasFlag(StatusFlags.HalfCarry))
                {
                    A -= 0x6;
                }
            }
            else
            {
                if (statusFlags.HasFlag(StatusFlags.Carry) || (A > 0x99))
                {
                    A += 0x60;
                    SetBitFlag(StatusFlags.Carry);
                }
                if (statusFlags.HasFlag(StatusFlags.HalfCarry) || (A & 0xF) > 0x9)
                {
                    A += 0x6;
                }
            }

            if(A == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }

            UnsetBitFlag(StatusFlags.HalfCarry);

        }
        #endregion

        #region 0xCB Prefixed Opcodes

        byte Swapu8(byte Reg)
        {
            byte Result = (byte)((Reg & 0xF0) >> 4 | (Reg & 0x0F) << 4);
            if(Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            UnsetBitFlag(StatusFlags.Negative);
            UnsetBitFlag(StatusFlags.HalfCarry);
            UnsetBitFlag(StatusFlags.Carry);
            return Result;
        }

        void TestBitu8(int Bit, byte Reg)
        {
            InstructionLength = 2;
            if(((Reg >> Bit) & 1) == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            SetBitFlag(StatusFlags.HalfCarry);
            UnsetBitFlag(StatusFlags.Negative);
        }

        byte SRL(byte Byte)
        {
            byte Result = (byte)(Byte >> 1); 
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            UnsetBitFlag(StatusFlags.Negative);
            UnsetBitFlag(StatusFlags.HalfCarry);
            if((Byte & 0x1) != 0)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }

            return Result;
        }

        byte RR(byte Byte)
        {
            byte Result = (byte)(Byte >> 1 | (statusFlags.HasFlag(StatusFlags.Carry) ? 0x80 : 0));
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            UnsetBitFlag(StatusFlags.Negative);
            UnsetBitFlag(StatusFlags.HalfCarry);
            if ((Byte & 0x1) != 0)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }
            return Result;
        }
        byte RL(byte Byte)
        {
            byte Result = (byte)(Byte << 1);
            if (Result == 0)
            {
                SetBitFlag(StatusFlags.Zero);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Zero);
            }
            if((int)(Byte << 1) > 255)
            {
                SetBitFlag(StatusFlags.Carry);
            }
            else
            {
                UnsetBitFlag(StatusFlags.Carry);
            }
            UnsetBitFlag(StatusFlags.Negative);
            UnsetBitFlag(StatusFlags.HalfCarry);
            return Result;
        }
        #endregion

    }
}
