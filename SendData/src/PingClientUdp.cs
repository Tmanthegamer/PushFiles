using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendData {

        class PingClientUdp {
        private bool Exit { get; set; } = false;
        private readonly Socket _pingClient;
        public PingClientUdp() {
            _pingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Run() {
            Task.Run(() => {
                while (!Exit) {
                    Task.Delay(10000).Wait();
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
