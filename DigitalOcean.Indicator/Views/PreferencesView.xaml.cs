using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Windows;
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
                Disposable.Create(() => Debug.WriteLine("VIEW ACTIVATED"));
            });
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            ViewModel.Close.Execute(null);
        }

        #region IViewFor<PreferencesViewModel> Members

        object IViewFor.ViewModel {
            get { return ViewModel; }
            set { ViewModel = (PreferencesViewModel)value; }
        }

        public PreferencesViewModel ViewModel { get; set; }

        #endregion
    }
}