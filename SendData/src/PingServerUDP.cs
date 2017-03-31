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
    class PingServerUdp {
        private bool Exit { get; set; } = false;
        private readonly Socket _pingClient;
        public PingServerUdp() {
            _pingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Run(ManualResetEvent scanDoneEvent) {
            Task.Run(() => {
                while (!Exit) {
                    Task.Delay(5000).Wait();
                    _pingClient.SendTo(Encoding.ASCII.GetBytes("Hello World!"),
                        new IPEndPoint(IPAddress.Parse("192.168.1.255"), PortSettings.PingPortList()[0]));
                }
            });
        }

        public void Shutdown() {
            Exit = true;
        }
    }
}
