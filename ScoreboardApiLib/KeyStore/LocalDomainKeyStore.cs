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
    private readonly Dictionary<string, List<Device>> m_devices = [];

    public string DefaultDomain { get; set; } = "default";

    public LocalDomainKeyStore(string defaultDomain = "default") {
      DefaultDomain = defaultDomain;
    }

    public Device? Get(int unitId) {
      return Get(unitId, DefaultDomain);
    }

    public Device? Get(int unitId, string domain) {
      if (m_devices.TryGetValue(domain, out List<Device>? domainDevices)) {
        return domainDevices.Find(device => device.UnitID == unitId);
      }
      return null;
    }

    public void Set(Device device) {
      Set(device, DefaultDomain);
    }

    public void Set(Device device, string domain) {
      ArgumentNullException.ThrowIfNull(device);
      ArgumentNullException.ThrowIfNull(domain);

      if (!m_devices.TryGetValue(domain, out List<Device>? domainDevices)) {
        domainDevices = [];
        m_devices.Add(domain, domainDevices);
      }

      domainDevices.RemoveAll(savedDevice => device.UnitID == savedDevice.UnitID);
      domainDevices.Add(device);
    }

    public void Remove(Device device) {
      Remove(device, DefaultDomain);
    }

    public void Remove(Device device, string domain) {
      if (m_devices.TryGetValue(domain, out List<Device>? domainDevices)) {
        domainDevices.RemoveAll(savedDevice => device.UnitID == savedDevice.UnitID);
      }
    }

    public void Save(string filename) {
      FileStream stream = new(filename, FileMode.Create);
      using (stream) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        formatter.Serialize(stream, this);
      }
    }

    public static LocalDomainKeyStore Load(string filename) {
      if (!File.Exists(filename)) {
        return new LocalDomainKeyStore();
      }
      FileStream stream = new(filename, FileMode.Open);
      using (stream) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        return (LocalDomainKeyStore)formatter.Deserialize(stream);
      }
    }

    #region Serialization
    private const int c_version = 2;
    private const string c_id_version = "Settings.version";
    private const string c_id_devices = "Settings.devices";

    protected LocalDomainKeyStore(SerializationInfo info, StreamingContext context) {
      m_devices = (info.GetValue(c_id_devices, typeof(Dictionary<string, List<Device>>)) as Dictionary<string, List<Device>>) ?? [];
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      info.AddValue(c_id_version, c_version);
      info.AddValue(c_id_devices, m_devices);
    }
    #endregion
  }
}
