using System;
using DigitalOcean.Indicator.Models;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.ViewModels {
    public class MainViewModel : ReactiveObject {
        private readonly UserSettings _userSettings;
        private bool _preferencesActive;

        public ReactiveCommand<object> Preferences { get; private set; }
        public ReactiveCommand<object> Close { get; private set; }

        public bool PreferencesActive {
            get { return _preferencesActive; }
            set { this.RaiseAndSetIfChanged(ref _preferencesActive, value); }
        }

        public MainViewModel() {
            _userSettings = Locator.Current.GetService<UserSettings>();

            Preferences = ReactiveCommand.Create(this.WhenAnyValue(x => x.PreferencesActive, pa => !pa));
            Preferences.Subscribe(_ => PreferencesActive = true);
            Close = ReactiveCommand.Create();
        }
    }
}