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
        public void print(char c)
        {
            richTextBox1.AppendText(c.ToString());
        }
        private void monitor_Load(object sender, EventArgs e)
        {

        }
    }
}
