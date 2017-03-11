using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace SendData {
    class PingClient {
        private readonly Socket _pingSocket;
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);

        public PingClient() {
            _pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public static IPAddress GetSubnetMask(IPAddress address) {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (
                    UnicastIPAddressInformation unicastIpAddressInformation in
                    adapter.GetIPProperties().UnicastAddresses) {
                    if (unicastIpAddressInformation.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    if (address.Equals(unicastIpAddressInformation.Address)) {
                        return unicastIpAddressInformation.IPv4Mask;
                    }
                }
            }
            throw new ArgumentException($"Can't find subnetmask for IP address {address}");
        }

        private string MaskIp(IPAddress ip, int size) {
            string lIp = ip.ToString();
            string[] slIp = lIp.Split('.');
            slIp = slIp.Take(size).ToArray();
            return slIp.Aggregate(string.Empty, (current, s) => current + s + ".");
        }

        private IEnumerable<IPAddress> IterateLocalIps(IPAddress localIp) {
            IPAddress mask = GetSubnetMask(localIp);
                Console.WriteLine(mask);
                string[] smask = mask.ToString().Split('.');
                int maskCount = smask.Count(s => s == "0");
                int ipsize = smask.Count(s => s == "255");
                string maskedAddr = MaskIp(localIp, ipsize);
                double cips = Math.Pow(255, maskCount);
                for (int i = 0; i < cips; i++) {
                    yield return IPAddress.Parse(maskedAddr + i);
                }
        }


        public void Run() {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = (
                from ipAddress in host.AddressList
                where ipAddress.AddressFamily == AddressFamily.InterNetwork
                select ipAddress.MapToIPv4()).FirstOrDefault();
            foreach (IPAddress ipAddress in IterateLocalIps(address)) {
                Console.WriteLine(ipAddress);
            }
            if (address == null) {
                throw new NullReferenceException("IPAddress was null in PingServer.Run");
            }
            /*try {
                foreach (int port in PortSettings.PingPortList()) {
                    _pingSocket.Bind(new IPEndPoint(address, port));
                    if (_pingSocket.IsBound) break;
                }
                if (_pingSocket.IsBound == false) throw new Exception("Socket Not Bound in PingServer.Run");
                _pingSocket.Listen(byte.MaxValue);
                while (true) {
                    AllDone.Reset();
                    Console.WriteLine(@"Waiting for a Connection");
                    _pingSocket.BeginAccept(AcceptCallback, _pingSocket);
                    AllDone.WaitOne();
                }
            }
            catch (SocketException e) {
                Console.WriteLine(e);
                Console.WriteLine(@"Inside PingServer.Run");
                throw;
            }
            */
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
