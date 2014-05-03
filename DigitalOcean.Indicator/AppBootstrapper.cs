using DigitalOcean.Indicator.Models;
using DigitalOcean.Indicator.ViewModels;
using DigitalOcean.Indicator.Views;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator {
    public class AppBootstrapper {
        public AppBootstrapper() {
            Locator.CurrentMutable.RegisterConstant(UserSettings.LoadFromCache(), typeof(UserSettings));
            Locator.CurrentMutable.RegisterConstant(new MainViewModel(), typeof(MainViewModel));
        }
    }
}