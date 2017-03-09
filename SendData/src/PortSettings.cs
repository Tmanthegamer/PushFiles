using System;

namespace SendData {
    public static class PortSettings {

        private const int PingPortStart = 1337;
        private const int DataPortStart = PingPortStart + 50;

        public static int[] PingPortList() {
            int[] plist = new int[50];
            for (int i = PingPortStart; i < DataPortStart; i++) {
                plist[i - 1337] = i;
            }
            return plist;
        }

        public static int[] DataPortList() {
            int[] plist = new int[50];
            for (int i = DataPortStart; i < DataPortStart + 50; i++) {
                plist[i - 1337] = i;
            }
            return plist;
        }
    }
}
