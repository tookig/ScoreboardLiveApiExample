using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text;

namespace ScoreboardLiveApi {
  public class Link {
    public class LinkResponse : ScoreboardResponse {
      [JsonPropertyName("link")]
      public Link Link { get; set; }
    }
    [JsonPropertyName("linkid"), JsonConverter(typeof(Converters.IntToString))]
    public int LinkID { get; set; }

    [JsonPropertyName("sourceclass"), JsonConverter(typeof(Converters.IntToString))]
    public int SourceClassID { get; set; }

    [JsonPropertyName("sourceplace"), JsonConverter(typeof(Converters.IntToString))]
    public int SourcePlace { get; set; }

    [JsonPropertyName("targetmatch"), JsonConverter(typeof(Converters.IntToString))]
    public int TargetMatchID { get; set; }

    [JsonPropertyName("targetteam")]
    public string TargetTeam { get; set; }
  }
}
