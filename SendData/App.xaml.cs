using System.Threading;
using System.Windows;

namespace SendData {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            Thread pingServerThread = new Thread(() => {
                PingServer ps = new PingServer();
                ps.Run();
            });
            pingServerThread.Start();
        }
    }
}