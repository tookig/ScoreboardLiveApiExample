using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ScoreboardLiveApi {
  public class Court {
    public class CourtResponse : ScoreboardResponse {
      [JsonPropertyName("courts")]
      public List<Court> Courts { get; set; }

      public CourtResponse() {
        Courts = new List<Court>();
      }
    }

    [JsonPropertyName("courtid"), JsonConverter(typeof(Converters.IntToString))]
    public int CourtID { get; set; }

    [JsonPropertyName("matchid"), JsonConverter(typeof(Converters.IntToString))]
    public int MatchID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("venue")]
    public Venue? Venue { get; set; }

    public override string ToString() {
      return String.Format("CourtID: {0}, Name: {1}, MatchID: {2}", CourtID, Name, MatchID);
    }
  }
}
