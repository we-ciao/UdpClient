﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DampingForwardUDP
{
    public class AsyncUDPState
    {
        // Client   socket.
        public UdpClient udpClient = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();

        public IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
    }
}
