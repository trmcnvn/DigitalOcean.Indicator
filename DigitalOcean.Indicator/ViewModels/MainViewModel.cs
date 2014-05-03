using System;
using ReactiveUI;

namespace DigitalOcean.Indicator.ViewModels {
    public class MainViewModel : ReactiveObject {
        private bool _preferencesActive;

        public ReactiveCommand<object> Close { get; private set; }
        public ReactiveCommand<object> Preferences { get; private set; }

        public bool PreferencesActive {
            get { return _preferencesActive; }
            set { this.RaiseAndSetIfChanged(ref _preferencesActive, value); }
        }

        public MainViewModel() {
            Close = ReactiveCommand.Create();
            Preferences = ReactiveCommand.Create(this.WhenAnyValue(x => x.PreferencesActive, pa => !pa));
            Preferences.Subscribe(_ => PreferencesActive = true);
        }
    }
}