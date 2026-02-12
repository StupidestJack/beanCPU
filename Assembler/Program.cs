using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assembler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("請輸入組語檔案名稱與輸出方式 (例如: Assembler.exe test.asm test.bin)\r\n或者只輸入檔案名稱，將儲存為{檔案名稱.bin}並保留原副檔名。如test.asm.bin。");
                return;
            }

            string sourcePath = args[0];
            string binPath = args.Length < 1 ? args[0] + ".bin" : args[1];

            // --- 1. 讀取現有二進位檔（大端序） ---
            ushort[] originalBinary = Array.Empty<ushort>();
            if (File.Exists(binPath))
            {
                byte[] bytes = File.ReadAllBytes(binPath);
                if (bytes.Length % 2 != 0)
                {
                    Console.WriteLine($"警告：二進位檔案長度 {bytes.Length} 不是偶數，可能已損壞");
                }

                int count = bytes.Length / 2;
                originalBinary = new ushort[count];
                for (int i = 0; i < count; i++)
                {
                    int idx = i * 2;
                    // 大端序：高位元組在前，低位元組在後
                    originalBinary[i] = (ushort)((bytes[idx] << 8) | bytes[idx + 1]);
                }
            }

            // --- 2. 編譯組合語言 ---
            ushort[] newBinary = Array.Empty<ushort>();
            bool compileSuccess = false;

            try
            {
                string sourceCode = File.ReadAllText(sourcePath);
                newBinary = Compiler.Compile(sourceCode);
                compileSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"編譯失敗：{ex.Message}");
            }

            // --- 3. 寫入二進位檔（大端序） ---
            if (compileSuccess && newBinary.Length > 0)
            {
                List<byte> output = new List<byte>();
                foreach (ushort value in newBinary)
                {
                    output.Add((byte)(value >> 8));   // 高位元組
                    output.Add((byte)(value & 0xFF)); // 低位元組
                }
                File.WriteAllBytes(binPath, output.ToArray());
                Console.WriteLine($"已寫入 {newBinary.Length} 個指令至 {binPath}");
            }

            // --- 4. 比較結果 ---
            int success = 0, fail = 0, newest = 0;
            if (newBinary.SequenceEqual(originalBinary))
                newest++;
            else if (newBinary.Length > 0)
                success++;
            else
                fail++;

            Console.WriteLine($"組譯: {success} 成功，{fail} 失敗，{newest} 最新狀態");
        }
    }
}