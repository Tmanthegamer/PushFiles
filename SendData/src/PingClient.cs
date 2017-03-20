using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendData {
    class PingClient {
        private bool Exit { get; set; } = false;
        private static List<IPAddress> ActiveIps { get; } = new List<IPAddress>();

        public PingClient() {}

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

        public void Run(ManualResetEvent scanDoneEvent) {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = (
                from ipAddress in host.AddressList
                where ipAddress.AddressFamily == AddressFamily.InterNetwork
                select ipAddress.MapToIPv4()).FirstOrDefault();
            Task.Run(() => {
                while (!Exit) {
                    Parallel.ForEach(IterateLocalIps(address), new ParallelOptions {MaxDegreeOfParallelism = 12},
                        (ipAddress) => {
                            if (Equals(ipAddress, address)) {}
                            else
                                foreach (int port in PortSettings.PingPortList()) {
                                    try {
                                        Socket pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                                            ProtocolType.Tcp);
                                        Console.WriteLine($@"{ipAddress} {port}");

                                        IAsyncResult ar = pingSocket.BeginConnect(ipAddress, port, ConnectCallback,
                                            pingSocket);
                                        if (!ar.AsyncWaitHandle.WaitOne(500)) {
                                            pingSocket.Close();
                                        }
                                    }
                                    catch (Exception e) {
                                        //Console.WriteLine(e);
                                    }
                                }
                        });
                    Console.WriteLine(@"Scanning Done, Writing to file");
                    File.WriteAllLines("ActiveIps", ActiveIps.Select(x=>x.ToString()));
                    scanDoneEvent.Set();
                    Console.WriteLine(@"Done Writing");
                    Console.WriteLine(@"Waiting 10 sec");
                    Thread.Sleep(10000);
                    scanDoneEvent.Reset();
                }
            });
        }

        public void Shutdown() {
            Exit = true;
        }

        private static void ConnectCallback(IAsyncResult ar) {
            try {
                Socket handler = (Socket) ar.AsyncState;
                StateObject state = new StateObject {Buffer = Encoding.ASCII.GetBytes("500")};
                handler.EndConnect(ar);
                ActiveIps.Add((handler.RemoteEndPoint as IPEndPoint)?.Address);
                handler.BeginSend(state.Buffer, 0, state.Buffer.Length, 0, SendCallback, state);
            }
            catch (Exception e) {
                //Console.WriteLine(e);
                //Console.WriteLine(@"Connect Timed out");
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
            }
        }
    }
}
