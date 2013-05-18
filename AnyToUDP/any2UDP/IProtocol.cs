using System;
using System.Collections.Generic;
using System.Text;

namespace AnyToUDP
{
    public delegate void OnReceiveData(byte[] data);
    public interface IProtocol
    {
        void register_event(OnReceiveData receiveData);
        bool prepare();
        void accept_msg(string data);
    }
}
