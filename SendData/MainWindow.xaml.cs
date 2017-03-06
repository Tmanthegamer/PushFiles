using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MahApps.Metro.Controls;

namespace SendData {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        public MainWindow() {
            InitializeComponent();
            //DebugMenu(StatusItemSelectedCount);
            /*for (int i = 0; i < 100; i++) 
                FolderListView.Items.Add(new Button { Content = i });*/
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
    }
}