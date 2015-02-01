using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.Indicator.Models;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.ViewModels {
    public class MainViewModel : ReactiveObject {
        private readonly CompositeDisposable _disposable;
        private readonly UserSettings _userSettings;
        private bool _preferencesOpened;

        public MainViewModel() {
            _userSettings = Locator.Current.GetService<UserSettings>();
            _disposable = new CompositeDisposable();

            Preferences = ReactiveCommand.Create(this.WhenAnyValue(x => x.PreferencesOpened, po => !po));
            Preferences.Subscribe(_ => PreferencesOpened = true);

            Refresh =
                ReactiveCommand.Create(this.WhenAnyValue(x => x._userSettings.ApiKey,
                    (a) => !String.IsNullOrWhiteSpace(a)));
            Refresh.Subscribe(_ => Droplets.Execute(null));

            Close = ReactiveCommand.Create();
            Droplets = ReactiveCommand.CreateAsyncObservable(_ => GetDroplets());
            Reboot = ReactiveCommand.CreateAsyncObservable(x => RebootDroplet((Droplet) x));
            PowerOff = ReactiveCommand.CreateAsyncObservable(x => PowerOffDroplet((Droplet) x));
            PowerOn = ReactiveCommand.CreateAsyncObservable(x => PowerOnDroplet((Droplet) x));

            RefreshDroplets();
        }

        public ReactiveCommand<object> Preferences { get; private set; }
        public ReactiveCommand<object> Refresh { get; private set; }
        public ReactiveCommand<object> Close { get; private set; }

        public ReactiveCommand<List<Droplet>> Droplets { get; set; }
        public ReactiveCommand<Droplet> Reboot { get; private set; }
        public ReactiveCommand<Droplet> PowerOff { get; private set; }
        public ReactiveCommand<Droplet> PowerOn { get; private set; }

        public bool PreferencesOpened {
            get { return _preferencesOpened; }
            set { this.RaiseAndSetIfChanged(ref _preferencesOpened, value); }
        }

        private IObservable<List<Droplet>> GetDroplets() {
            var client = new DigitalOceanClient(_userSettings.ApiKey);
            return Observable.StartAsync(async () => {
                var droplets = await client.Droplets.GetAll();
                return droplets.Select(droplet => new Droplet {
                    Id = droplet.Id,
                    Name = droplet.Name,
                    Address = droplet.Networks.v4[0].IpAddress,
                    Region = droplet.Region.Name,
                    Size = droplet.SizeSlug,
                    Image = droplet.Image.Name,
                    Status = droplet.Status == "active" ? DropletStatus.On : DropletStatus.Off
                }).ToList();
            });
        }

        private IObservable<Droplet> RebootDroplet(Droplet droplet) {
            var client = new DigitalOceanClient(_userSettings.ApiKey);
            return Observable.StartAsync(ct => Task.Run(async () => {
                var reboot = await client.DropletActions.Reboot(droplet.Id);
                await WaitForAction(client, reboot.Id);
                return droplet;
            }, ct));
        }

        private IObservable<Droplet> PowerOffDroplet(Droplet droplet) {
            var client = new DigitalOceanClient(_userSettings.ApiKey);
            return Observable.StartAsync(ct => Task.Run(async () => {
                var power = await client.DropletActions.PowerOff(droplet.Id);
                await WaitForAction(client, power.Id);
                return droplet;
            }, ct));
        }

        private IObservable<Droplet> PowerOnDroplet(Droplet droplet) {
            var client = new DigitalOceanClient(_userSettings.ApiKey);
            return Observable.StartAsync(ct => Task.Run(async () => {
                var power = await client.DropletActions.PowerOn(droplet.Id);
                await WaitForAction(client, power.Id);
                return droplet;
            }, ct));
        }

        private void RefreshDroplets() {
            if (!Refresh.CanExecute(null)) {
                return;
            }

            this.WhenAnyValue(x => x._userSettings.RefreshInterval)
                .Subscribe(x => {
                    _disposable.Clear();
                    _disposable.Add(Observable.Interval(TimeSpan.FromSeconds(x))
                        .Subscribe(_ => Refresh.Execute(null)));
                });
            Refresh.Execute(null);
        }

        private static async Task WaitForAction(IDigitalOceanClient client, int actionId) {
            while (true) {
                var @event = await client.Actions.Get(actionId);
                if (@event.CompletedAt != null) {
                    break;
                }
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}