using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace AnyToUDP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Protocol2UDP p2u = Protocol2UDP.getProtocol2UDP(new SerialPort2UDP("com9", 19200), "192.168.1.100", 5000,15000);
            if (p2u != null)
            {
                p2u.startRun();
            }
        }
    }
}
