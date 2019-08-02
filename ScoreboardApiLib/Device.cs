using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace ScoreboardLiveApi {
  [Serializable]
  [DataContract(Name = "device")]
  public class Device {
    [DataContract(Name = "deviceResponse")]
    public class DeviceResponse : ScoreboardResponse {
      [DataMember(Name = "device")]
      public Device Device { get; set; }
    }

    [DataMember(Name = "activationCode")]
    public string DeviceCode { get; set; }

    [DataMember(Name = "expiresAt")]
    private string JsonExpires { get; set; }

    [IgnoreDataMember]
    public DateTime Expires {
      get {
        return DateTime.ParseExact(JsonExpires, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
      }
    }

    [DataMember(Name = "clientToken")]
    public string ClientToken { get; set; }

    [DataMember(Name = "serverToken")]
    public string ServerToken { get; set; }

    [DataMember(Name = "unit")]
    public int UnitID { get; set; }

    public Device() {
    }

    public override string ToString() {
      return string.Format("Device {0} for unit with id {1} expires at {2}. Client token: {3}", DeviceCode, UnitID, Expires, ClientToken);
    }
  }
}
