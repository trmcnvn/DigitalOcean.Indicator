namespace DigitalOcean.Indicator.Models {
    public enum DropletStatus {
        On,
        Off
    }

    public class Droplet {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
        public string Region { get; set; }
        public string Size { get; set; }
        public DropletStatus Status { get; set; }

        public string Website {
            get { return string.Format("https://cloud.digitalocean.com/droplets/{0}", Id); }
        }
    }
}