using System;
using System.Collections.Generic;
using System.Linq;

namespace Assembler
{
    public class Compiler
    {
        // 豆子架構：Opcode 位於高位，RegIdx 位於低位
        // 格式：(Opcode << 8) | RegIdx
        private static readonly Dictionary<string, byte> OpCodes = new Dictionary<string, byte>
        {
            { "BRK",       0x00 }, // 停止
            { "MOV",       0x01 }, // reg[idx] = mem[pc++]
            { "ADD",       0x02 }, // reg[idx] += reg[mem[pc++]]
            { "SUB",       0x03 }, // reg[idx] -= reg[mem[pc++]]
            { "LD",        0x04 }, // reg[idx] = mem[mem[pc++]]
            { "ST",        0x05 }, // mem[mem[pc++]] = reg[idx] (含 MMIO 繪圖)
            { "JMP",       0x06 }, // pc = mem[pc]
            { "JZ",        0x07 }, // if (reg == 0) pc = mem[pc]
            { "PRINT_STR", 0x08 }, // 從 mem[pc++] 位址印出字串
            { "PUSH",      0x09 }, // mem[--SP] = reg[idx]
            { "POP",       0x0A }, // reg[idx] = mem[SP++]
            { "CALL",      0x0B }, // pc = mem[pc] mem[--SP] = pc + 2
            { "RET",       0x0C }, // pc = mem[SP++]
            { "MUL",       0x0D }, // reg[idx] *= reg[mem[pc++]]
            { "DIV",       0x0E }  // reg[idx] /= reg[mem[pc++]]
        };

        public static ushort[] Compile(string source)
        {
            var lines = source.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var defines = new Dictionary<string, string>();
            var labels = new Dictionary<string, ushort>();
            var instructions = new List<string>();
            var finalCode = new List<ushort>();

            // --- 第一階段：預處理 (DEFINE 與標籤清理) ---
            var preProcessed = new List<string>();
            foreach (var rawLine in lines)
            {
                var line = rawLine.Split(new[] { ';' }, StringSplitOptions.None)[0].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.ToUpper().StartsWith("DEFINE"))
                {
                    var p = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length >= 3) defines[p[1].ToUpper()] = p[2];
                    continue;
                }

                string processed = line;
                foreach (var def in defines)
                {
                    processed = System.Text.RegularExpressions.Regex.Replace(
                        processed, @"\b" + def.Key + @"\b", def.Value, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                preProcessed.Add(processed);
            }

            // --- 第二階段：第一輪掃描 (計算標籤位址) ---
            int currentAddr = 0;
            foreach (var line in preProcessed)
            {
                if (line.EndsWith(":"))
                {
                    labels[line.TrimEnd(':').ToUpper()] = (ushort)currentAddr;
                }
                else
                {
                    instructions.Add(line);
                    currentAddr += GetInstructionLength(line);
                }
            }

            // --- 第三階段：生成 16 位元機器碼 ---
            foreach (var line in instructions)
            {
                var parts = line.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string opName = parts[0].ToUpper();
                if (!OpCodes.ContainsKey(opName)) continue;

                byte op = OpCodes[opName];
                byte regIdx = 0;

                // 如果指令有參數且第一個參數是暫存器 (例如 MOV R0, 65)
                if (parts.Length > 1 && parts[1].ToUpper().StartsWith("R"))
                {
                    if (opName == "MOV" && parts.Length > 2 && parts[2].ToUpper().StartsWith("R"))
                    {
                        // 正確解析第一個和第二個暫存器編號
                        byte rd;
                        byte.TryParse(parts[1].Substring(1), out rd);
                        byte rs;
                        byte.TryParse(parts[2].Substring(1), out rs);

                        // SUB Rd, Rd
                        finalCode.Add((ushort)((0x03 << 8) | rd));
                        finalCode.Add(rd);

                        // ADD Rd, Rs
                        finalCode.Add((ushort)((0x02 << 8) | rd));
                        finalCode.Add(rs);

                        continue; // 跳過原本的 MOV 處理
                    }

                    byte.TryParse(parts[1].ToUpper().Substring(1), out regIdx);
                }

                // 封裝核心指令：(Op << 8) | RegIdx
                finalCode.Add((ushort)((op << 8) | regIdx));

                // 處理後續的運算元 (數值、位址或第二個暫存器索引)
                // 豆子的架構中，ADD/SUB 的第二個參數是存放在記憶體中的「暫存器索引」
                for (int i = (opName == "JMP" || opName == "PRINT_STR" || opName == "CALL" ? 1 : 2); i < parts.Length; i++)
                {
                    string p = parts[i].ToUpper();
                    if (p.StartsWith("R"))
                    {
                        byte.TryParse(p.Substring(1), out byte r);
                        finalCode.Add(r);
                    }
                    else if (labels.ContainsKey(p))
                    {
                        finalCode.Add(labels[p]);
                    }
                    else
                    {
                        finalCode.Add(SafeParseUshort(p));
                    }
                }
            }
            return finalCode.ToArray();
        }

        private static ushort SafeParseUshort(string s)
        {
            if (s.StartsWith("0X")) return Convert.ToUInt16(s.Substring(2), 16);
            return ushort.Parse(s);
        }

        private static int GetInstructionLength(string line)
        {
            var parts = line.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return 0;

            string op = parts[0].ToUpper();
            switch (op)
            {
                case "BRK":
                case "RET":   
                case "PUSH":  
                case "POP":   
                    return 1; // 僅有 (0x00 << 8 | 0)
                case "MOV":
                case "ADD":
                case "SUB":
                case "MUL":
                case "DIV":
                case "LD":
                case "ST":
                case "JMP":
                case "JZ":
                case "PRINT_STR":
                case "CALL":
                    return 2; // 操作碼 + 一個隨後的數值/位址/暫存器索引
                default:
                    return 1;
            }
        }
    }
}
