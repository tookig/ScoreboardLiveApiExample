using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

namespace ScoreboardLiveApi {
  /// <summary>
  /// Keystore able to store devices for different domains. Set the DefaultDomain property to a string identifying what domain
  /// the keystore should set and retrieve keys for. This enables the use of one key store for multiple URL's, and thus 
  /// removing the problem with identical Unit ID's being used on different websites.
  /// 
  /// The default DefaultDomain is set to 'default'.
  /// </summary>
  [Serializable]
  public class LocalDomainKeyStore : IKeyStore, ISerializable {
    private Dictionary<string, List<Device>> m_devices;

    public string DefaultDomain { get; set; }

    public LocalDomainKeyStore(string defaultDomain = "default") {
      m_devices = new Dictionary<string, List<Device>>();
      DefaultDomain = defaultDomain;
    }

    public Device Get(int unitId) {
      return Get(unitId, DefaultDomain);
    }

    public Device Get(int unitId, string domain) {
      List<Device> domainDevices;
      if (m_devices.TryGetValue(domain, out domainDevices)) {
        return domainDevices.Find(device => device.UnitID == unitId);
      }
      return null;
    }

    public void Set(Device device) {
      Set(device, DefaultDomain);
    }

    public void Set(Device device, string domain) {
      if (device == null) throw new ArgumentNullException(nameof(device), "Device reference cannot be null");
      if (domain == null) throw new ArgumentNullException(nameof(domain), "Domain identifier reference cannot be null");

      List<Device> domainDevices;
      if (!m_devices.TryGetValue(domain, out domainDevices)) {
        domainDevices = new List<Device>();
        m_devices.Add(domain, domainDevices);
      }

      domainDevices.RemoveAll(savedDevice => device.UnitID == savedDevice.UnitID);
      domainDevices.Add(device);
    }

    public void Remove(Device device) {
      Remove(device, DefaultDomain);
    }

    public void Remove(Device device, string domain) {
      List<Device> domainDevices;
      if (m_devices.TryGetValue(domain, out domainDevices)) {
        domainDevices.RemoveAll(savedDevice => device.UnitID == savedDevice.UnitID);
      }
      
    }

    public void Save(string filename) {
      FileStream stream = new FileStream(filename, FileMode.Create);
      using (stream) {
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        formatter.Serialize(stream, this);
      }
    }

    public static LocalDomainKeyStore Load(string filename) {
      if (!File.Exists(filename)) {
        return new LocalDomainKeyStore();
      }
      FileStream stream = new FileStream(filename, FileMode.Open);
      using (stream) {
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        return (LocalDomainKeyStore)formatter.Deserialize(stream);
      }
    }

    #region Serialization
    private const int c_version = 2;
    private const string c_id_version = "Settings.version";
    private const string c_id_devices = "Settings.devices";

    protected LocalDomainKeyStore(SerializationInfo info, StreamingContext context) {
      m_devices = (Dictionary<string, List<Device>>)info.GetValue(c_id_devices, typeof(Dictionary<string, List<Device>>));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      info.AddValue(c_id_version, c_version);
      info.AddValue(c_id_devices, m_devices);
    }
    #endregion
  }
}
