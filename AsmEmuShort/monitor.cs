using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsmEmuShort
{
    public partial class monitor : Form
    {
        public monitor()
        {
            InitializeComponent();
        }

        // 在 monitor 類別內加入
        public List<char> screenBuffer = new List<char>();
        private int charWidth = 6; // 5 像素寬 + 1 像素間距
        private int charHeight = 10; // 8 像素高 + 2 像素間距
        public void printString(string s)
        {
            label1.Text += s; // 或加入你們的點陣 screenBuffer
        }
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
        //Dictionary<Keys, byte> keyValuePairs = new Dictionary<Keys, byte>
        //{
        //    // 數字區 (0-9)
        //    { Keys.D0, 48 }, { Keys.D1, 49 }, { Keys.D2, 50 }, { Keys.D3, 51 }, { Keys.D4, 52 },
        //    { Keys.D5, 53 }, { Keys.D6, 54 }, { Keys.D7, 55 }, { Keys.D8, 56 }, { Keys.D9, 57 },

        //    // 大寫字母區 (A-Z) - 注意：若要區分大小寫，通常需偵測 Shift 鍵狀態，這裡先給標準大寫
        //    { Keys.A, 65 }, { Keys.B, 66 }, { Keys.C, 67 }, { Keys.D, 68 }, { Keys.E, 69 },
        //    { Keys.F, 70 }, { Keys.G, 71 }, { Keys.H, 72 }, { Keys.I, 73 }, { Keys.J, 74 },
        //    { Keys.K, 75 }, { Keys.L, 76 }, { Keys.M, 77 }, { Keys.N, 78 }, { Keys.O, 79 },
        //    { Keys.P, 80 }, { Keys.Q, 81 }, { Keys.R, 82 }, { Keys.S, 83 }, { Keys.T, 84 },
        //    { Keys.U, 85 }, { Keys.V, 86 }, { Keys.W, 87 }, { Keys.X, 88 }, { Keys.Y, 89 }, { Keys.Z, 90 },

        //    // 控制鍵
        //    { Keys.Enter, 10 },    // \n (換行)
        //    { Keys.Space, 32 },    // 空格
        //    { Keys.Back, 8 },      // Backspace (退格)
        //    { Keys.Escape, 27 },   // ESC
        //    { Keys.Tab, 9 },       // Tab

        //    // 基本符號
        //    { Keys.OemPeriod, 46 },    // .
        //    { Keys.Oemcomma, 44 },     // ,
        //    { Keys.OemQuestion, 47 },  // /
        //    { Keys.OemMinus, 45 },     // -
        //    { Keys.Oemplus, 61 },      // = (未按下 Shift 時)
    
        //    // 九宮格數字鍵 (Numpad)
        //    { Keys.NumPad0, 48 }, { Keys.NumPad1, 49 }, { Keys.NumPad2, 50 },
        //    { Keys.NumPad3, 51 }, { Keys.NumPad4, 52 }, { Keys.NumPad5, 53 },
        //    { Keys.NumPad6, 54 }, { Keys.NumPad7, 55 }, { Keys.NumPad8, 56 }, { Keys.NumPad9, 57 },

        //    // 上下左右方向鍵
        //    { Keys.Up,    17 }, // 或使用 11 (VT)
        //    { Keys.Down,  18 }, // 或使用 12 (FF)
        //    { Keys.Left,  19 }, // 或使用 20 (DC4)
        //    { Keys.Right, 20 },
        //};
        // 在 monitor.cs 類別內加入
        // --- monitor.cs ---
        private void monitor_Load(object sender, EventArgs e)
        {
            // 當視窗完全顯示在螢幕上後，才叫 CPU 開始跑，保證 ST 指令不漏接
            Task.Run(() => {
                Program.cpu.run();
            });
        }

        public void print(char c)
        {

            if (c == '\b') // Backspace
            {
                if (screenBuffer.Count > 0)
                {
                    screenBuffer.RemoveAt(screenBuffer.Count - 1); // 刪掉最後一個字
                }
            }
            else { screenBuffer.Add(c); }
        }

        // printRange 也要改
        public void printRange(string text)
        {
            foreach (char c in text) screenBuffer.Add(c);
            panel1.Invoke(new Action(() => {
                panel1.Invalidate();
                panel1.Update();
            }));
        }
        public void RefreshScreen()
        {
            if (panel1.InvokeRequired)
            {
                panel1.BeginInvoke(new Action(RefreshScreen));
                return;
            }
            panel1.Invalidate(); // 針對 Panel 進行無效化
            panel1.Update();     // 強制立刻重畫，不要等訊息隊列
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black); // 背景刷黑

            int x = 10; // 起始 X 座標
            int y = 10; // 起始 Y 座標

            foreach (char c in screenBuffer)
            {
                char displayChar = asciifont.ContainsKey(c) ? c : '?';
                byte[] data = asciifont[displayChar];
                if (c == '\r') // 換行
                {
                    x = 10;
                    y += charHeight * 2;
                    continue;
                }
                if (c == '\n') // 回到行首
                {
                    x = 10;
                    continue;
                }
                for (int col = 0; col < 5; col++) // 走訪 5 列
                {
                    for (int row = 0; row < 8; row++) // 走訪 8 行
                    {
                        // 檢查該位元是否為 1 (由下往上推的邏輯)
                        if ((data[col] & (1 << row)) != 0)
                        {
                            // 畫出像素方塊 (放大倍率可以自己調，這裡設為 2x2)
                            g.FillRectangle(Brushes.White, x + col * 2, y + row * 2, 2, 2);
                        }
                    }
                }
                x += charWidth * 2; // 移動到下一個字元位置
                if (x > this.Width - 20) { x = 10; y += charHeight * 2; } // 簡易換行
            }
        }

        private void monitor_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void monitor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Program.cpu.running)
            {
                Program.cpu.mem[0xFF10] = e.KeyChar;
                Program.cpu.mem[0xFF11] = 1; // 按鍵狀態：1 = 按下，0 = 放開 ... 尼瑪IntelliCode會讀心術
            }
        }
    }
}
