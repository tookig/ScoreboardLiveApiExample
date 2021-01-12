using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ScoreboardLiveApi {
  [Serializable]
public class Device {
    public class DeviceResponse : ScoreboardResponse {
      [JsonPropertyName("device")]
      public Device Device { get; set; }
    }

    [JsonPropertyName("activationCode")]
    public string DeviceCode { get; set; }

    [JsonPropertyName("expiresAt")]
    private string JsonExpires { get; set; }

    [JsonIgnore]
    public DateTime Expires {
      get {
        return DateTime.ParseExact(JsonExpires, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
      }
    }

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }

    [JsonPropertyName("serverToken")]
    public string ServerToken { get; set; }

    [JsonPropertyName("unit"), JsonConverter(typeof(Converters.IntToString))]
    public int UnitID { get; set; }

    public Device() {
    }

    public override string ToString() {
      return string.Format("Device {0} for unit with id {1} expires at {2}. Client token: {3}", DeviceCode, UnitID, Expires, ClientToken);
    }
  }
}
