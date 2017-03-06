using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace SendData {
    public class Connection {
        private Socket Socket { get; }
        private static ManualResetEvent ConnectDone { get; set; } = new ManualResetEvent(false);
        private static ManualResetEvent SendDone { get; set; } = new ManualResetEvent(false);
        private static ManualResetEvent ReceiveDone { get; set; } = new ManualResetEvent(false);

        public Connection(AddressFamily afam, SocketType stype, ProtocolType ptype) {
            Socket = new Socket(afam, stype, ptype);
        }

        #region recieving_data

        public void Listen(out List<byte> data) {
            data = default(List<byte>);
            Socket.BeginAccept(AcceptCallback, Socket);
        }

        public void BindConnection(string ip, int socket) {
            Socket.Bind(new IPEndPoint(IPAddress.Parse(ip).MapToIPv4(), socket));
        }

        private static void AcceptCallback(IAsyncResult ia) {
            
        }
        
        #endregion

        #region sending_data

        public void Connect(string ip, int port) {
            if (Socket.IsBound) return;
            Socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectCallback, Socket);
        }

        private static void ConnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine($@"Socket connected to {client.RemoteEndPoint}");

                // Signal that the connection has been made.  
                ConnectDone.Set();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(byte[] data) {
            Socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, Socket);
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine($@"Sent {bytesSent} bytes to server.");

                // Signal that all bytes have been sent.  
                SendDone.Set();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion
    }
}
