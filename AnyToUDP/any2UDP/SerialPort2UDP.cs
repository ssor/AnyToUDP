using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AnyToUDP
{
    public class SerialPort2UDP : IProtocol
    {
        event OnReceiveData evtReceiveData;
        SerialPort comport = null;
        string PortName;
        int BaudRate;

        public SerialPort2UDP(string portName, int baudRate)
        {
            this.PortName = portName;
            this.BaudRate = baudRate;
        }
        public void accept_msg(string data)
        {
            //处理成16进制
            MatchCollection mc = Regex.Matches(data, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中

            //依次添加到列表中
            foreach (Match m in mc)
            {
                //   Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber);
                buf.Add(Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber));
            }
            //  ;
            //转换列表为数组后发送
            if (comport.IsOpen)
                comport.Write(buf.ToArray(), 0, buf.Count);
        }
        public void register_event(OnReceiveData receiveData)
        {
            evtReceiveData += receiveData;
        }
        public bool prepare()
        {
            try
            {
                comport = new SerialPort(this.PortName, this.BaudRate);
                comport.DataReceived += port_DataReceived;

                comport.Open();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("prepare => " + ex.Message);
                return false;
            }
            return true;
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int n = comport.BytesToRead;//n为返回的字节数
                byte[] buf = new byte[n];//初始化buf 长度为n
                comport.Read(buf, 0, n);//读取返回数据并赋值到数组

                //string temp = comport.ReadExisting();
                //Debug.WriteLine("port_DataReceived => " + temp);
                if (this.evtReceiveData != null)
                {
                    evtReceiveData(buf);
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("port_DataReceived => " + ex.Message);
            }
        }
    }
}
