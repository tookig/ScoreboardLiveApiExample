using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ScoreboardLiveApi {
  public class ScoreboardResponse {
    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; }

    public ScoreboardResponse() {
      Errors = new List<string>();
    }
  }
}
