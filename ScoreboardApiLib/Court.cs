using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ScoreboardLiveApi {
  [DataContract(Name="court")]
  public class Court {
    
    [DataContract(Name="courtResponse")]
    public class CourtResponse : ScoreboardResponse {
      [DataMember(Name = "courts")]
      public List<Court> Courts { get; set; }

      public CourtResponse() {
        Courts = new List<Court>();
      }
    }

    [DataMember(Name="courtid")]
    public int CourtID { get; set; }

    [DataMember(Name = "gameid")]
    public int MatchID { get; set; }

    [DataMember(Name="name")]
    public string Name { get; set; }

    [DataMember(Name = "venue")]
    public Venue Venue { get; set; }

    public override string ToString() {
      return String.Format("CourtID: {0}, Name: {1}, MatchID: {2}", CourtID, Name, MatchID);
    }
  }
}
