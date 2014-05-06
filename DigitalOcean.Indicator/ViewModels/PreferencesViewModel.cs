using System;
using System.Diagnostics;
using System.Windows.Documents;
using DigitalOcean.Indicator.Models;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.ViewModels {
    public class PreferencesViewModel : ReactiveObject, ISupportsActivation {
        private readonly UserSettings _userSettings;
        private string _clientId;
        private string _apiKey;
        private int _refreshInterval;
        private bool _runOnStartup;

        public ReactiveCommand<object> Close { get; private set; }
        public ReactiveCommand<object> Closing { get; private set; }
        public ReactiveCommand<object> Save { get; private set; }

        public string ApiKey {
            get { return _apiKey; }
            set { this.RaiseAndSetIfChanged(ref _apiKey, value); }
        }

        public string ClientId {
            get { return _clientId; }
            set { this.RaiseAndSetIfChanged(ref _clientId, value); }
        }

        public int RefreshInterval {
            get { return _refreshInterval; }
            set { this.RaiseAndSetIfChanged(ref _refreshInterval, value); }
        }

        public bool RunOnStartup {
            get { return _runOnStartup; }
            set { this.RaiseAndSetIfChanged(ref _runOnStartup, value); }
        }

        public PreferencesViewModel() {
            Activator = new ViewModelActivator();
            _userSettings = Locator.Current.GetService<UserSettings>();
            _clientId = _userSettings.ClientId;
            _apiKey = _userSettings.ApiKey;
            _refreshInterval = _userSettings.RefreshInterval;
            _runOnStartup = _userSettings.RunOnStartup;

            this.WhenActivated(d => {
                d(Close = ReactiveCommand.Create());
                d(Closing = ReactiveCommand.Create());
                d(Save = ReactiveCommand.Create());

                d(Closing.Subscribe(_ => {
                    var vm = Locator.Current.GetService<MainViewModel>();
                    vm.PreferencesActive = false;
                }));

                d(Save.Subscribe(_ => {
                    _userSettings.ClientId = ClientId;
                    _userSettings.ApiKey = ApiKey;
                    _userSettings.RefreshInterval = RefreshInterval;
                    _userSettings.RunOnStartup = RunOnStartup;
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