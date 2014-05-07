using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
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

            this.BindCommand(ViewModel, x => x.Refresh, x => x.TrayCtxRefresh);
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

            this.WhenAnyObservable(x => x.ViewModel.Refresh)
                .Subscribe(_ => {
                    TrayCtxPlaceholder.Visibility = Visibility.Visible;
                    TrayCtxPlaceholder.Header = "Refreshing...";

                    // need to clear the previous entries, if any
                    var idx = TrayCtx.Items.IndexOf(TrayCtxPlaceholder);
                    if (idx != 0) {
                        var items = new List<object>();
                        for (var i = 0; i < idx; ++i) {
                            items.Add(TrayCtx.Items.GetItemAt(i));
                        }

                        foreach (var item in items) {
                            TrayCtx.Items.Remove(item);
                        }
                    }
                });

            this.WhenAnyObservable(x => x.ViewModel.Droplets)
                .SelectMany(x => x)
                .Subscribe(x => {
                    TrayCtxPlaceholder.Visibility = Visibility.Collapsed;

                    var menuItem = new MenuItem { Header = x.Name };
                    var subMenu = new List<Control> {
                        new MenuItem { Header = string.Format("IP: {0}", x.Address) },
                        new MenuItem { Header = string.Format("Type: {0}", x.Type) },
                        new MenuItem { Header = string.Format("Region: {0}", x.Region) },
                        new MenuItem { Header = string.Format("Size: {0}", x.Size) },
                        new Separator(),
                        new MenuItem { Header = "View on web..." },
                        new MenuItem { Header = "Power off..." },
                        new MenuItem { Header = "Reboot..." }
                    };

                    menuItem.ItemsSource = subMenu;
                    TrayCtx.Items.Insert(0, menuItem);
                });
        }

        #region IViewFor<MainViewModel> Members

        object IViewFor.ViewModel {
            get { return ViewModel; }
            set { ViewModel = (MainViewModel)value; }
        }

        public MainViewModel ViewModel { get; set; }

        #endregion

        /// <summary>
        /// Window has to be shown before it can be assigned as an Owner of another.
        /// </summary>
        protected override void OnContentRendered(EventArgs e) {
            base.OnContentRendered(e);
            Hide();
        }
    }
}