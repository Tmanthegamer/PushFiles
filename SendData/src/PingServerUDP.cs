﻿using System;
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
    public class PingServerUdp {
        private readonly UdpClient _pingSocket;
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        private bool Exit { get; set; } = false;
        private  List<IPEndPoint> ActiveIps { get; set; }
        public PingServerUdp(ref List<IPEndPoint> ips) {
            _pingSocket = new UdpClient(new IPEndPoint(IPAddress.Any, PortSettings.PingPortList()[0]));
            ActiveIps = ips;
        }

        public void Run(ManualResetEvent scanDoneEvent) {
            IPAddress currentIp = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(x=>x.AddressFamily == AddressFamily.InterNetwork);
            Task.Run(() => {
                while (!Exit) {
                    try {
                        Task<UdpReceiveResult> work = _pingSocket.ReceiveAsync();
                        UdpReceiveResult data = work.Result;
                        if (Equals(data.RemoteEndPoint.Address, currentIp)) continue;
                        Console.WriteLine($@"{Encoding.ASCII.GetString(data.Buffer)}:{data.RemoteEndPoint}");
                        if (!ActiveIps.Exists(e => e.Equals(data.RemoteEndPoint))) {
                            ActiveIps.Add(data.RemoteEndPoint);
                        }
                        scanDoneEvent.Set();
                        scanDoneEvent.Reset();
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
