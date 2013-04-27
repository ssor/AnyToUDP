using System;
using System.Collections.Generic;
using System.Text;

namespace AnyToUDP
{
    public delegate void OnReceiveData(string data);
    public interface IProtocol
    {
        void register_event(OnReceiveData receiveData);
        bool prepare();
    }
}
