using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendData {
    public static class NetworkScanner {
        public static async Task<IEnumerable<string>> Scan() {
             var ips = await Task.Run(() => {
                List<string> activeIPs = new List<string>();
                Parallel.For(0, byte.MaxValue-1, (i) => {
                    Ping p = new Ping();
                    PingReply pr = p.Send(IPAddress.Parse("192.168.1." + i), 10, Encoding.ASCII.GetBytes("0"));
                    if (pr?.Address != null)
                        activeIPs.Add(pr.Address.ToString());
                });
                return activeIPs;
            });
            return ips;
        }
    }
}