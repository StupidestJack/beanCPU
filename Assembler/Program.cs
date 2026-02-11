using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Assembler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("請輸入組語檔案名稱與輸出方式 (例如: Assembler.exe test.asm test.dat)");
                return;
            }
            string content = File.ReadAllText(args[0]);
            ushort[] binary = Compiler.Compile(content);
            List<byte> output = new List<byte>();
            foreach (ushort b in binary)
            {
                byte high = (byte)(b >> 8);
                byte low = (byte)(b & 0xFF);
                output.Add(high);
                output.Add(low);
            }
            File.WriteAllBytes(args[1], output.ToArray());
        }
    }
}
