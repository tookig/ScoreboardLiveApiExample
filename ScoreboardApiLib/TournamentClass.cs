using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ScoreboardLiveApi {
  [Serializable]
  public class TournamentClass {
    [JsonPropertyName("classid")]
    public int ID { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("tournament")]
    public int TournamentID { get; set; }

    [JsonPropertyName("parentclass")]
    public int ParentClassID { get; set; }

    [JsonPropertyName("size")]
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
