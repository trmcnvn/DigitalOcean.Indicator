using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Navigation;
using DigitalOcean.Indicator.ViewModels;
using ReactiveUI;

namespace DigitalOcean.Indicator.Views {
    /// <summary>
    /// Interaction logic for PreferencesView.xaml
    /// </summary>
    public partial class PreferencesView : Window, IViewFor<PreferencesViewModel> {
        public PreferencesView() {
            InitializeComponent();

            this.WhenActivated(d => {
                d(ApiLink.Events().RequestNavigate
                    .Subscribe(x => Process.Start(x.Uri.ToString())));

                d(this.Bind(ViewModel, x => x.ClientId, x => x.ClientId.Text));
                d(this.Bind(ViewModel, x => x.ApiKey, x => x.ApiKey.Text));
                d(this.Bind(ViewModel, x => x.RefreshInterval, x => x.RefreshInterval.Text));
                d(this.Bind(ViewModel, x => x.RunOnStartup, x => x.RunOnStartup.IsChecked, RunOnStartup.Events().Click));

                d(this.BindCommand(ViewModel, x => x.Save, x => x.BtnSave));
                d(this.BindCommand(ViewModel, x => x.Close, x => x.BtnClose));
                d(this.WhenAnyObservable(x => x.ViewModel.Close)
                    .Subscribe(_ => Close()));
            });
        }

        #region IViewFor<PreferencesViewModel> Members

        object IViewFor.ViewModel {
            get { return ViewModel; }
            set { ViewModel = (PreferencesViewModel)value; }
        }

        public PreferencesViewModel ViewModel { get; set; }

        #endregion

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            ViewModel.Closing.Execute(null);
        }
    }
}