using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ScoreboardLiveApi {
  [DataContract(Name = "response")]
  public class ScoreboardResponse {
    [DataMember(Name = "errors")]
    public List<string> Errors { get; set; }

    public ScoreboardResponse() {
      Errors = new List<string>();
    }
  }
}
