﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;

namespace AnyToUDP
{
    public interface ILog
    {
        void addLog(string data);
    }
    public class Protocol2UDP
    {
        public IPAddress remoteIPaddress;
        public int remoteUdpPort;
        public ILog logForm = null;

        Socket serverSocket = null; //The main server socket
        Socket clientSocket = null; //The main client socket

        public int localUdpPort;//本地作为udp服务端时使用的端口

        IProtocol ProtocolFrom;
        EndPoint epServer = null;
        byte[] byteData = new byte[1024];
        bool bHexStyle = false;

        public static Protocol2UDP getProtocol2UDP(IProtocol iFrom, string remoteIP, int remoteUdpPort, int localUdpPort)
        {
            try
            {
                IPAddress ipaddress = IPAddress.Parse(remoteIP);
                Protocol2UDP p2u = new Protocol2UDP(iFrom, ipaddress, remoteUdpPort, localUdpPort);
                return p2u;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("getProtocol2UDP => " + ex.Message);
                return null;
            }
        }
        public static string GetLocalIP4()
        {
            IPAddress ipAddress = null;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
            {
                ipAddress = ipHostInfo.AddressList[i];
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    break;
                }
                else
                {
                    ipAddress = null;
                }
            }
            if (null == ipAddress)
            {
                return null;
            }
            return ipAddress.ToString();
        }
        Protocol2UDP(IProtocol iFrom, IPAddress remoteIP, int remoteUdpPort, int localUdpPort)
        {
            this.ProtocolFrom = iFrom;
            this.remoteIPaddress = remoteIP;
            this.remoteUdpPort = remoteUdpPort;
            this.localUdpPort = localUdpPort;
        }
        public void startRun()
        {
            try
            {
                if (ProtocolFrom.prepare())
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    initial_udp_server(this.localUdpPort);
                    //IP address of the server machine
                    IPEndPoint ipEndPoint = new IPEndPoint(this.remoteIPaddress, this.remoteUdpPort);
                    epServer = (EndPoint)ipEndPoint;
                }
                ProtocolFrom.register_receive_data_event(OnDataReceived);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("startRun => " + ex.Message);
            }
            Debug.WriteLine("startRun => ");
        }
        public void initial_udp_server(int port)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork,
                            SocketType.Dgram, ProtocolType.Udp);
                IPAddress ip = IPAddress.Parse(GetLocalIP4());
                IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
                //Bind this address to the server
                serverSocket.Bind(ipEndPoint);
                //防止客户端强行中断造成的异常
                long IOC_IN = 0x80000000;
                long IOC_VENDOR = 0x18000000;
                long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];
                serverSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);

                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                //The epSender identifies the incoming clients
                EndPoint epSender = (EndPoint)ipeSender;

                //Start receiving data
                //Array.Clear(byteData, 0, byteData.Length);
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length,
                    SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        void outputLog(string log)
        {
            if (this.logForm != null)
            {
                logForm.addLog(log);
            }
        }
        // UDP => IFrom(eg. SerialPort)
        private void OnReceive(IAsyncResult ar)
        {

            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                serverSocket.EndReceiveFrom(ar, ref epSender);

                //string strReceived = Encoding.UTF8.GetString(byteData);

                //int i = strReceived.IndexOf("\0");
                int i = Array.IndexOf<byte>(byteData, 0);
                byte[] newData = new byte[i];
                //byteData.CopyTo(newData, 0);
                Array.Copy(byteData, 0, newData, 0, i);
                this.ProtocolFrom.write_data_to_serial_port(newData);
                Array.Clear(byteData, 0, byteData.Length);

                //string strReceived = Encoding.UTF8.GetString(byteData);

                //Array.Clear(byteData, 0, byteData.Length);
                //int i = strReceived.IndexOf("\0");
                //if (i > 0)
                //{
                //    string data = strReceived.Substring(0, i);
                //    Debug.WriteLine(" Data => SP: " + data);
                //    outputLog("UDP =>" + data);
                //    //todo here should deal with the received string
                //    this.ProtocolFrom.write_data_to_serial_port(data);
                //}


                //Start listening to the message send by the user
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender,
                    new AsyncCallback(OnReceive), epSender);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    string.Format("UDPServer.OnReceive  -> error = {0}"
                    , ex.Message));
            }
        }

        //IFrom(eg. SerialPort)  => UDP
        void OnDataReceived(byte[] data)
        {
            try
            {
                //Debug.WriteLine("OnDataReceived => " + data);

                //byte[] byteData = Encoding.UTF8.GetBytes(data);
                StringBuilder builder = new StringBuilder();
                if (bHexStyle)//16进制
                {
                    //依次的拼接出16进制字符串
                    foreach (byte b in data)
                    {
                        builder.Append(b.ToString("X2") + " ");
                    }

                }
                else
                {
                    //直接按ASCII规则转换成字符串
                    builder.Append(Encoding.UTF8.GetString(data));
                }
                outputLog("UDP <=" + builder.ToString());

                clientSocket.BeginSendTo(data, 0,
                                data.Length, SocketFlags.None,
                                epServer, new AsyncCallback(OnSend), null);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("startRun => " + ex.Message);
            }
        }
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    string.Format("OnSend  ->  = {0}"
                    , ex.Message));
            }
        }
    }
}
