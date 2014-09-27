using System;
using DigitalOcean.Indicator.Models;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.ViewModels {
    public class PreferencesViewModel : ReactiveObject, ISupportsActivation {
        private readonly UserSettings _userSettings;
        private string _apiKey;
        private int _refreshInterval;

        public ReactiveCommand<object> Close { get; private set; }
        public ReactiveCommand<object> Closing { get; private set; }
        public ReactiveCommand<object> Save { get; private set; }

        public string ApiKey {
            get { return _apiKey; }
            set { this.RaiseAndSetIfChanged(ref _apiKey, value); }
        }

        public int RefreshInterval {
            get { return _refreshInterval; }
            set { this.RaiseAndSetIfChanged(ref _refreshInterval, value); }
        }

        public PreferencesViewModel() {
            Activator = new ViewModelActivator();
            _userSettings = Locator.Current.GetService<UserSettings>();
            _apiKey = _userSettings.ApiKey;
            _refreshInterval = _userSettings.RefreshInterval;

            this.WhenActivated(d => {
                d(Close = ReactiveCommand.Create());
                d(Closing = ReactiveCommand.Create());
                d(Save = ReactiveCommand.Create());

                d(Closing.Subscribe(_ => {
                    var vm = Locator.Current.GetService<MainViewModel>();
                    vm.PreferencesOpened = false;
                }));

                d(Save.Subscribe(_ => {
                    _userSettings.ApiKey = ApiKey;
                    _userSettings.RefreshInterval = RefreshInterval;
                    _userSettings.Save();
                    Close.Execute(null);
                }));
            });
        }

        #region ISupportsActivation Members

        public ViewModelActivator Activator { get; private set; }

        #endregion
    }
}