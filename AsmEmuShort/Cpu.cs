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
        public int tick = 10;
        public monitor BoundScreen = new monitor();
        private System.Text.StringBuilder ioBuffer = new System.Text.StringBuilder();

        public void run()
        {
            running = true;
            ushort val;
            while (running)
            {
                ushort instruction = mem[pc++];
                byte op = (byte)(instruction >> 8);
                byte idx1 = (byte)((instruction & 0x00F0) >> 4);
                byte idx2 = (byte)(instruction & 0x000F);
                switch (op)
                {
                    case 0x00: running = false; break; //我選擇理解成BRK
                    case 0x01: reg[idx2] = mem[pc++]; break; //MOV
                    case 0x02: reg[idx2] += reg[mem[pc++]]; break; //ADD
                    case 0x03: reg[idx2] -= reg[mem[pc++]]; break; //SUB
                    case 0x04: reg[idx2] = mem[mem[pc++]]; break; //LD
                    case 0x05: // ST
                        ushort targetAddr = mem[pc++];
                        mem[targetAddr] = reg[idx2];

                        if (targetAddr == 0xFF00)
                        {
                            if (BoundScreen != null && BoundScreen.IsHandleCreated)
                            {
                                // 直接呼叫 print，不要累積在 buffer
                                char c = (char)reg[idx2];
                                BoundScreen.Invoke(new Action(() => BoundScreen.print(c)));
                            }
                        }
                        break;
                    case 0x06: pc = mem[pc++]; break; // 讀取位址參數並直接跳轉
                    case 0x07: // JZ
                        val = mem[pc++]; // 1. 直接讀取參數並把 pc 移到下一個指令起始點
                        if (reg[idx2] == 0) pc = val; // 2. 如果條件成立，直接覆蓋 pc
                        break; // 3. 如果條件不成立，pc 已經在正確的下一行，直接結束！
                    case 0x08:
                        ushort strAddr = mem[pc++];
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        int safetyCounter = 0; // 新增安全計數器
                        while (mem[strAddr] != 0x00 && safetyCounter < 256) // 限制字串長度
                        {
                            sb.Append((char)mem[strAddr++]);
                            safetyCounter++;
                        }
                        // ... 剩下的 Invoke 邏輯 ...
                        if (BoundScreen != null && BoundScreen.IsHandleCreated)
                        {
                            string finalStr = sb.ToString();
                            BoundScreen.Invoke(new Action(() => BoundScreen.printRange(finalStr)));
                        }
                        break;
                    // POP / PUSH
                    case 0x09:
                        mem[--sp] = reg[idx2];
                        break;
                    case 0x0A:
                        reg[idx2] = mem[sp++];
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
                        reg[idx2] = (ushort)(reg[idx2] * reg[mem[pc++]]);
                        break;
                    case 0x0E:
                        val = mem[pc++];
                        if (reg[val] != 0)
                        {
                            reg[idx2] = (ushort)(reg[idx2] / reg[val]); // 3. 安全除法
                        }
                        else
                        {
                            reg[idx2] = 0xFFFF; // 處理除以零
                        }
                        break;
                    case 0x0F: // JE
                        val = mem[pc++]; // 1. 直接讀取參數並把 pc 移到下一個指令起始點
                        if (reg[idx1] == reg[idx2]) pc = val; // 2. 如果條件成立，直接覆蓋 pc
                        break; // 3. 如果條件不成立，pc 已經在正確的下一行，直接結束！
                    case 0x10: // JNZ
                        val = mem[pc++]; // 1. 直接讀取參數並把 pc 移到下一個指令起始點
                        if (reg[idx2] != 0) pc = val; // 2. 如果條件成立，直接覆蓋 pc
                        break; // 3. 如果條件不成立，pc 已經在正確的下一行，直接結束！
                    case 0x11: // JNE
                        val = mem[pc++];
                        if (reg[idx1] != reg[idx2]) pc = val;
                        break;
                    case 0x12: // JG
                        val = mem[pc++];
                        if (reg[idx1] > reg[idx2]) pc = val;
                        break;
                    case 0x13: // JL
                        val = mem[pc++];
                        if (reg[idx1] < reg[idx2]) pc = val;
                        break;
                    case 0x14: // INT 指令
                        ushort interruptVector = mem[pc++];
                        switch (interruptVector) //
                        {
                            case 0x10:
                                if (reg[0] == 0x01)
                                {
                                    BoundScreen.RefreshScreen();
                                }
                                if (reg[0] == 0x02)
                                {
                                    BoundScreen.screenBuffer.Clear();
                                    BoundScreen.RefreshScreen();
                                }
                                break;
                            case 0x16: // 鍵盤服務
                                if (reg[0] == 0x00)
                                {
                                    if (mem[0xFF11] == 0) pc -= 2; // 如果沒按鍵，就重複執行 INT 0x16 指令
                                    else reg[0] = mem[0xFF10]; mem[0xFF11] = 0; // 否則把字元讀入 R0
                                }
                                break;
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