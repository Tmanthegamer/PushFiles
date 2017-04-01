using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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
        private List<IPEndPoint> _localIPs = new List<IPEndPoint>();
        private ManualResetEvent ScanDone {get; set;} = new ManualResetEvent(false);
        #endregion

        public MainWindow() {
            InitializeComponent();
            PingServerUdp psu = new PingServerUdp(ref _localIPs);
            PingClientUdp pcu = new PingClientUdp();
            psu.Run(ScanDone);
            pcu.Run();
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
            
        }

        private void AwaitScan() {
            Task.Run(() => {
                ScanDone.WaitOne();
                FillLocalIpsBox();
            });
        }

        private void FillLocalIpsBox() {
            if (_localIPs == null) return;
            foreach (IPEndPoint localIp in _localIPs) {
                Application.Current.Dispatcher.Invoke(() => FolderListView.Items.Add(new Button {Content = localIp}));
            }
        }

        private void Exit_Program(object sender, RoutedEventArgs e) {
            base.Close();
        }
    }
}