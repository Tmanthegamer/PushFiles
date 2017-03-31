using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendData {
    public class PingServerUdp {
        private readonly UdpClient _pingSocket;
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        public bool Exit { get; private set; } = false;

        public PingServerUdp() {
            foreach (int port in PortSettings.PingPortList()) {
                try {
                    _pingSocket = new UdpClient(port);
                    
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
        }

        public void Run() {
            while (!Exit) {
                try {
                    Task<UdpReceiveResult> work = _pingSocket.ReceiveAsync();
                    UdpReceiveResult data = work.Result;
                    Console.WriteLine(Encoding.ASCII.GetString(data.Buffer));
                }
                catch
                    (Exception e) {
                    Console.WriteLine(e);
                }
            }
        }

      

        public void Shutdown() {
            Exit = true;
        }
    }
}
