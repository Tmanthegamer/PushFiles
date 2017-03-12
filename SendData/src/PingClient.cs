using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SendData {
    class PingClient {
        private readonly Socket _pingSocket;
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        private bool Exit { get; set; } = false;
        public static List<IPAddress> ActiveIps { get; } = new List<IPAddress>();

        public PingClient() {
            _pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private static IPAddress GetSubnetMask(IPAddress address) {
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
            Console.WriteLine($@"Mask {mask}");
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
                Console.WriteLine($@"Scanning {ipAddress}: ");
                foreach (int port in PortSettings.PingPortList()) {
                    try {
                        AllDone.Reset();
                        Console.WriteLine($@"On Port: {port}");
                        if (_pingSocket.Connected) break;
                        IAsyncResult ar = _pingSocket.BeginConnect(new IPEndPoint(ipAddress, port), ConnectCallback,
                            _pingSocket);
                        if(!ar.AsyncWaitHandle.WaitOne(1000)) { 
                            Console.WriteLine(@"Connection Timed out");
                            throw new Exception("Failed to Connect");
                        }
                        AllDone.WaitOne();
                    }
                    catch (Exception e) {
                        Console.WriteLine(@"Could not connect.");
                    }
                }
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

        private static void ConnectCallback(IAsyncResult ar) {
            try {
                Socket handler = (Socket) ar.AsyncState;
                StateObject state = new StateObject {Buffer = Encoding.ASCII.GetBytes("500")};
                AllDone.Set();
                ActiveIps.Add((handler.RemoteEndPoint as IPEndPoint)?.Address);
                handler.BeginSend(state.Buffer, 0, state.Buffer.Length, 0, SendCallback, state);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private static void ReadCallback(IAsyncResult ar) {
            try {
                StateObject state = (StateObject) ar.AsyncState;
                Socket handler = state.WorkSocket;
                int bytesRead = handler.EndReceive(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                Console.WriteLine($@"Received {bytesRead}");
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                StateObject state = (StateObject) ar.AsyncState;
                Socket handler = state.WorkSocket;
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine($@"Sent {bytesSent} bytes to {handler.RemoteEndPoint}");
                handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReadCallback, state);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
