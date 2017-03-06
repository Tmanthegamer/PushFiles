using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MahApps.Metro.Controls;

namespace SendData {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        public bool WaitingOnAsync { get; set; } = false;
        private List<string> LocalIPs { get; set; } = new List<string>();

        public MainWindow() {
            InitializeComponent();
            LoadLocalIps();
            //DebugMenu(StatusItemSelectedCount);
        }

        /// <summary>
        ///  Writes the content of DependencyObject to standard output.
        /// </summary>
        /// <param name="item">DependencyObject which will be scanned for items. Text of those will be printed.</param>
        private static void DebugMenu(DependencyObject item) {
            foreach (DependencyObject dependencyObject in item.GetChildObjects()) {
                if (dependencyObject.GetType() == typeof(TextBlock)) {
                    Console.WriteLine(((TextBlock)dependencyObject).Text);
                }
            }
        }

        private void NewMessage_OnClick(object sender, RoutedEventArgs e) {
            FilePicker fp = new FilePicker();
            Stream[] fstreams = fp.SelectFiles();
        }

        private void Messages_OnClick(object sender, RoutedEventArgs e) {
            FileTransfer.SendBytes(new byte[] { 1, 2, 3, 4, 5 }, "192.168.1.71", 1337);
        }

        private async void Exit_Program(object sender, RoutedEventArgs e) {
            while (WaitingOnAsync) {
                await Task.Delay(100);
            }
            Close();
        }

        private async void LoadLocalIps() {
            WaitingOnAsync = true;
            IEnumerable<string> list = await NetworkScanner.Scan();
            LocalIPs = list.ToList();
            Console.WriteLine(@"sup dude");
            WaitingOnAsync = false;
            FillLocalIpsBox();
        }

        private void FillLocalIpsBox() {
            foreach (string localIP in LocalIPs) {
                FolderListView.Items.Add(new Button { Content = localIP });
            }
        }
    }
}