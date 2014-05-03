using System;
using System.Threading;
using System.Windows;
using Akavache;

namespace DigitalOcean.Indicator {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private const string ApplicationName = "DigitalOcean Indicator";
        private static Mutex _appMutex;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            _appMutex = new Mutex(true, "DigitalOcean.Indicator-E2AD7557-D4B0-4CE5-AA07-933216296FC0");
            if (!_appMutex.WaitOne(0, false)) {
                MessageBox.Show("Only a single instance of this application may run.", ApplicationName);
                Environment.Exit(0);
            }

            BlobCache.ApplicationName = ApplicationName;
            new AppBootstrapper();
        }
    }
}