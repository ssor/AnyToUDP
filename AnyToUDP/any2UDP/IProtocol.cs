using System;
using System.Collections.Generic;
using System.Text;

namespace AnyToUDP
{
    public delegate void OnReceiveData(byte[] data);
    public interface IProtocol
    {
        void register_receive_data_event(OnReceiveData receiveData);
        bool prepare();
        void write_data_to_serial_port(byte[] data);
    }
}
