using System;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.ViewModels {
    public class PreferencesViewModel : ReactiveObject {
        public ReactiveCommand<object> Close { get; private set; }

        public PreferencesViewModel() {
            Close = ReactiveCommand.Create();
            Close.Subscribe(_ => {
                var vm = Locator.Current.GetService<MainViewModel>();
                vm.PreferencesActive = false;
            });
        }
    }
}