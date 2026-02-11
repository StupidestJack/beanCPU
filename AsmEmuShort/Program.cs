using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AsmEmuShort
{

    internal class Program
    {
        public static Cpu cpu = new Cpu();

        public static void DrawMonitor(int page)
        {
            Console.SetCursorPosition(0, 0);

            int startAddr = page * 256;
            Console.WriteLine($"--- Memory Monitor [Page: {page:X2}/FF] | PC: {cpu.pc:X4} | POW: {(cpu.running ? "TRUE" : "FALSE" )} ---");
            Console.WriteLine("Addr | 00 01 02 03 04 05 06 07 | 08 09 0A 0B 0C 0D 0E 0F");
            Console.WriteLine("---------------------------------------------------------");

            for (int r = 0; r < 16; r++)
            {
                int rowAddr = startAddr + (r * 16);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{rowAddr:X4} | ");

                for (int c = 0; c < 16; c++)
                {
                    int currentAddr = rowAddr + c;
                    ushort val = cpu.mem[currentAddr];
                        
                    // --- 核心邏輯：判斷是否為 PC 指向的地方 ---
                    if (currentAddr == cpu.pc)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (val != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }

                    Console.Write($"{val:X4}");
                    Console.ResetColor(); // 畫完一格立刻重設顏色，避免溢出到下一格

                    // 處理間隔符號
                    if (c == 7) Console.Write(" | ");
                    else Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
        [STAThread]
        static void Main(string[] args)
        {
            int currentPage = 0;

            // 1. 啟動 Console 監視器執行緒
            Task.Run(() =>
            {
                while (true)
                {
                    DrawMonitor(currentPage);
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.RightArrow && currentPage < 255) currentPage++;
                        else if (key == ConsoleKey.LeftArrow && currentPage > 0) currentPage--;
                        else if (key == ConsoleKey.Escape) break;
                    }
                    System.Threading.Thread.Sleep(50);
                }
            });

            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    byte[] raw = File.ReadAllBytes(args[0]);
                    ushort[] translated = new ushort[raw.Length / 2];
                    for (int i = 0; i < translated.Length; i++)
                    {
                        // 高低位合併：Big Endian (0x01 在左，0x00 在右)
                        translated[i] = (ushort)((raw[i * 2] << 8) | raw[i * 2 + 1]);
                    }
                    cpu.write(translated);
                }
            }
            else if (File.Exists("code.dat"))
            {
                byte[] raw = File.ReadAllBytes("code.dat");
                ushort[] translated = new ushort[raw.Length / 2];
                for (int i = 0; i < translated.Length; i++)
                {
                    // 高低位合併：Big Endian (0x01 在左，0x00 在右)
                    translated[i] = (ushort)((raw[i * 2] << 8) | raw[i * 2 + 1]);
                }
                cpu.write(translated);
            }
            cpu.BoundScreen = new monitor();
            Task.Run(() =>
            {
                cpu.run();
            });
            Application.Run(cpu.BoundScreen);
        }
    }
}