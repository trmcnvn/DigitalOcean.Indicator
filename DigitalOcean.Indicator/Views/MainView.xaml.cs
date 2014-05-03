using System;
using System.Diagnostics;
using System.Windows;
using DigitalOcean.Indicator.ViewModels;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.Views {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window, IViewFor<MainViewModel> {
        public MainView() {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<MainViewModel>();

            this.BindCommand(ViewModel, x => x.Preferences, x => x.TrayCtxPrefs);
            this.BindCommand(ViewModel, x => x.Close, x => x.TrayCtxClose);

            this.WhenAnyObservable(x => x.ViewModel.Preferences)
                .Subscribe(_ => {
                    var view = new PreferencesView { Owner = this };
                    ((IViewFor)view).ViewModel = new PreferencesViewModel();
                    view.Show();
                });
            this.WhenAnyObservable(x => x.ViewModel.Close)
                .Subscribe(_ => Close());
        }

        // Window has to be shown before it can be assigned as an Owner of another.
        protected override void OnContentRendered(EventArgs e) {
            base.OnContentRendered(e);
            Hide();
        }

        #region IViewFor<IMainViewModel> Members

        object IViewFor.ViewModel {
            get { return ViewModel; }
            set { ViewModel = (MainViewModel)value; }
        }

        public MainViewModel ViewModel { get; set; }

        #endregion
    }
}