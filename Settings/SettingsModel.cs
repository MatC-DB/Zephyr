using System.Runtime.Serialization;

namespace Zephyr.Settings;

public class SettingsModel {
    [DataContract]
    public struct Settings {
        [DataMember]
        public bool ShowWorkAreas { get; set; }
    }
}
