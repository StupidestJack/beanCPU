    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace AsmEmuShort
    {
        internal class Cpu
        {
            public ushort[] mem = new ushort[65536];
            public ushort[] reg = new ushort[8];
            public ushort pc = 0;
            public ushort sp = 0xFFFF; //stack pointer
            public bool running = false;
            public int tick = 200;
            public monitor BoundScreen;
        public Dictionary<char, byte[]> asciifont = new Dictionary<char, byte[]>
        {
            { ' ', new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { '!', new byte[] { 0x00, 0x00, 0xBF, 0x00, 0x00 } },
            { '"', new byte[] { 0x00, 0x03, 0x00, 0x03, 0x00 } },
            { '#', new byte[] { 0x24, 0xFF, 0x24, 0xFF, 0x24 } },
            { '$', new byte[] { 0x44, 0x4A, 0xFF, 0x4A, 0x32 } },
            { '%', new byte[] { 0x83, 0x63, 0x18, 0xC6, 0xC1 } },
            { '&', new byte[] { 0x6E, 0x91, 0xA9, 0x46, 0xA0 } },
            { '\'', new byte[] { 0x00, 0x00, 0x03, 0x00, 0x00 } },
            { '(', new byte[] { 0x3C, 0x42, 0x81, 0x81, 0x00 } },
            { ')', new byte[] { 0x81, 0x81, 0x42, 0x3C, 0x00 } },
            { '*', new byte[] { 0x00, 0x06, 0x03, 0x06, 0x00 } },
            { '+', new byte[] { 0x00, 0x08, 0x1C, 0x08, 0x00 } },
            { ',', new byte[] { 0x00, 0x00, 0xC0, 0x00, 0x00 } },
            { '-', new byte[] { 0x00, 0x08, 0x08, 0x08, 0x00 } },
            { '.', new byte[] { 0x00, 0x00, 0x80, 0x00, 0x00 } },
            { '/', new byte[] { 0x80, 0x60, 0x18, 0x06, 0x01 } },
            { '0', new byte[] { 0x7E, 0xA1, 0x99, 0x85, 0x7E } },
            { '1', new byte[] { 0x00, 0x82, 0xFF, 0x80, 0x00 } },
            { '2', new byte[] { 0xC2, 0xA1, 0x91, 0x89, 0x86 } },
            { '3', new byte[] { 0x42, 0x89, 0x89, 0x89, 0x76 } },
            { '4', new byte[] { 0x18, 0x14, 0x12, 0xFF, 0x10 } },
            { '5', new byte[] { 0x4F, 0x89, 0x89, 0x89, 0x71 } },
            { '6', new byte[] { 0x7E, 0x89, 0x89, 0x89, 0x72 } },
            { '7', new byte[] { 0x03, 0xC1, 0x31, 0x0D, 0x03 } },
            { '8', new byte[] { 0x76, 0x89, 0x89, 0x89, 0x76 } },
            { '9', new byte[] { 0x4E, 0x91, 0x91, 0x91, 0x7E } },
            { ':', new byte[] { 0x00, 0x00, 0x42, 0x00, 0x00 } },
            { ';', new byte[] { 0x00, 0x00, 0xC2, 0x00, 0x00 } },
            { '<', new byte[] { 0x00, 0x08, 0x14, 0x22, 0x00 } },
            { '=', new byte[] { 0x00, 0x14, 0x14, 0x14, 0x00 } },
            { '>', new byte[] { 0x00, 0x22, 0x14, 0x08, 0x00 } },
            { '?', new byte[] { 0x02, 0x01, 0xB1, 0x09, 0x06 } },
            { '@', new byte[] { 0x7E, 0x81, 0x9D, 0xAD, 0x9E } },
            { 'A', new byte[] { 0xFE, 0x11, 0x11, 0x11, 0xFE } },
            { 'B', new byte[] { 0xFF, 0x89, 0x89, 0x89, 0x76 } },
            { 'C', new byte[] { 0x7E, 0x81, 0x81, 0x81, 0x42 } },
            { 'D', new byte[] { 0xFF, 0x81, 0x81, 0x81, 0x7E } },
            { 'E', new byte[] { 0xFF, 0x89, 0x89, 0x89, 0x89 } },
            { 'F', new byte[] { 0xFF, 0x09, 0x09, 0x09, 0x09 } },
            { 'G', new byte[] { 0x7E, 0x81, 0x91, 0x91, 0x72 } },
            { 'H', new byte[] { 0xFF, 0x08, 0x08, 0x08, 0xFF } },
            { 'I', new byte[] { 0x00, 0x81, 0xFF, 0x81, 0x00 } },
            { 'J', new byte[] { 0x61, 0x81, 0x81, 0x7F, 0x01 } },
            { 'K', new byte[] { 0xFF, 0x08, 0x14, 0x22, 0xC1 } },
            { 'L', new byte[] { 0xFF, 0x80, 0x80, 0x80, 0x80 } },
            { 'M', new byte[] { 0xFF, 0x02, 0x04, 0x02, 0xFF } },
            { 'N', new byte[] { 0xFF, 0x02, 0x0C, 0x10, 0xFF } },
            { 'O', new byte[] { 0x7E, 0x81, 0x81, 0x81, 0x7E } },
            { 'P', new byte[] { 0xFF, 0x09, 0x09, 0x09, 0x06 } },
            { 'Q', new byte[] { 0x7E, 0x81, 0xA1, 0xC1, 0xFE } },
            { 'R', new byte[] { 0xFF, 0x19, 0x29, 0x49, 0x86 } },
            { 'S', new byte[] { 0x46, 0x89, 0x89, 0x91, 0x62 } },
            { 'T', new byte[] { 0x01, 0x01, 0xFF, 0x01, 0x01 } },
            { 'U', new byte[] { 0x7F, 0x80, 0x80, 0x80, 0x7F } },
            { 'V', new byte[] { 0x1F, 0x60, 0x80, 0x60, 0x1F } },
            { 'W', new byte[] { 0xFF, 0x40, 0x20, 0x40, 0xFF } },
            { 'X', new byte[] { 0xE3, 0x14, 0x08, 0x14, 0xE3 } },
            { 'Y', new byte[] { 0x03, 0x04, 0xF8, 0x04, 0x03 } },
            { 'Z', new byte[] { 0xE1, 0x91, 0x89, 0x85, 0x83 } },
            { '[', new byte[] { 0x00, 0xFF, 0x81, 0x81, 0x00 } },
            { '\\', new byte[] { 0x01, 0x06, 0x18, 0x60, 0x80 } },
            { ']', new byte[] { 0x00, 0x81, 0x81, 0xFF, 0x00 } },
            { '^', new byte[] { 0x04, 0x02, 0x01, 0x02, 0x04 } },
            { '_', new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80 } },
            { '`', new byte[] { 0x00, 0x00, 0x01, 0x02, 0x00 } },
            { 'a', new byte[] { 0x48, 0xA8, 0xA8, 0xA8, 0xF0 } },
            { 'b', new byte[] { 0xFF, 0x88, 0x88, 0x88, 0x70 } },
            { 'c', new byte[] { 0x70, 0x88, 0x88, 0x88, 0x50 } },
            { 'd', new byte[] { 0x70, 0x88, 0x88, 0x88, 0xFF } },
            { 'e', new byte[] { 0x70, 0xA8, 0xA8, 0xA8, 0x30 } },
            { 'f', new byte[] { 0x10, 0x10, 0xFE, 0x11, 0x11 } },
            { 'g', new byte[] { 0x10, 0xA8, 0xA8, 0xA8, 0x70 } },
            { 'h', new byte[] { 0xFF, 0x08, 0x08, 0x08, 0xF0 } },
            { 'i', new byte[] { 0x00, 0x00, 0xFA, 0x00, 0x00 } },
            { 'j', new byte[] { 0x00, 0x80, 0x7A, 0x00, 0x00 } },
            { 'k', new byte[] { 0xFF, 0x20, 0x50, 0x88, 0x88 } },
            { 'l', new byte[] { 0x00, 0x00, 0xFF, 0x00, 0x00 } },
            { 'm', new byte[] { 0xF8, 0x08, 0xF8, 0x08, 0xF0 } },
            { 'n', new byte[] { 0xF8, 0x08, 0x08, 0x08, 0xF0 } },
            { 'o', new byte[] { 0x70, 0x88, 0x88, 0x88, 0x70 } },
            { 'p', new byte[] { 0xF0, 0x48, 0x48, 0x48, 0x30 } },
            { 'q', new byte[] { 0x30, 0x48, 0x48, 0x48, 0xF0 } },
            { 'r', new byte[] { 0xF8, 0x08, 0x08, 0x08, 0x10 } },
            { 's', new byte[] { 0x90, 0xA8, 0xA8, 0xA8, 0x48 } },
            { 't', new byte[] { 0x08, 0x08, 0x7E, 0x88, 0x88 } },
            { 'u', new byte[] { 0x78, 0x80, 0x80, 0x80, 0xF8 } },
            { 'v', new byte[] { 0x38, 0x40, 0x80, 0x40, 0x38 } },
            { 'w', new byte[] { 0xF8, 0x40, 0x20, 0x40, 0xF8 } },
            { 'x', new byte[] { 0x88, 0x50, 0x20, 0x50, 0x88 } },
            { 'y', new byte[] { 0x38, 0xA0, 0xA0, 0xA0, 0xF8 } },
            { 'z', new byte[] { 0x88, 0xC8, 0xA8, 0x98, 0x88 } },
            { '{', new byte[] { 0x00, 0x08, 0x76, 0x81, 0x81 } },
            { '|', new byte[] { 0x00, 0x00, 0xFF, 0x00, 0x00 } },
            { '}', new byte[] { 0x81, 0x81, 0x76, 0x08, 0x00 } },
            { '~', new byte[] { 0x06, 0x01, 0x02, 0x04, 0x03 } }
        };
        public void run()
            {
                running = true; 
                ushort val;
                while (running)
                {
                    ushort instruction = mem[pc++];
                    byte op = (byte)(instruction >> 8);
                    byte idx = (byte)(instruction & 0x00FF);
                    switch (op)
                    {
                        case 0x00: running = false; break; //我選擇理解成BRK
                        case 0x01: reg[idx] = mem[pc++]; break; //MOV
                        case 0x02: reg[idx] += reg[mem[pc++]]; break; //ADD
                        case 0x03: reg[idx] -= reg[mem[pc++]]; break; //SUB
                        case 0x04: reg[idx] = mem[mem[pc++]]; break; //LD
                        case 0x05: //ST + MIIO
                            ushort targetAddr = mem[pc++];
                            mem[targetAddr] = reg[idx];
                            if (targetAddr == 0xFF00) BoundScreen?.Invoke(new Action(() => BoundScreen.print((char)reg[idx])));
                            else if (targetAddr == 0xFF01) Console.Write(reg[idx]);
                            break;
                        case 0x06: pc = mem[pc++]; break; // 讀取位址參數並直接跳轉
                        case 0x07: // JZ
                            val = mem[pc++]; // 1. 直接讀取參數並把 pc 移到下一個指令起始點
                            if (reg[idx] == 0) pc = val; // 2. 如果條件成立，直接覆蓋 pc
                            break; // 3. 如果條件不成立，pc 已經在正確的下一行，直接結束！
                        case 0x08: //PRINT_STR
                            ushort strAddr = mem[pc++];
                            while (mem[strAddr] != 0x00)
                            {
                                char c = (char)mem[strAddr++];
                                BoundScreen?.Invoke(new Action(() => BoundScreen.print(c)));
                            }
                            break;
                        // POP / PUSH
                        case 0x09:
                            mem[--sp] = reg[idx];
                            break;
                        case 0x0A:
                            reg[idx] = mem[sp++];
                            break;
                        // CALL / RET
                        case 0x0B:
                            val = mem[pc++]; // 先讀取目標跳轉位址
                            mem[--sp] = pc; // 存入「參數之後」的位址，這樣 RET 才是回到下一行指令
                            pc = val; // 跳轉
                            break;
                        case 0x0C:
                            pc = mem[sp++];
                            break;
                        case 0x0D:
                            reg[idx] = (ushort)(reg[idx] * reg[mem[pc++]]);
                            break;  
                        case 0x0E:
                            val = mem[pc++];
                            if (reg[val] != 0)
                            {
                                reg[idx] = (ushort)(reg[idx] / reg[val]); // 3. 安全除法
                            }
                            else
                            {
                                reg[idx] = 0xFFFF; // 處理除以零
                            }
                            break;

                    }
                    if (tick > 0) System.Threading.Thread.Sleep(tick);
                    else System.Threading.Thread.Yield();
                }
            }
            public void write(ushort[] code)
            {
                for (int i = 0; i < code.Length; i++)
                {
                    mem[i] = code[i];
                }
            }
        }
    }
