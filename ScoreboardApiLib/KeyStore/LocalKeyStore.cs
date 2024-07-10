using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

namespace ScoreboardLiveApi {
  [Serializable]
  public class LocalKeyStore: IKeyStore {
    private List<Device> m_devices;

    public LocalKeyStore() {
      m_devices = new List<Device>();
    }

    public Device Get(int unitId) {
      return m_devices.Find(device => device.UnitID == unitId);
    }

    public void Set(Device device) {
      if (device == null) throw new ArgumentNullException(nameof(device), "Device reference cannot be null");
      m_devices.RemoveAll(savedDevice => device.UnitID == savedDevice.UnitID);
      m_devices.Add(device);
    }

    public void Remove(Device device) {
      m_devices.RemoveAll(savedDevice => device.UnitID == savedDevice.UnitID);
    }

    public void Save(string filename) {
      FileStream stream = new FileStream(filename, FileMode.Create);
      using (stream) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        formatter.Serialize(stream, this);
      }
    }

    public static LocalKeyStore Load(string filename) {
      if (!File.Exists(filename)) {
        return new LocalKeyStore();
      }
      FileStream stream = new FileStream(filename, FileMode.Open);
      using (stream) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        return (LocalKeyStore)formatter.Deserialize(stream);
      }
    }

    #region Serialization
    private const int c_version = 1;
    private const string c_id_version = "Settings.version";
    private const string c_id_devices = "Settings.devices";

    protected LocalKeyStore(SerializationInfo info, StreamingContext context) {
      m_devices = (List<Device>)info.GetValue(c_id_devices, typeof(List<Device>));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      info.AddValue(c_id_version, c_version);
      info.AddValue(c_id_devices, m_devices);
    }
    #endregion
  }
}
