using System.Threading;
using System.Windows;

namespace SendData {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private Thread _pingServerThread;
        private Thread _pingClientThread;
        private readonly PingServer _ps = new PingServer();
        private readonly PingClient _pc = new PingClient();

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            _pingServerThread = new Thread(() => {
                _ps.Run();
            });
            _pingClientThread = new Thread(() => {
                _pc.Run();
            });
            _pingServerThread.Start();
            _pingClientThread.Start();
        }

        protected  override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
            _ps.Shutdown();
            _pc.Shutdown();
            _pingClientThread.Abort();
            _pingServerThread.Abort();
        }
    }
}