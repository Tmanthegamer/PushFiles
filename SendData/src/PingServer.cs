using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SendData {
    public class StateObject {
        // Client  socket.  
        public Socket WorkSocket;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] Buffer = new byte[BufferSize];
        // Received data string.  
        public readonly StringBuilder Sb = new StringBuilder();
    }

    public class PingServer {
        private readonly Socket _pingSocket;
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        public bool Exit { get; private set; } = false;

        public PingServer() {
            _pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Run() {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = (
                from ipAddress in host.AddressList
                where ipAddress.AddressFamily == AddressFamily.InterNetwork
                select ipAddress.MapToIPv4()).FirstOrDefault();
            if (address == null) {
                throw new NullReferenceException("IPAddress was null in PingServer.Run");
            }
            try {
                foreach (int port in PortSettings.PingPortList()) {
                    try {
                        _pingSocket.Bind(new IPEndPoint(address, port));
                        if (_pingSocket.IsBound) break;
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }
                }
                if (_pingSocket.IsBound == false) throw new Exception("Socket Not Bound in PingServer.Run");
                _pingSocket.Listen(byte.MaxValue);
                while (true) {
                    AllDone.Reset();
                    if (Exit) return;
                    Console.WriteLine($@"Waiting for a Connection on {_pingSocket.LocalEndPoint}");
                    _pingSocket.BeginAccept(AcceptCallback, _pingSocket);
                    AllDone.WaitOne();
                }
            }
            catch (SocketException e) {
                Console.WriteLine(e);
                Console.WriteLine(@"Inside PingServer.Run");
                throw;
            }
        }

        public void Shutdown() {
            Exit = true;
            try {
                if (!_pingSocket.Connected) return;
                _pingSocket.Shutdown(SocketShutdown.Both);
                _pingSocket.Close();
            }
            catch (SocketException e) {
                Console.WriteLine(e);
            }
           
        }

        private static void AcceptCallback(IAsyncResult ar) {
            // Get the socket that handles the client request.  
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Signal the main thread to continue.
            AllDone.Set();
            // Create the state object.  
            StateObject state = new StateObject {WorkSocket = handler};
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
        }

        private static void ReadCallback(IAsyncResult ar) {
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.WorkSocket;
            int bytesRead = handler.EndReceive(ar);
            handler.BeginSend(state.Buffer, 0, state.Buffer.Length, 0, SendCallback, handler);
            Console.WriteLine($@"Received {bytesRead}");
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                Socket handler = (Socket) ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine($@"Sent {bytesSent} bytes to {handler.RemoteEndPoint}");
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
