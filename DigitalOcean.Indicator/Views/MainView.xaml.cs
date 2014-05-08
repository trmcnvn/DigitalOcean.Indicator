using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using DigitalOcean.Indicator.Models;
using DigitalOcean.Indicator.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.Views {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window, IViewFor<MainViewModel> {
        private readonly CompositeDisposable _disposables;

        public MainView() {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<MainViewModel>();
            _disposables = new CompositeDisposable();

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
                    TrayCtxStatus.Visibility = Visibility.Visible;
                    TrayCtxStatus.Header = "Refreshing...";
                    TrayCtxRefresh.IsEnabled = false;

                    // remove previous entries, if any
                    var statusIdx = TrayCtx.Items.IndexOf(TrayCtxStatus);
                    if (statusIdx != 0) {
                        var items = new List<object>();
                        for (var i = 0; i < statusIdx; i++) {
                            items.Add(TrayCtx.Items.GetItemAt(i));
                        }

                        foreach (var item in items) {
                            TrayCtx.Items.Remove(item);
                        }
                    }
                });

            this.WhenAnyObservable(x => x.ViewModel.Droplets)
                .Subscribe(x => {
                    TrayCtxStatus.Visibility = Visibility.Collapsed;
                    TrayCtxRefresh.IsEnabled = true;
                    _disposables.Clear();

                    foreach (var droplet in x) {
                        var menuItem = new MenuItem { Header = droplet.Name, ItemsSource = CreateDropletMenu(droplet) };
                        TrayCtx.Items.Insert(0, menuItem);
                    }
                });

            Observable.Merge(this.WhenAnyObservable(x => x.ViewModel.Reboot),
                this.WhenAnyObservable(x => x.ViewModel.PowerOff),
                this.WhenAnyObservable(x => x.ViewModel.PowerOn))
                .Subscribe(_ => ShowBalloonTip("Finished", BalloonIcon.Info));
        }

        #region IViewFor<MainViewModel> Members

        object IViewFor.ViewModel {
            get { return ViewModel; }
            set { ViewModel = (MainViewModel)value; }
        }

        public MainViewModel ViewModel { get; set; }

        #endregion

        private IEnumerable<Control> CreateDropletMenu(Droplet droplet) {
            var list = new List<Control> {
                new MenuItem { Header = string.Format("IP: {0}", droplet.Address) },
                new MenuItem { Header = string.Format("Image: {0}", droplet.Image) },
                new MenuItem { Header = string.Format("Region: {0}", droplet.Region) },
                new MenuItem { Header = string.Format("Size: {0}", droplet.Size) },
                new Separator(),
            };

            var websiteButton = new MenuItem { Header = "View on website", Tag = droplet };
            _disposables.Add(websiteButton.Events().Click
                .Select(x => (MenuItem)x.Source)
                .Select(x => (Droplet)x.Tag)
                .Subscribe(x => Process.Start(x.Website)));

            var rebootButton = new MenuItem { Header = "Reboot", Tag = droplet };
            _disposables.Add(rebootButton.Events().Click
                .Select(x => (MenuItem)x.Source)
                .Select(x => (Droplet)x.Tag)
                .Subscribe(x => {
                    ShowBalloonTip(string.Format("Rebooting {0}", x.Name), BalloonIcon.Info);
                    ViewModel.Reboot.Execute(x.Id);
                }));

            var powerButton = new MenuItem {
                Header = droplet.Status == DropletStatus.On ? "Power off" : "Power on",
                Tag = droplet.Id
            };
            _disposables.Add(powerButton.Events().Click
                .Select(x => (MenuItem)x.Source)
                .Select(x => (Droplet)x.Tag)
                .Subscribe(x => {
                    if (droplet.Status == DropletStatus.On) {
                        ShowBalloonTip(string.Format("Powering off {0}", x.Name), BalloonIcon.Info);
                        ViewModel.PowerOff.Execute(x.Id);
                    } else {
                        ShowBalloonTip(string.Format("Powering on {0}", x.Name), BalloonIcon.Info);
                        ViewModel.PowerOn.Execute(x.Id);
                    }
                }));

            list.Add(websiteButton);
            list.Add(rebootButton);
            list.Add(powerButton);
            return list;
        }

        /// <summary>
        /// Window has to be shown before it can be assigned as an Owner of another.
        /// </summary>
        protected override void OnContentRendered(EventArgs e) {
            base.OnContentRendered(e);
            Hide();
        }

        private void ShowBalloonTip(string message, BalloonIcon icon) {
            Tray.ShowBalloonTip("DigitalOcean Indicator", message, icon);
        }
    }
}