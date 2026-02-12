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
