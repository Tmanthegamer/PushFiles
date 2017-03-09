using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SendData {
    public class PingServer {

        private Socket _pingSocket;

        public PingServer() {
            _pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Run() {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = (from ipAddress in host.AddressList where ipAddress.AddressFamily == AddressFamily.InterNetwork select ipAddress.MapToIPv4()).FirstOrDefault();
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
                _pingSocket.BeginAccept(ar => {
                    Socket acceptSocket = (Socket) ar.AsyncState;
                    Console.WriteLine(acceptSocket.RemoteEndPoint.ToString());
                }, _pingSocket);
            }
            catch (SocketException e) {
                Console.WriteLine(e);
                Console.WriteLine(@"Inside PingServer.Run");
            }
        }

        public static void AcceptCallback(IAsyncResult ar) {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }
    }
}
