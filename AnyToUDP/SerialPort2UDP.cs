using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;

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
                string temp = comport.ReadExisting();
                Debug.WriteLine("port_DataReceived => " + temp);
                if (this.evtReceiveData != null)
                {
                    evtReceiveData(temp);
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("port_DataReceived => " + ex.Message);
            }
        }
    }
}
