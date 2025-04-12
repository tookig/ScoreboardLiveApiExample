using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ScoreboardLiveApi {
  public class Unit {
    public class UnitResponse : ScoreboardResponse {
      [JsonPropertyName("units")]
      public List<Unit> Units { get; set; }

      public UnitResponse() {
        Units = [];
      }
    }

    [JsonPropertyName("unitid"),JsonConverter(typeof(Converters.IntToString))]
    public int UnitID { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public override string ToString() {
      return string.Format("UnitID: {0}, Name: {1}", UnitID, Name);
    }
  }
}
