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
        private string _apiKey;
        private string _clientId;
        private TimeSpan _refreshInterval;

        [DataMember]
        public string ClientId {
            get { return _clientId; }
            set { this.RaiseAndSetIfChanged(ref _clientId, value); }
        }

        [DataMember]
        public string ApiKey {
            get { return _apiKey; }
            set { this.RaiseAndSetIfChanged(ref _apiKey, value); }
        }

        [DataMember]
        public TimeSpan RefreshInterval {
            get { return _refreshInterval; }
            set { this.RaiseAndSetIfChanged(ref _refreshInterval, value); }
        }

        public UserSettings() {
            SetDefaultValues();
        }

        public void SetDefaultValues() {
            ClientId = "";
            ApiKey = "";
            RefreshInterval = TimeSpan.FromMinutes(5);
        }

        public IObservable<Unit> Save() {
            return BlobCache.UserAccount.InsertObject(CacheKey, this).Retry(3);
        }

        public IDisposable AutoSave() {
            return this.AutoPersist(_ => Save(), TimeSpan.FromMilliseconds(100));
        }

        public static IObservable<UserSettings> LoadFromCache() {
            return BlobCache.UserAccount.GetOrCreateObject(CacheKey, () => new UserSettings());
        }
    }
}