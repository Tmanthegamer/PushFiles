using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendData {
    public class PingClientUdp {
        private readonly UdpClient _pingSocket;
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        private bool Exit { get; set; } = false;
        private static List<IPEndPoint> ActiveIps { get; } = new List<IPEndPoint>();
        public PingClientUdp() {
            _pingSocket = new UdpClient(new IPEndPoint(IPAddress.Any, PortSettings.PingPortList()[0]));
        }

        public void Run() {
            IPAddress currentIp = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(x=>x.AddressFamily == AddressFamily.InterNetwork);
            Task.Run(() => {
                while (!Exit) {
                    try {
                        Task<UdpReceiveResult> work = _pingSocket.ReceiveAsync();
                        UdpReceiveResult data = work.Result;
                        if (!Equals(data.RemoteEndPoint.Address, currentIp)) {
                            Console.WriteLine($@"{Encoding.ASCII.GetString(data.Buffer)}:{data.RemoteEndPoint}");
                            if (ActiveIps.Exists(e => e.Equals(data.RemoteEndPoint.Address))) {
                                ActiveIps.Add(data.RemoteEndPoint);
                            }
                        }
                    }
                    catch
                        (Exception e) {
                        Console.WriteLine(e);
                    }
                }
            });
        }

        public void Shutdown() {
            Exit = true;
        }
    }
}
