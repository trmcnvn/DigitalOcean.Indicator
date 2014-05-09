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

        public MainViewModel() {
            _userSettings = Locator.Current.GetService<UserSettings>();
            _disposable = new CompositeDisposable();

            Preferences = ReactiveCommand.Create(this.WhenAnyValue(x => x.PreferencesOpened, po => !po));
            Preferences.Subscribe(_ => PreferencesOpened = true);

            Refresh =
                ReactiveCommand.Create(this.WhenAnyValue(x => x._userSettings.ClientId, x => x._userSettings.ApiKey,
                    (c, a) => !String.IsNullOrWhiteSpace(c) && !String.IsNullOrWhiteSpace(a)));
            Refresh.Subscribe(_ => Droplets.Execute(null));

            Close = ReactiveCommand.Create();
            Droplets = ReactiveCommand.Create(_ => GetDroplets());
            Reboot = ReactiveCommand.Create(x => RebootDroplet((Droplet)x));
            PowerOff = ReactiveCommand.Create(x => PowerOffDroplet((Droplet)x));
            PowerOn = ReactiveCommand.Create(x => PowerOnDroplet((Droplet)x));

            RefreshDroplets();
        }

        private IObservable<List<Droplet>> GetDroplets() {
            var client = new DigitalOceanClient(_userSettings.ClientId, _userSettings.ApiKey);
            return Observable.StartAsync(async () => {
                var dropletList = new List<Droplet>();

                var droplets = await client.Droplets.GetDroplets();
                foreach (var droplet in droplets.droplets) {
                    var image = await client.Images.GetImage(droplet.image_id);

                    var regions = await client.Regions.GetRegions();
                    var regionName = regions.regions.First(x => x.id == droplet.region_id).name;

                    var sizes = await client.Sizes.GetSizes();
                    var sizeType = sizes.sizes.First(x => x.id == droplet.size_id).name;

                    dropletList.Add(new Droplet {
                        Id = droplet.id,
                        Name = droplet.name,
                        Address = droplet.ip_address,
                        Region = regionName,
                        Size = sizeType,
                        Image = image.image.name,
                        Status = droplet.status == "active" ? DropletStatus.On : DropletStatus.Off
                    });
                }
                return dropletList;
            });
        }

        private IObservable<Droplet> RebootDroplet(Droplet droplet) {
            var client = new DigitalOceanClient(_userSettings.ClientId, _userSettings.ApiKey);
            return Observable.StartAsync(ct => Task.Run(async () => {
                var reboot = await client.Droplets.RebootDroplet(droplet.Id);
                await WaitForEvent(client, reboot.event_id);
                return droplet;
            }, ct));
        }

        private IObservable<Droplet> PowerOffDroplet(Droplet droplet) {
            var client = new DigitalOceanClient(_userSettings.ClientId, _userSettings.ApiKey);
            return Observable.StartAsync(ct => Task.Run(async () => {
                var power = await client.Droplets.PowerOffDroplet(droplet.Id);
                await WaitForEvent(client, power.event_id);
                return droplet;
            }, ct));
        }

        private IObservable<Droplet> PowerOnDroplet(Droplet droplet) {
            var client = new DigitalOceanClient(_userSettings.ClientId, _userSettings.ApiKey);
            return Observable.StartAsync(ct => Task.Run(async () => {
                var power = await client.Droplets.PowerOnDroplet(droplet.Id);
                await WaitForEvent(client, power.event_id);
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

        private static async Task WaitForEvent(DigitalOceanClient client, int eventId) {
            while (true) {
                var @event = await client.Events.GetEvent(eventId);
                int percent;
                if (int.TryParse(@event.@event.percentage, out percent) && percent == 100) {
                    break;
                }
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}