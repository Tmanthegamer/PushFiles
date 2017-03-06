using System.Collections.Generic;
using System.Net.Sockets;

namespace SendData {
    public static class FileTransfer {

        private static Connection SendConnection { get; } = new Connection(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Connection RecieveConnection { get; } = new Connection(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public static void SendBytes(byte[] data, string destination, int port) {
            SendConnection.Connect(destination, port);
            SendConnection.Send(data);
        }

        public static void GetBytes(out List<byte> data ,string ip, int port) {
            RecieveConnection.BindConnection(ip, port);
            RecieveConnection.Listen(out data);
        }
    }
}