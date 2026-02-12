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
            label1.Text += c;
        }
        private void monitor_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
