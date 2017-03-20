using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace SendData {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        #region Variables
        private readonly Thread _pingServerThread;
        private readonly Thread _pingClientThread;
        private readonly PingServer _ps = new PingServer();
        private readonly PingClient _pc = new PingClient();
        private List<string> LocalIPs { get; set; } = new List<string>();
        public ManualResetEvent ScanDone {get; private set;} = new ManualResetEvent(false);
        #endregion

        public MainWindow() {
            InitializeComponent();
            _pingServerThread = new Thread(() => _ps.Run());
            _pingClientThread = new Thread(() => _pc.Run(ScanDone));
            _pingServerThread.Start();
            _pingClientThread.Start();
            AwaitScan();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            _pingClientThread.Abort();
            _pingServerThread.Abort();
        }

        /// <summary>
        ///     Writes the content of DependencyObject to standard output.
        /// </summary>
        /// <param name="item">DependencyObject which will be scanned for items. Text of those will be printed.</param>
        private static void DebugMenu(DependencyObject item) {
            foreach (DependencyObject dependencyObject in item.GetChildObjects())
                if (dependencyObject.GetType() == typeof(TextBlock))
                    Console.WriteLine(((TextBlock) dependencyObject).Text);
        }

        private void NewMessage_OnClick(object sender, RoutedEventArgs e) {
            FilePicker fp = new FilePicker();
            Stream[] fstreams = fp.SelectFiles();
        }

        private void Messages_OnClick(object sender, RoutedEventArgs e) {
            FileTransfer.SendBytes(new byte[] {1, 2, 3, 4, 5}, "192.168.1.71", 1337);
        }

        private void AwaitScan() {
            ScanDone.WaitOne();
            LocalIPs = File.ReadAllLines("ActiveIps").ToList();
        }

        private void FillLocalIpsBox() {
            if (LocalIPs == null) return;
            foreach (string localIp in LocalIPs) FolderListView.Items.Add(new Button {Content = localIp});
        }

        private void Exit_Program(object sender, RoutedEventArgs e) {
            base.Close();
        }
    }
}