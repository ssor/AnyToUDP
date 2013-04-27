using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace AnyToUDP
{
    public class Protocol2UDP
    {
        public IPAddress ipaddress;
        Socket clientSocket = null; //The main client socket
        public int UdpPort;
        IProtocol ProtocolFrom;
        EndPoint epServer = null;

        public static Protocol2UDP getProtocol2UDP(IProtocol iFrom, string serverIP, int udpPort)
        {
            try
            {
                IPAddress ipaddress = IPAddress.Parse(serverIP);
                Protocol2UDP p2u = new Protocol2UDP(iFrom, ipaddress, udpPort);
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
        Protocol2UDP(IProtocol iFrom, IPAddress serverIP, int udpPort)
        {
            this.ProtocolFrom = iFrom;
            this.ipaddress = serverIP;
            this.UdpPort = udpPort;
        }
        public void startRun()
        {
            try
            {
                if (ProtocolFrom.prepare())
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    //IP address of the server machine
                    IPEndPoint ipEndPoint = new IPEndPoint(this.ipaddress, this.UdpPort);
                    epServer = (EndPoint)ipEndPoint;
                }
                ProtocolFrom.register_event(OnDataReceived);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("startRun => " + ex.Message);
            }
            Debug.WriteLine("startRun => ");
        }
        void OnDataReceived(string data)
        {
            try
            {
                Debug.WriteLine("OnDataReceived => " + data);

                byte[] byteData = Encoding.UTF8.GetBytes(data);

                clientSocket.BeginSendTo(byteData, 0,
                                byteData.Length, SocketFlags.None,
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
