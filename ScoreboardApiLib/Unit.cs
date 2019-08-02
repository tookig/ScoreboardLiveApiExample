using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ScoreboardLiveApi {
  [DataContract(Name="unit")]
  public class Unit {
    
    [DataContract(Name="unitResponse")]
    public class UnitResponse : ScoreboardResponse {
      [DataMember(Name = "units")]
      public List<Unit> Units { get; set; }

      public UnitResponse() {
        Units = new List<Unit>();
      }
    }

    [DataMember(Name="unitid")]
    public int UnitID { get; set; }

    [DataMember(Name="name")]
    public string Name { get; set; }

    public override string ToString() {
      return String.Format("UnitID: {0}, Name: {1}", UnitID, Name);
    }
  }
}
