using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace ScoreboardLiveApi {
  [DataContract(Name = "tournament")]
  public class Tournament {
    [DataContract(Name = "tournamentResponse")]
    public class TournamentResponse : ScoreboardResponse {
      [DataMember(Name = "tournaments")]
      public List<Tournament> Tournaments { get; set; }
      public TournamentResponse() {
        Tournaments = new List<Tournament>();
      }
    }

    [DataMember(Name = "tournamentid")]
    public int TournamentID { get; set; }

    [DataMember(Name = "parenttournamentid")]
    public int ParentTournamentID { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "type")]
    public string TournamentType { get; set; }

    [DataMember(Name = "team1")]
    public string Team1 { get; set; }

    [DataMember(Name = "team2")]
    public string Team2 { get; set; }

    [DataMember(Name = "startdate")]
    private string JsonStartDate { get; set; }
    [IgnoreDataMember]
    public DateTime StartDate {
      get {
        return DateTime.ParseExact(JsonStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
      }
    }

    [DataMember(Name = "enddate")]
    private string JsonEndDate { get; set; }
    [IgnoreDataMember]
    public DateTime EndDate {
      get {
        return DateTime.ParseExact(JsonEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
      }
    }

    [DataMember(Name = "status")]
    public string Status { get; set; }

    [DataMember(Name = "scoresystem")]
    public string ScoreSystem { get; set; }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append(Name);
      if (!string.IsNullOrEmpty(Team1) && !string.IsNullOrEmpty(Team2)) {
        sb.AppendFormat(" {0} - {1}", Team1, Team2);
      }
      sb.AppendFormat(" ({0})", StartDate.ToShortDateString());
      return sb.ToString();
    }
  }
}
