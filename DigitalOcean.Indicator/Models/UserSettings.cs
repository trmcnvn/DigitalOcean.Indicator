using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using Akavache;
using ReactiveUI;

namespace DigitalOcean.Indicator.Models {
    [DataContract]
    public class UserSettings : ReactiveObject {
        private const string CacheKey = "__UserSettings__";
        private string _apiKey = "";
        private int _refreshInterval = 300;

        [DataMember]
        public string ApiKey {
            get { return _apiKey; }
            set { this.RaiseAndSetIfChanged(ref _apiKey, value); }
        }

        [DataMember]
        public int RefreshInterval {
            get { return _refreshInterval; }
            set { this.RaiseAndSetIfChanged(ref _refreshInterval, value); }
        }

        public IObservable<Unit> Save() {
            return BlobCache.UserAccount.InsertObject(CacheKey, this).Retry(3);
        }

        public static IObservable<UserSettings> LoadFromCache() {
            return BlobCache.UserAccount.GetOrCreateObject(CacheKey, () => new UserSettings());
        }
    }
}