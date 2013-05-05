using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;

namespace AnyToUDP
{
    public partial class Form1 : Form, ILog
    {
        public Form1()
        {
            InitializeComponent();

            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            cmbPortName.Items.AddRange(ports);

            if (cmbPortName.Items.Count > 0)
            {
                cmbPortName.SelectedIndex = 0;
            }
            this.cmbBaut.Items.AddRange(new object[] { "9600", "19200", "115200" });
            this.cmbBaut.SelectedIndex = 1;
            this.txtRemoteIP.Text = Protocol2UDP.GetLocalIP4();
            this.txtRemotePort.Text = "5000";
            this.txtLocalPort.Text = "15000";


        }

        private void button1_Click(object sender, EventArgs e)
        {
            string serialPortName = this.cmbPortName.Text;
            if (serialPortName == string.Empty) return;

            string baut = this.cmbBaut.Text;
            string patternInt = @"[0-9]{1,5}";
            if (baut.Length <= 0 || !Regex.IsMatch(baut, patternInt))
            {
                //MessageBox.Show("请填写一个符合要求的地址！");
                return;
            }

            string localPort = this.txtLocalPort.Text;
            if (localPort.Length <= 0 || !Regex.IsMatch(localPort, patternInt)) return;

            string remoteIP = this.txtRemoteIP.Text;
            string patternIp = @"\b(([01]?\d?\d|2[0-4]\d|25[0-5])\.){3}([01]?\d?\d|2[0-4]\d|25[0-5])\b";
            if (remoteIP.Length <= 0 || !Regex.IsMatch(remoteIP, patternIp))
            {
                return;
            }
            string remotePort = this.txtRemotePort.Text;
            if (remotePort.Length <= 0 || !Regex.IsMatch(remotePort, patternInt)) return;


            Protocol2UDP p2u = Protocol2UDP.getProtocol2UDP(new SerialPort2UDP(serialPortName, int.Parse(baut)), remoteIP, int.Parse(remotePort), int.Parse(localPort));
            if (p2u != null)
            {
                p2u.startRun();
            }

            this.button1.Enabled = false;
            this.button1.Text = "运行中";
        }


        public void addLog(string data)
        {
            Action<string> func = (log) =>
            {
                this.txtLog.Text = log + "\r\n" + this.txtLog.Text;
            };
            this.Invoke(func, data);
        }
    }
}
