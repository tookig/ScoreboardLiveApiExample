using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ScoreboardLiveApi {
  [DataContract(Name = "response")]
  public class ScoreboardResponse {
    [DataMember(Name = "success")]
    public int Success { get; set; }

    [DataMember(Name = "errors")]
    public List<string> Errors { get; set; }

    public ScoreboardResponse() {
      Errors = new List<string>();
    }
  }
}
