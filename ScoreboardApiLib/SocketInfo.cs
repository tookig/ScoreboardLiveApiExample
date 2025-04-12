using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ScoreboardLiveApi {
  public class SocketInfo {
    public class SocketInfoResponse : ScoreboardResponse {
      [JsonPropertyName("uri")]
      public string? URL { get; set; }
    }
  }
}
