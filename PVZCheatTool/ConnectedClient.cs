using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

// Thank You: https://github.com/tom-weiland/tcp-udp-networking/tree/tutorial-part2/GameServer/GameServer

namespace PvzHakTool
{
    class ConnectedClient
    {
        public static int dataBufferSize = 4096;

        public TCP client;

        public ConnectedClient()
        {
            client = new TCP();
        }

        public class TCP
        {
            public TcpClient socket;

            private NetworkStream stream;
            private string receivedData;
            private byte[] receivedBuffer;

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedBuffer = new byte[dataBufferSize];

                stream.BeginRead(receivedBuffer, 0, dataBufferSize, Receive, null);
            }

            public void Send(string _data)
            {
                try
                {
                    byte[] _dataBytes = Encoding.UTF8.GetBytes(_data);
                    if (socket != null) stream.BeginWrite(_dataBytes, 0, _dataBytes.Length, null, null);
                }
                catch (Exception _ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("WARNING: An error occured while receiving TCP data: " + _ex);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            private void Receive(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0) return;
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receivedBuffer, _data, _byteLength);
                    receivedData = Encoding.UTF8.GetString(_data);
                    stream.BeginRead(receivedBuffer, 0, dataBufferSize, Receive, null);
                }
                catch (Exception _ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("WARNING: An error occured while receiving TCP data: " + _ex);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }
    }
}
