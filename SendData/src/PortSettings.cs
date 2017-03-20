using System;

namespace SendData {
    public static class PortSettings {

        private const int PingPortStart = 1337;
        private const int PortSize = 3;
        private const int DataPortStart = PingPortStart + PortSize;

        public static int[] PingPortList() {
            int[] plist = new int[PortSize];
            for (int i = PingPortStart; i < DataPortStart; i++) {
                plist[i - 1337] = i;
            }
            return plist;
        }

        public static int[] DataPortList() {
            int[] plist = new int[PortSize];
            for (int i = DataPortStart; i < DataPortStart + PortSize; i++) {
                plist[i - 1337] = i;
            }
            return plist;
        }
    }
}
