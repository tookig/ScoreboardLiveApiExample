using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ScoreboardLiveApi {
  [Serializable]
  public class TournamentClass {
    public class TournamentClassResponse : ScoreboardResponse {
      [JsonPropertyName("class")]
      public TournamentClass TournamentClass { get; set; }
    }

    [JsonPropertyName("classid"), JsonConverter(typeof(Converters.IntToString))]
    public int ID { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("tournament"), JsonConverter(typeof(Converters.IntToString))]
    public int TournamentID { get; set; }

    [JsonPropertyName("parentclass"), JsonConverter(typeof(Converters.IntToString))]
    public int ParentClassID { get; set; }

    [JsonPropertyName("size"), JsonConverter(typeof(Converters.IntToString))]
    public int Size { get; set; }

    [JsonPropertyName("type")]
    public string ClassType { get; set; }

    public TournamentClass() {

    }

    public override string ToString() {
      return string.Format("Class {0} - {1}", ID, Description);
    }
  }
}
