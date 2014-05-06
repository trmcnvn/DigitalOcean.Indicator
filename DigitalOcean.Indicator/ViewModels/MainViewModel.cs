using System;
using DigitalOcean.Indicator.Models;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.ViewModels {
    public class MainViewModel : ReactiveObject {
        private readonly UserSettings _userSettings;
        private bool _preferencesOpened;

        public ReactiveCommand<object> Preferences { get; private set; }
        public ReactiveCommand<object> Refresh { get; private set; }
        public ReactiveCommand<object> Close { get; private set; }

        public bool PreferencesOpened {
            get { return _preferencesOpened; }
            set { this.RaiseAndSetIfChanged(ref _preferencesOpened, value); }
        }

        public MainViewModel() {
            _userSettings = Locator.Current.GetService<UserSettings>();

            Preferences = ReactiveCommand.Create(this.WhenAnyValue(x => x.PreferencesOpened, po => !po));
            Preferences.Subscribe(_ => PreferencesOpened = true);
            Refresh =
                ReactiveCommand.Create(this.WhenAnyValue(x => x._userSettings.ClientId, x => x._userSettings.ApiKey,
                    (c, a) => !String.IsNullOrWhiteSpace(c) && !String.IsNullOrWhiteSpace(a)));
            Close = ReactiveCommand.Create();
        }
    }
}