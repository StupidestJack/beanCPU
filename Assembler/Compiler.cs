using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assembler
{
    public class Compiler
    {
        // 豆子架構：Opcode 位於高位，RegIdx 位於低位
        // 格式：(Opcode << 8) | RegIdx
        private static readonly Dictionary<string, byte> OpCodes = new Dictionary<string, byte>
        {
            { "BRK",       0x00 }, // 停止
            { "MOV",       0x01 }, // reg[idx2] = mem[pc++]
            { "ADD",       0x02 }, // reg[idx2] += reg[mem[pc++]]
            { "SUB",       0x03 }, // reg[idx2] -= reg[mem[pc++]]
            { "LD",        0x04 }, // reg[idx2] = mem[mem[pc++]]
            { "ST",        0x05 }, // mem[mem[pc++]] = reg[idx2] (含 MMIO 繪圖)
            { "JMP",       0x06 }, // pc = mem[pc]
            { "JZ",        0x07 }, // if (reg == 0) pc = mem[pc]
            { "PRINT_STR", 0x08 }, // 從 mem[pc++] 位址印出字串
            { "PUSH",      0x09 }, // mem[--SP] = reg[idx2]
            { "POP",       0x0A }, // reg[idx2] = mem[SP++]
            { "CALL",      0x0B }, // pc = mem[pc] mem[--SP] = pc + 2
            { "RET",       0x0C }, // pc = mem[SP++]
            { "MUL",       0x0D }, // reg[idx2] *= reg[mem[pc++]]
            { "DIV",       0x0E }, // reg[idx2] /= reg[mem[pc++]]
            { "JE",        0x0F }, // if (reg1 == reg2) pc = mem[pc]
            { "JNZ",       0x10 }, // if (reg2 != 0) pc = mem[pc]
            { "JNE",       0x11 }, // if (reg1 != reg2) pc = mem[pc]
            { "JG",        0x12 }, // if (reg1 > reg2) pc = mem[pc]
            { "JL",        0x13 }, // if (reg1 < reg2) pc = mem[pc]
        };

        public static ushort[] Compile(string source)
        {
            // --- 第一階段：預處理 (USING) ---
            source = ExpandUsingAppend(source, new HashSet<string>()); // 或 ExpandUsingToFront

            var lines = source.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var defines = new Dictionary<string, string>();
            var labels = new Dictionary<string, ushort>();
            var instructions = new List<string>();
            var finalCode = new List<ushort>();

            // --- 第二階段：預處理 (DEFINE 與標籤清理) ---
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

            // --- 第三階段：第一輪掃描 (計算標籤位址) ---
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

            // --- 第四階段：生成 16 位元機器碼 ---
            foreach (var line in instructions)
            {
                var parts = line.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string opName = parts[0].ToUpper();

                if (opName == ".FILL")
                {
                    string val = parts[1].ToUpper();
                    if (labels.ContainsKey(val)) finalCode.Add(labels[val]); // 支援 .FILL LABEL
                    else finalCode.Add(SafeParseUshort(val)); // 支援 .FILL 65
                    continue;
                }

                if (!OpCodes.ContainsKey(opName)) continue;

                byte op = OpCodes[opName];
                byte regIdx = 0;

                // 如果指令有參數且第一個參數是暫存器 (例如 MOV R0, 65)
                if (parts.Length > 1 && parts[1].ToUpper().StartsWith("R"))
                {
                    // 語法糖，用於 MOV Rn, Rm
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
                    // 對於JE、JNE、JG、JL的特殊處裡
                    if (opName == "JE" || opName == "JNE" || opName == "JG" || opName == "JL")
                    {
                        // 預期格式：JE R1, R2, LABEL
                        byte r1 = byte.Parse(parts[1].ToUpper().Substring(1));
                        byte r2 = byte.Parse(parts[2].ToUpper().Substring(1));
                        ushort target = labels[parts[3].ToUpper()];

                        // 封裝：高位 Opcode (0x0F)，低位則是 (R1 << 4) | R2
                        finalCode.Add((ushort)((op << 8) | (r1 << 4) | r2)); //
                        finalCode.Add(target); // 下一個位置放跳轉位址
                        continue;
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
                        Console.WriteLine(p);
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

            // 特殊處理：MOV 暫存器到暫存器的語法糖
            if (op == "MOV" && parts.Length > 2 && parts[2].ToUpper().StartsWith("R"))
            {
                return 4; // SUB (2) + ADD (2)
            }

            switch (op)
            {
                case "BRK":
                case "RET":
                case ".FILL":
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
                case "JE":
                case "JNE":
                case "JG":
                case "JL":
                case "JNG":
                    return 2; // 操作碼 + 一個隨後的數值/位址/暫存器索引
                default:
                    return 1;
            }
        }
        // 在 Compiler 類別中加入以下方法（取代原 ExpandUsingInline）
        private static string ExpandUsingAppend(string source, HashSet<string> visited, string baseDirectory = null)
        {
            var mainBuilder = new StringBuilder();   // 主程式碼（非 USING 行）
            var libBuilder = new StringBuilder();    // 所有被包含檔案的展開內容

            var lines = source.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.StartsWith("USING", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                        throw new ArgumentException("USING 必須指定檔名");

                    var fileName = parts[1];
                    string fullPath;
                    if (Path.IsPathRooted(fileName))
                        fullPath = fileName;
                    else
                        fullPath = Path.GetFullPath(Path.Combine(baseDirectory ?? Directory.GetCurrentDirectory(), fileName));

                    if (visited.Contains(fullPath))
                        continue;

                    visited.Add(fullPath);
                    var content = File.ReadAllText(fullPath);
                    // 遞迴處理該檔案內部的 USING，並取得完整展開內容
                    var expanded = ExpandUsingAppend(content, visited, Path.GetDirectoryName(fullPath));
                    libBuilder.AppendLine(expanded); // 將整個檔案展開內容加入函式庫區塊
                }
                else
                {
                    mainBuilder.AppendLine(line); // 保留非 USING 行（主程式碼）
                }
            }

            // 主程式碼在前，所有函式庫程式碼在後
            return mainBuilder.ToString() + libBuilder.ToString();
        }
    }
}