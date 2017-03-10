using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SendData {
    public class PingServer {

        private readonly Socket _pingSocket;
        private static ManualResetEvent _allDone = new ManualResetEvent(false);

        public PingServer() {
            _pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Run() {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = (
                from ipAddress in host.AddressList
                where ipAddress.AddressFamily == AddressFamily.InterNetwork
                select ipAddress.MapToIPv4()).FirstOrDefault();
            Console.WriteLine(address);
            if (address == null) {
                throw new NullReferenceException("IPAddress was null in PingServer.Run");
            }
            IPAddress ip = address;
            try {
                foreach (int port in PortSettings.PingPortList()) {
                    _pingSocket.Bind(new IPEndPoint(ip, port));
                    if (_pingSocket.IsBound) break;
                }
                if (_pingSocket.IsBound == false) return;
                _pingSocket.Listen(byte.MaxValue);
                while (true) {
                    _allDone.Reset();
                    Console.WriteLine(@"Waiting for a Connection");
                    _pingSocket.BeginAccept(AcceptCallback, _pingSocket);
                    _allDone.WaitOne();
                }
            }
            catch (SocketException e) {
                Console.WriteLine(e);
                Console.WriteLine(@"Inside PingServer.Run");
                throw;
            }
        }

        private static void AcceptCallback(IAsyncResult ar) {
            // Signal the main thread to continue.  
            _allDone.Set();
            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject {workSocket = handler};
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
        }

        private static void ReadCallback(IAsyncResult ar) {
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);

            Console.WriteLine($@"Received {bytesRead}");
        }
    }
}
