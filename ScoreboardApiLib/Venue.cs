using System;
using System.Runtime.Serialization;

namespace ScoreboardLiveApi {
  [DataContract(Name="venue")]
  public class Venue {
    [DataMember(Name="venueid")]
    public int VenueID { get; set; }

    [DataMember(Name="name")]
    public string Name { get; set; }

    public override string ToString() {
      return String.Format("VenueID: {0}, Name: {1}", VenueID, Name);
    }
  }
}
