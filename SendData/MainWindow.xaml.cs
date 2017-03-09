using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace SendData {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        public MainWindow() {
            InitializeComponent();
            LoadLocalIps();
            //DebugMenu(StatusItemSelectedCount);
        }

        public bool WaitingOnAsync { get; set; }
        private List<string> LocalIPs { get; } = new List<string>();

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

        private async void Exit_Program(object sender, RoutedEventArgs e) {
            while (WaitingOnAsync) await Task.Delay(100);
            Close();
        }

        private async void LoadLocalIps() {
            WaitingOnAsync = true;

            WaitingOnAsync = false;
            FillLocalIpsBox();
        }

        private void FillLocalIpsBox() {
            if (LocalIPs == null) return;
            foreach (string localIp in LocalIPs) FolderListView.Items.Add(new Button {Content = localIp});
        }
    }
}