using System;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.ViewModels {
    public class PreferencesViewModel : ReactiveObject, ISupportsActivation {
        public ReactiveCommand<object> Close { get; private set; }

        public PreferencesViewModel() {
            Activator = new ViewModelActivator();
            Close = ReactiveCommand.Create();

            this.WhenActivated(d => {
                d(Close.Subscribe(_ => {
                    var vm = Locator.Current.GetService<MainViewModel>();
                    vm.PreferencesActive = false;
                }));
            });
        }

        #region ISupportsActivation Members

        public ViewModelActivator Activator { get; private set; }

        #endregion
    }
}