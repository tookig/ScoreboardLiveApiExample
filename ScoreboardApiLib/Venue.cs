using System;
using System.Text.Json.Serialization;

namespace ScoreboardLiveApi {
  public class Venue {
    [JsonPropertyName("venueid"), JsonConverter(typeof(Converters.IntToString))]
    public int VenueID { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public override string ToString() {
      return string.Format("VenueID: {0}, Name: {1}", VenueID, Name);
    }
  }
}
