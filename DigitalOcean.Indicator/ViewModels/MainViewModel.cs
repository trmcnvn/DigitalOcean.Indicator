using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DigitalOcean.API;
using DigitalOcean.API.Responses;
using DigitalOcean.Indicator.Models;
using ReactiveUI;
using Splat;
using Droplet = DigitalOcean.Indicator.Models.Droplet;

namespace DigitalOcean.Indicator.ViewModels {
    public class MainViewModel : ReactiveObject {
        private readonly UserSettings _userSettings;
        private bool _preferencesOpened;

        public ReactiveCommand<object> Preferences { get; private set; }
        public ReactiveCommand<object> Refresh { get; private set; }
        public ReactiveCommand<object> Close { get; private set; }

        public ReactiveCommand<List<Droplet>> Droplets { get; set; }
        public ReactiveCommand<EventPtr> Reboot { get; private set; }
        public ReactiveCommand<EventPtr> PowerOff { get; private set; }
        public ReactiveCommand<EventPtr> PowerOn { get; private set; }

        public bool PreferencesOpened {
            get { return _preferencesOpened; }
            set { this.RaiseAndSetIfChanged(ref _preferencesOpened, value); }
        }

        public MainViewModel() {
            _userSettings = Locator.Current.GetService<UserSettings>();

            Preferences = ReactiveCommand.Create(this.WhenAnyValue(x => x.PreferencesOpened, po => !po));
            Preferences.Subscribe(_ => PreferencesOpened = true);

            Refresh =
                ReactiveCommand.Create(this.WhenAnyValue(x => x._userSettings.ClientId, x => x._userSettings.ApiKey,
                    (c, a) => !String.IsNullOrWhiteSpace(c) && !String.IsNullOrWhiteSpace(a)));
            Refresh.Subscribe(_ => Droplets.Execute(null));

            Close = ReactiveCommand.Create();
            Droplets = ReactiveCommand.Create(_ => GetDroplets());
            Reboot = ReactiveCommand.Create(x => RebootDroplet(x));
            PowerOff = ReactiveCommand.Create(x => PowerOffDroplet(x));
            PowerOn = ReactiveCommand.Create(x => PowerOnDroplet(x));

            AuthCheck();
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

        private IObservable<EventPtr> RebootDroplet(object id) {
            var client = new DigitalOceanClient(_userSettings.ClientId, _userSettings.ApiKey);
            return Observable.StartAsync(async () => await client.Droplets.RebootDroplet((int)id));
        }

        private IObservable<EventPtr> PowerOffDroplet(object id) {
            var client = new DigitalOceanClient(_userSettings.ClientId, _userSettings.ApiKey);
            return Observable.StartAsync(async () => await client.Droplets.PowerOffDroplet((int)id));
        }

        private IObservable<EventPtr> PowerOnDroplet(object id) {
            var client = new DigitalOceanClient(_userSettings.ClientId, _userSettings.ApiKey);
            return Observable.StartAsync(async () => await client.Droplets.PowerOnDroplet((int)id));
        }

        private void AuthCheck() {
            if (Refresh.CanExecute(null)) {
                Refresh.Execute(null);
            }
        }
    }
}