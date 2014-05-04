using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
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
                d(Observable.FromEventPattern<RequestNavigateEventHandler, RequestNavigateEventArgs>(
                    h => APILink.RequestNavigate += h, h => APILink.RequestNavigate -= h)
                    .Subscribe(x => Process.Start(x.EventArgs.Uri.ToString())));
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
            ViewModel.Close.Execute(null);
        }
    }
}