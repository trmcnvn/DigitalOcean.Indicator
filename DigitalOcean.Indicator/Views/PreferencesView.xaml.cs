using System;
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
            ViewModel = new PreferencesViewModel();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
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