using System.Reactive.Linq;
using System.Threading.Tasks;
using DigitalOcean.Indicator.Models;
using DigitalOcean.Indicator.ViewModels;
using Splat;

namespace DigitalOcean.Indicator {
    public class AppBootstrapper {
        public AppBootstrapper() {
            Locator.CurrentMutable.RegisterConstant(Task.Run(async () => await UserSettings.LoadFromCache()).Result,
                typeof(UserSettings));
            Locator.CurrentMutable.RegisterConstant(new MainViewModel(), typeof(MainViewModel));
        }
    }
}