using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DigitalOcean.API;
using DigitalOcean.Indicator.Models;
using ReactiveUI;
using Splat;

namespace DigitalOcean.Indicator.ViewModels {
    public class MainViewModel : ReactiveObject {
        private readonly UserSettings _userSettings;
        private bool _preferencesOpened;

        public ReactiveCommand<object> Preferences { get; private set; }
        public ReactiveCommand<object> Refresh { get; private set; }
        public ReactiveCommand<object> Close { get; private set; }
        public ReactiveCommand<List<Droplet>> Droplets { get; set; }

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
                        Type = image.image.name,
                        Status = droplet.status == "active" ? DropletStatus.On : DropletStatus.Off
                    });
                }
                return dropletList;
            });
        }

        private void AuthCheck() {
            if (Refresh.CanExecute(null)) {
                Refresh.Execute(null);
            }
        }
    }
}